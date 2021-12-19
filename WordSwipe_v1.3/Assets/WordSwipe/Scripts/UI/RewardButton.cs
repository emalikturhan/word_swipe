using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
namespace Bimbimnet.WordBlocks
{
    public class RewardButton : MonoBehaviour
    {
       
        public void OnClick()
        {
            //Invoke("AddEvent", 0.3f);
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
            //Debug.Log("AMOUNT : " + amount);

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
        
    }
}

