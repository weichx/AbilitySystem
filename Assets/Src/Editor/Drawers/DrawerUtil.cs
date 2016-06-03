using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class DrawerUtil {

    //todo -- support custom inspectors for entity system base types?
    private static FieldInfo rectField;

    static DrawerUtil() {
        if (rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
    }

    public static object GetTarget(SerializedProperty serializedProperty) {
        if (serializedProperty == null || serializedProperty.serializedObject == null) return null;
        return serializedProperty.serializedObject.targetObject;
    }

    public static PropertyDrawer GetDrawer(SerializedProperty property) {
        object target = GetTarget(property);
        FieldInfo propertyFieldInfo = target.GetType().GetField(property.name);
        return Reflector.GetCustomPropertyDrawerFor(propertyFieldInfo.FieldType,
            typeof(DrawerUtil).Assembly);
    }

    public static PropertyDrawer GetPropertyDrawerForField(FieldInfo fInfo) {
        var drawer = Reflector.GetCustomPropertyDrawerFor(fInfo.FieldType, typeof(AbilityPage).Assembly);
        if (drawer != null) return drawer;
        var attrs = fInfo.GetCustomAttributes(false);
        if (attrs == null) return null;
        for (int i = 0; i < attrs.Length; i++) {
            drawer = Reflector.GetCustomPropertyDrawerFor(attrs[i].GetType(), typeof(AbilityPage).Assembly, typeof(EditorGUI).Assembly);
            if (drawer != null) {
                drawer.GetType()
                      .GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance)
                      .SetValue(drawer, attrs[i]);
                return drawer;
            }
        }

        return drawer;
    }

    public static PropertyDrawer GetPropertyDrawerForType(Type type) {
        var drawer = Reflector.GetCustomPropertyDrawerFor(type, typeof(DrawerUtil).Assembly);
        if (drawer != null) return drawer;
        var attrs = type.GetCustomAttributes(false);
        if (attrs == null) return null;
        for (int i = 0; i < attrs.Length; i++) {
            drawer = Reflector.GetCustomPropertyDrawerFor(attrs[i].GetType(), typeof(DrawerUtil).Assembly, typeof(EditorGUI).Assembly);
            if (drawer != null) {
                drawer.GetType().GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drawer, attrs[0]);
                return drawer;
            }
        }
        return drawer;
    }

    public static void DrawLayoutTexture(Texture2D texture, float height = -1) {
        if (rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
        if (height < 0) height = texture.height;
        Rect position = EditorGUILayout.GetControlRect(false, height);
        rectField.SetValue(null, position);
        GUI.DrawTexture(position, texture);
    }

    public static float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        PropertyDrawer drawer = GetDrawer(property);
        if (drawer != null) {
            return drawer.GetPropertyHeight(property, label);
        }
        else {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }

    public static void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
        PropertyDrawer drawer = GetDrawer(property);
        if (drawer != null) {
            drawer.OnGUI(rect, property, label);
        }
        else {
            EditorGUI.PropertyField(rect, property, label);
        }
    }

    private static Stack<float> labelWidthStack = new Stack<float>();
    public static void PushLabelWidth(float width) {
        labelWidthStack.Push(EditorGUIUtility.labelWidth);
        EditorGUIUtility.labelWidth = width;
    }

    public static void PopLabelWidth() {
        EditorGUIUtility.labelWidth = labelWidthStack.Pop();
    }

    private static Stack<int> indentStack = new Stack<int>();

    public static void PushIndentLevel(int indent) {
        EditorGUI.indentLevel += indent;
        indentStack.Push(indent);
    }

    public static void PopIndentLevel() {
        EditorGUI.indentLevel -= indentStack.Pop();
    }
}


