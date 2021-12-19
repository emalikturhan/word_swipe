using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class ThemeColorBehaviour : ThemeBehaviour
	{
		#region Member Variables

		private Graphic graphic;

		#endregion

		#region Properties

		protected override ThemeManager.Type ItemType { get { return ThemeManager.Type.Color; } }

		#endregion

		#region Unity Methods

		private void Awake()
		{
			graphic = gameObject.GetComponent<Graphic>();

			if (graphic == null)
			{
				Debug.LogError("[ThemeColorBehaviour] There is no Graphic component attached to this GameObject, gameObject.name: " + gameObject.name);
			}
		}

		#endregion

		#region Protected Methods

		protected override void SetTheme(ThemeManager.ThemeItem themeItem)
		{
			if (graphic != null)
			{
				graphic.color = themeItem.color;
			}
		}

		#endregion
	}
}
