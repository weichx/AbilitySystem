using System;
using UnityEngine;
using UnityEditor;

public static class EditorGUIUtilityX {

    private static GUIContent tempContent = new GUIContent();
    public static GUIContent TempLabel(string text) {
        tempContent.text = text;
        return tempContent;
    }

    public static bool LabelHasContent(GUIContent label) {
        return label == null || label.text != string.Empty || label.image != null;
    }

    private static float GetSinglePropertyHeight(Type type, GUIContent label) {
        if (type == typeof(Vector2) || type == typeof(Vector3)) {
            return ((!LabelHasContent(label) || EditorGUIUtility.wideMode ? 0.0f : 16.0f) + 16.0f);
        }
        else if (type == typeof(Rect)) {
            return ((!LabelHasContent(label) || EditorGUIUtility.wideMode ? 0.0f : 16.0f) + 32.0f);
        }
        else if (type == typeof(Bounds)) {
            return ((LabelHasContent(label) ? 16.0f : 0.0f) + 32.0f);
        }
        else {
            return 16f;
        }
    }

    private static Type[] BuildInTypes = new Type[] {
        typeof(Color), typeof(Vector2), typeof(Vector3), typeof(Rect), typeof(Bounds), typeof(Vector4)
    };

    //todo account for decorators eventually
    public static float GetHeight(SerializedPropertyX property, GUIContent label, bool includeChildren) {
        PropertyDrawerX drawerX = Reflector.GetCustomPropertyDrawerFor(property);
        if (drawerX != null) {
            return drawerX.GetPropertyHeight(property, label);
        }
        else if (!includeChildren || property.type.IsPrimitive || property.type.IsEnum || property.type == typeof(string) || Array.IndexOf(BuildInTypes, property.type) != -1) {
            return GetSinglePropertyHeight(property.type, label);
        }
        else if (property.type.IsArray) {
            if (property.isExpanded) {
                float height = 32f;
                for (int i = 0; i < property.ChildCount; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                    height += GetHeight(child, child.label, child.isExpanded);
                }
                return height;
            }
            return 16f;
        }
        else {
            float height = 16f;
            for (int i = 0; i < property.ChildCount; i++) {
                SerializedPropertyX child = property.GetChildAt(i);
                height += GetHeight(child, child.label, child.isExpanded);
            }
            return height;
        }
    }
}