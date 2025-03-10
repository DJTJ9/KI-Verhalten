//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using System;
using UnityEditor;
using UnityEngine;

namespace WarpedImagination.SceneViewBookmarkTool
{

    /// <summary>
    /// Scene view bookmark captures a point within a scene to jump to
    /// </summary>
	[Serializable]
    public class SceneViewBookmark
    {
        #region Variables

        [SerializeField]
        [HideInInspector]
        string _name = null;

        [SerializeField]
        [HideInInspector]
        SceneViewBookmarkOrientation _orientation;

        [SerializeField]
        [HideInInspector]
        Texture2D _thumbnail = null;

        #endregion

        #region Properties

        public string Name { get { return _name; } }

        public Texture2D Thumbnail { get { return _thumbnail; } }

        #endregion

        #region Construction

        public SceneViewBookmark(string name, Vector3 pivot, Quaternion rotation, float size, bool isOrthographic, Texture2D thumbnail)
        {
            // setup
            _name = name;
            _orientation = new SceneViewBookmarkOrientation()
            {
                Pivot = pivot,
                Rotation = rotation,
                Size = size,
                IsOrthographic = isOrthographic

            };
            _thumbnail = thumbnail;
        }

        #endregion

        #region Management 

        /// <summary>
        /// Rename the bookmark
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Set the last scene view orientation
        /// </summary>
        public void SetSceneViewOrientation()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                _orientation.SetSceneViewOrientation(sceneView);
                if (SceneViewBookmarksSettingsProvider.ShowNotifications)
                    sceneView.ShowNotification(new GUIContent(this.Name), 1);
            }
        }

        /// <summary>
        /// Change the orientation
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="isOrthographic"></param>
        public void ChangeOrientation(Vector3 pivot, Quaternion rotation, float size, bool isOrthographic)
        {
            _orientation.Pivot = pivot;
            _orientation.Rotation = rotation;
            _orientation.Size = size;
            _orientation.IsOrthographic = isOrthographic;
        }

        /// <summary>
        /// Set the thumbnail
        /// </summary>
        /// <param name="thumbnail"></param>
        public void SetThumbnail(Texture2D thumbnail)
        {
            _thumbnail = thumbnail;
        }

        #endregion
    }
}