//
// Copyright (c) 2022 Warped Imagination. All rights reserved.
//

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WarpedImagination.SceneViewBookmarkTool
{

	/// <summary>
	/// Scene View Bookmarks Directory Editor
	/// </summary>
	[CustomEditor(typeof(SceneViewBookmarksDirectory))]
	public class SceneViewBookmarksDirectoryEditor : Editor
	{
		#region Variables

		SceneViewBookmarksDirectory _directory = null;

		#endregion

		#region Construction

		void OnEnable()
		{
			_directory = (SceneViewBookmarksDirectory)target;
		}

		void OnDisable()
		{
			_directory = null;
		}

        #endregion

        #region Display

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

			GUILayout.Space(10f);

			if (!_directory.HasBookmarks)
			{
				GUILayout.Label("No bookmarks", EditorStyles.boldLabel);
			}
			else
			{
				bool isCurrentScene = _directory.IsLinkedToScene(EditorSceneManager.GetActiveScene());

				// display each child
				foreach (SceneViewBookmark bookmark in _directory.GetBookmarks())
				{
					GUILayout.BeginHorizontal();

					GUILayout.Label(bookmark.Name, GUILayout.Width(150f));

					// open function
					GUIContent openContent = new GUIContent(EditorGUIUtility.IconContent("CollabMoved Icon"));
					openContent.tooltip = "Open the bookmark";

					if (GUILayout.Button(openContent, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32)))
						_directory.OpenBookmark(bookmark);

					GUILayout.Space(5f);

					// rename function
					GUIContent editContent = new GUIContent(EditorGUIUtility.IconContent("CustomTool"));
					editContent.tooltip = "Rename the bookmark";

					if (GUILayout.Button(editContent, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32)))
					{
						_directory.RenameBookmark(bookmark);
						GUIUtility.ExitGUI();
					}

					// only enable the below if we are currently in the scene
					GUI.enabled = isCurrentScene;

					// change the bookmark view
					GUIContent changeContent = new GUIContent(EditorGUIUtility.IconContent("d_SceneViewCamera"));
					changeContent.tooltip = "Change bookmark to current view";

					if (GUILayout.Button(changeContent, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32)))
						_directory.ChangeBookmark(bookmark);

					// refresh the bookmark view
					GUIContent refreshContent = new GUIContent(EditorGUIUtility.IconContent("d_Refresh"));
					refreshContent.tooltip = "Refresh the thumbnail";

					if (GUILayout.Button(refreshContent, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32)))
						_directory.RefreshThumbnail(bookmark);

					GUI.enabled = true;

					GUILayout.Space(5f);

					// delete the bookmark
					GUIContent deleteContent = new GUIContent(EditorGUIUtility.IconContent("CrossIcon"));
					deleteContent.tooltip = "Delete the bookmark";

					if (GUILayout.Button(deleteContent, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32)))
					{
						if(EditorUtility.DisplayDialog("Delete Bookmark","Are you sure you want to delete this bookmark?", "Yes", "No"))
							_directory.DeleteBookmark(bookmark);
					}

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					if (bookmark.Thumbnail)
					{
						if (GUILayout.Button(bookmark.Thumbnail, GUILayout.Width(bookmark.Thumbnail.width), GUILayout.Height(bookmark.Thumbnail.height)))
							_directory.OpenBookmark(bookmark);
					}

					GUILayout.Space(5f);
					EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), new Color(0f, 0f, 0f, 0.3f));
					GUILayout.Space(5f);
				}
			}
		}

		#endregion
	}
}