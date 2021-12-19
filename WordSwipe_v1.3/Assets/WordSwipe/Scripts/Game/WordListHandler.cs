using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Bimbimnet.WordBlocks
{
    public class WordListHandler : UIMonoBehaviour
    {
        #region Classes

        [System.Serializable]
        public class TileSizeInfo
        {
            public int numWordsInLevel = 0;
            public int tileSizeToUse = 0;
        }

        private class LetterCell
        {
            public RectTransform placementTile;
            public WordLetterTile letterTile;
        }

        #endregion

        #region Inspector Variables

        [Tooltip("The prefab that will be instantiate and used as an empty placeholder for each letter in a word.")]
        public WordLetterTile letterTilePrefab = null;

        [Tooltip("The amount of space between each of the tiles.")]
        public float spaceBetweenLetters = 2f;

        [Tooltip("The amount of space between each group of tiles that make up a word.")]
        public float spaceBetweenWords = 15f;

        [Tooltip("The amount of space between each row of tiles.")]
        public float spaceBetweenRows = 15f;

        [Tooltip("The size a tile should be for the number of words in the level, will search this list for the closest number or words in the level to the current level.")]
        public List<TileSizeInfo> tileSizes = null;

        #endregion

        #region Member Variables

        private ObjectPool letterTilePool;
        private ObjectPool tilePlacementPool;
        private ObjectPool tileRowPool;
        private ObjectPool tileSpacerPool;

        private Dictionary<string, List<LetterCell>> wordLetterCells;

        private float tileSize;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            letterTilePool = new ObjectPool(letterTilePrefab.gameObject, 1, ObjectPool.CreatePoolContainer(transform, "letter_tile_pool"));

            wordLetterCells = new Dictionary<string, List<LetterCell>>();

            SetupTileContainer();

            CreateTilePlacementPool();
            CreateTileRowPool();
            CreateTileSpacerPool();
        }

        public void Setup(LevelData levelData, LevelSaveData levelSaveData)
        {
            Clear();

            tileSize = GetTileSize(levelData.Words.Count);

            bool wordAddedToRow = false;
            float currentRowWidth = 0f;
            GameObject currentTileRow = tileRowPool.GetObject(transform);

            // Go through every word that is on the board, we need to add a tilePrefab for each letter in each word
            //for(int i = levelData.Words.Count; i >0; i--)
            for (int i = 0; i < levelData.Words.Count; i++)
            {
                // Get the word we are adding tiles for and the space those tiles will take up
                string word = levelData.Words[i];
                float wordWidth = word.Length * tileSize + (word.Length - 1) * spaceBetweenLetters;

                // If a word has already been added to the current row, then we need to account for the spacing between words
                if (wordAddedToRow)
                {
                    wordWidth += spaceBetweenWords;
                }

                // Check if the adding the current word to the current row will make the row larger that the width of the overall container
                bool rowToLarge = (currentRowWidth + wordWidth > RectT.rect.width);

                // If the current row is now wider than the container then we need to add a new row
                if (rowToLarge)
                {
                    if (!wordAddedToRow)
                    {
                        Debug.LogWarningFormat("The word \"{0}\" is to large to fit in the tileContainer using a size of {1}", word, tileSize);
                    }
                    else
                    {
                        // Create a new row and set the wordAddedToRow and currentRowWidth values back to default
                        currentTileRow = tileRowPool.GetObject(transform);
                        wordAddedToRow = false;
                        currentRowWidth = 0f;
                    }
                }

                // If we added a word to the row already then we need to add a space GameObject
                if (wordAddedToRow)
                {
                    tileSpacerPool.GetObject<RectTransform>(currentTileRow.transform);
                }

                List<LetterCell> letterCells = new List<LetterCell>();

                bool wordFound = levelSaveData.foundLevelWords.Contains(word);

                // Add a new tile to the row for every letter in the word
                //// Verison RTL languae //for (int j = word.Length -1 ; j >=0; j--)
                for (int j = 0; j < word.Length; j++)
                {
                    RectTransform placementTile = tilePlacementPool.GetObject<RectTransform>(currentTileRow.transform);
                    WordLetterTile letterTile = letterTilePool.GetObject<WordLetterTile>(placementTile);

                    // Set the size of hte placeholder
                    placementTile.sizeDelta = new Vector2(tileSize, tileSize);

                    // Set all letters to the blank character at first
                    letterTile.Setup(word[j]);

                    if (wordFound)
                    {
                        letterTile.SetShown();
                    }
                    else
                    {
                        letterTile.SetBlank();

                        if (levelSaveData.letterHints.ContainsKey(word) && j <= levelSaveData.letterHints[word])
                        {
                            letterTile.ShowAsHint(false);
                        }
                    }

                    // Set the scale so it fits in the placement
                    SetLetterTileScale(letterTile.RectT);

                    // Create a LetterCell so we can know when LetterTile is in what placement tile
                    LetterCell letterCell = new LetterCell();
                    letterCell.placementTile = placementTile;
                    letterCell.letterTile = letterTile;

                    letterCells.Add(letterCell);
                }

                wordLetterCells.Add(word, letterCells);

                wordAddedToRow = true;
                currentRowWidth += wordWidth;
            }
        }

        /// <summary>
        /// Reset the board by removing all the GridTiles
        /// </summary>
        public void Clear()
        {
            letterTilePool.ReturnAllObjectsToPool();
            tilePlacementPool.ReturnAllObjectsToPool();
            tileRowPool.ReturnAllObjectsToPool();
            tileSpacerPool.ReturnAllObjectsToPool();

            wordLetterCells.Clear();
        }

        /// <summary>
        /// Gets the letter tiles for the given word
        /// </summary>
        public List<LetterTile> GetLetterTiles(string word)
        {
            List<LetterCell> letterCells = wordLetterCells[word];
            List<LetterTile> letterTiles = new List<LetterTile>();

            for (int i = 0; i < letterCells.Count; i++)
            {
                letterTiles.Add(letterCells[i].letterTile);
            }

            return letterTiles;
        }

        /// <summary>
        /// Shows the word on the list by fading in the background and once the background is full faded in shows the letter
        /// </summary>
        public void NotifyWordFound(string word)
        {
            List<LetterCell> letterCells = wordLetterCells[word];

            for (int i = 0; i < letterCells.Count; i++)
            {
                WordLetterTile letterTile = letterCells[i].letterTile;

                letterTile.Show(i);
            }
        }

        /// <summary>
        /// Shows the letter as a hint
        /// </summary>
        public void ShowLetterAsHint(string word, int letterIndex)
        {
            List<LetterCell> letterCells = wordLetterCells[word];

            letterCells[letterIndex].letterTile.ShowAsHint();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Scales the LetterTile so if fits the bounds of the placement tile
        /// </summary>
        private void SetLetterTileScale(RectTransform letterTileRectT)
        {
            float xScale = tileSize / letterTileRectT.rect.width;
            float yScale = tileSize / letterTileRectT.rect.height;
            float scale = Mathf.Min(xScale, yScale);

            letterTileRectT.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// Adds the necessary layout components to the tileContainer
        /// </summary>
        private void SetupTileContainer()
        {
            VerticalLayoutGroup vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spaceBetweenRows;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        /// <summary>
        /// Creates the template placement tile and the tile placement pool that is used as the root for all LetterTiles
        /// </summary>
        private void CreateTilePlacementPool()
        {
            GameObject tilePlacementTemplate = new GameObject("tile_placement_template", typeof(RectTransform));

            tilePlacementTemplate.SetActive(false);
            tilePlacementTemplate.transform.SetParent(transform, false);

            tilePlacementPool = new ObjectPool(tilePlacementTemplate, 1, ObjectPool.CreatePoolContainer(transform, "tile_placement_pool"));
        }

        /// <summary>
        /// Adds the necessary layout components to the tileRow GameObject
        /// </summary>
        private void CreateTileRowPool()
        {
            // Create the row and add it to tileContainer
            GameObject tileRowTemplate = new GameObject("tile_row_template", typeof(RectTransform));

            tileRowTemplate.SetActive(false);
            tileRowTemplate.transform.SetParent(transform, false);

            // Add a HorizontalLayoutGroup to the row
            HorizontalLayoutGroup hlg = tileRowTemplate.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = spaceBetweenLetters;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = false;
            hlg.childControlWidth = false;

            tileRowPool = new ObjectPool(tileRowTemplate, 1, ObjectPool.CreatePoolContainer(transform, "tile_row_pool"));
        }

        /// <summary>
        /// Creates a new spacer and adds it to the given row transform
        /// </summary>
        private void CreateTileSpacerPool()
        {
            // Create the space GameObject
            GameObject wordSpaceTemplate = new GameObject("word_space_tempalte", typeof(RectTransform));

            wordSpaceTemplate.SetActive(false);
            wordSpaceTemplate.transform.SetParent(transform, false);

            (wordSpaceTemplate.transform as RectTransform).sizeDelta = new Vector2(spaceBetweenWords, 0);

            tileSpacerPool = new ObjectPool(wordSpaceTemplate, 1, ObjectPool.CreatePoolContainer(transform, "tile_spacer_pool"));
        }

        /// <summary>
        /// Gets the best tile size to use for the number of words in the level
        /// </summary>
        private float GetTileSize(int numWordsInLevel)
        {
            tileSizes.Sort((TileSizeInfo ts1, TileSizeInfo ts2) => { return ts1.numWordsInLevel - ts2.numWordsInLevel; });

            for (int i = 0; i < tileSizes.Count; i++)
            {
                TileSizeInfo tileSize = tileSizes[i];

                if (tileSize.numWordsInLevel == numWordsInLevel)
                {
                    return tileSize.tileSizeToUse;
                }

                if (tileSize.numWordsInLevel > numWordsInLevel)
                {
                    if (i > 0 && numWordsInLevel - tileSizes[i - 1].numWordsInLevel < tileSize.numWordsInLevel - numWordsInLevel)
                    {
                        return tileSizes[i - 1].tileSizeToUse;
                    }

                    return tileSize.tileSizeToUse;
                }
            }

            return (tileSizes.Count > 0 ? tileSizes[tileSizes.Count - 1].tileSizeToUse : 50f);
        }

        #endregion
    }
}
