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
		
		private static readonly List<string> _baseButtons = new List<string>
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

		private static readonly HashSet<string> _rootKeep = new HashSet<string>
		{
			"Tablet",
			"Canvas",
			"Cube",
			"_pose"
		};

		private static readonly HashSet<string> _canvasKeep = new HashSet<string>
		{
			"PanelLabel",
			"Backing (2)"
		};

		private static void DeleteAllBut(Transform parent, HashSet<string> exceptions)
		{
			for (var i = parent.childCount - 1; i >= 0; i--)
			{
				var child = parent.GetChild(i);
				if (!exceptions.Contains(child.name))
				{
					GameObject.DestroyImmediate(child.gameObject);
				}
			}
		}

		private bool _askConfirmDisconnect;

		private PrivacyManager _privacy;

		private FVRPhysicalObject _optionsPanel;
		public FVRPhysicalObject OptionsPanel
		{
			get
			{
				if (_optionsPanel != null)
				{
					return _optionsPanel;
				}

				_log.LogDebug("Creating options panel...");
		
				// TODO: Probs move this to own class when we start to populate it with buttons
				// Clone from Spectator Panel
				var panelObject = GameObject.Instantiate(WristMenu.SpectatorPanelPrefab, Vector3.zero, Quaternion.identity);

				GameObject.DestroyImmediate(panelObject.GetComponent<SpectatorPanel>());
				panelObject.name = Plugin.NAME + "Panel";

				// Delete all unneeded children
				var panelTransform = panelObject.transform;
				DeleteAllBut(panelTransform, _rootKeep);

				// Get canvas & delete all unneeded children
				var canvasTransform = panelTransform.Find("Canvas");
				DeleteAllBut(canvasTransform, _canvasKeep);

				var background = canvasTransform.Find("Backing (2)");
				var backgroundRT = background.GetComponent<RectTransform>();
				backgroundRT.anchoredPosition = new Vector2(0, 5);
				backgroundRT.localScale = new Vector3(1, 1, 1);
				backgroundRT.sizeDelta = new Vector2(550, 350);

				var panelLabel = canvasTransform.Find("PanelLabel");
				var headerText = panelLabel.GetComponent<Text>();
				headerText.text = Plugin.NAME + " OPTIONS PANEL";
				headerText.fontSize = 19;

				// Temp warning so people don't expect a functional panel
				var warning = GameObject.Instantiate(panelLabel.gameObject, background);
				warning.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);

				var warningText = warning.GetComponent<Text>();
				warningText.text = "This panel is a WIP - options to be added later";
				warningText.fontSize = 40;

				_optionsPanel = panelObject.GetComponent<FVRPhysicalObject>();
				return _optionsPanel;
			}
		}


		public FVRWristMenu WristMenu { get; private set; }

		public WristMenuButtons(ManualLogSource log, PrivacyManager privacy)
		{
			_log = log;
			_privacy = privacy;
			_askConfirmDisconnect = false;
			
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
				var btnIndex = _baseButtons.IndexOf(posName);
				for (var i = btnIndex + 1; i < _baseButtons.Count; i++)
				{
					MoveButton(_baseButtons[i]);
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
			yield return CreateButton("Button_3_ReloadScene", "H3MP_Privacy", _privacy.Text, PrivacySelector);
			yield return CreateButton("Button_9_BackToMainMenu", "H3MP_Disconnect", DISCONNECT_TEXT, LeaveLobby);
		}

		// H3MP_Panel onClick
		private UnityAction SpawnPanel(Button button)
		{
			return () =>
			{
				Boop();

				FVRViveHand hand = (GM.CurrentPlayerBody.RightHand.Find("PointingLaser").gameObject.activeInHierarchy
					? GM.CurrentPlayerBody.LeftHand
					: GM.CurrentPlayerBody.RightHand)
					.GetComponent<FVRViveHand>();

				hand.RetrieveObject(OptionsPanel);
				_log.LogDebug("Spawned panel");
			};
		}

		// H3MP_Privacy onClick
		private UnityAction PrivacySelector(Button button)
		{
			var text = button.GetComponentInChildren<Text>();

			return () =>
			{
				Boop();

				_privacy.CyclePrivacy();

				text.text = _privacy.Text;
				_log.LogDebug("Changed party privacy");
			};
		}

		// H3MP_Disconnect onClick
		private UnityAction LeaveLobby(Button button)
		{
			return () =>
			{
				Boop();
				if (!_askConfirmDisconnect)
				{
					AskDisonnect_Confirm(button.GetComponentInChildren<Text>());
					return;
				}

				ResetDisconnect(button.GetComponentInChildren<Text>());
				//TODO: make this leave session after merge with feature/netcode-refactor

				for (var i = 0; i < GM.CurrentSceneSettings.QuitReceivers.Count; i++)
				{
					GM.CurrentSceneSettings.QuitReceivers[i].BroadcastMessage("QUIT", SendMessageOptions.DontRequireReceiver);
				}
				if (GM.LoadingCallback.IsCompleted)
				{
					SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name, false, 0.5f, 0f, 0f, 0f, 1f);
				}
		
				_log.LogDebug("Left party");
			};
		}

		public void ResetDisconnect(Text btnText)
		{
			_askConfirmDisconnect = false;
			btnText.text = DISCONNECT_TEXT;
		}

		private void AskDisonnect_Confirm(Text btnText)
		{
			_askConfirmDisconnect = true;
			btnText.text = "Confirm ???";
		}

		private void Boop()
		{
			WristMenu.Aud.PlayOneShot(WristMenu.AudClip_Engage, 1f);
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
