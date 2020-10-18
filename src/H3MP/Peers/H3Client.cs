using System;
using System.Collections.Generic;
using System.Net;
using BepInEx.Logging;
using Discord;
using FistVR;
using H3MP.HarmonyPatches;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	internal delegate void OnH3ClientDisconnect(DisconnectInfo info);

	public class H3Client : Client<H3Client>, IRenderUpdatable
	{
		private readonly ManualLogSource _log;
		private readonly StatefulActivity _discord;

		private readonly OnH3ClientDisconnect _onDisconnected;

		private readonly LoopTimer _timer;
		private readonly Dictionary<byte, Puppet> _players;

		private ServerTime _time;

		public double Time => _time?.Now ?? 0;

		internal H3Client(ManualLogSource log, StatefulActivity discord, PeerMessageList<H3Client> messages, byte channelsCount, Version version, IPEndPoint endpoint, ConnectionRequestMessage request, OnH3ClientDisconnect onDisconnected) 
			: base(log, messages, channelsCount, new Events(), version, endpoint, x => x.Put(request))
		{
			_log = log;
			_discord = discord;

			_onDisconnected = onDisconnected;

			_timer = new LoopTimer(2);
			_players = new Dictionary<byte, Puppet>();
		}

		public override void Update()
		{
			base.Update();

			if (_timer.TryReset())
			{
				Server.Send(PingMessage.Now);
			}

			if (!(_time is null))
			{
				var player = GM.CurrentPlayerBody;
				var transforms = new PlayerTransformsMessage(player.Head, player.LeftHand, player.RightHand);
				var timestamped = new Timestamped<PlayerTransformsMessage>(_time.Now, transforms);

				Server.Send(timestamped);
			}
		}

		public void RenderUpdate()
		{
			foreach (var player in _players.Values)
			{
				player.RenderUpdate();
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var player in _players.Values)
			{
				player.Dispose();
			}

			_discord.Update(x => 
			{
				x.Party = default;
				x.Secrets = default;

				return x;
			});
		}

		internal static void OnServerPong(H3Client self, Peer peer, Timestamped<PingMessage> message)
		{
			if (self._time is null)
			{
				self._time = new ServerTime(self._log, peer, message);
			}
			else 
			{
				self._time.ProcessPong(message);
			}
		}

		internal static void OnLevelChange(H3Client self, Peer peer, LevelChangeMessage message)
		{
			HarmonyState.LockLoadLevel = false;
			try
			{
				SteamVR_LoadLevel.Begin(message.Name);
			}
			finally
			{
				HarmonyState.LockLoadLevel = true;
			}
		}

		internal static void OnServerInit(H3Client self, Peer peer, PartyInitMessage message)
		{
			self._log.LogDebug("Initializing Discord party...");
			self._discord.Update(x =>
			{
				x.State = "In party";
				x.Party = new Discord.ActivityParty
				{
					Id = message.ID.ToString(),
					Size = message.Size
				};
				x.Secrets.Join = message.Secret.ToString();

				return x;
			});
		}

		internal static void OnServerPartyChange(H3Client self, Peer peer, PartyChangeMessage message)
		{
			self._discord.Update(x =>
			{
				x.Party.Size.CurrentSize = message.CurrentSize;

				return x;
			});
		}

		internal static void OnPlayerJoin(H3Client self, Peer peer, PlayerJoinMessage message)
		{
			var puppet = new Puppet(() => self._time);
			puppet.ProcessTransforms(message.Transforms);

			self._players.Add(message.ID, puppet);
		}

		internal static void OnPlayerLeave(H3Client self, Peer peer, PlayerLeaveMessage message)
		{
			var id = message.ID;
			if (!self._players.TryGetValue(id, out var player))
			{
				return;
			}

			player.Dispose();
			self._players.Remove(id);
		}

		internal static void OnPlayersMove(H3Client self, Peer peer, PlayerMovesMessage message)
		{
			foreach (KeyValuePair<byte, Timestamped<PlayerTransformsMessage>> delta in message.Players)
			{
				// In case the player hasn't spawned yet.
				if (self._players.TryGetValue(delta.Key, out var puppet))
				{
					puppet.ProcessTransforms(delta.Value);
				}
			}
		}

		internal class Events : IClientEvents<H3Client>
		{
			public void OnConnected(H3Client client)
			{
			}

			public void OnDisconnected(H3Client client, DisconnectInfo info)
			{
				client._onDisconnected(info);
			}
		}
	}
}
