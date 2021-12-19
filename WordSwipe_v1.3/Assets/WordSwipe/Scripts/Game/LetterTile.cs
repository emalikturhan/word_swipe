using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class LetterTile : UIMonoBehaviour
	{
		#region Inspector Variables

		[Header("Base")]
		[SerializeField] protected GameObject	letterTileObj	= null;
		[SerializeField] protected Text			letterText		= null;
		[SerializeField] protected Image		bkgImage		= null;

		#endregion

		#region Properties

		public Text LetterText { get { return letterText; } }

		#endregion

		#region Public Methods

		public virtual void Setup(char letter)
		{
			letterText.text = letter.ToString();

			if (letter == '\0')
			{
				SetBlank();
			}
			else
			{
				SetShown();
			}
		}

		public virtual void SetBlank()
		{
			letterTileObj.SetActive(false);
		}

		public virtual void SetShown()
		{
			letterTileObj.SetActive(true);
		}

		#endregion
	}
}
