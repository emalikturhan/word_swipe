using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class BoardCreator : MonoBehaviour
	{
		#region Classes

		private class Board
		{
			public int				xCells;
			public int				yCells;
			public List<List<Cell>>	grid;
			public System.Random	rand;
			public List<bool>		placedVerticals;

			public List<Dictionary<string, Undo>> undos;
		}

		private class Cell
		{
			public int	x;
			public int	y;
			public char	letter;
		}

		private class Placement
		{
			public enum Type
			{
				Horizontal,
				Vertical,
				ShiftLeft,
				ShiftRight
			}

			public Cell	cell;
			public Type type;

			public Placement(Cell cell, Type type)
			{
				this.cell = cell;
				this.type = type;
			}
		}

		private abstract class Undo
		{
			public enum Type
			{
				Cell,
				PlacedVertical
			}

			public abstract Type UndoType { get; }
		}

		private class CellUndo : Undo
		{
			public Cell	cell;
			public char	letter;

			public override Type UndoType { get { return Type.Cell; } }
		}

		private class PVUndo : Undo
		{
			public int	index;
			public bool	value;

			public override Type UndoType { get { return Type.PlacedVertical; } }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates the grid of characters for the given word
		/// </summary>
		public static BoardData Create(List<string> words, int preferredRows, int preferredCols, int seed = 0)
		{
			System.Random rand = (seed == 0) ? new System.Random() : new System.Random(seed);

			// If PlaceWords failed it means the preferred rows/cols given where to small to place all the words on teh board
			// In that case extra will increase by 1 and we will try again with a slightly larger board. We will set a hard limit
			// of only 1000 tries so it something really bad happens it doesn't hang indefinitely
			for (int extra = 0; extra < 1000; extra++)
			{
				int rows = preferredRows + extra;
				int cols = preferredCols + extra;

				if (extra != 0)
				{
					Debug.LogWarningFormat("[BoardCreator] Could not create valid board with rows:{0} cols:{1}... Trying again with rows:{2} cols{3}", rows - 1, cols - 1, rows, cols);
				}

				// Create the blank board
				Board board = CreateBoard(cols, rows);

				// Assign the Random variable to use
				board.rand = rand;

				// Place all the words on the blank board
				if (PlaceWords(board, words, 0))
				{
					//PrintCells(board, "Board complete");

					return CreateBoardData(board);
				}
			}

			Debug.LogErrorFormat("[BoardCreator] Failed creating a board with teh given input");

			return null;
		}

		public static void CreateBoardAsync(List<string> words, int preferredRows, int preferredCols, System.Action<BoardData> finishedCallback)
		{
			CreateBoardAsync(words, preferredRows, preferredCols, 0, finishedCallback);
		}

		public static void CreateBoardAsync(List<string> words, int preferredRows, int preferredCols, int seed, System.Action<BoardData> finishedCallback)
		{
			BoardCreatorWorker boardCreatorWorker = new BoardCreatorWorker();

			boardCreatorWorker.Words = words;
			boardCreatorWorker.Rows = preferredRows;
			boardCreatorWorker.Cols = preferredCols;
			boardCreatorWorker.Seed = seed;

			WorkerBehaviour.StartWorker(boardCreatorWorker, (Worker worker) =>
			{
				finishedCallback((worker as BoardCreatorWorker).FinishedBoardDada);
			});
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates the initial board or characters
		/// </summary>
		private static Board CreateBoard(int xCells, int yCells)
		{
			Board board = new Board();

			board.xCells			= xCells;
			board.yCells			= yCells;
			board.placedVerticals	= new List<bool>();
			board.grid				= new List<List<Cell>>();
			board.undos				= new List<Dictionary<string, Undo>>();

			for (int x = 0; x < xCells; x++)
			{
				board.grid.Add(new List<Cell>());

				board.placedVerticals.Add(false);

				for (int y = 0; y < yCells; y++)
				{
					Cell cell = new Cell();

					cell.x = x;
					cell.y = y;

					board.grid[x].Add(cell);
				}
			}

			return board;
		}

		/// <summary>
		/// Creates a BoardData object for the given board char matrix
		/// </summary>
		private static BoardData CreateBoardData(Board board)
		{
			BoardData boardData = new BoardData();

			boardData.rows	= board.yCells;
			boardData.cols	= board.xCells;
			boardData.board = new List<List<char>>();

			// Convert the Boards grid to the BoardDatas board
			for (int row = 0; row < board.yCells; row++)
			{
				boardData.board.Add(new List<char>());

				for (int col = 0; col < board.xCells; col++)
				{
					boardData.board[row].Add(board.grid[col][row].letter);
				}
			}

			// Trim any blank columns
			for (int col = boardData.cols - 1; col >= 0; col--)
			{
				// If any cell on row 0 (bottom row) is blank then the whole column is blank
				if (boardData.board[0][col] == '\0')
				{
					for (int row = 0; row < boardData.rows; row++)
					{
						boardData.board[row].RemoveAt(col);
					}

					boardData.cols--;
				}
			}

			// Trim any blank rows
			for (int row = boardData.rows - 1; row >= 0; row--)
			{
				bool isBlank = true;

				for (int col = 0; col < boardData.cols; col++)
				{
					if (boardData.board[row][col] != '\0')
					{
						isBlank = false;

						break;
					}
				}

				if (!isBlank)
				{
					break;
				}

				boardData.board.RemoveAt(row);
				boardData.rows--;
			}

			return boardData;
		}

		/// <summary>
		/// Places the next word on the board
		/// </summary>
		private static bool PlaceWords(Board board, List<string> words, int nextWordIndex)
		{
			if (nextWordIndex >= words.Count)
			{
				return true;
			}

			// Get the next word to place on the board
			string wordToPlace = words[nextWordIndex];
			
			int numHorizontal;

			// Get all the possible starting cells for this word
			List<Placement> possiblePlacements = GetPossibleStartingPlacements(board, wordToPlace.Length, nextWordIndex == 0, out numHorizontal);

			// Keep trying random placements until we find one that leads to a completed board
			for (int i = 0; i < possiblePlacements.Count; i++)
			{
				int randIndex;

				if (numHorizontal != 0)
				{
					if (nextWordIndex % 2 == 0)
					{
						// Pick a horizontal word
						randIndex = board.rand.Next(possiblePlacements.Count - numHorizontal, possiblePlacements.Count);
						numHorizontal--;
					}
					else
					{
						// Pick a vertical word
						randIndex = board.rand.Next(0, numHorizontal);
					}
				}
				else
				{
					randIndex = board.rand.Next(i, possiblePlacements.Count);
				}

				Placement placement = possiblePlacements[randIndex];

				possiblePlacements[randIndex]	= possiblePlacements[i];
				possiblePlacements[i]			= placement;

				board.undos.Add(new Dictionary<string, Undo>());

				// Place the word on the board
				PlaceWordAt(board, wordToPlace, placement);

				// Try and place the remaining words
				if (PlaceWords(board, words, nextWordIndex + 1))
				{
					// All words have been successfully placed on the board
					return true;
				}

				// If we get here the placing the word at startingCell makes it so one or more of the remaining words cannot be placed so remove the word and try
				// again with a different starting cell
				UndoChanges(board);

				//PrintCells(board, "Place failed, after undo");
			}

			return false;
		}

		/// <summary>
		/// Gets a list of possible starting cell placements for a word with the given length
		/// </summary>
		private static List<Placement> GetPossibleStartingPlacements(Board board, int len, bool firstWord, out int numHorizontal)
		{
			numHorizontal = 0;

			List<Placement> possiblePlacements = new List<Placement>();

			int[] colBlanks = new int[board.xCells];

			for (int y = board.yCells - 1; y >= 0; y--)
			{
				int hMaxLen = 0;
				int hMinLen = 1;

				bool	bottomRow				= (y == 0);
				int		bottomBlankCells		= 0;
				int		bottomBlankCellsSeen	= 0;

				// If we are on the bottom row first count the number of blank spaces
				if (bottomRow)
				{
					for (int x = 0; x < board.xCells; x++)
					{
						Cell curCell = board.grid[x][y];

						if (curCell.letter == '\0')
						{
							bottomBlankCells++;
						}
					}

					// Assign hMinLen to -1 at the start of the bottom row, this will then be set to 1 only when it first encounters a NON blank cell
					// We will also only add horizontal words if hMinLen is NOT -1. Doing this will make words connected on the bottom row so no gaps are between them
					hMinLen = -1;
				}

				for (int x = board.xCells - 1; x >= 0; x--)
				{
					Cell curCell	= board.grid[x][y];
					Cell belowCell	= (y == 0) ? null : board.grid[x][y - 1];

					// Update the number of blank cells in the column
					if (curCell.letter == '\0')
					{
						colBlanks[x]++;

						if (bottomRow)
						{
							bottomBlankCellsSeen++;
						}
					}
					else if (bottomRow && len <= board.yCells)
					{
						if (bottomBlankCellsSeen > 0 && x > 0 && board.grid[x - 1][0].letter != '\0')
						{
							possiblePlacements.Insert(0, (new Placement(curCell, Placement.Type.ShiftRight)));
						}

						if (bottomBlankCells - bottomBlankCellsSeen > 0 && x < board.xCells - 1 && board.grid[x + 1][0].letter != '\0')
						{
							possiblePlacements.Insert(0, new Placement(curCell, Placement.Type.ShiftLeft));
						}
					}

					// Update the maximum length a word can be
					if (colBlanks[x] > 0 && (belowCell == null || belowCell.letter != '\0'))
					{
						hMaxLen++;
					}
					else
					{
						hMaxLen = 0;
					}

					// Update the minumum length a word can be
					if (firstWord || curCell.letter != '\0')
					{
						hMinLen = 1;
					}
					else if (hMinLen != -1)
					{
						hMinLen++;
					}

					// Check if possible horizontal start
					if (hMinLen != -1 && len <= hMaxLen && len >= hMinLen)
					{
						possiblePlacements.Add(new Placement(curCell, Placement.Type.Horizontal));
						numHorizontal++;
					}

					// Check if possible vertical start
					if (!board.placedVerticals[x] && colBlanks[x] >= len && ((bottomRow && firstWord) || (curCell.letter != '\0')))
					{
						possiblePlacements.Insert(0, new Placement(curCell, Placement.Type.Vertical));
					}
				}
			}

			return possiblePlacements;
		}

		/// <summary>
		/// Places the givne word on the board starting at the given starting cell
		/// </summary>
		private static void PlaceWordAt(Board board, string wordToPlace, Placement placement)
		{
			//Debug.LogFormat("PlaceWordAt: {0}, x:{1}, y:{2}, type:{3}", wordToPlace, placement.cell.x, placement.cell.y, placement.type);

			switch (placement.type)
			{
				case Placement.Type.Vertical:
					PushLettersUp(board, placement.cell.x, placement.cell.y, wordToPlace.Length);
					break;
				case Placement.Type.ShiftRight:
					ShiftColumnsRight(board, placement.cell.x);
					break;
				case Placement.Type.ShiftLeft:
					ShiftColumnsLeft(board, placement.cell.x);
					break;
			}

			// 50% chance the word is flipped
			bool flipWord = board.rand.Next(0, 2) == 0;

			for (int i = 0; i < wordToPlace.Length; i++)
			{
				int cellX = placement.cell.x + (placement.type == Placement.Type.Horizontal ? i : 0);
				int cellY = placement.cell.y + (placement.type == Placement.Type.Horizontal ? 0 : i);

				Cell cell = GetCell(board, cellX, cellY);

				if (placement.type == Placement.Type.Horizontal)
				{
					// If we are adding a horizontal word the move all cells up one
					PushLettersUp(board, cellX, cellY, 1);
				}

				int letterIndex = flipWord ? wordToPlace.Length - i - 1 : i;

				ChangeCellLetter(board, cell, wordToPlace[letterIndex]);
			}

			if (placement.type != Placement.Type.Horizontal)
			{
				ChangePlacedVertical(board, placement.cell.x, true);
			}

			//PrintCells(board, "After place");
		}

		/// <summary>
		/// Pushes all letters up starting at the given x/y. Returns the top most blank cell after all letters have been pushed up.
		/// </summary>
		private static void PushLettersUp(Board board, int x, int y, int numSpaces)
		{
			if (GetCell(board, x, y).letter == '\0')
			{
				return;
			}

			PushLettersUp(board, x, y + 1, numSpaces);

			ChangeCellLetter(board, GetCell(board, x, y + numSpaces), GetCell(board, x, y).letter);
		}

		private static void ShiftColumnsLeft(Board board, int colX)
		{
			for (int x = 1; x <= colX; x++)
			{
				for (int y = 0; y < board.yCells; y++)
				{
					Cell fromCell	= GetCell(board, x, y);
					Cell toCell		= GetCell(board, x - 1, y);

					ChangeCellLetter(board, toCell, fromCell.letter);

					if (x == colX)
					{
						ChangeCellLetter(board, fromCell, '\0');
					}
				}
				
				ChangePlacedVertical(board, x - 1, board.placedVerticals[x]);
			}
		}

		private static void ShiftColumnsRight(Board board, int colX)
		{
			for (int x = board.xCells - 2; x >= colX; x--)
			{
				for (int y = 0; y < board.yCells; y++)
				{
					Cell fromCell	= GetCell(board, x, y);
					Cell toCell		= GetCell(board, x + 1, y);

					ChangeCellLetter(board, toCell, fromCell.letter);

					if (x == colX)
					{
						ChangeCellLetter(board, fromCell, '\0');
					}
				}

				ChangePlacedVertical(board, x + 1, board.placedVerticals[x]);
			}
		}

		/// <summary>
		/// CHanges the cells letter, logs an Undo action
		/// </summary>
		private static void ChangeCellLetter(Board board, Cell cell, char toLetter)
		{
			SetUndoForCell(board, cell);

			cell.letter = toLetter;
		}

		/// <summary>
		/// CHanges the placement value, logs an Undo action
		/// </summary>
		private static void ChangePlacedVertical(Board board, int index, bool toValue)
		{
			SetUndoForPV(board, index);

			board.placedVerticals[index] = toValue;
		}

		/// <summary>
		/// Gets the current Undo for the given cell
		/// </summary>
		private static void SetUndoForCell(Board board, Cell cell)
		{
			string key = cell.x + "_" + cell.y;

			Dictionary<string, Undo> curUndos = board.undos[board.undos.Count - 1];

			if (!curUndos.ContainsKey(key))
			{
				CellUndo undo = new CellUndo();

				undo.cell	= cell;
				undo.letter	= cell.letter;

				curUndos.Add(key, undo);
			}
		}

		/// <summary>
		/// Gets the current Undo for the given cell
		/// </summary>
		private static void SetUndoForPV(Board board, int index)
		{
			string key = index.ToString();

			Dictionary<string, Undo> curUndos = board.undos[board.undos.Count - 1];

			if (!curUndos.ContainsKey(key))
			{
				PVUndo undo = new PVUndo();

				undo.index	= index;
				undo.value	= board.placedVerticals[index];

				curUndos.Add(key, undo);
			}
		}

		/// <summary>
		/// Removed the given word that has been placed on the board starting at the given starting cell
		/// </summary>
		private static void UndoChanges(Board board)
		{
			Dictionary<string, Undo> curUndos = board.undos[board.undos.Count - 1];

			foreach (KeyValuePair<string, Undo> pair in curUndos)
			{
				Undo undo = pair.Value;

				switch (undo.UndoType)
				{
					case Undo.Type.Cell:
						Cell cell = (undo as CellUndo).cell;
						cell.letter	= (undo as CellUndo).letter;
						break;
					case Undo.Type.PlacedVertical:
						board.placedVerticals[(undo as PVUndo).index] = (undo as PVUndo).value;
						break;
				}
			}

			board.undos.RemoveAt(board.undos.Count - 1);
		}

		/// <summary>
		/// Gets the cell at the give x/y coords, expands the board if the y is out of bounds
		/// </summary>
		private static Cell GetCell(Board board, int x, int y)
		{
			while (y >= board.yCells)
			{
				for (int i = 0; i < board.xCells; i++)
				{
					Cell cell = new Cell();

					cell.x = i;
					cell.y = board.yCells;

					board.grid[i].Add(cell);
				}

				board.yCells++;
			}

			return board.grid[x][y];
		}

		private static void PrintCells(Board board, string desc)
		{
			string prnt = desc;

			for (int y = board.yCells - 1; y >= 0; y--)
			{
				prnt += "\n" + y;

				for (int x = 0; x < board.xCells; x++)
				{
					Cell cell = board.grid[x][y];

					prnt += (cell.letter == '\0') ? "_" : cell.letter.ToString();
				}

				if (y == 0)
				{
					prnt += "\n ";
					for (int x = 0; x < board.xCells; x++)
					{
						prnt += x;
					}
				}
			}

			Debug.Log(prnt);
		}

		#endregion
	}
}
