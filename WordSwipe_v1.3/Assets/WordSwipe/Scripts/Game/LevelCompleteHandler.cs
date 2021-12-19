using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	[RequireComponent(typeof(CanvasGroup))]
	public class LevelCompleteHandler : MonoBehaviour
	{
		#region Inspector Variables

		public Text			nextLevelText			= null;
		public Text			rewardProgressText		= null;
		public Slider			rewardProgressSlider	= null;
		public GameObject		coinContainer			= null;
		public RectTransform	coinMarker				= null;
		public Text			coinAmountText			= null;
		public float			fadeInAnimDuration		= 0;
		public float			progressBarAnimDuration	= 0;

		#endregion

		#region Member Variables

		#endregion

		#region Properties

		#endregion

		#region Unity Methods

		#endregion

		#region Public Methods

		public void Hide()
		{
			gameObject.GetComponent<CanvasGroup>().alpha			= 0f;
			gameObject.GetComponent<CanvasGroup>().interactable		= false;
			gameObject.GetComponent<CanvasGroup>().blocksRaycasts	= false;
		}

		public void ShowLevelComplete(int completedLevelNumber, bool nextLevelCompleted)
		{
			PackInfo packInfo = GameManager.Instance.ActivePackInfo;

			// Set the next level text
			nextLevelText.text = "Next Level "; //+ (GameManager.Instance.LastCompletedLevel + 1);

			// If the level that was completed doesn't equal the last completed level number then the player re-played an already completed level
			if (!nextLevelCompleted)
			{
				// Hide the progress
				rewardProgressSlider.gameObject.SetActive(false);

				StartCoroutine(AnimateIn());
			}
			else
			{
				rewardProgressSlider.gameObject.SetActive(true);
				coinContainer.SetActive(true);

				// Set the reward amount text
				coinAmountText.text = "x" + GameManager.Instance.PackCompleteReward.ToString();

				// Get the amount of levels completed in this pack
				int numCompleted = completedLevelNumber - packInfo.FromLevelNumber + 1;

				// Set the number of levels completed text
				rewardProgressText.text = string.Format("{0} / {1}", numCompleted, packInfo.NumLevelsInPack);

				// Set the from value on the progress bar
				rewardProgressSlider.value = ((float)numCompleted - 1f) / (float)packInfo.NumLevelsInPack;

				// Animate the level complete elements
				StartCoroutine(AnimateLevelComplete(numCompleted, packInfo.NumLevelsInPack));

				// Check if all levels in the pack are now complete
				if (numCompleted == packInfo.NumLevelsInPack)
				{
					// Don;t start the below animations until it faded in and the progress bar anim finishes
					float startDelay = fadeInAnimDuration + progressBarAnimDuration;

					// Reward the player with the coins
					GameManager.Instance.AddCoinsAnimate(GameManager.Instance.PackCompleteReward, coinMarker, startDelay);

					// Fade out the coin container
					UIAnimation anim = UIAnimation.Alpha(coinContainer, 0f, progressBarAnimDuration);

					anim.startDelay = startDelay;
					anim.OnAnimationFinished += (GameObject obj) => { obj.SetActive(false); };

					anim.Play();
				}
			}
		}

		#endregion

		#region Private Methods

		private IEnumerator AnimateIn()
		{
			UIAnimation.Alpha(gameObject, 1f, fadeInAnimDuration).Play();

			yield return new WaitForSeconds(fadeInAnimDuration);

			gameObject.GetComponent<CanvasGroup>().interactable		= true;
			gameObject.GetComponent<CanvasGroup>().blocksRaycasts	= true;
		}

		private IEnumerator AnimateLevelComplete(int numCompleted, int total)
		{
			yield return AnimateIn();

			bool	isAnimating	= true;
			float	timer		= 0f;

			float fromValue	= ((float)numCompleted - 1f) / (float)total;
			float toValue	= (float)numCompleted / (float)total;

			while (isAnimating)
			{
				yield return new WaitForEndOfFrame();

				timer += Time.deltaTime;

				if (timer >= progressBarAnimDuration)
				{
					isAnimating = false;
				}

				rewardProgressSlider.value = Mathf.Lerp(fromValue, toValue, timer / progressBarAnimDuration);
			}
		}

		#endregion
	}
}
