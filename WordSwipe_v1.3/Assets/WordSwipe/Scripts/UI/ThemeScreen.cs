using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class ThemeScreen : Screen
	{
		#region Inspector Variables

		[Space]

		public ThemeListItem	themeListItemPrefab	= null;
		public RectTransform	themeListContainer	= null;

		#endregion

		#region Member Variables

		private ObjectPool themeListItemPool;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();

			themeListItemPool = new ObjectPool(themeListItemPrefab.gameObject, 1, themeListContainer);
		}

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			if (!back)
			{
				themeListContainer.anchoredPosition = Vector2.zero;
			}

			UpdateUI();
		}

		#endregion

		#region Private Methods

		private void UpdateUI()
		{
			themeListItemPool.ReturnAllObjectsToPool();

			for (int i = 0; i < ThemeManager.Instance.Themes.Count; i++)
			{
				ThemeManager.Theme	theme			= ThemeManager.Instance.Themes[i];
				ThemeListItem		themeListItem	= themeListItemPool.GetObject<ThemeListItem>();

				themeListItem.Setup(theme);

				themeListItem.SetSelected(i == ThemeManager.Instance.ActiveThemeIndex);

				themeListItem.Index				= i;
				themeListItem.OnListItemClicked	= OnThemeListItemSelected;
			}
		}

		private void OnThemeListItemSelected(int index, object data)
		{
			ThemeManager.Theme theme = ThemeManager.Instance.Themes[index];

			// Check if the theme is locked
			if (ThemeManager.Instance.IsThemeLocked(theme))
			{
				// Show the unlock theme popup
				PopupManager.Instance.Show("unlock_theme", new object[] { theme.listItemImage, theme.coinsToUnlock }, (bool cancelled, object[] outData) => 
				{
					if (!cancelled)
					{
						UnlockTheme(theme);
					}
				});
			}
			else
			{
				// Set the theme as the active theme
				ThemeManager.Instance.SetActiveTheme(theme);

				UpdateUI();
			}
		}

		private void UnlockTheme(ThemeManager.Theme theme)
		{
			// Check if the player has enough coins
			if (theme.coinsToUnlock <= GameManager.Instance.NumCoins)
			{
				// Deduct the coins from the player
				GameManager.Instance.AddCoins(-theme.coinsToUnlock);

				// Unlock the theme
				ThemeManager.Instance.UnlockTheme(theme);

				// Set the theme as the active theme
				ThemeManager.Instance.SetActiveTheme(theme);

				UpdateUI();
			}
			else
			{
				PopupManager.Instance.Show("not_enough_coins");
			}
		}

		#endregion
	}
}
