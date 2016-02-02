using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace AbilitySystem {

    [CustomPropertyDrawer(typeof(VisibleAttribute))]
    public class VisibleAttributeDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            VisibleAttribute attr = attribute as VisibleAttribute;
            if (attr == null || attr.Result(DrawerUtil.GetTarget(property))) {
                return DrawerUtil.GetPropertyHeight(property, label);
            }
            else {
                return 0f;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            VisibleAttribute attr = attribute as VisibleAttribute;
            if (attr == null || attr.Result(DrawerUtil.GetTarget(property))) {
                DrawerUtil.OnGUI(position, property, label);
            }
        }

    }
}