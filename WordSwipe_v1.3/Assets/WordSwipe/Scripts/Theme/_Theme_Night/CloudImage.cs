using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	[RequireComponent(typeof(Image))]
	public class CloudImage : SpawnObject
	{
		#region Inspector Variables

		public List<Sprite>	cloudSprites;
		public float			minDuration;
		public float			maxDuration;

		#endregion

		#region Member Variables

		#endregion

		#region Properties

		public Image Image { get { return gameObject.GetComponent<Image>(); } }

		#endregion

		#region Unity Methods

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			base.Spawned();

			// Get a random cloud to display
			Image.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];

			// Get the size of this instance so fit the cloud
			Image.SetNativeSize();

			// Set the starting position
			//float startX = ParentRectT.rect.width / 2f + RectT.rect.width / 2f;
			float startX = Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY = Random.Range(-ParentRectT.rect.height / 2f, ParentRectT.rect.height / 2f);

			RectT.anchoredPosition = new Vector2(startX, startY);

			// Get a random duration to cross the screen
			float duration = Random.Range(minDuration, maxDuration);

			float rightX	= ParentRectT.rect.width / 2f + RectT.rect.width / 2f;
			float leftX		= -ParentRectT.rect.width / 2f - RectT.rect.width / 2f;
			float distance	= Mathf.Abs(rightX - leftX);

			duration *= Mathf.Abs(startX - leftX) / distance;

			// Fade it in
			float fadeDuration = duration / 3f;

			FadeIn(fadeDuration);

			// Animate it accross the screen
			UIAnimation.PositionX(RectT, leftX, duration).Play();

			// Fade out just before it moves off screen
			StartCoroutine(StartFade(duration - fadeDuration, fadeDuration));
		}

		#endregion

		#region Protected Methods

		#endregion

		#region Private Methods

		private IEnumerator StartFade(float delay, float fadeOutDuration)
		{
			yield return new WaitForSeconds(delay);

			FadeOut(fadeOutDuration);
		}

		#endregion
	}
}
