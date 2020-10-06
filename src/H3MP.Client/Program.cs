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

		private void Awake()
		{
			// put everything before this; this should only be set when the object is initialized
			Singleton = this;
		}
	}
}
