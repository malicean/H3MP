using BepInEx.Logging;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Newtonsoft.Json.Linq;

namespace H3MP.Utils
{
	public class ChangelogPanel
	{
		private const string REPO_PATH = "ash-hat/H3MP";

		private readonly ManualLogSource _log;
		private readonly Func<IEnumerator, Coroutine> _coroutine;
		private readonly Version _version;

		public ChangelogPanel(ManualLogSource log, Func<IEnumerator, Coroutine> coroutine, Version version)
		{
			_log = log;
			_coroutine = coroutine;
			_version = version;

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "MainMenu3")
			{
				// Creates a cloned mainscreen panel for H3MP changelog
				var panel = CreatePanel();
				panel.text = "Awaiting web request...";

				// Gets body text from GitHub to populate H3MP panel
				_coroutine(_UpdatePanel(panel));
			}
		}

		private IEnumerator _UpdatePanel(Text panel)
		{
			var currentTag = "v" + _version.ToString();

			// Get the main body text from GitHub
			using (UnityWebRequest wwwPanel = UnityWebRequest.Get("https://raw.githubusercontent.com/" + REPO_PATH + $"/{currentTag}/ui/mainmenu3-panel.txt"))
			{
				var awaitPanel = wwwPanel.Send();

				string latestTag;
				bool needsUpdate;

				// Get the latest release from GitHub
				using (UnityWebRequest wwwVersion = UnityWebRequest.Get("https://api.github.com/repos/" + REPO_PATH + "/releases/latest"))
				{
					yield return wwwVersion.Send();

					if (wwwVersion.isError)
					{
						var error = "Failed to get latest release: " + wwwVersion.error;

						_log.LogError(error);
						panel.text = error;

						yield break;
					}

					// Check if this matches the installed version
					latestTag = JObject.Parse(wwwVersion.downloadHandler.text)["tag_name"].ToString();
					needsUpdate = currentTag != latestTag;
				}

				yield return awaitPanel;

				if (wwwPanel.isError)
				{
					var error = "Failed to get main menu panel text: " + wwwPanel.error;

					_log.LogError(error);
					panel.text = error;

					yield break;
				}

				// Build the body text with version info & highlighting
				StringBuilder body = new StringBuilder();
				var versionTemplate = $"<color=#{{0}}>{currentTag}  ({{1}})</color></b>";

				body.Append("<b>Installed version: ");
				if (needsUpdate)
				{
					body.AppendLine(string.Format(versionTemplate, "ff0000", $"update available: {latestTag}"));
				}
				else
				{
					body.AppendLine(string.Format(versionTemplate, "00b300", "latest"));
				}
				body.AppendLine();
				body.Append(wwwPanel.downloadHandler.text);

				panel.text = body.ToString();
			}
		}

		private Text CreatePanel()
		{
			//TODO: Clean this up
			GameObject changelogPanel = GameObject.Find("MainScreen1");
			GameObject panel = GameObject.Instantiate(changelogPanel, changelogPanel.transform.parent);

			panel.transform.position = new Vector3(-1.376f, 1.046f, -3.621f);
			panel.transform.localEulerAngles = new Vector3(0, -71.437f, 0);
			panel.name = "H3MP";
			panel.layer = LayerMask.NameToLayer("Environment");

			var panelTexts = panel.GetComponentsInChildren<Text>();
			panelTexts[0].text = "H3MP"; // Title

			return panelTexts[1];
		}

		public void Dispose()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}
