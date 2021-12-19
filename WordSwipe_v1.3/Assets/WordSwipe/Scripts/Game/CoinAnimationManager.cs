using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
    public class CoinAnimationManager : SingletonComponent<CoinAnimationManager>
    {
        #region Inspector Variables

        public Text coinsText = null;
        public RectTransform animationContainer = null;
        public RectTransform animateToMarker = null;
        public RectTransform extraWordsMarker = null;
        public RectTransform coinPrefab = null;
        public int amountPerCoin = 0;
        public float animationDuration = 0;
        public float explodeAnimationDuration = 0;
        public float delayBeforeExtraWordsCoins = 0;
        public float explodeForceOffset = 0;

        #endregion

        #region Member Variables

        private ObjectPool coinPool;

        private int numCoinsAnimating;
        private int animCoinsAmount;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            coinPool = new ObjectPool(coinPrefab.gameObject, 1, ObjectPool.CreatePoolContainer(transform));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Increments the Coins by the give amount
        /// </summary>
        public void SetCoinsText(int coins)
        {
            coinsText.text = coins.ToString();
        }

        /// <summary>
        /// Animates the extra words coins.
        /// </summary>
        public void AnimateExtraWordsCoins(int fromCoinAmount, int toCoinAmount)
        {
            AnimateCoins(fromCoinAmount, toCoinAmount, extraWordsMarker, 0, delayBeforeExtraWordsCoins);
        }

        /// <summary>
        /// Animates coins to the coin container
        /// </summary>
        public void AnimateCoins(int fromCoinAmount, int toCoinAmount, RectTransform fromRect, float explodeForce, float startDelay = 0)
        {
            if (numCoinsAnimating == 0)
            {
                animCoinsAmount = fromCoinAmount;
            }

            int numCoins = Mathf.CeilToInt(((float)toCoinAmount - (float)fromCoinAmount) / (float)amountPerCoin);

            numCoinsAnimating += numCoins;

            for (int i = 1; i <= numCoins; i++)
            {
                StartCoroutine(AnimateCoin(fromRect, startDelay, explodeForce));
            }

            SoundManager.Instance.Play("coins-awarded", false, startDelay);
        }

        #endregion

        #region Private Methods

        private IEnumerator AnimateCoin(RectTransform fromRect, float startDelay, float explodeForce)
        {
            yield return new WaitForSeconds(startDelay);

            RectTransform coinToAnimate = coinPool.GetObject<RectTransform>(animationContainer);

            UIAnimation.DestroyAllAnimations(coinToAnimate.gameObject);

            coinToAnimate.anchoredPosition = Utilities.SwitchToRectTransform(fromRect, animationContainer);
            coinToAnimate.sizeDelta = fromRect.sizeDelta;

            yield return ExplodeCoinOut(coinToAnimate, explodeForce);

            Vector2 toPosition = Utilities.SwitchToRectTransform(animateToMarker, animationContainer);

            float duration = animationDuration + Random.Range(-0.1f, 0.1f);

            PlayAnimation(UIAnimation.PositionX(coinToAnimate, toPosition.x, duration));
            PlayAnimation(UIAnimation.PositionY(coinToAnimate, toPosition.y, duration));

            PlayAnimation(UIAnimation.Width(coinToAnimate, animateToMarker.sizeDelta.x, duration));
            PlayAnimation(UIAnimation.Height(coinToAnimate, animateToMarker.sizeDelta.y, duration));

            SoundManager.Instance.Play("coin", false, duration - 0.1f);

            yield return new WaitForSeconds(duration);

            IncCoinTextForAnimation();
        }

        private IEnumerator ExplodeCoinOut(RectTransform coinToAnimate, float explodeForce)
        {
            Vector2 randDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            Vector2 toPosition = coinToAnimate.anchoredPosition + randDir * (explodeForce + Random.Range(0, explodeForceOffset));

            UIAnimation anim;

            anim = UIAnimation.PositionX(coinToAnimate, toPosition.x, explodeAnimationDuration + Random.Range(-0.05f, 0.05f));
            anim.style = UIAnimation.Style.EaseOut;
            anim.Play();

            anim = UIAnimation.PositionY(coinToAnimate, toPosition.y, explodeAnimationDuration + Random.Range(-0.05f, 0.05f));
            anim.style = UIAnimation.Style.EaseOut;
            anim.Play();

            while (anim.IsPlaying)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Sets up and plays the UIAnimation for a coin
        /// </summary>
        private UIAnimation PlayAnimation(UIAnimation anim)
        {
            anim.style = UIAnimation.Style.EaseIn;

            anim.OnAnimationFinished += (GameObject target) =>
            {
                ObjectPool.ReturnObjectToPool(target);
            };

            anim.Play();

            return anim;
        }

        private void IncCoinTextForAnimation()
        {
            numCoinsAnimating--;
            animCoinsAmount += amountPerCoin;

            if (numCoinsAnimating == 0 || animCoinsAmount > GameManager.Instance.NumCoins)
            {
                SetCoinsText(GameManager.Instance.NumCoins);
            }
            else
            {
                SetCoinsText(animCoinsAmount);
            }
        }

        #endregion
    }
}
