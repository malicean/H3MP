using System;
using System.Threading;

using BepInEx;
using H3MP.Common;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.SceneManagement;

namespace H3MP.Client
{
	[BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		// usually i hate these but i dont plan on supporting more than 1 client per game.
		public static Plugin Singleton { get; private set; }

		public LnlClient Client { get; private set; }

		private void Awake()
		{
			Client = new LnlClient();

			// put everything before this; this should only be set when the object is initialized
			Singleton = this;
			new NetDataWriter().Put<>
		}

		public static void Main()
		{
			EventBasedNetListener listener = new EventBasedNetListener();
			NetManager client = new NetManager(listener);
			client.Start();

			Console.Write("key: ");
			var key = Console.ReadLine();
			client.Connect("localhost", 9099, key);

			var quit = false;
			listener.NetworkReceiveEvent += (peer, reader, method) =>
			{
				Console.WriteLine($"data received: {reader.GetString(100)}");
				reader.Recycle();
			};

			while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Q)
			{
				client.PollEvents();
				Thread.Sleep(10);
			}

			client.Stop();
		}
	}
}
