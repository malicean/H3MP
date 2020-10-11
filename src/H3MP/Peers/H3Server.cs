using System;
using System.Net;
using System.Security.Cryptography;
using BepInEx.Logging;
using Discord;
using H3MP.HarmonyPatches;
using H3MP.Messages;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP
{
    public class H3Server : Server<H3Server>
    {
        private readonly ManualLogSource _log;
        private readonly HostConfig _config;

        private Key32 PartyID { get; }
        public JoinSecret Secret { get; }

        internal H3Server(ManualLogSource log, RandomNumberGenerator rng, PeerMessageList<H3Server> messages, byte channelsCount, Version version, HostConfig config, IPEndPoint publicEndPoint) 
            : base(log, messages, channelsCount, new Events(messages.Definitions[typeof(PongMessage)]), version, config.Binding.IPv4.Value, config.Binding.IPv6.Value, config.Binding.Port.Value)
        {
            _log = log;
            _config = config;

            PartyID = Key32.FromRandom(rng);
            Secret = new JoinSecret(version, publicEndPoint, Key32.FromRandom(rng));
        }

        internal static void OnClientPing(H3Server self, Peer peer, PingMessage message)
        {
            peer.Send(new PongMessage(message.ClientTime));
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
                    _pong.Send(peer, writer, new PongMessage(message.ClientTime));
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

                // upsize party
                server.BroadcastExcept(peer, new PartyChangeMessage(count));
            }

            public void OnClientDisconnected(H3Server server, Peer peer, DisconnectInfo info)
            {
                // downsize party
                server.Broadcast(new PartyChangeMessage(server.ClientsCount));
            }
        }
    }
}