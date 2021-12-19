using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bimbimnet.WordBlocks
{
    public class GridHandler : UIMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        #region Classes

        private class GridCell
        {
            public int row;
            public int col;
            public char letter;
            public bool isBlank;
            public GridLetterTile letterTile;

            public GridCell(int row, int col, char letter) { this.row = row; this.col = col; this.letter = letter; }
        }

        #endregion

        #region Enums

        private enum SelectState
        {
            None,           // Nothing is selected
            Selecting       // Both selectStart and selectEnd have been set
        }

        #endregion

        #region Inspector Variables

        public GridLetterTile letterTilePrefab = null;

        #endregion

        #region Member Variables

        private ObjectPool letterTilePool;
        private RectTransform gridContainer;
        private Camera canvasCamera;

        private float activeTileSize;
        private LevelSaveData activeLevelSaveData;
        private List<List<GridCell>> grid;
        private List<GameController.SelectableWord> selectableWords;

        private SelectState selectState;
        private GridCell selectStart;
        private GridCell selectEnd;
        private List<GridCell> selectedGridCells;
        private string selectedWord;

        private int numTilesMoving;

        #endregion

        #region Properties

        public System.Action<string> OnSelectedWordChanged { get; set; }
        public System.Action<string, List<int[]>> OnWordSelected { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        public void Initialize()
        {
            CreateGridContainer();

            letterTilePool = new ObjectPool(letterTilePrefab.gameObject, 15, gridContainer, ObjectPool.PoolBehaviour.CanvasGroup);
            grid = new List<List<GridCell>>();
            selectedGridCells = new List<GridCell>();

            canvasCamera = Utilities.GetCanvasCamera(transform);
        }

        /// <summary>
        /// Sets up the grid using the BoardData in the levelSaveData
        /// </summary>
        public void Setup(LevelSaveData levelSaveData)
        {
            Clear();

            activeLevelSaveData = levelSaveData;

            SetupGrid();

            UpdateTileHints(false);
        }

        /// <summary>
        /// Clears all UI
        /// </summary>
        public void Clear()
        {
            letterTilePool.ReturnAllObjectsToPool();

            grid.Clear();
        }

        /// <summary>
        /// Invoked when the user first clicks the grid
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (numTilesMoving > 0)
            {
                return;
            }

            UpdateSelected(eventData.position);
        }

        /// <summary>
        /// Invoked when the user drags the mouse over the grid
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (numTilesMoving > 0)
            {
                return;
            }

            UpdateSelected(eventData.position);
        }

        /// <summary>
        /// Invoked when the user stops draggin
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (numTilesMoving > 0)
            {
                return;
            }

            UpdateSelected(eventData.position);

            if (selectState == SelectState.Selecting)
            {
                DeselectTiles(selectedGridCells);

                if (OnWordSelected != null)
                {
                    OnWordSelected(selectedWord, GetGridCellPositions(selectedGridCells));
                }
            }

            selectStart = null;
            selectEnd = null;
            selectState = SelectState.None;
            selectedWord = "";
            selectedGridCells.Clear();
        }

        /// <summary>
        /// Called when a selected word was not a valid word in the level
        /// </summary>
        public void NotifyWordNotValid(List<int[]> gridCellPositions)
        {
            List<GridCell> gridCells = GetGridCellsAtPositions(gridCellPositions);

            for (int i = 0; i < gridCells.Count; i++)
            {
                gridCells[i].letterTile.Shake();
            }
        }

        /// <summary>
        /// Called when a selected word has already been found
        /// </summary>
        public void NotifyWordAlreadyFound(List<int[]> gridCellPositions)
        {
            List<GridCell> gridCells = GetGridCellsAtPositions(gridCellPositions);

            for (int i = 0; i < gridCells.Count; i++)
            {
                gridCells[i].letterTile.Twist();
            }
        }

        /// <summary>
        /// Called when a selected word has been found
        /// </summary>
        public void NotifyWordFound(List<int[]> gridCellPositions)
        {
            List<GridCell> gridCells = GetGridCellsAtPositions(gridCellPositions);

            int minCol = int.MaxValue;
            int maxCol = int.MinValue;
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;

            for (int i = 0; i < gridCells.Count; i++)
            {
                GridCell gridCell = gridCells[i];

                // Fade out the tile and hide the letter, when the animation finishes return it to the pool
                gridCell.letterTile.Remove((GameObject obj) => { ObjectPool.ReturnObjectToPool(obj); });

                // Set the cell as blank on the grid
                SetBlank(gridCell);

                // Update the save data
                activeLevelSaveData.boardData.board[gridCell.row][gridCell.col] = '\0';

                minCol = Mathf.Min(minCol, gridCell.col);
                maxCol = Mathf.Max(maxCol, gridCell.col);
                minRow = Mathf.Min(minRow, gridCell.row);
                maxRow = Mathf.Max(maxRow, gridCell.row);
            }

            RepositionTiles(minCol, maxCol, minRow, maxRow);
        }

        /// <summary>
        /// Called when the GameController generates a new BoardData for the level
        /// </summary>
        public void NotifyNewBoardData()
        {
            StartCoroutine(WaitForMovesThenShuffle());
        }

        /// <summary>
        /// Sets the list of selectable words on the grid
        /// </summary>
        public void SetSelectableWords(List<GameController.SelectableWord> selectableWords)
        {
            this.selectableWords = selectableWords;
        }

        /// <summary>
        /// Shows the tile at the given row/col as a hint
        /// </summary>
        public void ShowHint(GameController.SelectableWord selectableWord, int letterIndex, bool animate = true)
        {
            int[] rowCol = GetHintTileRowCol(selectableWord, letterIndex);

            GridLetterTile letterTile = grid[rowCol[0]][rowCol[1]].letterTile;

            letterTile.ShowHint(animate);
        }

        /// <summary>
        /// Gets a list of the LetterTiles at the given positions in the grid
        /// </summary>
        public List<GridLetterTile> GetLetterTiles(List<int[]> gridCellPositions)
        {
            List<GridLetterTile> letterTiles = new List<GridLetterTile>();

            for (int i = 0; i < gridCellPositions.Count; i++)
            {
                int[] gridCellPosition = gridCellPositions[i];

                letterTiles.Add(grid[gridCellPosition[0]][gridCellPosition[1]].letterTile);
            }

            return letterTiles;
        }

        /// <summary>
        /// Updates the tile hints.
        /// </summary>
        public void UpdateTileHints(bool animate)
        {
            if (activeLevelSaveData.tileHints.Count == 0)
            {
                // If theres no tile hints to show then stop now
                return;
            }

            // Fist build a HashSet where the keys are the row_col of all hints that should be displayed
            HashSet<string> hintCells = new HashSet<string>();

            for (int i = 0; i < selectableWords.Count; i++)
            {
                GameController.SelectableWord selectableWord = selectableWords[i];

                // Check if the selectable word has any hints shown
                if (activeLevelSaveData.tileHints.ContainsKey(selectableWord.word))
                {
                    int hintIndex = activeLevelSaveData.tileHints[selectableWord.word];

                    // Get all hint letter tiles row/col
                    for (int j = 0; j <= hintIndex; j++)
                    {
                        int[] rowCol = GetHintTileRowCol(selectableWord, j);

                        hintCells.Add(rowCol[0] + "_" + rowCol[1]);
                    }
                }
            }

            // Now go through each GridCell with a letter cell and shown / hide the hints
            for (int row = 0; row < activeLevelSaveData.boardData.rows; row++)
            {
                for (int col = 0; col < activeLevelSaveData.boardData.cols; col++)
                {
                    GridCell gridCell = grid[row][col];

                    // Skip blank cells
                    if (gridCell.isBlank)
                    {
                        continue;
                    }

                    bool shouldShowHint = hintCells.Contains(row + "_" + col);

                    if (shouldShowHint && !gridCell.letterTile.IsHintDisplayed)
                    {
                        gridCell.letterTile.ShowHint(animate);
                    }
                    else if (!shouldShowHint && gridCell.letterTile.IsHintDisplayed)
                    {
                        gridCell.letterTile.HideHint(animate);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the starting boards LetterTiles
        /// </summary>
        private void SetupGrid()
        {
            BoardData boardData = activeLevelSaveData.boardData;

            float maxTileWidth = Mathf.Min(letterTilePrefab.RectT.rect.width, RectT.rect.width / (float)boardData.cols);
            float maxTileHeight = Mathf.Min(letterTilePrefab.RectT.rect.height, RectT.rect.height / (float)boardData.rows);

            activeTileSize = Mathf.Min(maxTileWidth, maxTileHeight);

            for (int row = 0; row < boardData.rows; row++)
            {
                grid.Add(new List<GridCell>());

                for (int col = 0; col < boardData.cols; col++)
                {
                    char letter = boardData.board[row][col];
                    bool isBlank = (letter == '\0');
                    GridCell gridCell = new GridCell(row, col, letter);

                    gridCell.isBlank = isBlank;

                    if (!isBlank)
                    {
                        GridLetterTile letterTile = letterTilePool.GetObject<GridLetterTile>();

                        letterTile.RectT.anchoredPosition = GetCellPosition(row, col);

                        SetLetterTileScale(letterTile.RectT);

                        letterTile.Setup(letter);

                        gridCell.letterTile = letterTile;
                    }

                    grid[row].Add(gridCell);
                }
            }
        }

        /// <summary>
        /// Gets the position in the grid for the given row/col
        /// </summary>
        private Vector2 GetCellPosition(int row, int col)
        {
            Vector2 bottomLeft = new Vector2(-((float)activeLevelSaveData.boardData.cols - 1) * activeTileSize / 2f, -gridContainer.rect.height / 2f + activeTileSize / 2f);

            return bottomLeft + new Vector2(col * activeTileSize, row * activeTileSize);
        }

        /// <summary>
        /// Scales the LetterTile so if fits the bounds of the placement tile
        /// </summary>
        private void SetLetterTileScale(RectTransform letterTileRectT)
        {
            float xScale = activeTileSize / letterTileRectT.rect.width;
            float yScale = activeTileSize / letterTileRectT.rect.height;
            float scale = Mathf.Min(xScale, yScale);

            letterTileRectT.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// Gets the local position in the grid from the given screen position
        /// </summary>
        private Vector2 GetLocalPosition(Vector2 screenPosition)
        {
            Vector2 localPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(gridContainer, screenPosition, canvasCamera, out localPosition);

            return localPosition;
        }

        /// <summary>
        /// Updates the selectStart and selectEnd
        /// </summary>
        private void UpdateSelected(Vector2 screenPosition)
        {
            if (selectState == SelectState.None)
            {
                SetSelectStart(GetLocalPosition(screenPosition));
            }

            if (selectState == SelectState.Selecting)
            {
                SetSelectEnd(GetLocalPosition(screenPosition));
            }

            UpdateSelectedTiles();
        }

        /// <summary>
        /// Sets the grid cell that the given position is over as the starting grid cell
        /// </summary>
        private void SetSelectStart(Vector2 localPosition)
        {
            GridCell gridCell = GetGridCellAt(localPosition);

            if (gridCell != null && !gridCell.isBlank)
            {
                selectStart = gridCell;
                selectState = SelectState.Selecting;
            }
        }

        /// <summary>
        /// Sets the grid cell that the given position is over as the starting grid cell
        /// </summary>
        private void SetSelectEnd(Vector2 localPosition)
        {
            GridCell gridCell = GetClosestGridCell(localPosition);

            // If the closest grid cell to the position is on the same row/column as the start cell then thats our end cell
            if (gridCell.row == selectStart.row || gridCell.col == selectStart.col)
            {
                selectEnd = gridCell;
            }
            // If the closest grid cell is not on the same row/col then we need to find the closest gridcell that is on the same row/col
            else
            {
                int rowDiff = Mathf.Abs(gridCell.row - selectStart.row);
                int colDiff = Mathf.Abs(gridCell.col - selectStart.col);

                if (rowDiff <= colDiff)
                {
                    selectEnd = grid[selectStart.row][gridCell.col];
                }
                else
                {
                    selectEnd = grid[gridCell.row][selectStart.col];
                }
            }
        }

        /// <summary>
        /// Updates the selected tiles when the user interacts with the screen
        /// </summary>
        private void UpdateSelectedTiles()
        {
            if (selectStart == null)
            {
                return;
            }

            // Deselect the last selected tiles
            DeselectTiles(selectedGridCells);

            // Set the tiles as selected and update the selectedWord
            SetSelectedTiles(selectStart, selectEnd);
        }

        /// <summary>
        /// Deselects the tiles from start to end
        /// </summary>
        private void DeselectTiles(List<GridCell> gridCells)
        {
            for (int i = 0; i < gridCells.Count; i++)
            {
                gridCells[i].letterTile.SetSelected(false, false);
            }
        }

        /// <summary>
        /// Sets the tiles from start to end as selected and updates the selectedWord
        /// </summary>
        private void SetSelectedTiles(GridCell start, GridCell end)
        {
            int rowInc = (start.row <= end.row) ? 1 : -1;
            int colInc = (start.col <= end.col) ? 1 : -1;

            List<GridCell> lastSelectedGridCells = new List<GridCell>(selectedGridCells);

            selectedWord = "";
            selectedGridCells.Clear();

            bool done = false;

            for (int row = start.row; (rowInc > 0 ? row <= end.row : row >= end.row); row += rowInc)
            {
                for (int col = start.col; (colInc > 0 ? col <= end.col : col >= end.col); col += colInc)
                {
                    GridCell gridCell = grid[row][col];

                    // If we encounter a blank cell then stop
                    if (gridCell.isBlank)
                    {
                        done = true;

                        break;
                    }

                    bool justSelected = !lastSelectedGridCells.Contains(gridCell);

                    if (justSelected)
                    {
                        SetLetterTileOnTop(gridCell);
                    }

                    gridCell.letterTile.SetSelected(true, justSelected);

                    selectedWord += gridCell.letter;

                    selectedGridCells.Add(gridCell);
                }

                if (done)
                {
                    break;
                }
            }

            if (SoundManager.Exists())
            {
                // If there are now more selected tiles then before then play the deselected sound
                if (lastSelectedGridCells.Count < selectedGridCells.Count)
                {
                    SoundManager.Instance.Play("tile-selected");
                }
                // If there are now less selected tiles then before then play the selected sound
                else if (lastSelectedGridCells.Count > selectedGridCells.Count)
                {
                    SoundManager.Instance.Play("tile-deselected");
                }
            }

            if (OnSelectedWordChanged != null)
            {
                OnSelectedWordChanged(selectedWord);
            }
        }

        /// <summary>
        /// Gets the grid cell which contains the given position
        /// </summary>
        private GridCell GetGridCellAt(Vector2 localPosition)
        {
            for (int row = 0; row < grid.Count; row++)
            {
                for (int col = 0; col < grid[row].Count; col++)
                {
                    GridCell gridCell = grid[row][col];
                    Vector2 cellPos = GetCellPosition(row, col);

                    float halfSize = activeTileSize / 2f;

                    float left = cellPos.x - halfSize;
                    float right = cellPos.x + halfSize;
                    float top = cellPos.y - halfSize;
                    float bottom = cellPos.y + halfSize;

                    if (localPosition.x >= left && localPosition.x <= right && localPosition.y >= top && localPosition.y <= bottom)
                    {
                        return gridCell;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the closest grid cell to the given position
        /// </summary>
        private GridCell GetClosestGridCell(Vector2 localPosition)
        {
            GridCell closestGridCell = null;
            float minDist = float.MaxValue;

            for (int row = 0; row < grid.Count; row++)
            {
                for (int col = 0; col < grid[row].Count; col++)
                {
                    GridCell gridCell = grid[row][col];
                    Vector2 cellPos = GetCellPosition(row, col);

                    float dist = Vector2.Distance(localPosition, cellPos);

                    if (dist < minDist)
                    {
                        closestGridCell = gridCell;
                        minDist = dist;
                    }
                }
            }

            return closestGridCell;
        }

        /// <summary>
        /// Gets a list of the row/col positions for each GridCell
        /// </summary>
        private List<int[]> GetGridCellPositions(List<GridCell> gridCells)
        {
            List<int[]> gridCellPositions = new List<int[]>();

            for (int i = 0; i < gridCells.Count; i++)
            {
                GridCell gridCell = gridCells[i];

                gridCellPositions.Add(new int[] { gridCell.row, gridCell.col });
            }

            return gridCellPositions;
        }

        /// <summary>
        /// Gets a list of the GridCells location at the given positions in the grid
        /// </summary>
        private List<GridCell> GetGridCellsAtPositions(List<int[]> gridCellPositions)
        {
            List<GridCell> gridCells = new List<GridCell>();

            for (int i = 0; i < gridCellPositions.Count; i++)
            {
                int[] gridCellPosition = gridCellPositions[i];

                gridCells.Add(grid[gridCellPosition[0]][gridCellPosition[1]]);
            }

            return gridCells;
        }

        /// <summary>
        /// Drops all tiles above the tiles specified by the given star/end row/col
        /// </summary>
        private void RepositionTiles(int startCol, int endCol, int startRow, int endRow)
        {
            // First drop the tiles above the word that was removed
            int dropAmount = endRow - startRow + 1;

            for (int col = startCol; col <= endCol; col++)
            {
                DropTiles(col, endRow + 1, dropAmount);
            }

            if (startRow == 0 || endRow == 0)
            {
                int firstNonBlankCol;
                int lastNonBlankCol;

                if (IsShiftNeeded(out firstNonBlankCol, out lastNonBlankCol))
                {
                    // Call ShiftTiles with setBoardData set to true so only the levelSaveData.boardData is updated, this wont move the tiles on the board
                    // This is so we get an updated board in the save data so if the user closes the app as the drop tiles are animating the new board with
                    // the shifts will be saved
                    ShiftTiles(firstNonBlankCol, lastNonBlankCol, true);

                    // Wait for the drops to complete then start the shift animations
                    StartCoroutine(WaitForDropThenShiftTiles(firstNonBlankCol, lastNonBlankCol));
                }
            }
        }

        /// <summary>
        /// Drops all tiles in the given column starting at the given row
        /// </summary>
        private void DropTiles(int col, int startRow, int dropAmount)
        {
            for (int row = startRow; row < activeLevelSaveData.boardData.rows; row++)
            {
                GridCell gridCell = grid[row][col];

                if (gridCell.isBlank)
                {
                    break;
                }

                // Update the board data in the LevelSaveData
                activeLevelSaveData.boardData.board[row - dropAmount][col] = activeLevelSaveData.boardData.board[row][col];
                activeLevelSaveData.boardData.board[row][col] = '\0';

                GridCell dropToCell = grid[row - dropAmount][col];

                numTilesMoving++;

                MoveTile(gridCell, dropToCell, (GameObject obj) => { numTilesMoving--; });
            }
        }

        /// <summary>
        /// Check if there is a blank cell between the first and last non-blank cell on the bottom row, if so a shift is needed
        /// </summary>
        private bool IsShiftNeeded(out int firstNonBlankCol, out int lastNonBlankCol)
        {
            firstNonBlankCol = -1;
            lastNonBlankCol = -1;

            int firstBlankCol = -1;

            for (int col = 0; col < activeLevelSaveData.boardData.cols; col++)
            {
                GridCell gridCell = grid[0][col];

                if (gridCell.isBlank)
                {
                    if (firstNonBlankCol != -1 && firstBlankCol == -1)
                    {
                        firstBlankCol = col;
                    }
                }
                else
                {
                    if (firstNonBlankCol == -1)
                    {
                        firstNonBlankCol = col;
                    }
                    else
                    {
                        lastNonBlankCol = col;
                    }
                }
            }

            return firstBlankCol != -1 && firstBlankCol < lastNonBlankCol;
        }

        /// <summary>
        /// Waits until all tiles have dropped before starting the shift animations
        /// </summary>
        private IEnumerator WaitForDropThenShiftTiles(int firstNonBlankCol, int lastNonBlankCol)
        {
            while (numTilesMoving > 0)
            {
                yield return new WaitForEndOfFrame();
            }

            ShiftTiles(firstNonBlankCol, lastNonBlankCol, false);
        }

        /// <summary>
        /// Shifts the tiles so there are no blanks between first and last non blank cell
        /// </summary>
        private void ShiftTiles(int firstNonBlankCol, int lastNonBlankCol, bool setBoardData)
        {
            // Now shift the board left/right if there are blank cells at the bottom
            if (activeLevelSaveData.boardData.cols - lastNonBlankCol > firstNonBlankCol + 1)
            {
                // Shift everything left of the last blank column right
                ShiftTiles(lastNonBlankCol, firstNonBlankCol, -1, setBoardData);
            }
            else
            {
                // Shift everything right of first blank column left
                ShiftTiles(firstNonBlankCol, lastNonBlankCol, 1, setBoardData);
            }
        }

        /// <summary>
        /// Shifts the tiles
        /// </summary>
        private void ShiftTiles(int startCol, int endCol, int colInc, bool setBoardData)
        {
            int shiftAmount = 0;

            for (int col = startCol; (colInc > 0 ? col <= endCol : col >= endCol); col += colInc)
            {
                GridCell gridCell = grid[0][col];

                // If the cell is blank increase the amount of cells to shift and continue to the next cell
                if (gridCell.isBlank)
                {
                    shiftAmount++;
                    continue;
                }

                // If the cell is not blank and shift amount if greater than 0 then we need to shift this whole column by the shift amount
                if (shiftAmount > 0)
                {
                    ShiftColumn(col, colInc > 0 ? -shiftAmount : shiftAmount, setBoardData);
                }
            }
        }

        /// <summary>
        /// Shifts all tiles in a column
        /// </summary>
        private void ShiftColumn(int col, int shiftAmount, bool setBoardData)
        {
            for (int row = 0; row < activeLevelSaveData.boardData.rows; row++)
            {
                GridCell gridCell = grid[row][col];

                // If the cell is blank we reached the top and we can stop
                if (gridCell.isBlank)
                {
                    return;
                }

                if (setBoardData)
                {
                    // Update the board data in the LevelSaveData
                    activeLevelSaveData.boardData.board[row][col + shiftAmount] = activeLevelSaveData.boardData.board[row][col];
                    activeLevelSaveData.boardData.board[row][col] = '\0';
                }
                else
                {
                    GridCell moveToCell = grid[row][col + shiftAmount];

                    numTilesMoving++;

                    MoveTile(gridCell, moveToCell, (GameObject obj) => { numTilesMoving--; });
                }
            }
        }

        /// <summary>
        /// Moves the letter tile
        /// </summary>
        private void MoveTile(GridCell fromCell, GridCell toCell, System.Action<GameObject> animFinished)
        {
            // Copy the values to the new cell
            toCell.letterTile = fromCell.letterTile;
            toCell.letter = fromCell.letter;
            toCell.isBlank = false;

            // Animate the tile to its new placement cell
            toCell.letterTile.Move(GetCellPosition(toCell.row, toCell.col), animFinished);

            // Set the from cell to blank
            SetBlank(fromCell);
        }

        /// <summary>
        /// Waits until all tiles have stopped moving then shuffles the board
        /// </summary>
        private IEnumerator WaitForMovesThenShuffle()
        {
            while (numTilesMoving > 0)
            {
                yield return new WaitForEndOfFrame();
            }

            ShuffleBoard();

            // Wait for the shuffle to finish
            while (numTilesMoving > 0)
            {
                yield return new WaitForEndOfFrame();
            }

            // Update the tile hints
            UpdateTileHints(true);
        }

        /// <summary>
        /// Suffles the board by moving all tiles to a new position on the board
        /// </summary>
        private void ShuffleBoard()
        {
            Dictionary<char, List<GridLetterTile>> allGridLetterTiles = new Dictionary<char, List<GridLetterTile>>();

            // Get all the grid letter tiles by letter
            for (int i = 0; i < grid.Count; i++)
            {
                for (int j = 0; j < grid[i].Count; j++)
                {
                    GridCell gridCell = grid[i][j];

                    if (gridCell.isBlank)
                    {
                        continue;
                    }

                    char letter = gridCell.letter;

                    if (!allGridLetterTiles.ContainsKey(letter))
                    {
                        allGridLetterTiles.Add(letter, new List<GridLetterTile>());
                    }

                    allGridLetterTiles[letter].Add(gridCell.letterTile);

                    SetBlank(gridCell);
                }
            }

            // Move all the letter tiles to their new positions on the grid
            for (int row = 0; row < activeLevelSaveData.boardData.rows; row++)
            {
                for (int col = 0; col < activeLevelSaveData.boardData.cols; col++)
                {
                    char letter = activeLevelSaveData.boardData.board[row][col];

                    if (letter == '\0')
                    {
                        continue;
                    }

                    List<GridLetterTile> gridLetterTiles = allGridLetterTiles[letter];
                    GridLetterTile letterTile = gridLetterTiles[0];
                    GridCell gridCell = grid[row][col];

                    gridLetterTiles.RemoveAt(0);

                    // Setup the letter tile
                    letterTile.Setup(letter);

                    // Set the grid cells new letter tile and letter
                    gridCell.letterTile = letterTile;
                    gridCell.letter = letter;
                    gridCell.isBlank = false;

                    numTilesMoving++;

                    // Animate the tile to its new position on the board
                    letterTile.Move(GetCellPosition(gridCell.row, gridCell.col), (GameObject obj) => { numTilesMoving--; });
                }
            }
        }

        /// <summary>
        /// Sets the grid cell as a blank cell
        /// </summary>
        private void SetBlank(GridCell gridCell)
        {
            gridCell.letter = '\0';
            gridCell.letterTile = null;
            gridCell.isBlank = true;
        }

        /// <summary>
        /// Sets the proper sibling index for the letter tile on the given GridCell
        /// </summary>
        private void SetLetterTileOnTop(GridCell gridCell)
        {
            gridCell.letterTile.transform.SetSiblingIndex(gridCell.letterTile.transform.parent.childCount - 1);
        }

        /// <summary>
        /// Gets the row/col for the hint
        /// </summary>
        private int[] GetHintTileRowCol(GameController.SelectableWord selectableWord, int letterIndex)
        {
            int row = selectableWord.startRow;
            int col = selectableWord.startCol;

            if (selectableWord.isVertical)
            {
                if (selectableWord.isReversed)
                {
                    row = selectableWord.startRow + selectableWord.word.Length - letterIndex - 1;
                }
                else
                {
                    row = selectableWord.startRow + letterIndex;
                }
            }
            else
            {
                if (selectableWord.isReversed)
                {
                    col = selectableWord.startCol + selectableWord.word.Length - letterIndex - 1;
                }
                else
                {
                    col = selectableWord.startCol + letterIndex;
                }
            }

            return new int[] { row, col };
        }

        /// <summary>
        /// Creates the GameObject which will hold all the tiles
        /// </summary>
        private void CreateGridContainer()
        {
            gridContainer = new GameObject("grid_container").AddComponent<RectTransform>();

            gridContainer.SetParent(transform, false);

            gridContainer.anchorMin = Vector2.zero;
            gridContainer.anchorMax = Vector2.one;

            gridContainer.offsetMin = Vector2.zero;
            gridContainer.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
