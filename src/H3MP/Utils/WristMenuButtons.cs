using BepInEx.Logging;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

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
			// Get wristmenu and set to active or else Find won't work
			FVRWristMenu wristMenu = Resources.FindObjectsOfTypeAll<FVRWristMenu>().First();
			wristMenu.gameObject.SetActive(true);

			// Get the canvas and sets
			Transform tf = wristMenu.transform.Find("MenuGo/Canvas");
			OptionsPanel_ButtonSet buttonSet = tf.GetComponent<OptionsPanel_ButtonSet>();
			RectTransform rt = tf.GetComponent<RectTransform>();
			tf.GetComponent<RectTransform>().sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + BUTTON_SIZE);

			// Clone get necessary base buttons
			GameObject reloadScene = tf.transform.Find("Button_3_ReloadScene").gameObject;
			GameObject quitToDesktop = tf.transform.Find("Button_10_QuitToDesktop").gameObject;

			// Create our button
			// TODO: create our own button instead?
			GameObject disconnect = GameObject.Instantiate(reloadScene, reloadScene.transform.parent);
			disconnect.transform.SetParent(tf, false);
			disconnect.transform.position = new Vector2(0, quitToDesktop.transform.position.y);

			// Move base quit button down
			quitToDesktop.transform.position = new Vector2(0, quitToDesktop.transform.position.y - BUTTON_PLACEMENT_DIF);

			// Add the actual button component to ours
			Button disconnectBtn = disconnect.GetComponent<Button>();
			disconnectBtn.onClick.AddListener(LeaveLobby);

			// Add our button to the list & set the index
			wristMenu.Buttons.Add(disconnectBtn);

			// Name our button & set text
			var t = wristMenu.Buttons.Count;
			disconnect.name = $"Button_{t}_Disconnect";
			var btnText = disconnect.GetComponentInChildren<Text>();
			btnText.text = "Leave Lobby Session";

			// Fix up the button lists
			// TODO: make this not hardcoded trash
			FVRWristMenuPointableButton wristMenuPointableButton = disconnect.GetComponent<FVRWristMenuPointableButton>();
			wristMenuPointableButton.ButtonIndex = buttonSet.ButtonImagesInSet.Length;
			Array.Resize(ref buttonSet.ButtonImagesInSet, buttonSet.ButtonImagesInSet.Length + 1);
			buttonSet.ButtonImagesInSet[17] = disconnect.GetComponent<Image>();
		}

		private void LeaveLobby()
		{
			//TODO: make this actually leave after merge with netcode refactor branch
			//TODO: make this take two clicks like reloadscene/quit buttons
			_log.LogDebug("This should leave lobby");
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
