using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class DotImage : SpawnObject
	{
		#region Inspector Variables

		public float speed		= 0;
		public float fadeDuration	= 0;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			List<int> sides = new List<int>() { 0, 1, 2, 3 };

			Vector2 startPosition	= RandPosition(sides);
			Vector2 endPosition		= RandPosition(sides);
			float	duration		= Vector2.Distance(startPosition, endPosition) / speed;

			RectT.anchoredPosition = startPosition;

			UIAnimation.DestroyAllAnimations(gameObject);

			CG.alpha = 0f;

			UIAnimation anim;

			anim = UIAnimation.Alpha(gameObject, 0f, 1f, fadeDuration);
			anim.Play();

			anim = UIAnimation.PositionX(RectT, endPosition.x, duration);
			anim.Play();

			anim = UIAnimation.PositionY(RectT, endPosition.y, duration);
			anim.OnAnimationFinished += (GameObject obj) => { Die(); };
			anim.Play();

			StartCoroutine(FadeOutAftWaitTime(duration - fadeDuration));
		}

		#endregion

		#region Private Methods

		private Vector2 RandPosition(List<int> sides)
		{
			float halfParentWidth	= ParentRectT.rect.width / 2f;
			float halfParentHeight	= ParentRectT.rect.height / 2f;

			int side = sides[Random.Range(0, sides.Count)];

			sides.Remove(side);

			Vector2 position = Vector2.zero;

			// Top
			if (side == 0)
			{
				position.x = Random.Range(-halfParentWidth, halfParentWidth);
				position.y = halfParentHeight;
			}
			// Bottom
			else if (side == 1)
			{
				position.x = Random.Range(-halfParentWidth, halfParentWidth);
				position.y = -halfParentHeight;
			}
			// Left
			else if (side == 2)
			{
				position.x = -halfParentWidth;
				position.y = Random.Range(-halfParentHeight, halfParentHeight);
			}
			// Right
			else if (side == 3)
			{
				position.x = halfParentWidth;
				position.y = Random.Range(-halfParentHeight, halfParentHeight);
			}

			return position;
		}

		private IEnumerator FadeOutAftWaitTime(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);

			UIAnimation.Alpha(gameObject, 1f, 0f, fadeDuration).Play();
		}

		#endregion
	}
}
