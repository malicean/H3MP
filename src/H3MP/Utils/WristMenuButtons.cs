using BepInEx.Logging;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		private enum PartyPrivacy
		{
			Open,
			Friends,
			InviteOnly,
			Closed
		}

		private static string PrivacyLocale(PartyPrivacy value)
		{
			switch (value)
			{
				case PartyPrivacy.Open: return "Open to All";
				case PartyPrivacy.Friends: return "Friends Only";
				case PartyPrivacy.InviteOnly: return "Invite Only";
				case PartyPrivacy.Closed: return "Closed to All";
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private const PartyPrivacy PARTYPRIVACY_FIRST = PartyPrivacy.Open;
		private const PartyPrivacy PARTYPRIVACY_LAST = PartyPrivacy.Closed;

		private PartyPrivacy _privacy = default;

		public static FVRWristMenu _wristMenu;
		public static bool askConfirm_Disconnect { get; set; }

		public WristMenuButtons(ManualLogSource log)
		{
			_log = log;

			WristMenuButtons.askConfirm_Disconnect = false;

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
			_wristMenu = Resources.FindObjectsOfTypeAll<FVRWristMenu>().First();
			_wristMenu.gameObject.SetActive(true);

			// Get the canvas and sets
			var canvasTF = _wristMenu.transform.Find("MenuGo/Canvas");
			var canvasRT = canvasTF.GetComponent<RectTransform>();
			canvasRT.sizeDelta = new Vector2(canvasRT.sizeDelta.x, canvasRT.sizeDelta.y + (BUTTON_SIZE * 2));
			var canvasBS = canvasTF.GetComponent<OptionsPanel_ButtonSet>();
			ref var images = ref canvasBS.ButtonImagesInSet;
			var originalButtonsLength = images.Length;

			// Create butons, resize array, and fill in our new buttons to the array
			var buttons = CreateButtons(canvasTF).ToList();
			Array.Resize(ref images, originalButtonsLength + buttons.Count);
			for (var i = 0; i < buttons.Count; ++i)
			{
				var button = buttons[i];
				_wristMenu.Buttons.Add(button);

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
			Button CreateButton(string posName, string name, string text, bool delete, Func<Button, UnityAction> callbackFactory)
			{
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.down;

				var dest = GameObject.Instantiate(source, posTransform.position, posTransform.rotation, posTransform.parent);
				dest.transform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.up;
				dest.name = name;
				dest.GetComponentInChildren<Text>().text = text;

				var button = dest.GetComponent<Button>();

				if (delete)
				{
					Component.DestroyImmediate(button);
					button = dest.AddComponent<Button>();
				}

				button.onClick.AddListener(callbackFactory(button));

				return button;
			}

			void MoveButton(string posName)
			{
				var posTransform = canvas.Find(posName);
				posTransform.position += BUTTON_PLACEMENT_DIFF * 2 * Vector3.down;
			}

			yield return CreateButton("Button_3_ReloadScene", "H3MP_Privacy", PrivacyLocale(_privacy), true, PrivacySelector);
			yield return CreateButton("Button_9_BackToMainMenu", "H3MP_Disconnect", "Leave Party", true, LeaveLobby);
			MoveButton("Button_10_QuitToDesktop");
		}

		private UnityAction LeaveLobby(Button button)
		{
			//TODO: make this leave session after merge with feature/netcode-refactor
			return () =>
			{
				_wristMenu.Aud.PlayOneShot(_wristMenu.AudClip_Engage, 1f);
				if (!askConfirm_Disconnect)
				{
					ResetDisconnect(button);
					AskDisonnect_Confirm(button);
					return;
				}
				ResetDisconnect(button);
				for (int i = 0; i < GM.CurrentSceneSettings.QuitReceivers.Count; i++)
				{
					GM.CurrentSceneSettings.QuitReceivers[i].BroadcastMessage("QUIT", SendMessageOptions.DontRequireReceiver);
				}
				if (GM.LoadingCallback.IsCompleted)
				{
					SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name, false, 0.5f, 0f, 0f, 0f, 1f);
				}
				_log.LogDebug("Left H3MP party");
			};
		}

		private UnityAction PrivacySelector(Button button)
		{
			//TODO: make this change the discord privacy
			return () =>
			{
				_wristMenu.Aud.PlayOneShot(_wristMenu.AudClip_Engage, 1f);
				if (++_privacy > PARTYPRIVACY_LAST)
				{
					_privacy = PARTYPRIVACY_FIRST;
				}

				var buttonText = button.GetComponentInChildren<Text>().text = PrivacyLocale(_privacy);

				_log.LogDebug("Changed party privacy");
			};
		}

		public static void ResetDisconnect(Button button)
		{
			askConfirm_Disconnect = false;
			button.GetComponentInChildren<Text>().text = "Leave Party";
		}

		private void AskDisonnect_Confirm(Button button)
		{
			askConfirm_Disconnect = true;
			button.GetComponentInChildren<Text>().text = "Confirm ???";
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
