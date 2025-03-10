//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using System;
using UnityEditor;
using UnityEngine;

namespace WarpedImagination.SceneViewBookmarkTool
{
    /// <summary>
    /// The setup for the orientation of a bookmark
    /// </summary>
    [Serializable]
    public struct SceneViewBookmarkOrientation 
    {
        public Vector3 Pivot;
        public Quaternion Rotation;
        public float Size;
        public bool IsOrthographic;

        public static SceneViewBookmarkOrientation CreateFromSceneView(SceneView sceneView)
        {
            SceneViewBookmarkOrientation orientation = new SceneViewBookmarkOrientation()
            {
                Pivot = sceneView.pivot,
                Rotation = sceneView.rotation,
                Size = sceneView.size,
                IsOrthographic = sceneView.orthographic
            };
            return orientation;
        }

        /// <summary>
        /// Set the scene view orientation from this Scene View Bookmark Orientation
        /// </summary>
        /// <param name="sceneView"></param>
        public void SetSceneViewOrientation(SceneView sceneView, bool repaint = true)
        {
            sceneView.pivot = Pivot;
            if (!sceneView.in2DMode)
                sceneView.rotation = Rotation;
            sceneView.size = Size;
            sceneView.orthographic = IsOrthographic;
            if(repaint)
                sceneView.Repaint();
        }
    }
}
