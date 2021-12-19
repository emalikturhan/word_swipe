using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class DailyGiftPopup : Popup
	{
		#region Inspector Variables

		[Space]

		public List<GiftBox>		giftBoxes		= null;
		public Transform			giftAnimMarker	= null;
		public AnimationCurve		giftAnimCurve	= null;

		#endregion

		#region Public Methods

		public void OnGiftSelected(int index)
		{
			GiftBox giftBox = giftBoxes[index];

			for (int i = 0; i < giftBoxes.Count; i++)
			{
				giftBoxes[i].GetComponent<Button>().interactable = false;
			}

			// Move the give to the animation marker
			giftBox.transform.SetParent(giftAnimMarker, true);

			// Pick a random gift
			DailyGiftManager.GiftInfo giftInfo = DailyGiftManager.Instance.PickGift();

			// Animate the gift to the marker
			StartCoroutine(AnimateGift(giftBox, giftInfo));

			// Fade out the popup container
			UIAnimation.Alpha(animContainer.gameObject, 0f, 1f).Play();
		}

		#endregion

		#region Protected Methods

		private IEnumerator AnimateGift(GiftBox giftBox, DailyGiftManager.GiftInfo giftInfo)
		{
			RectTransform giftRectT = giftBox.transform as RectTransform;

			UIAnimation anim;

			// Move the gift to the center
			anim				= UIAnimation.PositionX(giftRectT, 0f, 1f);
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= giftAnimCurve;
			anim.Play();

			anim				= UIAnimation.PositionY(giftRectT, 0f, 1f);
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= giftAnimCurve;
			anim.Play();

			// Scale the gift up a bit
			anim				= UIAnimation.ScaleX(giftRectT, giftAnimMarker.localScale.x, 1f);
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= giftAnimCurve;
			anim.Play();

			anim				= UIAnimation.ScaleY(giftRectT, giftAnimMarker.localScale.y, 1f);
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= giftAnimCurve;
			anim.Play();

			// Wait for the gift to reach the center of the screen
			while (anim.IsPlaying)
			{
				yield return null;
			}

			// Play the gift opening animation
			yield return giftBox.PlayOpenAnimation(giftInfo.giftImage, giftInfo.giftAmount);

			// After the open animation wait a second
			yield return new WaitForSeconds(1f);

			// If its a coin animation then animate the coins
			if (giftInfo.giftId == "coin")
			{
				CoinAnimationManager.Instance.AnimateCoins(GameManager.Instance.NumCoins - giftInfo.giftAmount, GameManager.Instance.NumCoins, giftBox.CoinMarker, 200);
			}

			// Hide the popup
			Hide(false);
		}

		#endregion
	}
}
