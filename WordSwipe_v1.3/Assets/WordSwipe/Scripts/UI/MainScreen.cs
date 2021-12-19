using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class MainScreen : Screen
	{
		#region Inspector Variables

		[Space]
		public Text		playButtonText	= null;
		public GameObject	playButton		= null;

		#endregion

		#region Public Methods

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			SetPlayButtonText();
		}

		public void OnPlayButtonClicked()
		{
			if (GameManager.Instance.LastCompletedLevel == 0)
			{
				// If last completed level is -1 then start the first level in the game
				GameManager.Instance.StartLevel(0, 0);

				return;
			}

			PackInfo packInfo;
			LevelData levelData;

			// Get the pack/level for the next level
			if (!GameManager.Instance.GetPackAndLevel(GameManager.Instance.LastCompletedLevel + 1, out packInfo, out levelData))
			{
				// Stop if we could not get the pack/level
				return;
			}

			GameManager.Instance.StartLevel(packInfo, levelData);
		}

		#endregion

		#region Private Methods

		private void SetPlayButtonText()
		{
			// Set the button text for the current level
			int lastCompletedLevel = GameManager.Instance.LastCompletedLevel;

			if (lastCompletedLevel == 0)
			{
				playButtonText.text = "Level 1";

				return;
			}

			PackInfo packInfo;
			LevelData levelData;

			if (!GameManager.Instance.GetPackAndLevel(lastCompletedLevel + 1, out packInfo, out levelData))
			{
				// If we could not get the next level after the last completed level then hide the play button
				playButton.SetActive(false);

				return;
			}

			playButtonText.text = "Level " + levelData.LevelNumber;
		}

		#endregion
	}
}
