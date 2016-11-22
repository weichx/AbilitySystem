using System;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization;

public static class EditorGUIX {

   //TODO --- Deprecate EditorGUIX in favor of the EditorGUILayoutX, manual rects are dumb

    //public static void PropertyField(Rect position, SerializedPropertyX property) {
    //    PropertyField(position, property, property.label, true);
    //}

    //public static void PropertyField(SerializedPropertyX property, GUIContent label, bool includeChildren = true) {
    //    var drawer = Reflector.GetCustomPropertyDrawerFor(property);
    //    if (drawer != null) {
    //        drawer.OnGUI(property, label);
    //    }
    //    else {
    //        PropertyFieldExtendedValue(position, property, label);
    //    }
    //}

    private static void PropertyFieldExtendedValue(Rect position, SerializedPropertyX property, GUIContent label = null, GUIStyle style = null) {
        Type type = property.type;
        if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
            property.Value = EditorGUI.ObjectField(position, label, (UnityEngine.Object)property.Value, type, true);
        }
        else if (type.IsArray) {
            if (property.Value == null) {
               property.Value = Array.CreateInstance(type.GetElementType(), 1);
            }
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded) {
                position.y += 16f;
                position.height -= 16f;
                EditorGUI.indentLevel++;
                Array array = (Array)property.Value;
                int length = array.Length;
                int newLength = EditorGUI.IntField(position, new GUIContent("Size"), length);

                if (newLength < 0) newLength = 0;
                if (length != newLength) {
                    var newArray = Array.CreateInstance(type.GetElementType(), newLength);
                    for (int i = 0; i < newLength; i++) {
                        if (i == array.Length) break;
                        newArray.SetValue(array.GetValue(i), i);
                    }
                    array.CopyTo(newArray, 0);
                    array = newArray;
                }
                position.y += 16f;
                position.height -= 16f;

                Type elementType = array.GetType().GetElementType();

                for (int i = 0; i < array.Length; i++) {
                    if (array.GetValue(i) == null) {
                        array.SetValue(CreateInstance(elementType), i);
                    }
                    //array.SetValue(PropertyFieldExtendedValue(position, elementType, array.GetValue(i), new GUIContent("Element " + i), null), i);
                    //position.y += 48f; //needs to be += getheight
                }
                EditorGUI.indentLevel--;
            }
        }
        else if (type.IsEnum) {
            if (style == null) style = EditorStyles.popup; //todo unity default is popup field
            property.Value = EditorGUI.EnumPopup(position, label, (Enum)property.Value, style);
        }
        else if (type == typeof(Color)) {
            property.Value = EditorGUI.ColorField(position, label, (Color)property.Value);
        }
        else if (type == typeof(Bounds)) {
            Bounds b = (Bounds)property.Value;
            position = EditorGUI.PrefixLabel(position, label);
            position.x -= 48f;
            EditorGUI.LabelField(position, new GUIContent("Center:"));
            position.x += 53f;
            position.width -= 5f;
            b.center = EditorGUI.Vector3Field(position, GUIContent.none, b.center);
            position.y += 16f;
            position.x -= 53f;
            EditorGUI.LabelField(position, new GUIContent("Extents:"));
            position.x += 53f;
            b.extents = EditorGUI.Vector3Field(position, GUIContent.none, b.extents);
            property.Value = b;
        }
        else if (type == typeof(AnimationCurve)) {
            if (property.Value == null) property.Value = new AnimationCurve();
            position.width = 200f;
            property.Value = EditorGUI.CurveField(position, label, (AnimationCurve)property.Value);
        }
        else if (type == typeof(double)) {
            if (style == null) style = EditorStyles.numberField;
            property.Value = EditorGUI.DoubleField(position, label, (double)property.Value, style);
        }
        else if (type == typeof(float)) {
            if (style == null) style = EditorStyles.numberField;
            property.Value = EditorGUI.FloatField(position, label, (float)property.Value, style);
        }
        else if (type == typeof(int)) {
            if (style == null) style = EditorStyles.numberField;
            property.Value = EditorGUI.IntField(position, label, (int)property.Value, style);
        }
        else if (type == typeof(long)) {
            if (style == null) style = EditorStyles.numberField;
            property.Value = EditorGUI.LongField(position, label, (long)property.Value, style);
        }
        else if (type == typeof(Rect)) {
            property.Value = EditorGUI.RectField(position, label, (Rect)property.Value);
        }
        else if (type == typeof(bool)) {
            if (style == null) style = EditorStyles.toggle;
            property.Value = EditorGUI.Toggle(position, label, (bool)property.Value, style);
        }
        else if (type == typeof(Vector2)) {
            property.Value = EditorGUI.Vector2Field(position, label, (Vector2)property.Value);
        }
        else if (type == typeof(Vector3)) {
            property.Value = EditorGUI.Vector3Field(position, label, (Vector3)property.Value);
        }
        else if (type == typeof(Vector4)) {
            property.Value = EditorGUI.Vector4Field(position, label.text, (Vector4)property.Value);
        }
        else if (type == typeof(string)) {
            if (style == null) style = EditorStyles.textField;
            property.Value = EditorGUI.TextField(position, label, (string)property.Value, style);
        }
        else {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                position.y += 16f;
                position.height -= 16f;
                for (int i = 0; i < property.ChildCount; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                  //  PropertyField(position, child);
                    float propHeight = EditorGUIUtilityX.GetHeight(child, child.label, child.isExpanded);
                    position.y += propHeight;
                    position.height -= propHeight;
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    private static object CreateInstance(Type type) {
        object retn = null;
        if (type == typeof(string)) return "";
        if (type.IsArray) {
            retn = Array.CreateInstance(type.GetElementType(), 0);
        }
        else if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
            return null;
        }
        else {
            try {
                retn = Activator.CreateInstance(type);
            }
            catch (MissingMethodException) {
                retn = FormatterServices.GetUninitializedObject(type);
            }
        }
        return retn;
    }
}