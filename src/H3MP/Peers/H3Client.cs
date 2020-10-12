using System;
using System.Collections.Generic;
using System.Net;
using BepInEx.Logging;
using Discord;
using H3MP.Messages;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib;

namespace H3MP
{
    internal delegate void OnH3ClientDisconnect(DisconnectInfo info);

    public class H3Client : Client<H3Client>
    {
		// All the scenes with rich presence assets and their names.
		private static readonly Dictionary<string, string> _sceneNames = new Dictionary<string, string>
		{
			["MainMenu3"] = "Main Menu",
			["ArizonaTargets"] = "Arizona Range - Day",
			["ArizonaTargets_Night"] = "Arizona Range - Night",
			["BreachAndClear_TestScene1"] = "Breaching Proto",
			["Cappocolosseum"] = "Cappocolosseum",
			["GrenadeSkeeball"] = "Boomskee",
			["HickokRange"] = "Friendly 45",
			["IndoorRange"] = "Indoor Range",
			["MeatGrinder"] = "Meat Grinder",
			["MeatGrinder_StartingScene"] = "Starting Meat Grinder",
			["MF2_MainScene"] = "Meat Fortress 2",
			["ObstacleCourseScene1"] = "The Gunnasium",
			["ObstacleCourseScene2"] = "Mini-Arena",
			//["OmnisequencerTesting3"] = "" is this an accessible scene?
			["ProvingGround"] = "Proving Grounds",
			["SniperRange"] = "Sniper Range",
			["ReturnOfTheRotwieners"] = "Rise of the Rotwieners",
			["RotWienersStagingScene"] = "Starting Rise of the Rotwieners",
			["SamplerPlatter"] = "Sampler Platter",
			["TakeAndHold_1"] = "Take & Hold Containment",
			["TakeAndHold_Lobby_2"] = "Take & Hold Lobby",
			["TakeAndHoldClassic"] = "Take & Hold",
			["Testing3_LaserSword"] = "Arcade Proto",
			["TileSetTest1"] = "Arena Proto",
			["WarehouseRange_Rebuilt"] = "Warehouse Range",
			["Wurstwurld1"] = "Wurstwurld",
			["Xmas"] = "Meatmas Snowglobe"
        };

        private readonly ManualLogSource _log;
        private readonly StatefulActivity _discord;
        private readonly bool _isHost;

        private readonly OnH3ClientDisconnect _onDisconnected;

        private readonly LoopTimer _timer;

        private ServerTime _time;

        public double Time => _time?.Now ?? 0;

        internal H3Client(ManualLogSource log, StatefulActivity discord, PeerMessageList<H3Client> messages, byte channelsCount, Version version, bool isHost, IPEndPoint endpoint, ConnectionRequestMessage request, OnH3ClientDisconnect onDisconnected) 
            : base(log, messages, channelsCount, new Events(), version, endpoint, x => x.Put(request))
        {
            _log = log;
            _discord = discord;
            _isHost = isHost;

            _onDisconnected = onDisconnected;

            _timer = new LoopTimer(2);
        }

        public override void Update()
        {
            base.Update();

            if (_timer.TryReset())
            {
                Server.Send(PingMessage.Now);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _discord.Update(x => 
            {
                x.Party = default;
                x.Secrets = default;

                return x;
            });
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
            var levelName = message.Name;

            {
                string asset;
                if (_sceneNames.TryGetValue(levelName, out var tooltip))
                {
                    asset = levelName.ToLower();
                }
                else
                {
                    asset = "unknown";
                    tooltip = levelName;
                }

                self._discord.Update(x =>
                {
                    x.Assets = new ActivityAssets
                    {
                        LargeImage = "scene_" + asset,
                        LargeText = tooltip
                    };
                    x.Timestamps = new Discord.ActivityTimestamps
                    {
                        Start = DateTime.UtcNow.ToUnixTimestamp()
                    };

                    return x;
                });
            }

            if (self._isHost)
            {
                return;
            }

            SteamVR_LoadLevel.Begin(levelName);
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
