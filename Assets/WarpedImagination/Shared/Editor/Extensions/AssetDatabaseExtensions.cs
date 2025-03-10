//
// Copyright (c) 2023 Warped Imagination. All rights reserved. 
//

using System.IO;
using UnityEditor;

namespace WarpedImagination
{
    /// <summary>
    /// Asset Database Extensions
    /// </summary>
    public static class AssetDatabaseExtensions 
    {
        /// <summary>
        /// Get path to a script
        /// </summary>
        /// <typeparam name="T">script</typeparam>
        /// <returns></returns>
        public static string GetPathToScript<T>() where T : class
        {
            string name = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets($"t:Script {name}");
            if (guids == null || guids.Length == 0)
                return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return path;
        }

        /// <summary>
        /// Get the directory of a script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetDirectoryOfScript<T>() where T : class
        {
            string relativePath = GetPathToScript<T>();
            return string.IsNullOrEmpty(relativePath) ? null : Path.GetDirectoryName(relativePath);
        }
    }
}
