using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class SettingsPopup : Popup
	{
		#region Inspector Variables

		[Space]

		public ToggleSlider	musicToggle = null;
		public ToggleSlider	soundToggle = null;

		#endregion

        public void OnPolicyClick()
        {
			Application.OpenURL("https://www.google.com/");
        }

		#region Unity Methods

		private void Start()
		{
			musicToggle.SetToggle(SoundManager.Instance.IsMusicOn, false);
			soundToggle.SetToggle(SoundManager.Instance.IsSoundEffectsOn, false);

			musicToggle.OnValueChanged += OnMusicValueChanged;
			soundToggle.OnValueChanged += OnSoundEffectsValueChanged;
		}

		#endregion

		#region Private Methods

		private void OnMusicValueChanged(bool isOn)
		{
			SoundManager.Instance.SetSoundTypeOnOff(SoundManager.SoundType.Music, isOn);
		}

		private void OnSoundEffectsValueChanged(bool isOn)
		{
			SoundManager.Instance.SetSoundTypeOnOff(SoundManager.SoundType.SoundEffect, isOn);
		}

		#endregion
	}
}
