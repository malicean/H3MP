using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using BepInEx.Logging;
using H3MP.Configs;
using H3MP.Differentiation;
using H3MP.Extensions;
using H3MP.Fitting;
using H3MP.HarmonyPatches;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	public class Server : Peer<WorldSnapshotMessage, HostConfig>
	{
		public delegate void DeltaSnapshotReceivedHandler(Husk client, uint clientTick, DeltaInputSnapshotMessage delta);
		public delegate void SnapshotReceivedHandler(Husk client, uint clientTick, InputSnapshotMessage delta);

		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;
		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<Tickstamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<ResponseTickstamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		public readonly Key32 AdminKey;

		private readonly Dictionary<NetPeer, int> _peerIDs;

		public readonly IFitter<BodyMessage> BodyFitter;
		public readonly Option<Husk>[] Husks;

		public IEnumerable<Husk> ConnectedHusks
		{
			get
			{
				foreach (Option<Husk> husk in Husks)
				{
					if (husk.MatchSome(out var connected))
					{
						yield return connected;
					}
				}
			}
		}

		public event DeltaSnapshotReceivedHandler DeltaSnapshotReceived;
		public event SnapshotReceivedHandler SnapshotUpdated;

		public Server(Log log, HostConfig config, double tickStep, IPEndPoint publicEndPoint) : base(log, config, tickStep)
		{
			var maxPlayers = config.PlayerLimit.Value;
			var rng = Plugin.Instance.Random;

			_worldDiff = new WorldSnapshotMessageDifferentiator();
			_inputDiff = new InputSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new ResponseTickstampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			LocalSnapshot.PartyID = Key32.FromRandom(rng);
			LocalSnapshot.Secret = new JoinSecret(Plugin.Instance.Version, publicEndPoint, Key32.FromRandom(rng), tickStep, maxPlayers);

			_peerIDs = new Dictionary<NetPeer, int>();

			BodyFitter = new BodyMessageFitter();
			Husks = new Option<Husk>[maxPlayers];

			Listener.ConnectionRequestEvent += ConnectionRequested;
			Listener.PeerDisconnectedEvent += PeerDisconnected;

			Ticked += Update;
		}

		private void ConnectionRequested(ConnectionRequest request)
		{
			var reader = new BitPackReader(request.Data);

			var content = _requestSerializer.Deserialize(ref reader);
			if (!reader.Bits.Done || !reader.Bytes.Done)
			{
				request.Reject();
				return;
			}

			var isAdmin = content.IsAdmin;
			var key = isAdmin ? AdminKey : LocalSnapshot.Secret.Key;
			if (content.Key != key)
			{
				request.Reject();
				return;
			}

			for (var i = 0; i < Husks.Length; ++i)
			{
				ref var slot = ref Husks[i];
				if (slot.IsSome)
				{
					continue;
				}

				var peer = request.Accept();
				slot = Option.Some<Husk>(new Husk(i, peer, isAdmin));
			}

			// Full
			request.Reject();
		}

		private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			var id = _peerIDs[peer];

			LocalSnapshot.PlayerBodies[id] = Option.None<BodyMessage>();
			Husks[id] = Option.None<Husk>();
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			var husk = Husks[_peerIDs[peer]].Unwrap();
			var delta = _inputSerializer.Deserialize(ref reader);

			husk.InputBuffer.Enqueue(new QueueTickstamped<DeltaInputSnapshotMessage>
			{
				ReceivedTick = delta.Tick,
				QueuedTick = Tick,
				Content = delta.Content
			});
		}

		private void Update()
		{
			foreach (var husk in ConnectedHusks)
			{
				if (husk.InputBuffer.Count > 0)
				{
					if (husk.InputBuffer.Count > husk.InputBufferSize)
					{
						// Drop old input
						husk.InputBuffer.Dequeue();
					}

					husk.Input = Option.Some(husk.InputBuffer.Dequeue());
					husk.InputIsDuplicated = false;
					--husk.InputBufferSize;
				}

				++husk.InputBufferSize;
				husk.InputIsDuplicated = true;
			}
		}

		protected override void SendSnapshot(WorldSnapshotMessage snapshot)
		{
			var data = new NetDataWriter();

			foreach (var husk in ConnectedHusks)
			{
				if (!_worldDiff.CreateDelta(snapshot, husk.LastSent).MatchSome(out var delta))
				{
					continue;
				}

				var writer = new BitPackWriter(data);
				_worldSerializer.Serialize(ref writer, new ResponseTickstamped<DeltaWorldSnapshotMessage>
				{
					DuplicatedInput = husk.InputIsDuplicated,
					QueuedTick = husk.Input.Map(x => x.QueuedTick),
					ReceivedTick = husk.Input.Map(x => x.ReceivedTick),
					SentTick = Tick,
					Content = delta
				});

				writer.Dispose();
				husk.Peer.Send(data, DeliveryMethod.ReliableOrdered);

				husk.LastSent = Option.Some(snapshot);

				data.Reset();
			}
		}

		public void Start(IPAddress ipv4, IPAddress ipv6, ushort port)
		{
			Net.Start(ipv4, ipv6, port);
		}

		public void Stop()
		{
			Net.Stop();
		}

		public class Husk
		{
			public readonly int ID;
			public readonly NetPeer Peer;
			public readonly bool IsAdmin;

			public readonly Queue<QueueTickstamped<DeltaInputSnapshotMessage>> InputBuffer;
			public int InputBufferSize;

			public bool InputIsDuplicated;
			public Option<QueueTickstamped<DeltaInputSnapshotMessage>> Input;
			public Option<WorldSnapshotMessage> LastSent;

			public Husk(int id, NetPeer peer, bool isAdmin)
			{
				ID = id;
				Peer = peer;
				IsAdmin = isAdmin;

				InputBuffer = new Queue<QueueTickstamped<DeltaInputSnapshotMessage>>();
				InputBufferSize = 1;

				Input = Option.None<QueueTickstamped<DeltaInputSnapshotMessage>>();
				LastSent = Option.None<WorldSnapshotMessage>();
			}
		}
    }
}
