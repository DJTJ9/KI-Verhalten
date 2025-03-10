//
// Copyright (c) 2022 Warped Imagination. All rights reserved. 
//

using UnityEditor;

namespace WarpedImagination.SceneViewBookmarkTool
{
	/// <summary>
	/// A set of asset processors dealing with actions that happen to the asset database
	/// </summary>
    public class SceneViewBookmarksAssetModificationProcessor : UnityEditor.AssetModificationProcessor
	{
		/// <summary>
		/// We use this to check for scene renaming
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="destinationPath"></param>
		/// <returns></returns>
		static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
			// is this is a scene
			if (AssetDatabase.GetMainAssetTypeAtPath(sourcePath) != typeof(SceneAsset))
				return AssetMoveResult.DidNotMove;

			// check if scene has a directory
			SceneViewBookmarksDirectory directory = SceneViewBookmarksDirectory.Find(sourcePath);
			if(directory)
            {
				if(EditorUtility.DisplayDialog("Scene Name Changed", "Would you like to rename the bookmarks directory?","Yes","No"))
                {
					string directoyPath = AssetDatabase.GetAssetPath(directory);
					string newDirectoryPath = SceneViewBookmarksDirectory.GetDirectoryAssetPath(destinationPath);
					AssetDatabase.MoveAsset(directoyPath, newDirectoryPath);
                }
            }

			return AssetMoveResult.DidNotMove;
		}

		/// <summary>
		/// We use this to check for scene deletion
		/// </summary>
		/// <param name="path"></param>
		/// <param name="removeAssetOptions"></param>
		/// <returns></returns>
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions removeAssetOptions)
		{
			// is this is a scene
			if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(SceneAsset))
				return AssetDeleteResult.DidNotDelete;

			// check if scene has a directory
			SceneViewBookmarksDirectory directory = SceneViewBookmarksDirectory.Find(path);
			if (directory)
			{
				if (EditorUtility.DisplayDialog("Scene Deleted", "Would you like to delete the bookmarks directory as well? You cannot undo the directoy deletion", "Yes", "No"))
				{
					string directoyPath = AssetDatabase.GetAssetPath(directory);
					AssetDatabase.DeleteAsset(directoyPath);
				}
			}

			return AssetDeleteResult.DidNotDelete;
		}
	}
}