using System;
using System.Net;
using BepInEx.Logging;
using DiscordRPC;
using H3MP.Messages;
using H3MP.Networking;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP
{
    internal delegate void OnH3ClientDisconnect(DisconnectInfo info);

    public class H3Client : Client<H3Client>
    {
        private readonly ManualLogSource _log;
        private readonly DiscordRpcClient _discord;
        private readonly OnH3ClientDisconnect _onDisconnected;

        private ServerTime _time;

        public double Time => _time?.Now ?? 0;

        internal H3Client(ManualLogSource log, DiscordRpcClient discord, PeerMessageList<H3Client> messages, Version version, IPEndPoint endpoint, ConnectionRequestMessage request, OnH3ClientDisconnect onDisconnected) 
            : base(log, messages, new Events(), version, endpoint, x => x.Put(request))
        {
            _log = log;
            _discord = discord;
            _onDisconnected = onDisconnected;
        }

        public override void Dispose()
        {
            base.Dispose();

            _discord.UpdateParty(new DiscordRPC.Party());
            _discord.UpdateSecrets(new DiscordRPC.Secrets());
        }

        internal static void OnServerPong(H3Client self, Peer peer, PongMessage message)
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
            SteamVR_LoadLevel.Begin(message.Name);
        }

        internal static void OnServerPartyInit(H3Client self, Peer peer, PartyInitMessage message)
        {
            self._discord.UpdateParty(new Party
            {
                ID = message.ID.ToString(),
                Size = message.Size,
                Max = message.Max
            });

            self._discord.UpdateSecrets(new Secrets
            {
                JoinSecret = message.Secret.ToString()
            });
        }

        internal static void OnServerPartyChange(H3Client self, Peer peer, PartyChangeMessage message)
        {
            self._discord.UpdatePartySize(message.Size);
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