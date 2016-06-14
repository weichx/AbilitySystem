using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UIQuestRewardToggle), true)]
	public class UIQuestRewardToggleEditor : ToggleEditor {
		
		private SerializedProperty m_TargetImageProperty;
		private SerializedProperty m_ActiveSpriteProperty;
		private SerializedProperty m_GroupProperty;
		private SerializedProperty m_IsOnProperty;
		private SerializedProperty m_OnValueChangedProperty;
				
		protected override void OnEnable ()
		{
			base.OnEnable ();
			this.m_TargetImageProperty = base.serializedObject.FindProperty("m_TargetImage");
			this.m_ActiveSpriteProperty = this.serializedObject.FindProperty("m_ActiveSprite");
			this.m_GroupProperty = base.serializedObject.FindProperty("m_Group");
			this.m_IsOnProperty = base.serializedObject.FindProperty("m_IsOn");
			this.m_OnValueChangedProperty = base.serializedObject.FindProperty("onValueChanged");
		}
		
		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_IsOnProperty);
			EditorGUILayout.PropertyField(this.m_GroupProperty);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_TargetImageProperty);
			EditorGUILayout.PropertyField(this.m_ActiveSpriteProperty);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_OnValueChangedProperty, true);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}