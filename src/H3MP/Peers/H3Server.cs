using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using BepInEx.Logging;
using Discord;
using H3MP.HarmonyPatches;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	public class H3Server : Server<H3Server>
	{
		private readonly ManualLogSource _log;
		private readonly HostConfig _config;
		private readonly Dictionary<Peer, byte> _peerIDs;
		private readonly Dictionary<byte, Husk> _husks;

		private Key32 PartyID { get; }

		public JoinSecret Secret { get; }

		internal H3Server(ManualLogSource log, RandomNumberGenerator rng, PeerMessageList<H3Server> messages, byte channelsCount, Version version, HostConfig config, IPEndPoint publicEndPoint) 
			: base(log, messages, channelsCount, new Events(messages.Definitions[typeof(Timestamped<PingMessage>)]), version, config.Binding.IPv4.Value, config.Binding.IPv6.Value, config.Binding.Port.Value)
		{
			_log = log;
			_config = config;
			_peerIDs = new Dictionary<Peer, byte>();
			_husks = new Dictionary<byte, Husk>();

			PartyID = Key32.FromRandom(rng);
			Secret = new JoinSecret(version, publicEndPoint, Key32.FromRandom(rng));
		}

		public override void Update()
		{
			base.Update();

			var deltas = new Dictionary<byte, Timestamped<PlayerTransformsMessage>>();
			foreach (KeyValuePair<byte, Husk> husk in _husks)
			{
				var delta = husk.Value.Delta;
				if (delta.HasValue)
				{
					deltas.Add(husk.Key, delta.Value);
				}
			}

			Broadcast(new PlayerMovesMessage(deltas));
		}

		internal static void OnClientPing(H3Server self, Peer peer, PingMessage message)
		{
			peer.Send(Timestamped<PingMessage>.Now(message));
		}

		internal static void OnPlayerMove(H3Server self, Peer peer, Timestamped<PlayerTransformsMessage> message)
		{
			self._husks[self._peerIDs[peer]].Latest = message;
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

				if (message.Key != server.Secret.Key)
				{
					server._log.LogWarning($"Join request {request.RemoteEndPoint} had an incorrect key.");

					rejectionContent.Put(JoinError.MismatchedKey);
					request.Reject(rejectionContent);
					return;
				}

				var peer = request.Accept();

				using (WriterPool.Instance.Borrow(out var writer))
				{
					_pong.Send(peer, writer, Timestamped<PingMessage>.Now(new PingMessage(message.ClientTime)));
				}
			}

			public void OnClientConnected(H3Server server, Peer peer)
			{
				var count = server.ClientsCount;
				var size = new PartySize
				{
					CurrentSize = (byte) count,
					MaxSize = (byte) server._config.PlayerLimit.Value
				};

				peer.Send(new PartyInitMessage(server.PartyID, size, server.Secret));
				peer.Send(new LevelChangeMessage(LoadLevelPatch.CurrentName));

				// Upsize party
				server.BroadcastExcept(peer, new PartyChangeMessage(count));

				byte id;
				for (id = 0; id < byte.MaxValue; ++id)
				{
					if (!server._husks.ContainsKey(id))
					{
						break;
					}
				}

				// Initialize existing puppets on just-joined client
				foreach (KeyValuePair<byte, Husk> husk in server._husks) 
				{
					peer.Send(new PlayerJoinMessage(husk.Key, husk.Value.Latest));
				}

				server._peerIDs.Add(peer, id);
				server._husks.Add(id, new Husk());

				// Initialize just-joined puppet on other clients
				server.BroadcastExcept(peer, new PlayerJoinMessage(id, Timestamped<PlayerTransformsMessage>.Now(default)));
			}

			public void OnClientDisconnected(H3Server server, Peer peer, DisconnectInfo info)
			{
				// Downsize party
				server.Broadcast(new PartyChangeMessage(server.ClientsCount));

				var id = server._peerIDs[peer];
				server._peerIDs.Remove(peer);
				server._husks.Remove(id);

				server.Broadcast(new PlayerLeaveMessage(id));
			}
		}
	}
}