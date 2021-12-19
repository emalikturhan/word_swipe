using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bimbimnet.WordBlocks
{
	public static class MenuItems
	{
		[MenuItem("Bimbimnet/Word Swipe/Delete Save Data")]
		public static void DeleteSaveData()
		{
			if (!System.IO.File.Exists(SaveManager.Instance.SaveFilePath))
			{
				EditorUtility.DisplayDialog("Delete Save File", "There is no save file.", "Ok");

				return;
			}

			bool delete = EditorUtility.DisplayDialog("Delete Save File", "Delete the save file located at " + SaveManager.Instance.SaveFilePath, "Yes", "No");

			if (delete)
			{
				System.IO.File.Delete(SaveManager.Instance.SaveFilePath);

				EditorUtility.DisplayDialog("Delete Save File", "Save file has been deleted.", "Ok");
			}
		}

		[MenuItem("Bimbimnet/Word Swipe/Print Save File To Console")] 
		public static void PrintSaveFileToConsole()
		{
			if (!System.IO.File.Exists(SaveManager.Instance.SaveFilePath))
			{
				EditorUtility.DisplayDialog("Delete Save File", "There is no save file.", "Ok");

				return;
			}

			string contents = System.IO.File.ReadAllText(SaveManager.Instance.SaveFilePath);

			Debug.Log(contents);
		}
	}
}