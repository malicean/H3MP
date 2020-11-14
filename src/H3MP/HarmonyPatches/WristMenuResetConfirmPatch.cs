using FistVR;
using H3MP.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(FVRWristMenu), "ResetConfirm")]
	internal class WristMenuResetConfirmPatch
	{
		private static void Postfix()
		{
			var log = HarmonyState.Log;

			// Get the wristmenu & canvas
			var wristMenu = WristMenuButtons._wristMenu;
			var canvasTF = wristMenu.transform.Find("MenuGo/Canvas");

			// Get H3MP Disconnect button
			var button = canvasTF.Find("H3MP_Disconnect").gameObject.GetComponent<Button>();

			// Reset button states
			WristMenuButtons.ResetDisconnect(button);
		}
	}
}
