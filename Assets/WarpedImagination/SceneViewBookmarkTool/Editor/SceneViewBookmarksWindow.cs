//
// Copyright (c) 2023 Warped Imagination. All rights reserved.
//

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarpedImagination.SceneViewBookmarkTool
{

    /// <summary>
    /// Scene View Bookmarks Window
    /// </summary>
    public class SceneViewBookmarksWindow : EditorWindow
    {
        #region Constants

        readonly Color BAR_BACKGROUND_COLOR = new Color(0.8172549019607843f, 0.8172549019607843f, 0.8172549019607843f);
        readonly Color BAR_BACKGROUND_PRO_COLOR = new Color(0.23529f, 0.23529f, 0.23529f);

        readonly Color HOVER_COLOR = new Color(1f, 1f, 1f, .5f);

        #endregion

        #region Variables

        static Texture2D _createIcon = null;
        static Texture2D _selectIcon = null;
        GUIStyle _buttonStyle = null;
        GUIStyle _contentStyle = null;
        Rect _createButtonRect;
        Vector2 _scrollPos;
        float _thumbnailSize = 1f;
        SceneViewBookmarksDirectory _currentDirectory = null;
        int _gridSelectionIndex = 0;
        GUIContent[] _bookmarksContent = null;

        #endregion

        #region Construction

        private void OnEnable()
        {
            RefreshBookmarksDirectory(EditorSceneManager.GetActiveScene());

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            SceneViewBookmarksDirectory.DirectoryCreated += OnDirectoryCreated;

            _thumbnailSize = EditorPrefs.GetFloat($"{this.GetType().Name}_thumbnailSize", 1f);
        }

        private void OnDisable()
        {
            _currentDirectory = null;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            SceneViewBookmarksDirectory.DirectoryCreated -= OnDirectoryCreated;
        }

        private void OnSceneClosed(Scene scene)
        {
            _currentDirectory = null;
            RefreshBookmarksDirectoryContent();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RefreshBookmarksDirectory(scene);
        }

        private void OnDirectoryCreated()
        {
            RefreshBookmarksDirectory(EditorSceneManager.GetActiveScene());
        }

        #endregion

        #region Management

        void RefreshBookmarksDirectory(Scene scene)
        {
            if (_currentDirectory != null)
                _currentDirectory.BookmarksChanged -= OnBookmarksChanged;

            _currentDirectory = SceneViewBookmarksDirectory.Find(scene);

            if (_currentDirectory != null)
                _currentDirectory.BookmarksChanged += OnBookmarksChanged;

            RefreshBookmarksDirectoryContent();
        }

        void RefreshBookmarksDirectoryContent()
        {
            if (_currentDirectory == null)
            {
                _bookmarksContent = null;
            }
            else
            {
                // create content
                _bookmarksContent = new GUIContent[_currentDirectory.Count];

                for (int i = 0; i < _currentDirectory.Count; i++)
                {
                    SceneViewBookmark bookmark = _currentDirectory.GetBookmark(i);
                    GUIContent content = new GUIContent(bookmark.Name, bookmark.Thumbnail);
                    _bookmarksContent[i] = content;
                }
            }

            Repaint();
        }

        void OnBookmarksChanged()
        {
            RefreshBookmarksDirectoryContent();
        }

        #endregion

        #region Display

        void OnGUI()
        {
            if (_createIcon == null || _selectIcon == null)
            {
                // get path to this script for relative path to icon
                string path = AssetDatabaseExtensions.GetDirectoryOfScript<SceneViewBookmarksWindow>();

                string createPath = Path.Combine(path, "Icons/BookmarksCreateIcon.png");
                _createIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(createPath);

                string selectPath = Path.Combine(path, "Icons/BookmarksSelectIcon.png");
                _selectIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(selectPath);
            }

            // toolbar at the top
            TopBarGUI();

            // display all bookmarks in a grid
            if (_currentDirectory)
            {
                // create style
                if (_contentStyle == null)
                {
                    _contentStyle = new GUIStyle(GUI.skin.button);
                    _contentStyle.imagePosition = ImagePosition.ImageAbove;
                    _contentStyle.fontSize = 14;
                }

                // display as list
                if (Mathf.Approximately(_thumbnailSize, .5f))
                {
                    EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(this.position.width - 20));

                    for (int i = 0; i < _bookmarksContent.Length; i++)
                    {
                        if (GUILayout.Button(_bookmarksContent[i].text, GUILayout.Width(200f)))
                        {
                            _gridSelectionIndex = i;
                            _currentDirectory.OpenBookmark(_gridSelectionIndex);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }
                else // display as grid
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(this.position.width - 20));

                    // alter the size of the grid elements by slider in top tab
                    int thumbnailSize = Mathf.FloorToInt((float)SceneViewBookmarksDirectory.THUMBNAIL_SIZE * _thumbnailSize);
                    _contentStyle.fixedWidth = thumbnailSize;
                    _contentStyle.fixedHeight = thumbnailSize;

                    int gridXCount = Mathf.Max(1, Mathf.FloorToInt((this.position.width - (thumbnailSize / 1.2f)) / thumbnailSize));

                    int gridSelectionIndex = GUILayout.SelectionGrid(_gridSelectionIndex, _bookmarksContent, gridXCount, _contentStyle);
                    if (gridSelectionIndex != _gridSelectionIndex)
                    {
                        _gridSelectionIndex = gridSelectionIndex;
                        _currentDirectory.OpenBookmark(_gridSelectionIndex);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                GUILayout.Label("No directory found");
            }

        }

        void TopBarGUI()
        {
            // create style
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.clipping = TextClipping.Overflow;
                _buttonStyle.overflow = new RectOffset(30, 30, 30, 30);
                _buttonStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            Color topBarBackgroundColor = (EditorGUIUtility.isProSkin ? BAR_BACKGROUND_PRO_COLOR : BAR_BACKGROUND_COLOR);
            Rect topbarRect = new Rect(0f, 0f, Screen.width, 22f);
            EditorGUI.DrawRect(topbarRect, topBarBackgroundColor);
            Rect topbarRectLine = new Rect(0f, 22f, Screen.width, 1f);
            EditorGUI.DrawRect(topbarRectLine, Color.black);

            GUILayout.BeginHorizontal();

            Color originalBackgroundColor = GUI.backgroundColor;

            bool hover = Event.current.type == EventType.Repaint && _createButtonRect.Contains(Event.current.mousePosition);

            GUI.backgroundColor = hover ? HOVER_COLOR : Color.clear;

            GUIContent createButtonContent = new GUIContent(_createIcon);
            createButtonContent.tooltip = "Create Bookmark";
            if (GUILayout.Button(createButtonContent, _buttonStyle, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                SceneViewBookmarksDirectory.CreateBookmark();
                RefreshBookmarksDirectory(EditorSceneManager.GetActiveScene());
                GUIUtility.ExitGUI();
                return;
            }

            GUILayout.Space(10f);

            GUIContent selectButtonContent = new GUIContent(_selectIcon);
            selectButtonContent.tooltip = "Select Bookmark Directory In Project";
            if (GUILayout.Button(selectButtonContent, _buttonStyle, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                Selection.activeObject = _currentDirectory;
            }

            if (Event.current.type == EventType.Repaint)
                _createButtonRect = GUILayoutUtility.GetLastRect();


            GUI.backgroundColor = originalBackgroundColor;

            GUILayout.FlexibleSpace();

            // slider to change the size of the thumbnails
            float thumbnailSize = GUILayout.HorizontalSlider(_thumbnailSize, 0.5f, 1f, GUILayout.Width(70f));
            if (!Mathf.Approximately(_thumbnailSize, thumbnailSize))
            {
                EditorPrefs.SetFloat($"{this.GetType().Name}_thumbnailSize", thumbnailSize);
                _thumbnailSize = thumbnailSize;
            }

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}