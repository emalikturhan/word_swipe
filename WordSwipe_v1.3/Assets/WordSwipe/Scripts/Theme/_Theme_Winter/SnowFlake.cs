using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class SnowFlake : SpawnObject
	{
		#region Inspector Variables

		public float minSize;
		public float maxSize;
		public float minDuration;
		public float maxDuration;
		public float minSwayAmount;
		public float maxSwayAmount;
		public float minSwayDuration;
		public float maxSwayDuration;
		public AnimationCurve swayAnimCurve;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			float size		= Random.Range(minSize, maxSize);
			float duration	= Mathf.Lerp(maxDuration, minDuration, size / maxSize);

			RectT.sizeDelta = new Vector2(size, size);

			float startX = Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY = ParentRectT.rect.height / 2f + size;

			RectT.anchoredPosition = new Vector2(startX, startY);

			UIAnimation anim = UIAnimation.PositionY(RectT, -ParentRectT.rect.height / 2f - size, duration);
			anim.OnAnimationFinished += (GameObject obj) => { Die(); };
			anim.Play();

			float swayAmount	= Mathf.Lerp(minSwayAmount, maxSwayAmount, size / maxSize);
			float swayDuration	= Random.Range(minSwayDuration, maxSwayDuration);
			float startSwayDir	= Random.Range(0, 2) == 0 ? -1 : 1;

			anim				= UIAnimation.PositionX(RectT, RectT.anchoredPosition.x - swayAmount * startSwayDir, RectT.anchoredPosition.x + swayAmount * startSwayDir, swayDuration);
			anim.loopType		= UIAnimation.LoopType.Reverse;
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= swayAnimCurve;
			anim.Play();

			anim.elapsedTime = Random.Range(0, swayDuration);
		}

		#endregion
	}
}
