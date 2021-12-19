using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bimbimnet.WordBlocks
{
	public class BoardCreatorWorker : Worker
	{
		#region Properties

		public List<string> Words	{ get; set; }
		public int			Rows	{ get; set; }
		public int			Cols	{ get; set; }
		public int			Seed	{ get; set; }

		public BoardData FinishedBoardDada { get; private set; }

		#endregion

		#region Protected Methods

		protected override void Begin() { }

		protected override void DoWork()
		{
			FinishedBoardDada = BoardCreator.Create(Words, Rows, Cols, Seed);
			Stop();
		}

		#endregion
	}
}
