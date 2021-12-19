using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

namespace Bimbimnet.WordBlocks
{
	[RequireComponent(typeof(Button))]
	public class RewardAdButton : MonoBehaviour
	{
		#region Properties

		public Button Button { get { return gameObject.GetComponent<Button>(); } }

		#endregion

		#region Unity Methods

		private void Awake()
		{
			Button.onClick.AddListener(OnClick);
		}

        private void Start()
        {
            Invoke("AddEvent", 0.3f);
        }

        private void AddEvent()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (AdmobController.instance.rewardBasedVideo != null)
            {
                AdmobController.instance.rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (AdmobController.instance.rewardBasedVideo != null)
            {
                AdmobController.instance.rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            }
#endif
        }

        #endregion

        #region Private Methods

        private void OnClick()
		{
            if (IsAdAvailable())
            {
#if UNITY_EDITOR
                HandleRewardBasedVideoRewarded(null, null);
#else
                AdmobController.instance.ShowRewardBasedVideo();
#endif
            }
        }

        private void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            int amount = GameManager.Instance.watchAdReward;
            Debug.Log("AMOUNT : " + amount);

            // Give the coins right away
            GameManager.Instance.AddCoins(amount, false);

            // Show the popup to the user so they know they got the coins
            PopupManager.Instance.Show("reward_ad_granted", new object[] { amount });
        }

        private bool IsAdAvailable()
        {
            if (AdmobController.instance.rewardBasedVideo == null) return false;
            bool isLoaded = AdmobController.instance.rewardBasedVideo.IsLoaded();
            return isLoaded;
        }
		#endregion
	}
}
