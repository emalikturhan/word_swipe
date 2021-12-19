using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class ThemeImageBehaviour : ThemeBehaviour
	{
		#region Member Variables

		private Image image;

		#endregion

		#region Properties

		protected override ThemeManager.Type ItemType { get { return ThemeManager.Type.Image; } }

		#endregion

		#region Unity Methods

		private void Awake()
		{
			image = gameObject.GetComponent<Image>();

			if (image == null)
			{
				Debug.LogError("[ThemeImageBehaviour] There is no Image component attached to this GameObject, gameObject.name: " + gameObject.name);
			}
		}

		#endregion

		#region Protected Methods

		protected override void SetTheme(ThemeManager.ThemeItem themeItem)
		{
			if (image != null)
			{
				image.sprite = themeItem.image;
			}
		}

		#endregion
	}
}
