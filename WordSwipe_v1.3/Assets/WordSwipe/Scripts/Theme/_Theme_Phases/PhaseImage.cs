using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	[RequireComponent(typeof(Image))]
	public class PhaseImage : MonoBehaviour
	{
		#region Inspector Variables

		public List<Color>	colors;
		public float			startDelay;
		public float			phaseDuration;
		public float			stayDuration;
		public AnimationCurve	animationCurve;

		#endregion

		#region Member Variables
		private Image image;
		private Color lastColor;

		#endregion

		#region Unity Methods

		private void Start()
		{
			image = gameObject.GetComponent<Image>();

			Color firstColor = colors[Random.Range(0, colors.Count)];

			image.color = firstColor;

			lastColor = firstColor;

			PhaseToNextColor(startDelay);
		}

		private void PhaseToNextColor(float delay = 0)
		{
			Color nextColor = PickRandColor();

			lastColor = nextColor;

			UIAnimation anim = UIAnimation.Color(image, nextColor, phaseDuration);

			anim.startDelay		= stayDuration + delay;
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= animationCurve;

			anim.OnAnimationFinished += (GameObject obj) => 
			{
				PhaseToNextColor();
			};

			anim.Play();
		}

		private Color PickRandColor()
		{
			List<Color> temp = new List<Color>(colors);

			temp.Remove(lastColor);

			return temp[Random.Range(0, temp.Count)];
		}

		#endregion
	}
}
