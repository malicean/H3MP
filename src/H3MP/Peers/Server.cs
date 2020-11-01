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

		private readonly Version _version;
		public readonly Key32 AdminKey;

		private readonly Dictionary<NetPeer, int> _peerIDs;

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

		public Server(Log log, HostConfig config, double tickStep, Version version, IPEndPoint publicEndPoint) : base(log, config, tickStep)
		{
			var maxPlayers = config.PlayerLimit.Value;
			var rng = Plugin.Instance.Random;

			_worldDiff = new WorldSnapshotMessageDifferentiator();
			_inputDiff = new InputSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new ResponseTickstampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			LocalSnapshot.PartyID = Key32.FromRandom(rng);
			LocalSnapshot.Secret = new JoinSecret(version, publicEndPoint, Key32.FromRandom(rng), tickStep, maxPlayers);
			LocalSnapshot.Level = HarmonyState.CurrentLevel;
			LocalSnapshot.PlayerBodies = new Option<BodyMessage>[maxPlayers];

			_version = version;
			AdminKey = Key32.FromRandom(rng);

			_peerIDs = new Dictionary<NetPeer, int>();

			Husks = new Option<Husk>[maxPlayers];

			Listener.ConnectionRequestEvent += ConnectionRequested;
			Listener.PeerDisconnectedEvent += PeerDisconnected;

			Ticked += Update;
		}

		private void ConnectionRequested(ConnectionRequest request)
		{
			const string rejectionPrefix = "Attempt rejected: ";
			if (Log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogInfo($"Incoming connection attempt from {request.RemoteEndPoint}...");
			}
			else
			{
				Log.Common.LogInfo("Incoming connection attempt...");
			}

			var reader = new BitPackReader(request.Data);

			ConnectionRequestMessage content;
			try
			{
				content = _requestSerializer.Deserialize(ref reader);
			}
			catch (Exception e)
			{
				Log.Common.LogInfo(rejectionPrefix + "data malformation");
#if DEBUG
				Log.Common.LogError("Malformation error: " + e);
#endif

				request.Reject();
				return;
			}

			if (!content.Version.CompatibleWith(_version))
			{
				if (Log.Sensitive.MatchSome(out sensitiveLog))
				{
					sensitiveLog.LogInfo(rejectionPrefix + $"incompatible version (server: {_version}; requested: {content.Version})");
				}
				else
				{
					Log.Common.LogInfo(rejectionPrefix + $"incompatible version (server: {_version})");
				}

				request.Reject();
				return;
			}

			if (!reader.Bits.Done || !reader.Bytes.Done)
			{
				Log.Common.LogInfo(rejectionPrefix + "excess data (form of data malformation)");

				request.Reject();
				return;
			}

			var isAdmin = content.IsAdmin;
			var key = isAdmin ? AdminKey : LocalSnapshot.Secret.Key;
			if (content.Key != key)
			{
				Log.Common.LogInfo(rejectionPrefix + $"invalid key (admin: {isAdmin})");

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
				_peerIDs.Add(peer, i);
				slot = Option.Some<Husk>(new Husk(i, peer, isAdmin));

				Log.Common.LogInfo("Attempt accepted.");
				return;
			}

			// Full
			Log.Common.LogInfo(rejectionPrefix + "full lobby");
			request.Reject();
		}

		private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			var id = _peerIDs[peer];

			_peerIDs.Remove(peer);
			LocalSnapshot.PlayerBodies[id] = Option.None<BodyMessage>();
			Husks[id] = Option.None<Husk>();
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			var husk = Husks[_peerIDs[peer]].Unwrap();

			Tickstamped<DeltaInputSnapshotMessage> delta;
			try
			{
				delta = _inputSerializer.Deserialize(ref reader);
			}
			catch (Exception e)
			{
				if (Log.Sensitive.MatchSome(out var sensitiveLog))
				{
					sensitiveLog.LogInfo($"Received malformed data from peer ({peer.Id}; {peer.EndPoint})");
				}
				else
				{
					Log.Common.LogInfo($"Received malformed data from peer ({peer.Id})");
				}

				Log.Common.LogDebug("Malformation error: " + e);

				Net.DisconnectPeer(peer);
				return;
			}

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
					Buffer = husk.Input.Map(x => new BufferTicks
					{
						Received = x.ReceivedTick,
						Queued = x.QueuedTick
					}),
					SentTick = Tick,
					Content = delta
				});

				writer.Dispose();
				husk.Peer.Send(data, DeliveryMethod.ReliableOrdered);

				husk.LastSent = Option.Some(snapshot);

				data.Reset();
			}
		}

		public void Start(ListenBinding binding)
		{
			Net.Start(binding.IPv4, binding.IPv6, binding.Port);
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
