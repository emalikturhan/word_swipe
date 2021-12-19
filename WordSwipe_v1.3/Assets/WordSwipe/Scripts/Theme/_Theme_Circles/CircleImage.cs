using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	[RequireComponent(typeof(Image))]
	public class CircleImage : SpawnObject
	{
		#region Inspector Variables

		public Color		color1;
		public Color		color2;
		public Vector2	sizeRange;
		public Vector2	speedRange;
		public Vector2	durationRange;

		#endregion

		#region Properties

		private Image Image { get { return gameObject.GetComponent<Image>(); } }

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			base.Spawned();

			UIAnimation.DestroyAllAnimations(gameObject);

			Color fromColor	= GetRandomColor();
			Color toColor	= GetRandomColor();

			Image.color = fromColor;

			float size		= Random.Range(sizeRange.x, sizeRange.y);
			float speed		= Random.Range(speedRange.x, speedRange.y);
			float duration	= Random.Range(durationRange.x, durationRange.y);

			RectT.sizeDelta = new Vector2(size, size);

			float fadeDuration = duration / 5f;

			// Fade it in
			FadeIn(fadeDuration);

			// Move it in a random direction
			float startX = Random.Range(-ParentRectT.rect.width / 2f, ParentRectT.rect.width / 2f);
			float startY = Random.Range(-ParentRectT.rect.height / 2f, ParentRectT.rect.height / 2f);
			Vector2 v = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * speed;

			RectT.anchoredPosition = new Vector2(startX, startY);

			UIAnimation.PositionX(RectT, startX + v.x, duration + fadeDuration).Play();
			UIAnimation.PositionY(RectT, startY + v.y, duration + fadeDuration).Play();

			// Start the color transition
			UIAnimation colorAnim = UIAnimation.Color(Image, fromColor, toColor, duration);

			colorAnim.OnAnimationFinished += (GameObject obj) => 
			{
				FadeOut(fadeDuration);
			};

			colorAnim.Play();
		}

		#endregion

		#region Private Methods

		private Color GetRandomColor()
		{
			return Color.Lerp(color1, color2, Random.Range(0f, 1f));
		}

		#endregion
	}
}
