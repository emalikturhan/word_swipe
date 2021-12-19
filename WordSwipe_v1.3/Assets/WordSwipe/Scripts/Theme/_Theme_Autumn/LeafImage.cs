using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class LeafImage : SpawnObject
	{
		#region Inspector Variables

		public Image			image;
		public List<Sprite>	leafSprites;
		public float			minScale;
		public float			maxScale;
		public float			rotationAmount;
		public float			rotationDuration;
		public AnimationCurve	rotationAnimCurve;
		public float			fallDuration;
		public float			fallOffset;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			float scale		= Random.Range(minScale, maxScale);
			float rotAmt	= rotationAmount * (1f / scale);
			float rotDur	= rotationDuration * (1f / scale);
			float fallDur	= fallDuration * (1f / scale);

			image.sprite						= leafSprites[Random.Range(0, leafSprites.Count)];
			image.transform.localScale			= new Vector3(scale, scale, 1f);
			image.transform.localEulerAngles	= new Vector3(0, 0, Random.Range(0, 360));
			image.SetNativeSize();

			float startX	= Random.Range((-ParentRectT.rect.width + RectT.rect.width) / 2f,
			                               (ParentRectT.rect.width - RectT.rect.width) / 2f);
			float endX		= startX + Random.Range(0f, fallOffset);
			float startY	= RectT.rect.height + ParentRectT.rect.height / 2f;
			float endY		= -RectT.rect.height - ParentRectT.rect.height / 2f;

			RectT.anchoredPosition = new Vector2(startX, startY);

			UIAnimation anim;

			anim				= UIAnimation.RotationZ(RectT, rotAmt, -rotAmt, rotDur);
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= rotationAnimCurve;
			anim.loopType		= UIAnimation.LoopType.Reverse;
			anim.Play();

			anim = UIAnimation.PositionX(RectT, startX, endX, fallDur);
			anim.Play();

			anim = UIAnimation.PositionY(RectT, startY, endY, fallDur);
			anim.OnAnimationFinished += (GameObject obj) => { Die(); };
			anim.Play();
		}

		#endregion
	}
}
