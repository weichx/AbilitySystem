using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage_RequirementSection : AbilityPage_SectionBase {

    private bool shown;
    private SearchBox<AbilityRequirement> searchBox;
    FieldInfo rectField;

    public AbilityPage_RequirementSection() {
        shown = true;
        searchBox = new SearchBox<AbilityRequirement>(null, AddRequirement, "Add Requirement", "Requirements");
        rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
    }

    private void AddRequirement(Type type) {
        targetItem.InstanceRef.requirements.Add(Activator.CreateInstance(type) as AbilityRequirement);
        targetItem.Rebuild();
    }

    public void RenderChildren(SerializedProperty p, Type componentType) {
        EditorGUI.indentLevel += 2;
        while (p.NextVisible(true)) {
            if (p.name.StartsWith("__requirement")) {
                break;
            }
            FieldInfo fInfo = componentType.GetField(p.name);
            if (fInfo != null) {
                var drawer = Reflector.GetCustomPropertyDrawerFor(fInfo.FieldType, typeof(AbilityPage).Assembly);
                if (drawer == null) {
                    var attrs = fInfo.GetCustomAttributes(false);
                    if (attrs.Length > 0) {
                        drawer = Reflector.GetCustomPropertyDrawerFor(attrs[0].GetType(), typeof(AbilityPage).Assembly, typeof(EditorGUI).Assembly);
                        if (drawer != null) {
                            drawer.GetType().GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drawer, attrs[0]);
                        } 
                    }
                }
                if (drawer != null) {
                    GUIContent label = new GUIContent(Util.SplitAndTitlize(p.name));
                    Rect position = EditorGUILayout.GetControlRect(true, drawer.GetPropertyHeight(p, label));
                    rectField.SetValue(null, position);
                    drawer.OnGUI(position, p, label);
                }
                else {
                    EditorGUILayout.PropertyField(p, true);
                }
            }
        }
        EditorGUI.indentLevel -= 2;
    }

    public override void Render() {
        if (serialRoot == null) return;

        shown = EditorGUILayout.Foldout(shown, "Requirements");

        if (shown) {
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250f;
            EditorGUI.indentLevel += 2;
            for (int i = 0; i < targetItem.InstanceRef.requirements.Count; i++) {
                string propName = "__requirement" + i;
                string type = serialRoot.FindProperty(propName).type;
                if(EditorGUILayout.PropertyField(serialRoot.FindProperty(propName), new GUIContent(type, icon), false)) {
                    SerializedProperty p = serialRoot.FindProperty(propName);
                    RenderChildren(p, targetItem.InstanceRef.requirements[i].GetType());
                }
            }
            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.RenderLayout();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }


}