using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class ThemeListItem : ClickableListItem
	{
		#region Inspector Variables

		public Image		themeImage			= null;
		public Text		themeNameText		= null;
		public Image		borderImage			= null;
		public Color		borderNormalColor	= Color.white;
		public Color		borderSelectedColor	= Color.white;
		public GameObject	lockedContainer		= null;
		public Text		coinsAmountText		= null;

		#endregion

		#region Public Methods

		public void Setup(ThemeManager.Theme theme)
		{
			themeImage.sprite	= theme.listItemImage;
			themeNameText.text	= theme.name;

			lockedContainer.SetActive(ThemeManager.Instance.IsThemeLocked(theme));
			coinsAmountText.text = theme.coinsToUnlock.ToString();
		}

		public void SetSelected(bool isSelected)
		{
			borderImage.color = isSelected ? borderSelectedColor : borderNormalColor;
		}

		#endregion
	}
}
