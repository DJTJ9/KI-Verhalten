//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarpedImagination.SceneViewBookmarkTool
{

	/// <summary>
	/// Scene View Bookmark Menu are a collection of options for scene view bookmark management
	/// </summary>
	public static class SceneViewBookmarkMenu 
	{
		#region Constants

		const string MENU_PATH = "Tools/Bookmarks/";
		const string PROJECT_MENU_PATH = "Assets/Bookmarks/";
		const string DELETE_MENU_PATH = PROJECT_MENU_PATH + "Delete Bookmark";

		#endregion

		#region Variables

		static int _bookmarkIndex = 0;
		static SceneViewBookmarksDirectory _lastOpenedDirectory = null;
		static int _lastOpenedIndex = 0;

		#endregion

		#region Menu

		/// <summary>
		/// Menu item for opening the bookmarks window
		/// </summary>
		[MenuItem(MENU_PATH + "Bookmarks Window")]
		public static void Init()
		{
			SceneViewBookmarksWindow window = (SceneViewBookmarksWindow)EditorWindow.GetWindow(typeof(SceneViewBookmarksWindow));
			window.titleContent = new GUIContent("Bookmarks", EditorGUIUtility.IconContent("d_SceneViewOrtho").image);
			window.Show();
		}

		/// <summary>
		/// Menu item for creating a bookmark which will create a directory if it does not exist
		/// </summary>
		[MenuItem(MENU_PATH + "Create")]
		public static void CreateSceneViewBookmarkMainMenuItem()
		{
			SceneViewBookmarksDirectory.CreateBookmark();
		}

		[MenuItem(MENU_PATH + "Switch _b")]
		public static void SwitchSceneViewBookmarkMainMenuItem()
		{
			SceneViewBookmarksDirectory directory = SceneViewBookmarksDirectory.Find(EditorSceneManager.GetActiveScene());
			if(directory != null)
            {
				SceneViewBookmark bookmark = directory.GetBookmark(_bookmarkIndex);
				if (bookmark != null)
					bookmark.SetSceneViewOrientation();

				_bookmarkIndex++;
				if (_bookmarkIndex >= directory.Count)
					_bookmarkIndex = 0;
            }
		}

		[MenuItem(DELETE_MENU_PATH, priority = 25)]
		public static void DeleteBookmarkInProjectMenuItem()
        {
			Object selection = Selection.activeObject;
			if (selection != null &&
				selection is Texture2D texture &&
				AssetDatabase.IsSubAsset(selection) &&
				AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(selection)) == typeof(SceneViewBookmarksDirectory))
			{
				SceneViewBookmarksDirectory texturesDirectory = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(selection.GetInstanceID())) as SceneViewBookmarksDirectory;
				SceneViewBookmark texturesBookmark = texturesDirectory.GetBookmark(texture);
				if (texturesBookmark != null)
				{
					if (EditorUtility.DisplayDialog("Delete Bookmark", "Are you sure you want to delete this bookmark?", "Yes", "No"))
						texturesDirectory.DeleteBookmark(texturesBookmark);
				}
			}
		}

		[MenuItem(DELETE_MENU_PATH, true)]
		public static bool DeleteBookmarkInProjectMenuItemValidation()
        {
			Object selection = Selection.activeObject;
			if(selection != null &&
				selection is Texture2D &&
				AssetDatabase.IsSubAsset(selection) &&
				AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(selection)) == typeof(SceneViewBookmarksDirectory))
			{ 
				return true;
            }

			return false;
        }




		/// <summary>
		/// Callback for when an asset is double clicked on the project view
		/// If the asset is a bookmark directory it will open in the scene (and open the scene if needed)
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		[OnOpenAsset]
		public static bool OnOpenAsset(int instanceId, int line)
		{
			UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);

			if(obj is SceneViewBookmarksDirectory directory)
            {
				_lastOpenedIndex = (_lastOpenedDirectory == directory) ? _lastOpenedIndex+1 : 0;
				if (_lastOpenedIndex >= directory.Count)
					_lastOpenedIndex = 0;

				if (directory.HasBookmarks)
					 directory.OpenBookmark(directory.GetBookmark(_lastOpenedIndex));

				_lastOpenedDirectory = directory;

				return true;
            }
			else
			if(obj is Texture2D texture && 
				AssetDatabase.IsSubAsset(instanceId) && 
				AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(instanceId)) == typeof(SceneViewBookmarksDirectory))
            {
				SceneViewBookmarksDirectory texturesDirectory = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(instanceId)) as SceneViewBookmarksDirectory;
				SceneViewBookmark texturesBookmark = texturesDirectory.GetBookmark(texture);
				if (texturesBookmark != null)
					texturesDirectory.OpenBookmark(texturesBookmark);

				return true;
			}

			return false;
		}


		#endregion

		#region Construction

		[InitializeOnLoadMethod]
		static void Initialize()
        {
			EditorSceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

		/// <summary>
		/// Callback from when the active scene changes
		/// </summary>
		/// <param name="replacedScene"></param>
		/// <param name="nextScene"></param>
		private static void OnActiveSceneChanged(Scene replacedScene, Scene nextScene)
        {
			_bookmarkIndex = 0;
		}

        #endregion
	}
}