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
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;
		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<BodyMessage, DeltaBodyMessage> _bodyDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<TickstampedMessage<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<TickstampedResponseMessage<DeltaWorldSnapshotMessage>> _worldSerializer;

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

		public Server(Log log, HostConfig config, double tickStep, IPEndPoint publicEndPoint) : base(log, config, tickStep)
		{
			var maxPlayers = config.PlayerLimit.Value;
			var rng = Plugin.Instance.Random;

			_worldDiff = new WorldSnapshotMessageDifferentiator();
			_inputDiff = new InputSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedMessageSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new TickstampedResponseMessageSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			LocalSnapshot.PartyID = Key32.FromRandom(rng);
			LocalSnapshot.Secret = new JoinSecret(Plugin.Instance.Version, publicEndPoint, Key32.FromRandom(rng), tickStep, maxPlayers);

			_peerIDs = new Dictionary<NetPeer, int>();

			BodyFitter = new BodyMessageFitter();
			Husks = new Option<Husk>[maxPlayers];

			Listener.ConnectionRequestEvent += ConnectionRequested;
			Listener.PeerDisconnectedEvent += PeerDisconnected;
		}

		private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			var id = _peerIDs[peer];

			LocalSnapshot.PlayerBodies[id] = Option.None<BodyMessage>();
			Husks[id] = Option.None<Husk>();
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

		private void HandleInput()
		{
			foreach (var husk in ConnectedHusks)
			{
				if (!husk.InputDelta.MatchSome(out var inputDelta))
				{
					return;
				}

				var baseline = husk.InputSnapshots.LastOrNone().Map(x => x.Value);
				var snapshot = _inputDiff.ConsumeDelta(inputDelta.Content, baseline);

				husk.InputSnapshots.Add(new KeyValuePair<uint, InputSnapshotMessage>(Tick, snapshot));

				LocalSnapshot.PlayerBodies[husk.ID] = Option.Some(snapshot.Body);

				if (snapshot.Level.MatchSome(out var level))
				{
					var allowed = husk.IsAdmin;
					if (!allowed)
					{
						var isReload = level == HarmonyState.CurrentLevel;
						allowed = isReload ? Config.Permissions.SceneReloading.Value : Config.Permissions.SceneChanging.Value;
					}

					if (allowed)
					{
						LocalSnapshot.Level = level;
					}
				}
			}
		}

        protected override void OnNetUpdate()
        {
			base.OnNetUpdate();

			HandleInput();
        }

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			var husk = Husks[_peerIDs[peer]].Unwrap();

			husk.InputDelta = Option.Some(_inputSerializer.Deserialize(ref reader));
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
				_worldSerializer.Serialize(ref writer, new TickstampedResponseMessage<DeltaWorldSnapshotMessage>
				{
					Tick = Tick,
					RespondingToTick = husk.InputDelta.Map(x => x.Tick),
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

			public readonly List<KeyValuePair<uint, InputSnapshotMessage>> InputSnapshots;
			public Option<TickstampedMessage<DeltaInputSnapshotMessage>> InputDelta;

			public Option<WorldSnapshotMessage> LastSent;

			public Husk(int id, NetPeer peer, bool isAdmin)
			{
				ID = id;
				Peer = peer;

				IsAdmin = isAdmin;

				InputSnapshots = new List<KeyValuePair<uint, InputSnapshotMessage>>();
				InputDelta = Option.None<TickstampedMessage<DeltaInputSnapshotMessage>>();

				LastSent = Option.None<WorldSnapshotMessage>();
			}
		}
    }
}
