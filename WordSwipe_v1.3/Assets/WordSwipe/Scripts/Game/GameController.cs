using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bimbimnet.WordBlocks
{
	public class GameController : MonoBehaviour
	{
		#region Classes

		public class SelectableWord
		{
			public string	word;
			public int		startRow;
			public int		startCol;
			public bool		isVertical;
			public bool		isReversed;

			public override string ToString()
			{
				return string.Format("{0} row:{1} col{2} isVert:{3} isRev:{4}", word, startRow, startCol, isVertical, isReversed);
			}
		}

		#endregion

		#region Inspector Variables

		[Header("Components")]
		public RectTransform			boardContainer			= null;
		public WordListHandler		wordListHandler			= null;
		public SelectedWordHandler	selectedWordHandler		= null;
		public GridHandler			gridHandler				= null;
		public LevelCompleteHandler	levelCompleteHandler	= null;
		public Text					levelNumberText			= null;
		public Text					levelHintText			= null;
		public RectTransform			extraWordsMarker		= null;
		public ExtraWordsButton		extraWordsButton		= null;

		[Space]

		[Header("Anim Settings - Grid Letters")]
		public float letterAnimDuration	= 0;
		public float delayBetweenLetters	= 0;

		[Space]

		[Header("Anim Settings - NextLevel")]
		public float fadeDuration	= 0;

		#endregion

		#region Member Variables

		private Transform				animationContainer;
		private ObjectPool				tileLetterTextPool;

		private LevelData				activeLevelData;
		private LevelSaveData			activeLevelSaveData;
		private bool					activeLevelComplete;

		private List<SelectableWord>	selectableWords;

		private int levelCompleteCount = 0;

		#endregion

		#region Public Methods

		public void Initialize()
		{
			selectableWords = new List<SelectableWord>();

			wordListHandler.Initialize();
			selectedWordHandler.Initialize();
			gridHandler.Initialize();

			gridHandler.OnSelectedWordChanged	+= OnGridHandlerSelectedWordChanged;
			gridHandler.OnWordSelected			+= OnGridHandlerWordSelected;

			CreateAnimationContainer();
		}

		public void SetupGame(LevelData levelData, LevelSaveData levelSaveData)
		{
			activeLevelData		= levelData;
			activeLevelSaveData	= levelSaveData;
			activeLevelComplete	= false;

			// Set the level number and hint text
			levelNumberText.text	= "Level " + levelData.LevelNumber;
			levelHintText.text		= " - " + levelData.Hint + " - ";

			gridHandler.enabled = true;

			// Need to clear the grid first so the layout adjusts it size properly
			gridHandler.Clear();

			// Make sure the complete handler is hidden
			levelCompleteHandler.Hide();

			// Setup the WordListHandler first
			wordListHandler.Setup(levelData, levelSaveData);

			// Forst the board container to rebuild, this will set the grid handlers size right away
			LayoutRebuilder.ForceRebuildLayoutImmediate(boardContainer);

			if (levelSaveData.boardData == null)
			{
				// If the level save data doesnt have a board then create one and setup the grid handler when its done
				CreateInitialBoardData(levelData);
			}
			else
			{
				// Get the list of selectable words
				UpdateSelectableWords();

				// Setup the grid handler
				gridHandler.Setup(levelSaveData);
			}
		}

		public bool ShuffleBoard()
		{
			if (activeLevelComplete)
			{
				return false;
			}

			List<string> remainingWords = new List<string>();

			// Get all the remaining words on teh board
			for (int i = 0; i < activeLevelData.Words.Count; i++)
			{
				string word = activeLevelData.Words[i];

				if (!activeLevelSaveData.foundLevelWords.Contains(word))
				{
					remainingWords.Add(word);
				}
			}

			// Create a new board data
			BoardCreator.CreateBoardAsync(remainingWords, activeLevelSaveData.boardData.rows, activeLevelSaveData.boardData.cols, OnNewBoardCreated);

			return true;
		}

		/// <summary>
		/// Tries to show a letter in the word list, returns false if no hints where shown
		/// </summary>
		public bool ShowLetterHint()
		{
			if (activeLevelComplete)
			{
				return false;
			}

			SelectableWord	hintWord;
			int 			hintIndex;

			if (SelectHint(activeLevelSaveData.letterHints, out hintWord, out hintIndex))
			{
				// Show the letter as a hint on the word list
				wordListHandler.ShowLetterAsHint(hintWord.word, hintIndex);

				return true;
			}

			// No hint was shown
			return false;
		}

		public bool ShowTileHint()
		{
			if (activeLevelComplete)
			{
				return false;
			}

			SelectableWord	hintWord;
			int 			hintIndex;

			if (SelectHint(activeLevelSaveData.tileHints, out hintWord, out hintIndex))
			{
				gridHandler.ShowHint(hintWord, hintIndex);

				return true;
			}

			// No hint was shown
			return false;
		}

		/// <summary>
		/// Starts the next level after a completed level
		/// </summary>
		public void NextLevel()
		{
            SoundManager.Instance.Play("btn-click");

            StartCoroutine(NextLevelAnimations());
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates the Board, calls OnBoardCreated when the board is somplete
		/// </summary>
		private void CreateInitialBoardData(LevelData levelData)
		{
			int cols = levelData.MaxWordLength;
			int rows = Mathf.Max(levelData.Words.Count, levelData.MaxWordLength + 2);

			BoardCreator.CreateBoardAsync(levelData.Words, rows, cols, OnBoardCreated);
		}

		/// <summary>
		/// Invoked when BoardCreator.CreateBoardAsync has finished
		/// </summary>
		private void OnBoardCreated(BoardData boardData)
		{
			activeLevelSaveData.boardData = boardData;

			UpdateSelectableWords();

			gridHandler.Setup(activeLevelSaveData);
		}

		/// <summary>
		/// Invoked when BoardCreator.CreateBoardAsync has finished
		/// </summary>
		private void OnNewBoardCreated(BoardData boardData)
		{
			activeLevelSaveData.boardData = boardData;

			UpdateSelectableWords();

			gridHandler.NotifyNewBoardData();
		}

		/// <summary>
		/// Invoked when the selected word changes
		/// </summary>
		private void OnGridHandlerSelectedWordChanged(string word)
		{
			selectedWordHandler.SetSelectedLetters(word);
		}

		/// <summary>
		/// Invoked when the player selects a word on the GridHandler
		/// </summary>
		private void OnGridHandlerWordSelected(string word, List<int[]> gridCellPositions)
		{
			if (IsWordInLevel(word))
			{
				HandleWordInLevelSelected(word, gridCellPositions);
			}
			else if (IsExtraWord(word))
			{
				HandleExtraWordSelected(word, gridCellPositions);
			}
			else
			{
				HandleInvalidWordSelected(gridCellPositions);
			}
		}

		/// <summary>
		/// Handles a selected word that is part of the level
		/// </summary>
		private void HandleWordInLevelSelected(string word, List<int[]> gridCellPositions)
		{
			selectedWordHandler.FadeOut();

			// Check if the word has already been found
			if (IsWordFound(word))
			{
				gridHandler.NotifyWordAlreadyFound(gridCellPositions);

				// Play the word already found sound
				SoundManager.Instance.Play("word_already_found");
				
				return;
			}

			// Set the word as found in the save data
			activeLevelSaveData.foundLevelWords.Add(word);

			// Aniamte the letters from the grid to the word list
			AnimateLettersToWordList(word, gridCellPositions);

			// Notify the word list handler and the grid handler that a word has been found
			wordListHandler.NotifyWordFound(word);
			gridHandler.NotifyWordFound(gridCellPositions);

			// Play the word found sound
			SoundManager.Instance.Play("word_correct");

			// Check if the level is complete
			if (IsLevelComplete())
			{
				
				
				AdmobController.instance.ShowInterstitial();
			
				activeLevelComplete = true;

				bool nextLevelCompleted = GameManager.Instance.LevelCompleted(activeLevelData.LevelNumber);

				levelCompleteHandler.ShowLevelComplete(activeLevelData.LevelNumber, nextLevelCompleted);

                gridHandler.enabled = false;
			}
			else
			{
				// GridHandler will update the activeLevelSaveData.boardData when we call NotifyWordFound and it removes it from the board so we need to update the
				// selectedable words now
				UpdateSelectableWords();

				// If there are no selectable words then we need to reshuffle the board
				if (selectableWords.Count == 0)
				{
					ShuffleBoard();
				}
				else
				{
					gridHandler.UpdateTileHints(true);
				}
			}

            
		}

		/// <summary>
		/// Handles a selected word that is an extra word and not in the level
		/// </summary>
		private void HandleExtraWordSelected(string word, List<int[]> gridCellPositions)
		{
			selectedWordHandler.FadeOut();

			// Check if the extra word has already been found
			if (IsWordFound(word))
			{
				gridHandler.NotifyWordAlreadyFound(gridCellPositions);

				extraWordsButton.Pulse();

				// Play the word already found sound
				SoundManager.Instance.Play("word_already_found");

				return;
			}
			
			// Set the word as found
			GameManager.Instance.ExtraWordFound(word);

			// Animate the letters from the grid to the extra words marker
			AnimateLettersToExtraWords(word, gridCellPositions);

			// Play the extra word found sound
			SoundManager.Instance.Play("word_extra");
		}

		/// <summary>
		/// Handles a selected word that is not valid (Not in the level and not an extra word)
		/// </summary>
		private void HandleInvalidWordSelected(List<int[]> gridCellPositions)
		{
			selectedWordHandler.ShakeAndFadeOut();
			gridHandler.NotifyWordNotValid(gridCellPositions);

			// Play the invalid word sound
			SoundManager.Instance.Play("word_invalid");
		}

		/// <summary>
		/// Checks if the active level is complete
		/// </summary>
		private bool IsLevelComplete()
		{
			for (int i = 0; i < activeLevelData.Words.Count; i++)
			{
				if (!activeLevelSaveData.foundLevelWords.Contains(activeLevelData.Words[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Animates the letters for the LetterTiles to the word on the wordlist
		/// </summary>
		private void AnimateLettersToWordList(string word, List<int[]> gridPositions)
		{
			List<GridLetterTile>	gridLetterTiles = gridHandler.GetLetterTiles(gridPositions);
			List<LetterTile>		wordLetterTiles = wordListHandler.GetLetterTiles(word);

			// Sanity check
			if (word.Length == 0 || word.Length != gridLetterTiles.Count || word.Length != wordLetterTiles.Count)
			{
				Debug.LogError("[GridController] AnimateLettersToWordList: The word length does not match!!");
				return;
			}

			// If a pool does not exist yet then this is the first time we are animating letters, create the pool now
			if (tileLetterTextPool == null)
			{
				CreateTileLetterTextPool(gridLetterTiles[0]);
			}

			for (int i = 0; i < gridLetterTiles.Count; i++)
			{
				GridLetterTile	gridLetterTile	= gridLetterTiles[i];
				LetterTile		wordLetterTile	= wordLetterTiles[i];
				Text			letterText		= tileLetterTextPool.GetObject<Text>(animationContainer);

				// Set the text and position to match the one we are about to animate
				letterText.text					= gridLetterTile.LetterText.text;
				letterText.transform.position	= gridLetterTile.LetterText.transform.position;
				letterText.transform.localScale	= gridLetterTile.LetterText.transform.lossyScale;

				// Get the position of the wordLetterTile in the animation container. We do this by moving it to the animation container, getting the
				// anchoredPosition, then moving it back to its original parent in the word list
				Transform origParent = wordLetterTile.transform.parent;			// Get a reference to the original parent
				wordLetterTile.transform.SetParent(animationContainer, true);	// Move it to the animation container
				Vector2 toPosition = wordLetterTile.RectT.anchoredPosition;		// Gets its position in the animation container
				Vector2 toScale = wordLetterTile.transform.localScale;			// Gets its scale in the animation container
				wordLetterTile.transform.SetParent(origParent, true);			// Move it back

				RectTransform	letterTextRectT	= letterText.transform as RectTransform;
				float			startDelay		= i * delayBetweenLetters;

				toScale *= ((float)wordLetterTile.LetterText.fontSize / (float)letterText.fontSize);

				UIAnimation.DestroyAllAnimations(letterTextRectT.gameObject);

				PlayLetterAnim(UIAnimation.PositionX(letterTextRectT, toPosition.x, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.PositionY(letterTextRectT, toPosition.y, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.ScaleX(letterTextRectT, toScale.x, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.ScaleY(letterTextRectT, toScale.y, letterAnimDuration), startDelay, true);
			}
		}

		/// <summary>
		/// Animates the letters for the LetterTiles to the word on the wordlist
		/// </summary>
		private void AnimateLettersToExtraWords(string word, List<int[]> gridPositions)
		{
			List<GridLetterTile> gridLetterTiles = gridHandler.GetLetterTiles(gridPositions);

			// Sanity check
			if (word.Length == 0 || word.Length != gridLetterTiles.Count)
			{
				Debug.LogError("[GridController] AnimateLettersToExtraWords: The word length does not match!!");
				return;
			}

			// If a pool does not exist yet then this is the first time we are animating letters, create the pool now
			if (tileLetterTextPool == null)
			{
				CreateTileLetterTextPool(gridLetterTiles[0]);
			}

			for (int i = 0; i < gridLetterTiles.Count; i++)
			{
				GridLetterTile	gridLetterTile	= gridLetterTiles[i];
				Text			letterText		= tileLetterTextPool.GetObject<Text>(animationContainer);

				// Set the text and position to match the one we are about to animate
				letterText.text					= gridLetterTile.LetterText.text;
				letterText.transform.position	= gridLetterTile.LetterText.transform.position;
				letterText.transform.localScale	= gridLetterTile.LetterText.transform.lossyScale;

				Transform origParent = extraWordsMarker.parent;					// Get a reference to the original parent
				extraWordsMarker.SetParent(animationContainer, true);			// Move it to the animation container
				Vector2 toPosition = extraWordsMarker.anchoredPosition;		// Gets its position in the animation container
				Vector2 toScale = extraWordsMarker.localScale;			// Gets its scale in the animation container
				extraWordsMarker.SetParent(origParent, true);			// Move it back

				RectTransform	letterTextRectT	= letterText.transform as RectTransform;
				float			startDelay		= i * delayBetweenLetters;

				float fadeDuration		= letterAnimDuration / 3f;
				float fadeStartDelay	= startDelay + letterAnimDuration - fadeDuration;

				UIAnimation.DestroyAllAnimations(letterTextRectT.gameObject);

				PlayLetterAnim(UIAnimation.PositionX(letterTextRectT, toPosition.x, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.PositionY(letterTextRectT, toPosition.y, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.ScaleX(letterTextRectT, toScale.x, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.ScaleY(letterTextRectT, toScale.y, letterAnimDuration), startDelay);
				PlayLetterAnim(UIAnimation.Alpha(letterTextRectT.gameObject, 0f, fadeDuration), fadeStartDelay, true);
			}
		}

		/// <summary>
		/// Plays a letter text animation
		/// </summary>
		private void PlayLetterAnim(UIAnimation anim, float startDelay, bool returnToPoolWhenDone = false)
		{
			anim.style				= UIAnimation.Style.EaseOut;
			anim.startOnFirstFrame	= true;
			anim.startDelay			= startDelay;

			if (returnToPoolWhenDone)
			{
				anim.OnAnimationFinished += (GameObject obj) => 
				{
					ObjectPool.ReturnObjectToPool(obj);
				};
			}

			anim.Play();
		}

		/// <summary>
		/// Checks if the word is part of the active level
		/// </summary>
		private bool IsWordInLevel(string word)
		{
			return activeLevelData.IsWordInLevel(word);
		}
		
		/// <summary>
		/// Checks if the word is an extra word
		/// </summary>
		private bool IsExtraWord(string word)
		{
			return GameManager.Instance.ExtraWords.Contains(word);
		}

		/// <summary>
		/// Checks if the word has been found for the active level
		/// </summary>
		private bool IsWordFound(string word)
		{
			return activeLevelSaveData.foundLevelWords.Contains(word) || activeLevelSaveData.foundExtraWords.Contains(word);
		}

		/// <summary>
		/// Updates the list of selectable words on the current board
		/// </summary>
		private void UpdateSelectableWords()
		{
			selectableWords.Clear();

			Dictionary<char, List<string>> remainingWords			= new Dictionary<char, List<string>>();
			Dictionary<char, List<string>> remainingWordsReversed	= new Dictionary<char, List<string>>();

			// Get a list of words that havent been found yet
			for (int i = 0; i < activeLevelData.Words.Count; i++)
			{
				string word = activeLevelData.Words[i];

				if (!activeLevelSaveData.foundLevelWords.Contains(word))
				{
					// Add the word to remainingWords using the first letter as the key
					char firstLetter = word[0];

					if (!remainingWords.ContainsKey(firstLetter))
					{
						remainingWords.Add(firstLetter, new List<string>());
					}

					remainingWords[firstLetter].Add(word);

					// Add the word to remainingWordsReversed using the last letter as the key
					char lastLetter = word[word.Length - 1];

					if (!remainingWordsReversed.ContainsKey(lastLetter))
					{
						remainingWordsReversed.Add(lastLetter, new List<string>());
					}

					remainingWordsReversed[lastLetter].Add(word);
				}
			}

			// Look for those words on the board
			for (int row = 0; row < activeLevelSaveData.boardData.rows; row++)
			{
				for (int col = 0; col < activeLevelSaveData.boardData.cols; col++)
				{
					char letter = activeLevelSaveData.boardData.board[row][col];

					bool wordStart			= remainingWords.ContainsKey(letter);
					bool reverseWordStart	= remainingWordsReversed.ContainsKey(letter);

					if (wordStart || reverseWordStart)
					{
						// Check horizontally for any word
						CheckForWords(row, col, wordStart ? remainingWords : null, reverseWordStart ? remainingWordsReversed : null, false);

						// Update the values because we might have found a word which would remove it from the dictionary
						wordStart			= remainingWords.ContainsKey(letter);
						reverseWordStart	= remainingWordsReversed.ContainsKey(letter);

						if (wordStart || reverseWordStart)
						{
							// Check verticllay for any word
							CheckForWords(row, col, wordStart ? remainingWords : null, reverseWordStart ? remainingWordsReversed : null, true);
						}
					}
				}
			}

			// Get the GirdHandler a copy of the selectable words
			gridHandler.SetSelectableWords(selectableWords);
		}

		/// <summary>
		/// Checks for any of the words in remainingWords or remainingWordsReversed whos first letter is the letter at the given row/col on the board. This method
		/// assumes that the letter at row/col on the board has an entry in either remainingWords or remainingWordsReversed
		/// </summary>
		private void CheckForWords(int row, int col, Dictionary<char, List<string>> remainingWords, Dictionary<char, List<string>> remainingWordsReversed, bool isVertical)
		{
			// Get the frist letter then get a copy of the list of words and/orlist of reverse words
			char			firstLetter		= activeLevelSaveData.boardData.board[row][col];
			List<string>	words			= (remainingWords != null) ? new List<string>(remainingWords[firstLetter]) : null;
			List<string>	wordsReversed	= (remainingWordsReversed != null) ? new List<string>(remainingWordsReversed[firstLetter]) : null;

			// Get some for loop values for how to iterate over the rows/cols
			int startRow	= isVertical ? row + 1 : row;
			int startCol	= isVertical ? col : col + 1;
			int colInc		= isVertical ? 0 : 1;
			int rowInc		= isVertical ? 1 : 0;

			for (int c = startCol, r = startRow; c < activeLevelSaveData.boardData.cols && r < activeLevelSaveData.boardData.rows; c += colInc, r += rowInc)
			{
				// Get the current letter on the board we are checking for
				char boardLetter = activeLevelSaveData.boardData.board[r][c];

				// Get the index into the words we need to comare to boardLetter
				int letterIndex = isVertical ? r - row : c - col;

				string wordOnBoard;

				// Check for normal words first (CheckWords will remove elements from words and reverseWords if the boardLetter doesn't match)
				if (words != null && CheckWords(words, boardLetter, letterIndex, false, out wordOnBoard))
				{
					// We found a word so create a new entry in selectableWords
					CreateSelectableWord(wordOnBoard, row, col, isVertical, false);

					// Remove the word from both the normal and reverse lists
					RemoveWordHelper(wordOnBoard, firstLetter, remainingWords);
					RemoveWordHelper(wordOnBoard, firstLetter, remainingWordsReversed);

					// We found a word at row/col so stop looking
					return;
				}

				// Do the same as above but using the wordsReversed list
				if (wordsReversed != null && CheckWords(wordsReversed, boardLetter, letterIndex, true, out wordOnBoard))
				{
					CreateSelectableWord(wordOnBoard, row, col, isVertical, true);

					RemoveWordHelper(wordOnBoard, firstLetter, remainingWords);
					RemoveWordHelper(wordOnBoard, firstLetter, remainingWordsReversed);

					// We found a word at row/col so stop looking
					return;
				}

				// Check if there are still valid words to check for
				if ((words == null || words.Count == 0) && (wordsReversed == null || wordsReversed.Count == 0))
				{
					// No words are selectable starting at row/col so stop now
					return;
				}
			}
		}

		/// <summary>
		/// Goes through each word and checks if the boardLetter matchs the letter at the given letterIndex, if not it removes it from the words list. If
		/// the letter matches and its the last letter in the list then we return true and set wordOnBoard to that word
		/// </summary>
		private bool CheckWords(List<string> words, char boardLetter, int letterIndex, bool reverse, out string wordOnBoard)
		{
			for (int i = 0; i < words.Count; i++)
			{
				string	word		= words[i];
				char	wordLetter	= reverse ? word[word.Length - letterIndex - 1] : word[letterIndex];

				if (boardLetter != wordLetter)
				{
					words.RemoveAt(i);
					i--;

					continue;
				}

				if (letterIndex == word.Length - 1)
				{
					wordOnBoard = word;

					return true;
				}
			}

			wordOnBoard = null;

			return false;
		}

		/// <summary>
		/// Creates a new SelectableWord entry in selectableWords
		/// </summary>
		private void CreateSelectableWord(string word, int startRow, int startCol, bool isVertical, bool isReversed)
		{
			SelectableWord selectableWord = new SelectableWord();

			selectableWord.word			= word;
			selectableWord.startRow		= startRow;
			selectableWord.startCol		= startCol;
			selectableWord.isVertical	= isVertical;
			selectableWord.isReversed	= isReversed;

			selectableWords.Add(selectableWord);
		}

		/// <summary>
		/// Simple helper method to remove the word from the dictionary (And remove the list or words it its now empty)
		/// </summary>
		private void RemoveWordHelper(string word, char letter, Dictionary<char, List<string>> remainingWords)
		{
			if (remainingWords != null && remainingWords.ContainsKey(letter))
			{
				remainingWords[letter].Remove(word);

				if (remainingWords[letter].Count == 0)
				{
					remainingWords.Remove(letter);
				}
			}
		}

		/// <summary>
		/// Selets the next word and letter to show for a hint using the given dictionary of already shown hints
		/// </summary>
		private bool SelectHint(Dictionary<string, int> hintIndicies, out SelectableWord hintWord, out int hintIndex)
		{
			hintWord	= null;
			hintIndex	= int.MaxValue;

			// Gets a word to show a hint for by looking for the word with the least shown hint index
			for (int i = 0; i < selectableWords.Count; i++)
			{
				string	word			= selectableWords[i].word;
				int		nextHintIndex	= 0;

				if (hintIndicies.ContainsKey(word))
				{
					nextHintIndex = hintIndicies[word] + 1;
				}

				if (nextHintIndex < word.Length && nextHintIndex < hintIndex)
				{
					hintWord	= selectableWords[i];
					hintIndex	= nextHintIndex;
				}
			}

			if (hintWord == null)
			{
				// There is no hint we can show
				return false;
			}

			// If hintIndex is 0 then its the first time we are showing a hint for this word so we need to add it to the letterHints dictionary
			if (hintIndex == 0)
			{
				hintIndicies.Add(hintWord.word, 0);
			}
			else
			{
				hintIndicies[hintWord.word] = hintIndex;
			}

			return true;
		}

		private IEnumerator NextLevelAnimations()
		{
			UIAnimation anim = UIAnimation.Alpha(boardContainer.gameObject, 0f, fadeDuration);
			anim.style = UIAnimation.Style.EaseOut;
			anim.Play();

			yield return new WaitForSeconds(fadeDuration);

			GameManager.Instance.StartNextLevel();

			anim = UIAnimation.Alpha(boardContainer.gameObject, 1f, fadeDuration);
			anim.style = UIAnimation.Style.EaseIn;
			anim.Play();
		}
		
		/// <summary>
		/// Creates the tile letter text pool.
		/// </summary>
		/// <param name="letterTile">Letter tile.</param>
		private void CreateTileLetterTextPool(GridLetterTile letterTile)
		{
			// Instantiate a copy of the letterTile text to use as a template for the ObjectPool
			Text letterTileTextTemplate = Instantiate(letterTile.LetterText);
			letterTileTextTemplate.name	= "letter_tile_text_template";
			letterTileTextTemplate.gameObject.SetActive(false);
			letterTileTextTemplate.transform.SetParent(transform);

			tileLetterTextPool = new ObjectPool(letterTileTextTemplate.gameObject, 10, ObjectPool.CreatePoolContainer(transform, "letter_tile_text_pool_container"));
		}

		/// <summary>
		/// Creates an animation container to use for animating letters from the grid
		/// </summary>
		private void CreateAnimationContainer()
		{
			animationContainer = new GameObject("animation_container", typeof(RectTransform)).transform;

			animationContainer.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

			animationContainer.SetParent(boardContainer, false);
		}

		#endregion
	}
}
