using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UICharacterSelect_Unit), true)]
	public class UICharacterSelect_UnitEditor : ToggleEditor {
		
		public SerializedProperty avatarImageComponentProperty;
		public SerializedProperty nameTextComponentProperty;
		public SerializedProperty classTextComponentProperty;
		public SerializedProperty levelTextComponentProperty;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			this.avatarImageComponentProperty = this.serializedObject.FindProperty("m_AvatarImageComponent");
			this.nameTextComponentProperty = this.serializedObject.FindProperty("m_NameTextComponent");
			this.classTextComponentProperty = this.serializedObject.FindProperty("m_ClassTextComponent");
			this.levelTextComponentProperty = this.serializedObject.FindProperty("m_LevelTextComponent");
		}
		
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			
			EditorGUILayout.LabelField("Custom Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(avatarImageComponentProperty, new GUIContent("Avatar Image"));
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(nameTextComponentProperty, new GUIContent("Name Text"));
			if (nameTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_NameNormalColor"), new GUIContent("Name Normal"));
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_NameHighlightColor"), new GUIContent("Name Highlighted"));
			}
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(this.classTextComponentProperty, new GUIContent("Class Text"));
			if (this.classTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_ClassNormalColor"), new GUIContent("Class Normal"));
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_ClassHighlightColor"), new GUIContent("Class Highlighted"));
			}
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(this.levelTextComponentProperty, new GUIContent("Level Text"));
			if (this.levelTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_LevelNormalColor"), new GUIContent("Level Normal"));
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_LevelHighlightColor"), new GUIContent("Level Highlighted"));
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			this.serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Toggle Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			base.OnInspectorGUI();
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
	}
}