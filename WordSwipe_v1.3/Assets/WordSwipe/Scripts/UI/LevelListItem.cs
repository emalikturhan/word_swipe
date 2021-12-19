using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class LevelListItem : MonoBehaviour
	{
		#region Inspector Variables

		public Button		itemButton		= null;
		public Text		levelNumberText	= null; 
		public Text		levelHintText	= null;
		public GameObject playIcon		= null;
		public GameObject lockedIcon		= null;

		#endregion

		#region Member Variables

		private LevelData levelData;

		#endregion

		#region Properties

		public System.Action<LevelData> OnLevelItemSelected { get; set; }

		#endregion

		#region Public Methods

		public void Setup(LevelData levelData)
		{
			this.levelData = levelData;

			levelNumberText.text	= string.Format("Level {0}", levelData.LevelNumber);
			levelHintText.text		= levelData.Hint;

			bool isLocked = GameManager.Instance.IsLevelLocked(levelData.LevelNumber);

			itemButton.interactable = !isLocked;

			playIcon.SetActive(!isLocked);
			levelHintText.gameObject.SetActive(!isLocked);

			lockedIcon.SetActive(isLocked);
		}

		public void OnClicked()
		{
			if (OnLevelItemSelected != null)
			{
				OnLevelItemSelected(levelData);
			}
		}

		#endregion
	}
}
