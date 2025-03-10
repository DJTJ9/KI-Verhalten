//
// Copyright (c) 2023 Warped Imagination. All rights reserved. 
//

using UnityEngine;

namespace WarpedImagination
{
	/// <summary>
	/// Texture2D Extensions
	/// </summary>
	public static class Texture2DExtensions
	{
		/// <summary>
		/// Performa a very basic resize of a texture
		/// </summary>
		/// <param name="newWidth"></param>
		/// <param name="newHeight"></param>
		/// <returns></returns>
		public static Texture2D BasicResize(this Texture2D source, int newWidth, int newHeight)
		{
			source.filterMode = FilterMode.Bilinear;
			RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
			rt.filterMode = FilterMode.Bilinear;
			RenderTexture.active = rt;
			Graphics.Blit(source, rt, Vector2.one, new Vector2(0, 0));
			Texture2D texture = new Texture2D(newWidth, newHeight);

			texture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
			texture.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(rt);
			return texture;
		}
	}
}