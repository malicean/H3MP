using BepInEx.Logging;
using FistVR;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace H3MP.Utils
{
	public class WristMenuButtons
	{
		private readonly ManualLogSource _log;

		private const float BUTTON_SIZE = 31f;
		private const float BUTTON_PLACEMENT_DIF = 0.01565f;

		public WristMenuButtons(ManualLogSource log)
		{
			_log = log;

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{			
			CreateWristMenuButtons();
		}

		private void CreateWristMenuButtons()
		{
			_log.LogDebug("Adding disconnect button...");

			// Get wristmenu and set to active or else Find won't work
			var wristMenu = Resources.FindObjectsOfTypeAll<FVRWristMenu>().First();
			wristMenu.gameObject.SetActive(true);

			// Get the canvas and sets
			var canvasTF = wristMenu.transform.Find("MenuGo/Canvas");
			var canvasRT = canvasTF.GetComponent<RectTransform>();
			canvasRT.sizeDelta = new Vector2(canvasRT.sizeDelta.x, canvasRT.sizeDelta.y + BUTTON_SIZE);

			// Get necessary base buttons
			var reloadScene = canvasTF.Find("Button_3_ReloadScene").gameObject;
			var quitToDesktop = canvasTF.Find("Button_10_QuitToDesktop").gameObject;

			// Create our button
			// TODO: create button from scratch instead?
			var disconnect = GameObject.Instantiate(reloadScene, reloadScene.transform.parent);
			disconnect.transform.position = new Vector2(0, quitToDesktop.transform.position.y);
			disconnect.name = "H3MP_Disconnect";
			disconnect.GetComponentInChildren<Text>().text = "Leave Lobby Session";

			// Add the actual button component to ours
			var disconnectBtn = disconnect.GetComponent<Button>();
			disconnectBtn.onClick.AddListener(LeaveLobby);

			// Move base quit button down
			quitToDesktop.transform.position = new Vector2(0, quitToDesktop.transform.position.y - BUTTON_PLACEMENT_DIF);

			// Add our button to the list & set the index
			wristMenu.Buttons.Add(disconnectBtn);

			// Fix up the button lists
			// TODO: this still looks like trash?
			var buttonSet = canvasTF.GetComponent<OptionsPanel_ButtonSet>();
			var wristMenuPointableButton = disconnect.GetComponent<FVRWristMenuPointableButton>();
			wristMenuPointableButton.ButtonIndex = buttonSet.ButtonImagesInSet.Length;
			Array.Resize(ref buttonSet.ButtonImagesInSet, buttonSet.ButtonImagesInSet.Length + 1);
			buttonSet.ButtonImagesInSet[wristMenuPointableButton.ButtonIndex] = disconnect.GetComponent<Image>();
		}

		private void LeaveLobby()
		{
			//TODO: make this actually leave session after merge with feature/netcode-refactor
			//TODO: maybe make this take two clicks like reloadscene/quit buttons?
			_log.LogDebug("Left H3MP session");
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
