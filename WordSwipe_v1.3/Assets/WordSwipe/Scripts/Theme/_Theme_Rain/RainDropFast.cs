using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class RainDropFast : SpawnObject
	{
		#region Inspector Variables

		public float minDuration = 0f;
		public float maxDuration = 0f;

		public float minScale = 0f;
		public float maxScale = 0f;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			// Set size
			float scale = Random.Range(minScale, maxScale);
			transform.localScale = new Vector3(scale, scale, 1f);

			// Set random x starting position
			float startX	= Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY	= (ParentRectT.rect.height) / 2f;
			float endY		= (-ParentRectT.rect.height) / 2f;

			RectT.anchoredPosition = new Vector2(startX, startY);

			// Get random duration (Time it takes to go from top of screen to bottom of screen)
			float duration = Random.Range(minDuration, maxDuration);

			// Animate y position
			UIAnimation anim = UIAnimation.PositionY(RectT, endY, duration);

			anim.OnAnimationFinished += (GameObject obj) => 
			{
				Die();
			};

			anim.Play();
		}

		#endregion
	}
}
