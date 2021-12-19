using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class WordLetterTile : LetterTile
	{
		#region Inspector Variables

		[Header("Word Tile")]
		[SerializeField] protected GameObject blankTileObj = null;

		[Header("Anim Settings - Show")]
		public float fadeAnimDuration	= 0;
		public float startDelay		= 0;

		[Header("Anim Settings - Hint")]
		public float hintAnimDuration	= 0;
		public float hintPulseScale	= 1f;

		#endregion

		#region Member Variables

		private bool shownAsHint;

		#endregion

		#region Public Methods

		public override void Setup(char letter)
		{
			base.Setup(letter);

			shownAsHint = false;

			bkgImage.gameObject.SetActive(true);
		}

		public override void SetBlank()
		{
			base.SetBlank();

			blankTileObj.SetActive(true);
		}

		public override void SetShown()
		{
			base.SetShown();

			blankTileObj.SetActive(false);
		}

		public void ShowAsHint(bool animate = true)
		{
			// Show the tile object but hide the background because the word has not been found yet
			letterTileObj.SetActive(true);
			bkgImage.gameObject.SetActive(false);

			if (animate)
			{
				// Fade in the letter
				StartCoroutine(PlayShowHintAnimation());
			}

			shownAsHint = true;
		}

		public void Show(int index)
		{
			// Show the tile object
			letterTileObj.SetActive(true);
			bkgImage.gameObject.SetActive(true);

			if (!shownAsHint)
			{
				letterText.gameObject.SetActive(false);
			}

			// Fade in the background of the tile, when the fade finishes show the letter
			FadeIn(bkgImage.gameObject, fadeAnimDuration, startDelay * index, (GameObject obj) => { letterText.gameObject.SetActive(true); } );
		}

		#endregion

		#region Private Methods

		private IEnumerator PlayShowHintAnimation()
		{
			FadeIn(letterText.gameObject, hintAnimDuration / 2f);

			UIAnimation.ScaleX(letterText.rectTransform, hintPulseScale, hintAnimDuration / 2f).Play();
			UIAnimation.ScaleY(letterText.rectTransform, hintPulseScale, hintAnimDuration / 2f).Play();

			yield return new WaitForSeconds(hintAnimDuration / 2f);

			UIAnimation.ScaleX(letterText.rectTransform, 1f, hintAnimDuration / 2f).Play();
			UIAnimation.ScaleY(letterText.rectTransform, 1f, hintAnimDuration / 2f).Play();
		}

		private void FadeIn(GameObject obj, float duration, float delay = 0, System.Action<GameObject> animFinished = null)
		{
			// Fade the letter in
			UIAnimation anim = UIAnimation.Alpha(obj, 0, 1, duration);

			anim.startDelay = delay;
			anim.startOnFirstFrame = true;

			if (animFinished != null)
			{
				anim.OnAnimationFinished += animFinished;
			}

			anim.Play();
		}

		#endregion
	}
}
