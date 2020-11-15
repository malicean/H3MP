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

		private const string DISCONNECT_TEXT = "Leave Party";
		
		private readonly List<string> _wristMenuBaseButtons = new List<string>
		{
			"Button_1_OptionsPanel",
			"Button_16_SpectatorPanel",
			"Label_Template (2)",			// Clean Up header
			"Button_2_CleanupScene",		// Clean Empty Mags
			"Button_2_CleanupScene (1)",	// Clean All Mags
			"Button_2_CleanupScene (2)",	// Clean Guns & Melee
			"Button_3_ReloadScene",
			"Button_9_BackToMainMenu",
			"Button_10_QuitToDesktop"
		};

		private static GameObject H3MPOptionsPanel { get; set; }

		public static FVRWristMenu WristMenu;
		public static bool AskConfirmDisconnect { get; set; }

		public WristMenuButtons(ManualLogSource log)
		{
			_log = log;

			WristMenuButtons.AskConfirmDisconnect = false;
			
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			CreateWristMenuButtons();
		}

		private void CreateWristMenuButtons()
		{
			_log.LogDebug("Adding wristmenu buttons...");		

			// Get wristmenu and set to active or else Find won't work
			WristMenu = Resources.FindObjectsOfTypeAll<FVRWristMenu>().First();
			WristMenu.gameObject.SetActive(true);

			// Get the canvas and button sets
			var canvasTF = WristMenu.transform.Find("MenuGo/Canvas");
			var canvasRT = canvasTF.GetComponent<RectTransform>();
			var canvasBS = canvasTF.GetComponent<OptionsPanel_ButtonSet>();

			ref var images = ref canvasBS.ButtonImagesInSet;
			var originalButtonsLength = images.Length;

			// Create buttons, resize array & canvas, and fill in our new buttons to the array
			var buttons = CreateButtons(canvasTF).ToList();
			canvasRT.sizeDelta = new Vector2(canvasRT.sizeDelta.x, canvasRT.sizeDelta.y + (BUTTON_SIZE * buttons.Count));
			Array.Resize(ref images, originalButtonsLength + buttons.Count);
			for (var i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				WristMenu.Buttons.Add(button);

				var image = button.GetComponent<Image>();
				var pointable = button.GetComponent<FVRWristMenuPointableButton>();
				var index = originalButtonsLength + i;

				pointable.ButtonIndex = index;
				images[index] = image;
			}			
		}

		private IEnumerable<Button> CreateButtons(Transform canvas)
		{
			var source = canvas.Find("Button_3_ReloadScene").gameObject;
			Button CreateButton(string posName, string name, string text, Func<Button, UnityAction> callbackFactory)
			{
				// Move down source button
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * Vector3.down;

				// Create our button
				var dest = GameObject.Instantiate(source, posTransform.position, posTransform.rotation, posTransform.parent);
				dest.transform.position += BUTTON_PLACEMENT_DIFF * Vector3.up;
				dest.name = name;
				dest.GetComponentInChildren<Text>().text = text;

				// Destroy base button to remove persistent onClick listener & add our own
				var button = dest.GetComponent<Button>();
				Component.DestroyImmediate(button);			
				button = dest.AddComponent<Button>();

				button.onClick.AddListener(callbackFactory(button));

				// Shift all base buttons below down
				var btnIndex = _wristMenuBaseButtons.IndexOf(posName);
				for (var i = btnIndex + 1; i < _wristMenuBaseButtons.Count; i++)
				{
					MoveButton(_wristMenuBaseButtons[i]);
				}

				return button;
			}

			void MoveButton(string posName)
			{
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * Vector3.down;
			}
		
			// Add new buttons from top to bottom
			yield return CreateButton("Button_1_OptionsPanel", "H3MP_Panel", "Spawn H3MP Panel", SpawnPanel);
			yield return CreateButton("Button_3_ReloadScene", "H3MP_Privacy", PrivacyManager.PrivacyText, PrivacySelector);
			yield return CreateButton("Button_9_BackToMainMenu", "H3MP_Disconnect", DISCONNECT_TEXT, LeaveLobby);
		}

		// H3MP_Panel onClick
		private UnityAction SpawnPanel(Button button)
		{
			return () =>
			{
				WristMenu.Aud.PlayOneShot(WristMenu.AudClip_Engage, 1f);

				FVRViveHand hand = GM.CurrentPlayerBody.RightHand.transform.Find("PointingLaser").gameObject.activeInHierarchy
					? GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>()
					: GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>();

				hand.RetrieveObject(GetPanel());
				_log.LogDebug("Spawned panel");
			};
		}

		// H3MP_Privacy onClick
		private UnityAction PrivacySelector(Button button)
		{
			return () =>
			{
				WristMenu.Aud.PlayOneShot(WristMenu.AudClip_Engage, 1f);

				PrivacyManager.CyclePrivacy();

				var buttonText = button.GetComponentInChildren<Text>().text = PrivacyManager.PrivacyText;
				_log.LogDebug($"Changed party privacy to {PrivacyManager.PrivacyText}");
			};
		}

		// H3MP_Disconnect onClick
		private UnityAction LeaveLobby(Button button)
		{	
			return () =>
			{
				WristMenu.Aud.PlayOneShot(WristMenu.AudClip_Engage, 1f);
				if (!AskConfirmDisconnect)
				{
					ResetDisconnect(button);
					AskDisonnect_Confirm(button);
					return;
				}

				ResetDisconnect(button);
				for (var i = 0; i < GM.CurrentSceneSettings.QuitReceivers.Count; i++)
				{
					GM.CurrentSceneSettings.QuitReceivers[i].BroadcastMessage("QUIT", SendMessageOptions.DontRequireReceiver);
				}
				if (GM.LoadingCallback.IsCompleted)
				{
					SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name, false, 0.5f, 0f, 0f, 0f, 1f);
				}
				//TODO: make this leave session after merge with feature/netcode-refactor
				_log.LogDebug("Left party");
			};
		}

		public static void ResetDisconnect(Button button)
		{
			AskConfirmDisconnect = false;
			button.GetComponentInChildren<Text>().text = DISCONNECT_TEXT;
		}

		private void AskDisonnect_Confirm(Button button)
		{
			AskConfirmDisconnect = true;
			button.GetComponentInChildren<Text>().text = "Confirm ???";
		}		
		
		private FVRPhysicalObject GetPanel()
		{
			if (H3MPOptionsPanel != null)
			{
				return H3MPOptionsPanel.GetComponent<FVRPhysicalObject>();
			}
			else
			{
				// TODO: Probs move this to own class when we start to populate it with buttons
				// Clone from Spectator Panel
				GameObject currentH3MPPanel = GameObject.Instantiate(WristMenu.SpectatorPanelPrefab, Vector3.zero, Quaternion.identity);
				H3MPOptionsPanel = currentH3MPPanel;

				GameObject.DestroyImmediate(H3MPOptionsPanel.GetComponent<SpectatorPanel>());
				H3MPOptionsPanel.name = "H3MPPanel";

				// Delete all unneeded children
				var childs = H3MPOptionsPanel.transform.childCount;
				for (var i = childs - 1; i >= 0; i--)
				{
					switch (H3MPOptionsPanel.transform.GetChild(i).name)
					{
						case "Tablet":
						case "Canvas":
						case "Cube":
						case "_pose":
							break;

						default:
							GameObject.DestroyImmediate(H3MPOptionsPanel.transform.GetChild(i).gameObject);
							break;
					}
				}

				// Get canvas & delete all unneeded children
				var canvas = H3MPOptionsPanel.transform.Find("Canvas");
				childs = canvas.transform.childCount;
				for (var i = childs - 1; i >= 0; i--)
				{
					switch (canvas.transform.GetChild(i).name)
					{
						case "PanelLabel":
						case "Backing (2)":
							break;

						default:
							GameObject.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
							break;
					}
				}

				var background = canvas.transform.Find("Backing (2)");
				var backgroundRT = background.GetComponent<RectTransform>();
				backgroundRT.anchoredPosition = new Vector2(0, 5);
				backgroundRT.localScale = new Vector3(1, 1, 1);
				backgroundRT.sizeDelta = new Vector2(550, 350);

				var panelLabel = canvas.transform.Find("PanelLabel");
				var headerText = panelLabel.GetComponent<Text>();
				headerText.text = "H3MP OPTIONS PANEL";
				headerText.fontSize = 19;

				// Temp warning so people don't expect a functional panel
				var warning = GameObject.Instantiate(panelLabel.gameObject, background);
				warning.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);

				var warningText = warning.GetComponent<Text>();
				warningText.text = "This panel is a WIP - options to be added later";
				warningText.fontSize = 40;

				return H3MPOptionsPanel.GetComponent<FVRPhysicalObject>();
			}
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
