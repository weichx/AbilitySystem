using System;
using UnityEditor;
using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [CustomEditor (typeof(ColorCorrectionLookup))]
    class ColorCorrectionLookupEditor : Editor
    {
        private Texture2D tempClutTex2D;

        public override void OnInspectorGUI () {
        
        	DrawDefaultInspector();
        	
        	EditorGUILayout.LabelField("Convert 2D texture to LUT", EditorStyles.miniLabel);
        	GUILayout.BeginHorizontal();
        	tempClutTex2D = EditorGUILayout.ObjectField(tempClutTex2D, typeof(Texture2D), true) as Texture2D;
			bool isValidDimensions = tempClutTex2D && ColorCorrectionLookup.ValidDimensions(tempClutTex2D);
			EditorGUI.BeginDisabledGroup(!isValidDimensions);
        	if(GUILayout.Button ("Convert..."))
        	{
        		var sourcePath = AssetDatabase.GetAssetPath (tempClutTex2D);
				var targetPath = EditorUtility.SaveFilePanelInProject("Save converted LUT", tempClutTex2D.name + "_ConvertedLUT.asset", "asset", "Save the converted LUT", System.IO.Path.GetDirectoryName(sourcePath));
        		if(String.IsNullOrEmpty(targetPath)) return;
        		
				TextureImporter textureImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
				bool doImport = textureImporter.isReadable == false;
				if (textureImporter.mipmapEnabled == true) {
					doImport = true;
				}
				if (textureImporter.textureFormat != TextureImporterFormat.AutomaticTruecolor) {
					doImport = true;
				}
				
				if (doImport)
				{
					textureImporter.isReadable = true;
					textureImporter.mipmapEnabled = false;
					textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
					AssetDatabase.ImportAsset (sourcePath, ImportAssetOptions.ForceUpdate);
				}
				
				if(System.IO.File.Exists (targetPath))
					FileUtil.DeleteFileOrDirectory(targetPath);
					
				var lut = ColorCorrectionLookup.Convert(tempClutTex2D);
				AssetDatabase.CreateAsset(lut, targetPath);
				
				serializedObject.Update ();
				serializedObject.FindProperty("lut").objectReferenceValue = lut;
				serializedObject.ApplyModifiedProperties();
        	}
        	EditorGUI.EndDisabledGroup();
        	GUILayout.EndHorizontal();
        	if(tempClutTex2D && !isValidDimensions)
        	{
				EditorGUILayout.HelpBox ("Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
        	}
        }
    }
}
