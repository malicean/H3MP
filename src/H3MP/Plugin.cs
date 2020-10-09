using BepInEx;
using DiscordRPC;
using H3MP.Utils;
using System.Security.Cryptography;
using System;

namespace H3MP
{
	[BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		// Unity moment
		public static Plugin Instance { get; private set; }

		private const string DISCORD_APP_ID = "762557783768956929"; // 3rd party rpc application
		private const string STEAM_APP_ID = "450540"; // h3vr

		private readonly Version _version;
		private readonly DiscordRpcClient _discord;
		private readonly RandomNumberGenerator _rng;

		public double Time => _client is null ? LocalTime.Now : _client.Time;

		public Plugin()
		{
			_version = Info.Metadata.Version;

			Logger.LogDebug("Initializing utilities...");
			_discord = new DiscordRpcClient(DISCORD_APP_ID, autoEvents: false);
			_rng = RandomNumberGenerator.Create();

			_discord.OnError += (sender, args) => Logger.LogError($"Discord RPC code {args.Code}: {args.Message}");
			_discord.OnConnectionEstablished += (sender, args) => Logger.LogInfo("Connected to Discord RPC.");
			_discord.OnConnectionFailed += (sender, args) => Logger.LogError("Failed to connect to Discord RPC. Ensure the Discord client is running.");
			_discord.OnReady += (sender, args) => Logger.LogDebug($"Ready to use Discord RPC as {args.User}.");
			_discord.OnClose += (sender, args) => Logger.LogError($"Connection to Discord RPC closed with code {args.Code}: {args.Reason}");
			_discord.OnJoin += (sender, args) =>
			{
				// TODO: add sanitization to avoid an exception

				var connectionArgs = args.Secret.Split(':');
				Connect(connectionArgs[0], ushort.Parse(connectionArgs[1]), ConnectionKey.FromString(connectionArgs[2]));
			};
		}

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			Logger.LogDebug("Starting Discord RPC...");
			_discord.Initialize();
			_discord.RegisterUriScheme(STEAM_APP_ID);
		}

		private void Update()
		{
			_discord.Invoke();

			_client?.Update();
			_server?.Update();
		}

		private void OnDestroy()
		{
			_discord.Dispose();
		}
	}
}
