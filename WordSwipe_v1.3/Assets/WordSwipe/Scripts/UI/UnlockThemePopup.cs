using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class UnlockThemePopup : Popup
	{
		#region Inspector Variables

		[Space]

		public Image	listItemImage	= null;
		public Text	costText		= null;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			listItemImage.sprite	= inData[0] as Sprite;
			costText.text			= ((int)inData[1]).ToString();
		}

		#endregion
	}
}
