using BepInEx.Logging;
using Discord;
using H3MP.Configs;
using H3MP.Extensions;
using H3MP.HarmonyPatches;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Networking;
using H3MP.Networking.Extensions;
using H3MP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	public class H3Server : Server<H3Server>
	{
		private readonly ManualLogSource _log;
		private readonly HostConfig _config;
		private readonly Key32 _partyID;

		private readonly LoopTimer _tickTimer;
		private readonly Dictionary<Peer, byte> _peerIDs;
		private readonly Dictionary<byte, Husk> _husks;

		private int _selfID;

		public JoinSecret Secret { get; }

		public Key32 HostKey { get; }

		internal H3Server(ManualLogSource log, HostConfig config, RandomNumberGenerator rng, PeerMessageList<H3Server> messages, byte channelsCount, Version version, double tickDeltaTime, IPEndPoint publicEndPoint)
			: base(log, messages, channelsCount, new Events(messages.Definitions[typeof(Timestamped<PingMessage>)]), version, config.Binding.IPv4.Value, config.Binding.IPv6.Value, config.Binding.Port.Value)
		{
			_log = log;
			_config = config;
			_partyID = Key32.FromRandom(rng);

			_tickTimer = new LoopTimer(tickDeltaTime);
			_peerIDs = new Dictionary<Peer, byte>();
			_husks = new Dictionary<byte, Husk>();

			_selfID = -1;

			Secret = new JoinSecret(version, publicEndPoint, Key32.FromRandom(rng), tickDeltaTime);
			HostKey = Key32.FromRandom(rng);
		}

		public override void Update()
		{
			if (!_tickTimer.TryCycle())
			{
				return;
			}

			base.Update();

			var deltas = new Dictionary<byte, Timestamped<BodyMessage>>();
			foreach (KeyValuePair<byte, Husk> husk in _husks)
			{
				var delta = husk.Value.Delta;
				if (delta.HasValue)
				{
					deltas.Add(husk.Key, delta.Value);
				}
			}

			if (deltas.Count > 0)
			{
				foreach (KeyValuePair<Peer, byte> peerID in _peerIDs)
				{
					var peer = peerID.Key;
					var id = peerID.Value;

					Timestamped<BodyMessage>? popped;
					if (deltas.TryGetValue(id, out var delta))
					{
						popped = delta;
						deltas.Remove(id);
					}
					else
					{
						popped = null;
					}

					peer.Send(new PlayerMovesMessage(deltas));

					if (popped.HasValue)
					{
						deltas.Add(id, popped.Value);
					}
				}
			}
		}

		private Husk this[Peer peer] => _husks[_peerIDs[peer]];

		internal static void OnClientPing(H3Server self, Peer peer, PingMessage message)
		{
			peer.Send(Timestamped<PingMessage>.Now(message));
		}

		internal static void OnPlayerMoveDelta(H3Server self, Peer peer, Timestamped<BodyMessage> message)
		{
			self[peer].LastDelta = message;
		}

		internal static void OnLevelChange(H3Server self, Peer peer, LevelChangeMessage message)
		{
			var husk = self[peer];
			if (!husk.IsSelf) // self always has permissions
			{
				var config = self._config.Permissions;

				// scene reload
				if (HarmonyState.CurrentLevel == message.Name)
				{
					if (!config.SceneReloading.Value)
					{
						return;
					}
				}
				else // scene change
				{
					if (!config.SceneChanging.Value)
					{
						return;
					}
				}
			}

			self.Broadcast(message);
		}

		private class Events : IServerEvents<H3Server>
		{
			private readonly MessageDefinition _pong;

			public Events(MessageDefinition pong)
			{
				_pong = pong;
			}

			public void OnConnectionRequest(H3Server server, ConnectionRequest request, NetDataWriter rejectionContent)
			{
				var currentClients = server.ClientsCount;
				var maxClients = server._config.PlayerLimit.Value;
				if (currentClients >= maxClients)
				{
					server._log.LogWarning($"Rejecting join request from {request.RemoteEndPoint} because of full party ({currentClients} / {maxClients}).");

					rejectionContent.Put(JoinError.Full);
					request.Reject(rejectionContent);
					return;
				}

				var reader = request.Data;

				if (!reader.TryGet<ConnectionRequestMessage>(out var message))
				{
					server._log.LogWarning($"Join request from {request.RemoteEndPoint} had a malformed request.");

					rejectionContent.Put(JoinError.MalformedMessage);
					request.Reject(rejectionContent);
					return;
				}

				if (message.AccessKey != server.Secret.Key)
				{
					server._log.LogWarning($"Join request {request.RemoteEndPoint} had an incorrect key.");

					rejectionContent.Put(JoinError.MismatchedKey);
					request.Reject(rejectionContent);
					return;
				}

				var peer = request.Accept();
				if (message.HostKey == server.HostKey)
				{
					server._selfID = peer.Id;
				}

				using (WriterPool.Instance.Borrow(out var writer))
				{
					_pong.Send(peer, writer, Timestamped<PingMessage>.Now(new PingMessage(message.ClientTime)));
				}
			}

			public void OnClientConnected(H3Server server, Peer peer)
			{
				byte count = (byte) server._husks.Count;
				byte max = server._config.PlayerLimit.Value;
				var level = new LevelChangeMessage(HarmonyState.CurrentLevel);
				var players = new PlayerJoinMessage[count];
				{
					var i = 0;
					foreach (var husk in server._husks)
					{
						players[i++] = new PlayerJoinMessage(husk.Key, husk.Value.LastDelta);
					}
				}

				peer.Send(new InitMessage(server._partyID, server.Secret, max, level, players));

				// Find first available player ID.
				byte id;
				for (id = 0; id < byte.MaxValue; ++id)
				{
					if (!server._husks.ContainsKey(id))
					{
						break;
					}
				}

				var peerHusk = new Husk(server._selfID == peer.ID);
				server._peerIDs.Add(peer, id);
				server._husks.Add(id, peerHusk);

				// Initialize just-joined puppet on other clients
				server.BroadcastExcept(peer, new PlayerJoinMessage(id, Timestamped<BodyMessage>.Now(default)));
			}

			public void OnClientDisconnected(H3Server server, Peer peer, DisconnectInfo info)
			{
				var id = server._peerIDs[peer];
				server._peerIDs.Remove(peer);
				server._husks.Remove(id);

				server.Broadcast(new PlayerLeaveMessage(id));
			}
		}
	}
}
