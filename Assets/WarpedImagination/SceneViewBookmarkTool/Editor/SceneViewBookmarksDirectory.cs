//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarpedImagination.SceneViewBookmarkTool
{
    /// <summary>
    /// Scene view bookmarks directory holds a collection of bookmarks
    /// </summary>
    public class SceneViewBookmarksDirectory : ScriptableObject
	{
		#region Constants

		public const int THUMBNAIL_SIZE = 128;

		#endregion

		#region Variables

		[SerializeField]
		[HideInInspector]
		string _sceneGuid = null;

		[SerializeField]
		[HideInInspector]
		SceneViewBookmark[] _bookmarks = null;

		// stored values for refreshing bookmarks
		SceneViewBookmarkOrientation _savedOrientation;
		SceneViewBookmark _refeshingBookmark = null;

		#endregion

		#region Properties

		public string ScenePath
		{
			get { return AssetDatabase.GUIDToAssetPath(_sceneGuid); }
		}

		public bool HasBookmarks
		{
			get { return _bookmarks != null && _bookmarks.Length > 0; }
		}

		public int Count
		{
			get { return _bookmarks == null ? 0 : _bookmarks.Length; }
		}

		#endregion

		#region Callbacks

		public static Action DirectoryCreated;
		public Action BookmarksChanged;

        #endregion

        #region Construction

        /// <summary>
        /// Finds the bookmarks directory for scene or creates one
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        static SceneViewBookmarksDirectory Get(Scene scene)
		{
			string sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);

			// search for asset
			SceneViewBookmarksDirectory directory = Find(scene);

			if (directory == null)
			{
				directory = ScriptableObject.CreateInstance<SceneViewBookmarksDirectory>();
				directory._sceneGuid = sceneGuid;
				AssetDatabase.CreateAsset(directory, GetDirectoryAssetPath(scene.path));

				DirectoryCreated?.Invoke();
			}

			return directory;
		}

		#endregion

		#region Management

		/// <summary>
		/// Create a bookmark and a directory if required
		/// </summary>
		public static void CreateBookmark()
        {
            // validate
            Scene scene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrWhiteSpace(scene.path))
            {
                Debug.LogError("Scene view bookmarks can only be created on scenes saved in the project");
                return;
            }

            // get name using text dialog box
            string name = TextEntryDialog.Show("Bookmarks","Enter the bookmark name", "Name");
            if (string.IsNullOrEmpty(name))
                return;

            // get directory (or create if doesnt exist)
            SceneViewBookmarksDirectory directory = Get(scene);

			SceneView sceneView = SceneView.lastActiveSceneView;

			// create thumbnail
			Texture2D thumbnail = CreateThumbnail(sceneView, name, THUMBNAIL_SIZE);

			// create bookmark
			SceneViewBookmark bookmark = new SceneViewBookmark(name, sceneView.pivot, sceneView.rotation, sceneView.size, sceneView.orthographic, thumbnail);
			directory.AddBookmark(bookmark);

			// add thumbnail to directory as a sub object
            AssetDatabase.AddObjectToAsset(thumbnail, directory);

#if UNITY_2020_3_OR_NEWER
			AssetDatabase.SaveAssetIfDirty(directory);
#else
			AssetDatabase.SaveAssets();
#endif
		}

		public static Texture2D CreateThumbnail(SceneView sceneView, string name, int thumbnailSize)
        {
			// capture texture rendered from camera
			int size = Mathf.Min(sceneView.camera.pixelWidth, sceneView.camera.pixelHeight);
			Texture2D capture = new Texture2D(size, size, TextureFormat.RGB24, false);
			sceneView.camera.Render();
			RenderTexture.active = sceneView.camera.targetTexture;

			int xOffset = (sceneView.camera.pixelWidth - size) / 2;
			int yOffset = (sceneView.camera.pixelHeight - size) / 2;

			capture.ReadPixels(new Rect(xOffset, yOffset, size, size), 0, 0);
			capture.Apply();

			// resize for thumbnail
			Texture2D thumbnail = capture.BasicResize(thumbnailSize, thumbnailSize);
			thumbnail.name = name;

			return thumbnail;
		}

		/// <summary>
		/// Find the bookmarks directory for provided scene
		/// </summary>
		/// <param name="scene"></param>
		/// <returns></returns>
		public static SceneViewBookmarksDirectory Find(Scene scene)
		{
			return Find(scene.path);
		}

		/// <summary>
		/// Find the bookmarks directory for provided scene path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static SceneViewBookmarksDirectory Find(string path)
		{
			string sceneGuid = AssetDatabase.AssetPathToGUID(path);
			foreach (string directoryGuid in AssetDatabase.FindAssets("t:SceneViewBookmarksDirectory"))
			{
				string pathToAsset = AssetDatabase.GUIDToAssetPath(directoryGuid);
				SceneViewBookmarksDirectory directory = AssetDatabase.LoadAssetAtPath<SceneViewBookmarksDirectory>(pathToAsset);
				if (directory.IsLinkedToScene(sceneGuid))
					return directory;
			}
			return null;
		}

		/// <summary>
		/// Returns whether the directory is for a particular scene
		/// </summary>
		/// <param name="scene"></param>
		/// <returns></returns>
		public bool IsLinkedToScene(Scene scene)
        {
			return IsLinkedToScene(AssetDatabase.AssetPathToGUID(scene.path));
		}

		/// <summary>
		/// Returns whether the directory is for a particular scene
		/// </summary>
		/// <param name="sceneGuid"></param>
		/// <returns></returns>
		public bool IsLinkedToScene(string sceneGuid)
        {
			return this._sceneGuid.Equals(sceneGuid);
        }

		/// <summary>
		/// returns a sequence of bookmarks one at a time
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SceneViewBookmark> GetBookmarks()
		{
			foreach (SceneViewBookmark bookmark in _bookmarks)
				yield return bookmark;
		}

		/// <summary>
		/// Get the bookmark at the provided index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SceneViewBookmark GetBookmark(int index)
		{
			if (_bookmarks == null || _bookmarks.Length <= index)
				return null;

			return _bookmarks[index];
		}

		/// <summary>
		/// Get the bookmark linked to the provided texture
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public SceneViewBookmark GetBookmark(Texture2D texture)
		{
			if (_bookmarks == null)
				return null;

			foreach (SceneViewBookmark bookmark in _bookmarks)
			{
				if (bookmark.Thumbnail == texture)
					return bookmark;
			}

			return null;
		}

		/// <summary>
		/// Delete a bookmark from the dictionary
		/// </summary>
		/// <param name="index"></param>
		public void DeleteBookmark(int index)
        {
			if (_bookmarks == null || _bookmarks.Length <= index)
				return;

			// collect thumbnail to delete
			Texture2D thumbnail = _bookmarks[index].Thumbnail;

            // remove from array
            SceneViewBookmark[] bookmarks = new SceneViewBookmark[_bookmarks.Length - 1];
			if (index > 0)
				Array.Copy(_bookmarks, 0, bookmarks, 0, index);

			if (index < _bookmarks.Length - 1)
				Array.Copy(_bookmarks, index + 1, bookmarks, index, _bookmarks.Length - index - 1);

			_bookmarks = bookmarks;

			// remove thumbnail from asset
			AssetDatabase.RemoveObjectFromAsset(thumbnail);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			BookmarksChanged?.Invoke();
		}

		/// <summary>
		/// Delete a bookmark from the dictionary
		/// </summary>
		/// <param name="bookmark"></param>
		public void DeleteBookmark(SceneViewBookmark bookmark)
        {
			if (_bookmarks == null)
				return;

			// find and delete from array
            for (int i = 0; i < _bookmarks.Length; i++)
            {
				if(_bookmarks[i] == bookmark)
                {
					DeleteBookmark(i);
					return;
                }
            }

        }

		/// <summary>
		/// Add a bookmark to this directory
		/// </summary>
		/// <param name="bookmark"></param>
		void AddBookmark(SceneViewBookmark bookmark)
        {
			if (_bookmarks == null)
			{
				_bookmarks = new SceneViewBookmark[1];
				_bookmarks[0] = bookmark;
			}
			else
			{
				SceneViewBookmark[] bookmarks = new SceneViewBookmark[_bookmarks.Length + 1];
				Array.Copy(_bookmarks, bookmarks, _bookmarks.Length);
				bookmarks[_bookmarks.Length] = bookmark;
				_bookmarks = bookmarks;
			}

			BookmarksChanged?.Invoke();
		}

		/// <summary>
		/// Opens a bookmark checking if we are currently in that scene
		/// </summary>
		public void OpenBookmark(int index)
        {
			if (_bookmarks == null || _bookmarks.Length <= index)
				return;

			OpenBookmark(_bookmarks[index]);
		}

		/// <summary>
		/// Opens a bookmark checking if we are currently in that scene
		/// </summary>
		public void OpenBookmark(SceneViewBookmark bookmark)
		{
			// are we current in this scene
			if (!IsCurrentScene(this.ScenePath))
			{
				if (!OpenScene(this.ScenePath))
					return;
			}

			bookmark.SetSceneViewOrientation();
		}

		/// <summary>
		/// Changes the bookmark to the current sceneview
		/// </summary>
		/// <param name="bookmark"></param>
		public void ChangeBookmark(SceneViewBookmark bookmark)
        {
			SceneView sceneView = SceneView.lastActiveSceneView;

			bookmark.ChangeOrientation(sceneView.pivot, sceneView.rotation, sceneView.size, sceneView.orthographic);

			// remove the current thumbnail
			AssetDatabase.RemoveObjectFromAsset(bookmark.Thumbnail);

			Texture2D thumbnail = CreateThumbnail(sceneView, bookmark.Name, THUMBNAIL_SIZE);
			thumbnail.name = bookmark.Name; 
			bookmark.SetThumbnail(thumbnail);

			// refresh asset
			AssetDatabase.AddObjectToAsset(thumbnail, this);

#if UNITY_2020_3_OR_NEWER
			AssetDatabase.SaveAssetIfDirty(this);
#else
			AssetDatabase.SaveAssets();
#endif

			BookmarksChanged?.Invoke();
		}

		/// <summary>
		/// Ranem a bookmark
		/// </summary>
		/// <param name="bookmark"></param>
		public void RenameBookmark(SceneViewBookmark bookmark)
		{
			// get name using text dialog box
			string name = TextEntryDialog.Show("Bookmarks", "Rename the bookmark", "Name", bookmark.Name);
			if (string.IsNullOrEmpty(name) || bookmark.Name.Equals(name))
				return;
			bookmark.Rename(name);

			// rename the thumbnail
			bookmark.Thumbnail.name = name;

#if UNITY_2020_3_OR_NEWER
			AssetDatabase.SaveAssetIfDirty(this);
#else
			AssetDatabase.SaveAssets();
#endif

			BookmarksChanged?.Invoke();
		}

		/// <summary>
		/// Refresh a thumbnail of a bookmark
		/// </summary>
		/// <param name="bookmark"></param>
		public void RefreshThumbnail(SceneViewBookmark bookmark)
        {
			_refeshingBookmark = bookmark;

			// remove the current thumbnail
			AssetDatabase.RemoveObjectFromAsset(bookmark.Thumbnail);

			// store details
			SceneView sceneView = SceneView.lastActiveSceneView;
			_savedOrientation = SceneViewBookmarkOrientation.CreateFromSceneView(sceneView);

			// move scene view
			bookmark.SetSceneViewOrientation();

			// wait until the scene view performs it's gui function to repaint
			SceneView.beforeSceneGui += OnSceneGUIRepaint;
		}

		void OnSceneGUIRepaint(SceneView sceneView)
        {
			// remove callback
			SceneView.beforeSceneGui -= OnSceneGUIRepaint;

			//capture texture rendered from camera
			Texture2D thumbnail = CreateThumbnail(sceneView, _refeshingBookmark.Name, THUMBNAIL_SIZE);
			_refeshingBookmark.SetThumbnail(thumbnail);

			// refresh asset
			AssetDatabase.AddObjectToAsset(thumbnail, this);

#if UNITY_2020_3_OR_NEWER
			AssetDatabase.SaveAssetIfDirty(this);
#else
			AssetDatabase.SaveAssets();
#endif

			AssetDatabase.Refresh();

			// perform event to notify listeners
			BookmarksChanged?.Invoke();

			_refeshingBookmark = null;

			// reset the position of the scene view
			_savedOrientation.SetSceneViewOrientation(sceneView);
			sceneView.Repaint();
		}


		#endregion

		#region Utilities

		/// <summary>
		/// Gets a path for the directory from a scene path
		/// </summary>
		/// <param name="scenePath"></param>
		/// <returns></returns>
		public static string GetDirectoryAssetPath(string scenePath)
		{
			string sceneName = Path.GetFileNameWithoutExtension(scenePath);
			string path = scenePath.Substring(0, scenePath.Length - Path.GetFileName(scenePath).Length);
			return Path.Combine(path, sceneName + "Bookmarks.asset");
		}

		/// <summary>
		/// Is the path the current scene
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static bool IsCurrentScene(string path)
		{
			return EditorSceneManager.GetActiveScene().path.Equals(path);
		}

		/// <summary>
		/// Opens a scene with a check for user to save
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static bool OpenScene(string path)
		{
			// check scene still exists
			if(AssetDatabase.LoadMainAssetAtPath(path) == null)
            {
				Debug.LogError("Scene no longer exists");
				return false;
            }

			if (EditorSceneManager.GetActiveScene().isDirty)
			{
				if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					return false;
			}

			EditorSceneManager.OpenScene(path);

			return true;
		}

		#endregion

	}
}