using System;
using System.Collections.Generic;
using System.Net;
using H3MP.Configs;
using H3MP.Differentiation;
using H3MP.Extensions;
using H3MP.HarmonyPatches;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Timing;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	public class Server : Peer<WorldSnapshotMessage, HostConfig>
	{
		public delegate void DeltaSnapshotReceivedHandler(Husk client, long clientTick, DeltaInputSnapshotMessage delta);
		public delegate void SnapshotReceivedHandler(Husk client, long clientTick, InputSnapshotMessage snapshot);

		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;
		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<TickStamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<TickStamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		private readonly Version _version;
		public readonly Key32 AdminKey;

		private readonly Dictionary<NetPeer, int> _peerIDs;

		public readonly Option<Husk>[] Husks;

		public IEnumerable<Husk> ConnectedHusks => Husks.WhereSome();

		public event DeltaSnapshotReceivedHandler DeltaSnapshotProcessed;
		public event SnapshotReceivedHandler SnapshotUpdated;

		public Server(Log log, HostConfig config, TickFrameClock clock, Version version, IPEndPoint publicEndPoint) : base(log, config, clock)
		{
			var maxPlayers = config.PlayerLimit.Value;
			var rng = Plugin.Instance.Random;

			_worldDiff = new WorldSnapshotMessageDifferentiator();
			_inputDiff = new InputSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickStampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new TickStampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			LocalSnapshot.PartyID = Key32.FromRandom(rng);
			LocalSnapshot.Secret = new JoinSecret(version, publicEndPoint, Key32.FromRandom(rng), Clock.DeltaTime, maxPlayers);
			LocalSnapshot.Level = HarmonyState.CurrentLevel;
			LocalSnapshot.PlayerBodies = new Option<BodyMessage>[maxPlayers];

			_version = version;
			AdminKey = Key32.FromRandom(rng);

			_peerIDs = new Dictionary<NetPeer, int>();

			Husks = new Option<Husk>[maxPlayers];

			Listener.ConnectionRequestEvent += ConnectionRequested;
			Listener.PeerDisconnectedEvent += PeerDisconnected;

			Simulate += UpdateInputs;
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
				Log.Common.LogDebug("Malformation error: " + e);

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

			TickStamped<DeltaInputSnapshotMessage> delta;
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

			husk.InputBuffer.Enqueue(delta);
		}

		private void UpdateInputs()
		{
			foreach (var husk in ConnectedHusks)
			{
				if (husk.InputBuffer.Count > 1)
				{
					TickStamped<InputSnapshotMessage> finalInput;
					do
					{
						var delta = husk.InputBuffer.Dequeue();
						var snapshot = _inputDiff.ConsumeDelta(delta.Content, husk.Input.Map(x => x.Content));
						var stamped = delta.WithContent(snapshot);

						husk.Input = Option.Some(stamped);
						finalInput = stamped;

						DeltaSnapshotProcessed?.Invoke(husk, delta.Stamp, delta.Content);
					} while (husk.InputBuffer.Count > 1);

					SnapshotUpdated?.Invoke(husk, finalInput.Stamp, finalInput.Content);
				}
				else if (husk.Input.MatchSome(out var input))
				{
					SnapshotUpdated?.Invoke(husk, input.Stamp, input.Content);
				}
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

				var stamped = new TickStamped<DeltaWorldSnapshotMessage>(Clock.Tick, delta);

				var writer = new BitPackWriter(data);
				_worldSerializer.Serialize(ref writer, stamped);

				writer.Dispose();
				husk.Peer.Send(data, DeliveryMethod.ReliableOrdered);

				husk.LastSent = Option.Some(snapshot.Copy());

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

			public readonly Queue<TickStamped<DeltaInputSnapshotMessage>> InputBuffer;

			public Option<TickStamped<InputSnapshotMessage>> Input;
			public Option<WorldSnapshotMessage> LastSent;

			public Husk(int id, NetPeer peer, bool isAdmin)
			{
				ID = id;
				Peer = peer;
				IsAdmin = isAdmin;

				InputBuffer = new Queue<TickStamped<DeltaInputSnapshotMessage>>();

				Input = Option.None<TickStamped<InputSnapshotMessage>>();
				LastSent = Option.None<WorldSnapshotMessage>();
			}
		}
    }
}
