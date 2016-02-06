using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace AbilitySystem {

    public class DrawerUtil {

        public static string[] GetFloatFormulaOptions<T>(SerializedProperty serializedProperty) where T : FormulaAttribute {
            var genericArguments = GetGenericArguments(serializedProperty);
            if (genericArguments == null) return new string[0];
            Type[] parameters = new Type[genericArguments.Length + 1];
            for (int i = 0; i < genericArguments.Length; i++) {
                parameters[i] = genericArguments[i];
            }
            parameters[parameters.Length - 1] = typeof(float);

            var pointerList = Reflector.FindMethodPointersWithAttribute<T>(typeof(float), parameters);
            var formattedList = pointerList.Select((ptr) => {
                return ptr.ToString();
            }).ToList();

            formattedList.Insert(0, "-- None --");
            return  formattedList.ToArray();
        }

        public static string[] GetFloatFormulaOptions<T>(params Type[] parameters) where T : FormulaAttribute {
            Array.Resize(ref parameters, parameters.Length + 1);
            parameters[parameters.Length - 1] = typeof(float);
            var pointerList = Reflector.FindMethodPointersWithAttribute<T>(typeof(float), parameters);
            var formattedList = pointerList.Select((ptr) => {
                return ptr.ToString();
            }).ToList();

            formattedList.Insert(0, "-- None --");
            return formattedList.ToArray();
        }

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
            return Reflector.GetCustomPropertyDrawerFor(propertyFieldInfo,
                typeof(VisibleAttributeDrawer).Assembly);
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            PropertyDrawer drawer = GetDrawer(property);
            if(drawer != null) {
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


}