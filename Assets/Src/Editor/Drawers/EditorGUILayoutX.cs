using UnityEngine;
using UnityEditor;
using System;

public static class EditorGUILayoutX {

    public static void DrawProperties(SerializedObjectX obj) {
        DrawProperties(obj.Root);
    }

    public static void DrawProperties(SerializedPropertyX root) {
        for (int i = 0; i < root.ChildCount; i++) {
            SerializedPropertyX property = root.GetChildAt(i);
            PropertyField(property, property.label, property.isExpanded);
        }
    }

    public static Rect GetControlRect(SerializedPropertyX property, GUIContent label = null) {
        label = label ?? property.label;
        return EditorGUILayout.GetControlRect(true, EditorGUIUtilityX.GetHeight(property, label, property.isExpanded));
    }

    public static void PropertyField(SerializedPropertyX property, params GUILayoutOption[] options) {
        PropertyField(property, property.label, true, options);
    }

    public static void PropertyField(SerializedPropertyX property, bool includeChildren, params GUILayoutOption[] options) {
        PropertyField(property, property.label, includeChildren, options);
    }

    public static void PropertyField(SerializedPropertyX property, GUIContent label, bool includeChildren, params GUILayoutOption[] options) {
        Type type = property.type;
        ExtendedPropertyDrawer drawer = Reflector.GetExtendedPropertyDrawerFor(property.type);
        if (drawer != null) {
            drawer.OnGUI(GetControlRect(property), property, label);
            return;
        }
        if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
            property.Value = EditorGUILayout.ObjectField(label, (UnityEngine.Object)property.Value, type, true, options);
        }
        else if (type.IsArray) {
            if (property.Value == null) {
                property.Value = Array.CreateInstance(type.GetElementType(), 1);
            }
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label.text);
            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                property.ArraySize = EditorGUILayout.IntField(new GUIContent("Size"), property.ArraySize);
                for (int i = 0; i < property.ArraySize; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                    PropertyField(child, child.label, child.isExpanded, options);
                }
                EditorGUI.indentLevel--;
            }
        }
        else if (type.IsEnum) {
            property.Value = EditorGUILayout.EnumMaskField(label, (Enum)property.Value, options);
        }
        else if (type == typeof(Color)) {
            property.Value = EditorGUILayout.ColorField(label, (Color)property.Value);
        }
        else if (type == typeof(Bounds)) {
            Bounds b = (Bounds)property.Value;
            property.Value = EditorGUILayout.BoundsField(label, b, options);
        }
        else if (type == typeof(AnimationCurve)) {
            if (property.Value == null) property.Value = new AnimationCurve();
            property.Value = EditorGUILayout.CurveField(label, (AnimationCurve)property.Value, options);
        }
        else if (type == typeof(double)) {
            property.Value = EditorGUILayout.DoubleField(label, (double)property.Value);
        }
        else if (type == typeof(float)) {
            property.Value = EditorGUILayout.FloatField(label, (float)property.Value);
        }
        else if (type == typeof(int)) {
            property.Value = EditorGUILayout.IntField(label, (int)property.Value, options);
        }
        else if (type == typeof(long)) {
            property.Value = EditorGUILayout.LongField(label, (long)property.Value, options);
        }
        else if (type == typeof(Rect)) {
            property.Value = EditorGUILayout.RectField(label, (Rect)property.Value, options);
        }
        else if (type == typeof(bool)) {
            property.Value = EditorGUILayout.Toggle(label, (bool)property.Value, options);
        }
        else if (type == typeof(Vector2)) {
            property.Value = EditorGUILayout.Vector2Field(label, (Vector2)property.Value, options);
        }
        else if (type == typeof(Vector3)) {
            property.Value = EditorGUILayout.Vector3Field(label, (Vector3)property.Value, options);
        }
        else if (type == typeof(Vector4)) {
            property.Value = EditorGUILayout.Vector4Field(label.text, (Vector4)property.Value, options);
        }
        else if (type == typeof(string)) {
            property.Value = EditorGUILayout.TextField(label, (string)property.Value, options);
        }
        else {
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < property.ChildCount; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                    PropertyField(child, child.label, child.isExpanded, options);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}