using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class ThemePrefabBehaviour : ThemeBehaviour
	{
		#region Member Variables

		private GameObject instantiatedObj;

		#endregion

		#region Properties

		protected override ThemeManager.Type ItemType { get { return ThemeManager.Type.Prefab; } }

		#endregion

		#region Protected Methods

		protected override void SetTheme(ThemeManager.ThemeItem themeItem)
		{
			if (instantiatedObj != null)
			{
				Destroy(instantiatedObj);
			}

			if (themeItem.prefab != null)
			{
				instantiatedObj = Instantiate(themeItem.prefab, transform, false);
			}
		}

		#endregion
	}
}
