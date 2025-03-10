//
// Copyright (c) 2023 Warped Imagination. All rights reserved. 
//

using UnityEditor;
using UnityEngine;

namespace WarpedImagination.SceneViewBookmarkTool
{
    /// <summary>
    /// Displays entries for the Scene View Bookmarks settings under Unitys Preferences window
    /// </summary>
    public class SceneViewBookmarksSettingsProvider : SettingsProvider
    {
        const string NOTIFICATION_PREFERENCE = "SceneViewBookmarksNotifications";

        public SceneViewBookmarksSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        { }

        public static bool ShowNotifications
        {
            get { return EditorPrefs.GetBool(NOTIFICATION_PREFERENCE, true); }
            set { EditorPrefs.SetBool(NOTIFICATION_PREFERENCE, value); }
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            GUILayout.Space(20f);

            GUIContent showNotificationsContent = new GUIContent("Show Notifications", "Toggles showing scene view notifications when switching bookmarks");
            bool showNotifications = EditorGUILayout.Toggle(showNotificationsContent, ShowNotifications);
            if (ShowNotifications != showNotifications)
                ShowNotifications = showNotifications;

            GUILayout.Space(10f);

            GUIContent resetQuickSelectionWindowContent = new GUIContent("Reset Quick Selection Window", "Resets the position and size of the bookmark selection window");
            if (GUILayout.Button(resetQuickSelectionWindowContent, GUILayout.Width(200f)))
                SceneViewBookmarksQuickSelectionWindow.ResetPosition();
        }


        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            // Note: change the first argument path to move these settings elsewhere under Preferences window
            // Note: change second argument if you prefer the settings to be under Player Settings
            return new SceneViewBookmarksSettingsProvider("Preferences/Tools/Scene View Bookmarks Tool", SettingsScope.User);
        }

    }
}