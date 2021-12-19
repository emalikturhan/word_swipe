using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class RainDropSlow : SpawnObject
	{
		#region Inspector Variables

		public float minSize				= 0f;
		public float maxSize				= 0f;
		public float minDuration			= 0f;
		public float maxDuration			= 0f;
		public float shrinkRate			= 0f;
		public float minShrinkSize		= 0f;

		[Space]

		public float rainDropStayDuration	= 0f;
		public float rainDropFadeDuration	= 0f;
		public float rainDropSpacing		= 0f;
		public float rainDropScaleDown	= 0f;
		public float rainDropOffset		= 0f;

		public Image rainDropPrefab		= null;

		#endregion

		#region Member Variables

		// ObjectPool shared by all instances of RainDropSlow
		private static ObjectPool rainDropPool;

		private List<GameObject>	activeDrops = new List<GameObject>();
		private bool				startDropping;
		private bool				waitForDrops;
		private float				nextDropTimer;
		private bool				nextDropDir;

		#endregion

		#region Unity Methods

		private void Update()
		{
			if (startDropping)
			{
				RectT.sizeDelta = new Vector2(RectT.sizeDelta.x - shrinkRate * Time.deltaTime, RectT.sizeDelta.y - shrinkRate * Time.deltaTime);

				if (RectT.sizeDelta.x <= minShrinkSize)
				{
					UIAnimation.DestroyAllAnimations(gameObject);

					startDropping	= false;
					waitForDrops	= true;
				}

				nextDropTimer += Time.deltaTime;

				if (nextDropTimer >= rainDropSpacing)
				{
					nextDropTimer = 0;

					Image drop = rainDropPool.GetObject<Image>();

					float randOffset = Random.Range(0, nextDropDir ? -rainDropOffset : rainDropOffset);

					nextDropDir = !nextDropDir;

					drop.color							= rainDropPrefab.color;
					drop.rectTransform.sizeDelta		= RectT.sizeDelta * rainDropScaleDown;
					drop.rectTransform.anchoredPosition	= RectT.anchoredPosition + new Vector2(randOffset, 0f);

					activeDrops.Add(drop.gameObject);

					UIAnimation anim;

					anim			= UIAnimation.Color(drop, new Color(1f, 1f, 1f, 0f), rainDropFadeDuration);
					anim.startDelay	= rainDropStayDuration;

					anim.OnAnimationFinished += (GameObject obj) => 
					{
						ObjectPool.ReturnObjectToPool(obj);
						activeDrops.Remove(obj);
					};

					anim.Play();
				}
			}

			if (waitForDrops && activeDrops.Count == 0)
			{
				waitForDrops = false;

				Die();
			}
		}

		private void OnDestroy()
		{
			rainDropPool = null;
		}

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			if (rainDropPool == null)
			{
				rainDropPool = new ObjectPool(rainDropPrefab.gameObject, 1, transform.parent, ObjectPool.PoolBehaviour.CanvasGroup);
			}

			// Set random size
			float size = Random.Range(minSize, maxSize);

			RectT.sizeDelta = new Vector2(size, size);

			// Set random x starting position
			float startX	= Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY	= (ParentRectT.rect.height + size) / 2f;
			float endY		= (-ParentRectT.rect.height - size) / 2f;

			RectT.anchoredPosition = new Vector2(startX, startY);

			// Get random duration (Time it takes to go from top of screen to bottom of screen)
			float duration = Random.Range(minDuration, maxDuration);

			// Animate y position
			UIAnimation anim = UIAnimation.PositionY(RectT, endY, duration);

			anim.OnAnimationFinished += (GameObject obj) => 
			{
				startDropping	= false;
				waitForDrops	= true;
			};

			anim.Play();

			startDropping	= true;
			nextDropTimer	= 0;
		}

		#endregion
	}
}
