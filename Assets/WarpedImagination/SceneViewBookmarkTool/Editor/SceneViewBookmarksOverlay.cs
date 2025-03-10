//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Overlays were introduced in Unity Editor 2021 
#if UNITY_2021_1_OR_NEWER
using UnityEditor.Overlays;
using UnityEditor.Toolbars;

namespace WarpedImagination.SceneViewBookmarkTool
{

    /// <summary>
    /// Scene View Bookmarks Overlay allows users to skip through the scenes bookmarks
    /// </summary>
    [Overlay(typeof(SceneView), "Scene Bookmarks")]
    public class SceneViewBookmarksOverlay : ToolbarOverlay
    {
        #region Construction

        SceneViewBookmarksOverlay() : base(CreateToolbarButton.ID, DropdownToggle.ID) { }

        #endregion

        #region Controls

        [EditorToolbarElement(ID, typeof(SceneView))]
        class CreateToolbarButton : EditorToolbarButton, IAccessContainerWindow
        {
            public const string ID = "SceneViewBookmarksOverlay/Create";

            public EditorWindow containerWindow { get; set; }

            public CreateToolbarButton()
            {
                this.text = "Create";
                this.tooltip = "Create a bookmark for this view in the scene";

                // get path to this script for relative path to icon
                string path = AssetDatabaseExtensions.GetDirectoryOfScript<SceneViewBookmarksOverlay>();
                path = Path.Combine(path, "Icons/BookmarksCreateIcon.png");

                this.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                
                clicked += OnClick;
            }

            void OnClick()
            {
                SceneViewBookmarksDirectory.CreateBookmark();
            }
        }


        [EditorToolbarElement(ID, typeof(SceneView))]
        class DropdownToggle : EditorToolbarDropdownToggle, IAccessContainerWindow
        {
            public const string ID = "SceneViewBookmarksOverlay/Bookmarks";

            public EditorWindow containerWindow { get; set; }

            DropdownToggle()
            {
                text = "Bookmarks";
                tooltip = "Select a bookmark to jump to";

                // get path to this script for relative path to icon
                string path = AssetDatabaseExtensions.GetDirectoryOfScript<SceneViewBookmarksOverlay>();
                path = Path.Combine(path, "Icons/BookmarksIcon.png"); 

                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                dropdownClicked += ShowBookmarksMenu;
            }

            void ShowBookmarksMenu()
            {
                GenericMenu menu = new GenericMenu();
                menu.allowDuplicateNames = true;

                SceneViewBookmarksDirectory directory = SceneViewBookmarksDirectory.Find(EditorSceneManager.GetActiveScene());
                if (directory == null)
                {
                    menu.AddDisabledItem(new GUIContent("No Bookmarks"));
                }
                else
                {
                    foreach(SceneViewBookmark bookmark in directory.GetBookmarks())
                        menu.AddItem(new GUIContent(bookmark.Name), false, () => bookmark.SetSceneViewOrientation());
                }

                menu.ShowAsContext();
            }
        }

        #endregion

    }
}
#endif