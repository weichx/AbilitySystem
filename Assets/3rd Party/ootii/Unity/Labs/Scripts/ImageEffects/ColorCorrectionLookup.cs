using System;
using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu ("Image Effects/Color Adjustments/Color Correction (3D Lookup Texture)")]
	public class ColorCorrectionLookup : PostEffectsBase
	{
		public Shader shader;
		private Material material;
		public Texture3D lut = null;

		public override bool CheckResources ()
		{
			CheckSupport (false);

			material = CheckShaderAndCreateMaterial (shader, material);

			if (!isSupported || !SystemInfo.supports3DTextures)
				ReportAutoDisable ();
			return isSupported;
		}

		void OnDisable ()
		{
			if (material) {
				DestroyImmediate (material);
				material = null;
			}
		}

		public static bool ValidDimensions (Texture2D tex2d)
		{
			if (!tex2d)
				return false;
			int h = tex2d.height;
			if (h != Mathf.FloorToInt (Mathf.Sqrt (tex2d.width))) {
				return false;
			}
			return true;
		}

		public static Texture3D Convert (Texture2D temp2DTex)
		{

			if (!temp2DTex)
				throw new ArgumentNullException ("temp2DTex");

			// conversion fun: the given 2D texture needs to be of the format
			//  w * h, wheras h is the 'depth' (or 3d dimension 'dim') and w = dim * dim

            
			int dim = temp2DTex.width * temp2DTex.height;
			dim = temp2DTex.height;

			if (!ValidDimensions (temp2DTex)) {
				Debug.LogWarning ("The given 2D texture " + temp2DTex.name + " cannot be used as a 3D LUT.");
				return null;
			}

			var c = temp2DTex.GetPixels ();
			var newC = new Color[c.Length];

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					for (int k = 0; k < dim; k++) {
						int j_ = dim - j - 1;
						newC [i + (j * dim) + (k * dim * dim)] = c [k * dim + i + j_ * dim * dim];
					}
				}
			}
                
			var result = new Texture3D (dim, dim, dim, TextureFormat.ARGB32, false);
			result.SetPixels (newC);
			result.Apply ();
			return result;
		}

		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (CheckResources () == false || !SystemInfo.supports3DTextures || !lut) {
				Graphics.Blit (source, destination);
				return;
			}

			int lutSize = lut.width;
			lut.wrapMode = TextureWrapMode.Clamp;
			material.SetFloat ("_Scale", (lutSize - 1) / (1.0f * lutSize));
			material.SetFloat ("_Offset", 1.0f / (2.0f * lutSize));
			material.SetTexture ("_ClutTex", lut);

			Graphics.Blit (source, destination, material, QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
		}
	}
}
