using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System;

internal class GIEmissiveOnlyShaderGUI : ShaderGUI
{
	MaterialProperty m_EmissionColor = null;

	public void FindProperties (MaterialProperty[] props)
	{
		m_EmissionColor = FindProperty ("_EmissionColor", props);
	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
	{
		FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();
		
		materialEditor.ShaderProperty(m_EmissionColor, "Emission");
		materialEditor.LightmapEmissionProperty ();

		foreach (Material material in materialEditor.targets)
		{
			// We assume this object always has emissive, because thats what it's for...	
			material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
		}
	}
}