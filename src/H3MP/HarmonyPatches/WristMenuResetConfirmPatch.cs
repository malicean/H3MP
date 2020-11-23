using FistVR;
using HarmonyLib;
using UnityEngine.UI;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(FVRWristMenu), "ResetConfirm")]
	internal class WristMenuResetConfirmPatch
	{
		private static void Postfix()
		{
			// Get the wristmenu & canvas
			var wristMenuButtons = HarmonyState.WristMenuButtons;
			var wristMenu = wristMenuButtons.WristMenu;
			var canvasTF = wristMenu.transform.Find("MenuGo/Canvas");

			// Get H3MP buttons that need text to reset when wristmenu deactivates
			var button = canvasTF.Find("H3MP_Disconnect").gameObject.GetComponent<Button>();

			// Reset button states
			wristMenuButtons.ResetDisconnect(button.GetComponentInChildren<Text>());
		}
	}
}
