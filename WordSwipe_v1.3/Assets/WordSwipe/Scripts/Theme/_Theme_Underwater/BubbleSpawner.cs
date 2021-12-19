using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class BubbleSpawner : ObjectSpawner
	{
		#region Inspector Variables

		public int	minBubbleGroupSize;
		public int	maxBubbleGroupSize;
		public float	minBubbleOffset;
		public float	maxBubbleOffset;

		#endregion

		#region Protected Methods

		protected override void SpawnObject()
		{
			int numToSpawn = Random.Range(minBubbleGroupSize, maxBubbleGroupSize);

			float groupX = Random.Range(-RectT.rect.width / 2f, RectT.rect.width / 2f);
			float groupY = -RectT.rect.height / 2f - maxBubbleOffset * 2f;

			Vector2 groupPosition = new Vector2(groupX, groupY);

			for (int i = 0; i < numToSpawn; i++)
			{
				float	offset			= Random.Range(minBubbleOffset, maxBubbleOffset);
				Vector2 randDir			= new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
				Vector2 bubblePosition	= groupPosition + randDir * offset;

				SpawnObject obj = spawnObjectPool.GetObject<SpawnObject>();

				obj.RectT.anchoredPosition = bubblePosition;

				obj.Spawned();
			}
		}

		#endregion
	}
}
