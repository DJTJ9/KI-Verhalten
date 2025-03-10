//
// Copyright (c) 2023 Warped Imagination. All rights reserved. 
//

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;

namespace WarpedImagination.SceneViewBookmarkTool
{
    /// <summary>
    /// Scene View Bookmarks Quick Selection Window to select a bookmark within a pop up window
    /// </summary>
    public class SceneViewBookmarksQuickSelectionWindow : EditorWindow
    {
        const float PADDING = 2f;

        SceneViewBookmarksDirectory _currentDirectory = null;
        GUIContent[] _bookmarksContent = null;

        GUIStyle _contentStyle = null;
        GUIStyle _labelStyle = null;

        // selection is static to retain index between sessions
        static int _selectionIndex = 0;
        bool _selectionMade = true;
        Vector2 _scrollPos;
        int _gridXCount = 1;
        float _contentHeight = 0f;
        float _totalContentHeight = 0f;
        float _scrollExtent = 0f;
        bool _gridCalculated = false;
        Rect _startingPosition;
        Vector2 _previousSize;

        [Shortcut("SceneViewBookmarksQuickSelectionWindow/Show", KeyCode.B, ShortcutModifiers.Alt)]
        static void ShowSceneViewBookmarksQuickSelectionWindow()
        {
            SceneViewBookmarksQuickSelectionWindow dialog = EditorWindow.CreateWindow<SceneViewBookmarksQuickSelectionWindow>();
            dialog.titleContent = new GUIContent("Bookmarks Directory");

            dialog.LoadPosition();

            dialog.ShowModalUtility();
        }

        void OnEnable()
        {
            _currentDirectory = SceneViewBookmarksDirectory.Find(EditorSceneManager.GetActiveScene());
            if (_currentDirectory != null)
            {
                _bookmarksContent = new GUIContent[_currentDirectory.Count];

                for (int i = 0; i < _currentDirectory.Count; i++)
                {
                    SceneViewBookmark bookmark = _currentDirectory.GetBookmark(i);
                    GUIContent content = new GUIContent(bookmark.Name, bookmark.Thumbnail);
                    _bookmarksContent[i] = content;
                }

                if (_selectionIndex >= _currentDirectory.Count)
                    _selectionIndex = 0;
            }

            _startingPosition = this.position;
            _previousSize = this.position.size;
        }

        private void OnLostFocus()
        {
            SavePosition();
        }

        void OnGUI()
        {
            // check if the current scene has a bookmarks directory
            if(_currentDirectory == null || _currentDirectory.Count == 0)
            {
                // listen to input
                if (Event.current != null && Event.current.modifiers != EventModifiers.Alt)
                {
                    Close();
                    GUIUtility.ExitGUI();
                }

                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    _labelStyle.fontSize = 20;
                }

                GUILayout.Space(20f);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("No Bookmarks Directory For Active Scene", _labelStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                return;
            }

            // listen to input
            if (Event.current != null && Event.current.modifiers == EventModifiers.Alt)
            {
                if (Event.current.keyCode == KeyCode.B && !_selectionMade)
                {
                    _selectionMade = true;
                    _selectionIndex++;
                    if (_selectionIndex >= _currentDirectory.Count)
                        _selectionIndex = 0;

                    // do this to refresh the window as modal window will not update
                    // unless action happens from the editor
                    Repaint();
                }

                if (Event.current.type == EventType.KeyUp)
                {
                    _selectionMade = false;
                }
            }
            else
            {
                OpenBookmark(_selectionIndex);
            }

            // content
            if (_contentStyle == null)
            {
                _contentStyle = new GUIStyle(GUI.skin.button);
                _contentStyle.imagePosition = ImagePosition.ImageAbove;
                _contentStyle.fontSize = 14;
            }

            // layout
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            int selectionIndex = GUILayout.SelectionGrid(_selectionIndex, _bookmarksContent, _gridXCount, _contentStyle);

            // check if a selection was made directly on the grid
            if (selectionIndex != _selectionIndex)
                OpenBookmark(selectionIndex);

            EditorGUILayout.EndScrollView();

            CalculateGrid();

            ScrollToSelectedIndex(_selectionIndex);
        }

        void CalculateGrid()
        {
            // check for resize
            if (this.position.size != _previousSize)
            {
                _previousSize = this.position.size;
                _gridCalculated = false;
            }

            // create grid
            if (!_gridCalculated)
            {
                _gridCalculated = true;
                _gridXCount = Mathf.FloorToInt((this.position.width - (SceneViewBookmarksDirectory.THUMBNAIL_SIZE / 1.2f)) / SceneViewBookmarksDirectory.THUMBNAIL_SIZE);
                int gridYCount = Mathf.CeilToInt((float)_currentDirectory.Count / (float)_gridXCount);
                float windowHeight = this.position.height;
                _contentHeight = _contentStyle.CalcSize(_bookmarksContent[0]).y + PADDING;
                _totalContentHeight = (_contentHeight * gridYCount) + PADDING;
                _scrollExtent = _totalContentHeight - windowHeight;
            }
        }

        void ScrollToSelectedIndex(int selectedIndex)
        {
            int selectedRow = selectedIndex / _gridXCount;
            float contentScrollPosition = selectedRow == 0 ? 0f : _contentHeight * (selectedRow + 1);
            float normalizedPosition = contentScrollPosition / _totalContentHeight;
            _scrollPos.y = normalizedPosition * _scrollExtent;
        }

        void OpenBookmark(int index)
        {
            _currentDirectory.OpenBookmark(index);
            Close();
            GUIUtility.ExitGUI();
        }

        static string GetPrefName(string parameter)
        {
            return $"{typeof(SceneViewBookmarksQuickSelectionWindow).Name}_{parameter}";
        }

        void SavePosition()
        {
            if (this.position != _startingPosition)
            {
                EditorPrefs.SetFloat(GetPrefName("x"), this.position.x);
                EditorPrefs.SetFloat(GetPrefName("y"), this.position.y);
                EditorPrefs.SetFloat(GetPrefName("width"), this.position.width);
                EditorPrefs.SetFloat(GetPrefName("height"), this.position.height);
            }
        }

        void LoadPosition()
        {
            float screenWidth = Screen.currentResolution.width;
            float screenHeight = Screen.currentResolution.height;
            Vector2 dialogSize = new Vector2(screenWidth * .3f, screenHeight * .3f);

            float windowX = (screenWidth - dialogSize.x) * .5f;
            float windowY = (screenHeight - dialogSize.y) * .5f;

            Rect rect = new Rect(windowX, windowY, dialogSize.x, dialogSize.y);

            float x = EditorPrefs.GetFloat(GetPrefName("x"), rect.x);
            float y = EditorPrefs.GetFloat(GetPrefName("y"), rect.y);
            float width = EditorPrefs.GetFloat(GetPrefName("width"), rect.width);
            float height = EditorPrefs.GetFloat(GetPrefName("height"), rect.height);

            this.position = new Rect(x, y, width, height);
        }

        /// <summary>
        /// Reset the position of the window for opening in the future
        /// </summary>
        public static void ResetPosition()
        {
            EditorPrefs.DeleteKey(GetPrefName("x"));
            EditorPrefs.DeleteKey(GetPrefName("y"));
            EditorPrefs.DeleteKey(GetPrefName("width"));
            EditorPrefs.DeleteKey(GetPrefName("height"));
        }
    }
}