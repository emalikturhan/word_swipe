using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet
{
	[RequireComponent(typeof(Button))]
	public class ButtonSound : MonoBehaviour
	{
		#region Inspector Variables

		public string soundId;

		#endregion

		#region Unity Methods

		private void Awake()
		{
			gameObject.GetComponent<Button>().onClick.AddListener(PlaySound);
		}

		#endregion

		#region Private Methods

		private void PlaySound()
		{
			if (SoundManager.Exists())
			{
				SoundManager.Instance.Play(soundId);
			}
		}

		#endregion
	}
}
