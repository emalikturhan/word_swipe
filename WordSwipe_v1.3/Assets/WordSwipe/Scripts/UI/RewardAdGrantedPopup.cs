using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class RewardAdGrantedPopup : Popup
	{
		#region Inspector Variables

		[Space]

		public Text			coinsRewardedText	= null;
		public RectTransform	coinMarker			= null;

		#endregion

		#region Member Variables

		private int coinsRewarded;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			base.OnShowing(inData);

			coinsRewarded = (int)inData[0];

			coinsRewardedText.text = "x " + coinsRewarded;

			coinMarker.gameObject.SetActive(true);
		}

		public void OnClaimButtonClicked()
		{
			Hide(false);
		}

		public override void OnHiding()
		{
			base.OnHiding();

			CoinAnimationManager.Instance.AnimateCoins(GameManager.Instance.NumCoins - coinsRewarded, GameManager.Instance.NumCoins, coinMarker, 100);

			coinMarker.gameObject.SetActive(false);
		}

		#endregion
	}
}
