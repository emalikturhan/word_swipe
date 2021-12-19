using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class PhaseCircleImage : SpawnObject
	{
		#region Inspector Variables

		public RectTransform	endMarker	= null;
		public float			minWidth	= 0;
		public float			maxWidth	= 0;
		public float			minSpeed	= 0;
		public float			maxSpeed	= 0;
		public float			spacing		= 0;
		public float			leftYOffset	= 0;

		#endregion

		#region Member Variables

		private static List<float> startingRightYs;
		private static List<float> startingLeftYs;

		private bool	isMoving;
		private bool	isLeft;
		private float	startY;
		private float	speed;

		#endregion

		#region Unity Methods

		private void Update()
		{
			if (isMoving)
			{
				float change = speed * Time.deltaTime;

				RectT.anchoredPosition = RectT.anchoredPosition + new Vector2(-change, change);

				Vector2 endMarkerPos = RectTransformUtility.CalculateRelativeRectTransformBounds(ParentRectT, endMarker).center;

				if (isLeft)
				{
					if (endMarkerPos.x > ParentRectT.rect.width / 2f || endMarkerPos.y < -ParentRectT.rect.height / 2f)
					{
						isMoving = false;

						startingLeftYs.Add(startY);

						Die();
					}
				}
				else
				{
					if (endMarkerPos.x < -ParentRectT.rect.width / 2f || endMarkerPos.y > ParentRectT.rect.height / 2f)
					{
						isMoving = false;

						startingRightYs.Add(startY);

						Die();
					}
				}
			}
		}

		private void OnDestroy()
		{
			if (isMoving)
			{
				if (isLeft)
				{
					startingLeftYs.Add(startY);
				}
				else
				{
					startingRightYs.Add(startY);
				}
			}
		}

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			if (startingRightYs == null)
			{
				startingRightYs = new List<float>();
				startingLeftYs = new List<float>();

				InitializeYs(startingRightYs, false);
				InitializeYs(startingLeftYs, true);
			}

			// Set a random size
			float width = Random.Range(minWidth, maxWidth);

			RectT.sizeDelta = new Vector2(width, RectT.sizeDelta.y);

			// Get a random side to start on
			isLeft = Random.Range(0, 2) == 0;

			RectT.localEulerAngles = new Vector3(0, 0, isLeft ? 135 : -45);

			// Set a random position
			float startX = ParentRectT.rect.width / 2f * (isLeft ? -1 : 1);
			float y = 0;

			if (!GetRandomY(isLeft, out y))
			{
				return;
			}

			startY = y;

			RectT.anchoredPosition = new Vector2(startX, startY);

			// Set a random speed
			speed = Random.Range(minSpeed, maxSpeed) * (isLeft ? -1 : 1);

			isMoving = true;
		}

		#endregion

		#region Private Methods

		private bool GetRandomY(bool isLeft, out float y)
		{
			y = 0;

			if (isLeft)
			{
				if (startingLeftYs.Count == 0)
				{
					Die();

					return false;
				}

				int index = Random.Range(0, startingLeftYs.Count);

				y = startingLeftYs[index];

				startingLeftYs.RemoveAt(index);
			}
			else
			{
				if (startingRightYs.Count == 0)
				{
					Die();

					return false;
				}

				int index = Random.Range(0, startingRightYs.Count);

				y = startingRightYs[index];

				startingRightYs.RemoveAt(index);
			}

			return true;
		}

		private void InitializeYs(List<float> startingYs, bool isLeft)
		{
			for (int i = 0; ; i++)
			{
				float y = (float)i * spacing * 2f - (isLeft ? leftYOffset : 0);

				if (y > ParentRectT.rect.height / 2f)
				{
					break;
				}

				if (i != 0)
				{
					startingYs.Add(-y);
				}

				startingYs.Add(y);
			}
		}

		#endregion
	}
}
