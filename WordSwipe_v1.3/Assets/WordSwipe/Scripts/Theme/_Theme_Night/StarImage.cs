using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class StarImage : SpawnObject
	{
		#region Inspector Variables

		public float minSize;
		public float maxSize;
		public float minDuration;
		public float maxDuration;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			transform.localScale = new Vector3(0f, 0f, 1f);

			// Set a random position
			float startX = Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY = Random.Range(-ParentRectT.rect.height / 2f, ParentRectT.rect.height / 2f);

			RectT.anchoredPosition = new Vector2(startX, startY);

			// Set a random size
			float size = Random.Range(minSize, maxSize);

			RectT.sizeDelta = new Vector2(size, size);

			// Get a random duration
			float duration = Random.Range(minDuration, maxDuration);

			// Pulse and fade in
			FadeIn(duration);

			UIAnimation anim;

			anim = UIAnimation.ScaleX(RectT, 0f, 1f, duration);
			anim.Play();

			anim = UIAnimation.ScaleY(RectT, 0f, 1f, duration);
			anim.OnAnimationFinished += (GameObject obj) => 
			{
				FadeOut(duration);

				UIAnimation.ScaleX(RectT, 1f, 0f, duration).Play();
				UIAnimation.ScaleY(RectT, 1f, 0f, duration).Play();
			};

			anim.Play();
		}

		#endregion
	}
}
