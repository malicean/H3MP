using FistVR;
using H3MP.Peers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(FVRPlayerBody), nameof(FVRPlayerBody.SetOutfit))]
	internal class SetLinkAccessoryPatch
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(gs[UnityEngine.Random.Range(0, gs.Count)
			// --> 
			// --> dup
			// --> call proxy
			//].GetGameObject(), l.position, l.rotation);

			var loadOutfit = typeof(List<SosigOutfitConfig>).GetMethod("get_Item", new[] { typeof(int) });
			var proxy = typeof(HarmonyState).GetMethod(nameof(HarmonyState.InvokeOnSpectatorOutfitRandomized));

			foreach (var il in instructions)
			{
				yield return il;
				
				if (il.opcode == OpCodes.Callvirt && il.operand == loadOutfit)
				{
					HarmonyState.Log.LogDebug("Successfully found spectator outfit randomization");

					yield return new CodeInstruction(OpCodes.Dup); 			// +1
					yield return new CodeInstruction(OpCodes.Call, proxy);	//  0
				}
			}
		}
	}
}
