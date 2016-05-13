using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;
using AbilitySystem;

public class DrawerUtil {

    public static int GetMatchingIndex(string signature, string[] options) {
        if (signature == null) return 0;
        for (int i = 1; i < options.Length; i++) {
            if (signature == options[i]) return i;
        }
        return 0;
    }

    public static Type[] GetGenericArguments(SerializedProperty serializedProperty) {
        FieldInfo field = Reflector.GetProperty(GetTarget(serializedProperty), serializedProperty.name);
        if (field == null) return null;
        var baseType = FindGenericBase(field.FieldType);
        if (baseType == null) return null;
        return baseType.GetGenericArguments();
    }

    public static SignatureAttribute GetSignatureAttribute(SerializedProperty serializedProperty) {
        object target = GetTarget(serializedProperty);
        if (target == null) return null;
        FieldInfo fieldInfo = Reflector.GetProperty(target, serializedProperty.name);
        if (fieldInfo == null) return null;
        object[] attrs = fieldInfo.FieldType.GetCustomAttributes(typeof(SignatureAttribute), false);
        if (attrs == null || attrs.Length == 0) return null;
        return attrs[0] as SignatureAttribute;
    }

    public static object GetTarget(SerializedProperty serializedProperty) {
        if (serializedProperty == null || serializedProperty.serializedObject == null) return null;
        return serializedProperty.serializedObject.targetObject;
    }

    public static Type FindGenericBase(Type type) {
        var baseType = type.BaseType;
        int safetyCount = 0;
        while (safetyCount < 10 && baseType != null) {
            if (baseType.IsGenericType) {
                return baseType;
            }
            safetyCount++;
        }
        return null;
    }

    public static PropertyDrawer GetDrawer(SerializedProperty property) {
        object target = GetTarget(property);
        FieldInfo propertyFieldInfo = target.GetType().GetField(property.name);
        return Reflector.GetCustomPropertyDrawerFor(propertyFieldInfo.FieldType,
            typeof(VisibleAttributeDrawer).Assembly);
    }

    private static FieldInfo rectField;
    public static void DrawProperty(SerializedProperty p, Type parentType) {
        if(rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
        FieldInfo fInfo = parentType.GetField(p.name);
        if (fInfo != null) {
            var drawer = Reflector.GetCustomPropertyDrawerFor(fInfo.FieldType, typeof(AbilityPage).Assembly);
            if (drawer == null) {
                var attrs = fInfo.GetCustomAttributes(false);
                if (attrs.Length > 0) {
                    drawer = Reflector.GetCustomPropertyDrawerFor(attrs[0].GetType(), typeof(AbilityPage).Assembly);
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
}


