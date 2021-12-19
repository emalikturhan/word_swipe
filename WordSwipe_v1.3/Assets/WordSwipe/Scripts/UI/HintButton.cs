using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class HintButton : MonoBehaviour
	{
		#region Inspector Variables

		public string		hintId;
		public GameObject	costContainer;
		public Text		costText;
		public GameObject amountContainer;
		public Text		amountText;

		#endregion

		#region Member Variables

		private HintInfo hintInfo;

		#endregion

		#region Unity Methods

		private void Start()
		{
			hintInfo = GameManager.Instance.GetHintInfo(hintId);

			if (hintInfo == null)
			{
				Debug.LogError("[HintButton] There is no hint info with id: " + hintId);
			}

			UpdateUI();

			GameManager.Instance.OnHintAmountUpdated += (string usedHintId) => { if (hintId == usedHintId) UpdateUI(); };
		}

		#endregion

		#region Private Methods

		private void UpdateUI()
		{
			if (hintInfo != null)
			{
				int hintAmount = GameManager.Instance.HintAmounts[hintId];

				costContainer.SetActive(hintAmount <= 0);
				amountContainer.SetActive(hintAmount > 0);

				costText.text	= hintInfo.cost.ToString();
				amountText.text	= hintAmount.ToString();
			}
		}

		#endregion
	}
}
