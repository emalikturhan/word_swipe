using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public abstract class ThemeBehaviour : MonoBehaviour, IThemeBehaviour
	{
		#region Inspector Variables

		public string id = "";

		#endregion

		#region Abstract Methods

		protected abstract ThemeManager.Type ItemType { get; }

		protected abstract void	SetTheme(ThemeManager.ThemeItem themeItem);

		#endregion

		#region Unity Methods

		private void Start()
		{
			if (ThemeManager.Exists() && ThemeManager.Instance.Enabled)
			{
				ThemeManager.Instance.Register(this);

				SetTheme();
			}
		}

		#endregion

		#region Public Methods

		public void NotifyThemeChanged()
		{
			SetTheme();
		}

		#endregion

		#region Private Methods

		private void SetTheme()
		{
			ThemeManager.ThemeItem	themeItem;
			ThemeManager.ItemId		itemId;

			if (ThemeManager.Instance.GetThemeItem(id, out themeItem, out itemId))
			{
				if (itemId.type != ItemType)
				{
					Debug.LogErrorFormat("[ThemeBehaviour] The theme id \"{0}\" is set to type \"{1}\" but we were expecting it to be \"{2}\"", id, itemId.type, ItemType);

					return;
				}

				SetTheme(themeItem);
			}
			else
			{
				Debug.LogErrorFormat("[ThemeBehaviour] Could not find theme id \"{0}\", gameObject: {1}", id, gameObject.name);
			}
		}

		#endregion
	}
}
