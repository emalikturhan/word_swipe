using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class ConnectionSpawner : ObjectSpawner
	{
		#region Inspector Variables

		public float	dotLineThreshold1;
		public float	dotLineThreshold2;
		public UILine	uiLinePrefab;

		#endregion

		#region Member Variables

		private ObjectPool					uiLinePool;
		private List<SpawnObject>			spawnedObjs;
		private Dictionary<string, UILine>	activeUiLines;

		#endregion

		#region Properties

		#endregion

		#region Unity Methods

		protected override void Start()
		{
			base.Start();

			uiLinePool = new ObjectPool(uiLinePrefab.gameObject, 0, transform, ObjectPool.PoolBehaviour.CanvasGroup);

			spawnedObjs		= new List<SpawnObject>();
			activeUiLines	= new Dictionary<string, UILine>();
		}

		private void LateUpdate()
		{
			for (int i = 0; i < spawnedObjs.Count; i++)
			{
				SpawnObject obj1 = spawnedObjs[i];

				for (int j = i + 1; j < spawnedObjs.Count; j++)
				{
					SpawnObject obj2 = spawnedObjs[j];

					// Get the distance between the two dots
					float dist = Vector2.Distance(obj1.RectT.anchoredPosition, obj2.RectT.anchoredPosition);

					// Get the key to look up the line between the dots if one exists
					string lineKey = i + "_" + j;

					// Check if the dots are close enough to create a line
					if (dist < dotLineThreshold1)
					{
						// If there is not alreay a line between these dots get one
						if (!activeUiLines.ContainsKey(lineKey))
						{
							activeUiLines.Add(lineKey, uiLinePool.GetObject<UILine>());
						}

						UILine uILine = activeUiLines[lineKey];

						// Calcualte the alpha for the line based on the distance of the dots and the alpha of the dots
						float lineAlpha = (dotLineThreshold1 - dist) / (dotLineThreshold1 - dotLineThreshold2);

						Utilities.SetAlpha(uILine, lineAlpha * obj1.CG.alpha * obj2.CG.alpha);

						if (uILine.LinePoints.Count == 0)
						{
							uILine.LinePoints.Add(obj1.RectT.anchoredPosition);
							uILine.LinePoints.Add(obj2.RectT.anchoredPosition);
						}
						else
						{
							uILine.LinePoints[0] = obj1.RectT.anchoredPosition;
							uILine.LinePoints[1] = obj2.RectT.anchoredPosition;
						}

						uILine.LinePointsUpdated();
					}
					else if (activeUiLines.ContainsKey(lineKey))
					{
						ObjectPool.ReturnObjectToPool(activeUiLines[lineKey].gameObject);
						activeUiLines.Remove(lineKey);
					}
				}
			}
		}

		#endregion

		#region Public Methods

		#endregion

		#region Protected Methods

		protected override void SpawnObject()
		{
			bool instantiated;

			SpawnObject obj = spawnObjectPool.GetObject<SpawnObject>(out instantiated);

			obj.Spawned();

			if (instantiated)
			{
				spawnedObjs.Add(obj);
			}
		}

		#endregion

		#region Private Methods

		#endregion
	}
}
