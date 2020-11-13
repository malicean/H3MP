using BepInEx.Logging;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace H3MP.Utils
{
	public class WristMenuButtons
	{
		private readonly ManualLogSource _log;

		private const float BUTTON_SIZE = 31f;
		private const float BUTTON_PLACEMENT_DIFF = 0.01565f;

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
			canvasRT.sizeDelta = new Vector2(canvasRT.sizeDelta.x, canvasRT.sizeDelta.y + (BUTTON_SIZE*2));
			var canvasBS = canvasTF.GetComponent<OptionsPanel_ButtonSet>();
			var originalButtonsLength = canvasBS.ButtonImagesInSet.Length;

			// Create butons, resize array, and fill in our new buttons to the array
			var buttons = CreateButtons(canvasTF).ToList();
			Array.Resize(ref canvasBS.ButtonImagesInSet, originalButtonsLength + buttons.Count);
			for (var i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				wristMenu.Buttons.Add(button);
				
				var image = button.GetComponent<Image>();
				var pointable = button.GetComponent<FVRWristMenuPointableButton>();
				var index = originalButtonsLength+i;
				
				pointable.ButtonIndex = index;
				canvasBS.ButtonImagesInSet[index] = image;
			}
		}

		private IEnumerable<Button> CreateButtons(Transform canvas)
		{
			var source = canvas.Find("Button_3_ReloadScene").gameObject;
			Button CreateButton(string posName, string name, string text, bool delete, UnityAction callback)
			{
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.down;

				var dest = GameObject.Instantiate(source, posTransform.position, posTransform.rotation, posTransform.parent);
				dest.transform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.up;
				dest.name = name;
				dest.GetComponentInChildren<Text>().text = text;

				var button = dest.GetComponent<Button>();

				if (delete) {
					Component.DestroyImmediate(button);
					button = dest.AddComponent<Button>();
				}

				button.onClick.AddListener(callback);

				return button;
			}

			void MoveButton(string posName)
			{
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.down;
			}

			yield return CreateButton("Button_3_ReloadScene", "H3MP_Privacy", "Open Party", true, PrivacySelector);
			yield return CreateButton("Button_9_BackToMainMenu", "H3MP_Disconnect", "Leave Party", false, LeaveLobby);
			MoveButton("Button_10_QuitToDesktop");
		}

		private void LeaveLobby()
		{
			//TODO: make this actually leave session after merge with feature/netcode-refactor
			//TODO: maybe make this take two clicks like reloadscene/quit buttons?
			_log.LogDebug("Left H3MP party");
		}

		private void PrivacySelector()
		{
			//TODO: maybe make this cycle lobby privacy & set button text to reflect that
			_log.LogDebug("Changed party privacy");
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
