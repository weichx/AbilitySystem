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

    public static void PropertyFieldExtendedLayout(object parent, FieldInfo fieldInfo, GUIContent label, bool includeChildren = false, params GUILayoutOption[] options) {
        if (label == null) {
            label = new GUIContent(Util.SplitAndTitlize(fieldInfo.Name));
        }
        Rect position = EditorGUILayout.GetControlRect();
        rectField.SetValue(null, position);
        bool labelHasContent = LabelHasContent(label);
        Rect rect1 = EditorGUILayout.GetControlRect(labelHasContent, GetHeight(parent, fieldInfo, label, true), options);
        Rect position2 = fieldInfo.FieldType != typeof(bool) ? rect1 : GetToggleRect(true, options);

        PropertyFieldExtended(position, parent, fieldInfo, label);
    }

    //todo this needs major work and needs to be recursive. must respect include children. dont forget array recursion.
    //if custom inspector is provided be sure to grab height from there
    private static float GetHeight(object parent, FieldInfo fieldInfo, GUIContent label, bool includeChildren) {
        if (fieldInfo.FieldType == typeof(Bounds) || fieldInfo.FieldType == typeof(Rect)) {
            return EditorGUIUtility.singleLineHeight;
        }
        else if (fieldInfo.FieldType.IsArray) {
            var value = fieldInfo.GetValue(parent);
            if (value == null || !includeChildren) return EditorGUIUtility.singleLineHeight;
            else {
                return EditorGUIUtility.singleLineHeight * ((((object[])value).Length * 3) + 1);
            }
        }
        return 0;
    }

    private static float kLabelFloatMinW {
        get {
            return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5.0f;
        }
    }

    private static float kLabelFloatMaxW {
        get {
            return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5.0f;
        }
    }

    private static bool LabelHasContent(GUIContent label) {
        if (label == null || label.text != string.Empty) {
            return true;
        }
        return label.image != null;
    }

    private static Rect GetToggleRect(bool hasLabel, params GUILayoutOption[] options) {
        float num = 10f - EditorGUIUtility.fieldWidth;
        return GUILayoutUtility.GetRect(!hasLabel ? EditorGUIUtility.fieldWidth + num : kLabelFloatMinW + num, kLabelFloatMaxW + num, 16f, 16f, EditorStyles.numberField, options);
    }

    public static object PropertyFieldExtendedValue(Rect position, Type type, object value, GUIContent label, GUIStyle style) {
        object newValue;
        ExtendedPropertyDrawer d = Reflector.GetExtendedPropertyDrawerFor(typeof(AbstractMethodPointer));
        if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
            newValue = EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, type, true);
        }
        else if (type.IsArray) {
            if (value == null) {
                value = Array.CreateInstance(type.GetElementType(), 1);
            }
            int ctrlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var state = (FoldOutState)GUIUtility.GetStateObject(typeof(FoldOutState), ctrlId);
            state.isShown = EditorGUI.Foldout(position, state.isShown, label);
            PushLabelWidth(300);
            if (state.isShown) {
                position.y += 16f;
                EditorGUI.indentLevel++;
                Array array = (Array)value;
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

                Type elementType = array.GetType().GetElementType();
                
                for (int i = 0; i < array.Length; i++) {
                    if (array.GetValue(i) == null) {
                        array.SetValue(CreateInstance(elementType), i);
                    }
                    array.SetValue(PropertyFieldExtendedValue(position, elementType, array.GetValue(i), new GUIContent("Element " + i), null), i);
                    position.y += 48f; //needs to be += getheight
                }
                value = array;
                EditorGUI.indentLevel--;
            }
            PopLabelWidth();
            newValue = value;
        }
        else if (type.IsEnum) {
            if (style == null) style = EditorStyles.popup; //todo unity default is popup field
            newValue = EditorGUI.EnumMaskField(position, label, (Enum)value, style);
        }
        else if (type == typeof(Color)) {
            newValue = EditorGUI.ColorField(position, label, (Color)value);
        }
        else if (type == typeof(Bounds)) {
            Bounds b = (Bounds)value;
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
            newValue = b;
        }
        else if (type == typeof(AnimationCurve)) {
            if (value == null) value = new AnimationCurve();
            position.width = 200f;
            newValue = EditorGUI.CurveField(position, label, (AnimationCurve)value);
        }
        else if (type == typeof(double)) {
            if (style == null) style = EditorStyles.numberField;
            newValue = EditorGUI.DoubleField(position, label, (double)value, style);
        }
        else if (type == typeof(float)) {
            if (style == null) style = EditorStyles.numberField;
            newValue = EditorGUI.FloatField(position, label, (float)value, style);
        }
        else if (type == typeof(int)) {
            if (style == null) style = EditorStyles.numberField;
            newValue = EditorGUI.IntField(position, label, (int)value, style);
        }
        else if (type == typeof(long)) {
            if (style == null) style = EditorStyles.numberField;
            newValue = EditorGUI.LongField(position, label, (long)value, style);
        }
        else if (type == typeof(Rect)) {
            newValue = EditorGUI.RectField(position, label, (Rect)value);
        }
        else if (type == typeof(bool)) {
            if (style == null) style = EditorStyles.toggle;
            newValue = EditorGUI.Toggle(position, label, (bool)value, style);
        }
        else if (type == typeof(Vector2)) {
            newValue = EditorGUI.Vector2Field(position, label, (Vector2)value);
        }
        else if (type == typeof(Vector3)) {
            newValue = EditorGUI.Vector3Field(position, label, (Vector3)value);
        }
        else if (type == typeof(Vector4)) {
            newValue = EditorGUI.Vector4Field(position, label.text, (Vector4)value);
        }
        else if (type == typeof(string)) {
            if (style == null) style = EditorStyles.textField;
            newValue = EditorGUI.TextField(position, label, (string)value, style);
        }
        else {
            int ctrlId = GUIUtility.GetControlID(FocusType.Passive);
            var state = (FoldOutState)GUIUtility.GetStateObject(typeof(FoldOutState), ctrlId);
            state.isShown = EditorGUI.Foldout(position, state.isShown, label);
            if (state.isShown) {
                position.y += 16f;
                RenderChildrenByRect(position, value);
            }
            newValue = value;
        }
        return newValue;
    }

    public static void PropertyFieldExtended(Rect position, object parent, FieldInfo field, GUIContent label, GUIStyle style = null) {
        Type fieldType = field.FieldType;
        object value = field.GetValue(parent);
        object newValue = PropertyFieldExtendedValue(position, fieldType, value, label, style);
        field.SetValue(parent, newValue);
    }

    private class FoldOutState {
        public bool isShown = true;
    }

    private static object CreateInstance(Type type) {
        object retn = null;
        if (type == typeof(string)) return "";
        if (type.IsArray) {
            retn = Array.CreateInstance(type.GetElementType(), 0);
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

    ///<summary>Render child properties of a serialized property, using CustomPropertyDrawers where needed</summary>
    public static void RenderChildren(SerializedProperty p, Type type, int indentDelta = 1) {
        RenderChildrenExcept(p, type, null, indentDelta);
    }

    public static void RenderChildrenExceptExperimental(SerializedProperty p, object target, string[] exceptions, int indentDelta = 1) {
        EditorGUI.indentLevel += indentDelta;
        FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++) {
            FieldInfo fInfo = fields[i];
            if (fInfo.IsNotSerialized || Array.IndexOf(exceptions, fInfo.Name) != -1) continue;

            DrawNonSerialized(target, fInfo);
        }

        EditorGUI.indentLevel -= indentDelta;
    }

    public static void RenderChildren2(object target) {
        EditorGUI.indentLevel += 1;
        FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++) {
            FieldInfo fInfo = fields[i];
            if (fInfo.IsNotSerialized) continue;
            DrawNonSerialized(target, fInfo);
        }

        EditorGUI.indentLevel -= 1;
    }

    public static void RenderChildrenByRect(Rect position, object target) {
        EditorGUI.indentLevel += 1;

        FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++) {
            FieldInfo fInfo = fields[i];
            if (fInfo.IsNotSerialized) continue;
            DrawNonSerialized(position, target, fInfo);
            position.y += 16f; //todo use GetHeight
        }

        EditorGUI.indentLevel -= 1;
    }

    private static void DrawNonSerialized(Rect position, object target, FieldInfo fieldInfo) {
        Type fieldType = fieldInfo.FieldType;
        string fieldName = Util.SplitAndTitlize(fieldInfo.Name);
        //PropertyDrawer drawer = Reflector.GetCustomPropertyDrawerFor(fieldType, typeof(EntitySystemWindow).Assembly);
        //if (drawer != null && drawer as ExtendedPropertyDrawer != null) {
        //    object source = fieldInfo.GetValue(target);
        //    if (source == null) {
        //        //todo make this iron clad
        //        source = Activator.CreateInstance(fieldType);
        //        fieldInfo.SetValue(target, source);
        //    }
        //    DrawExtendedPropertyLayout(drawer as ExtendedPropertyDrawer, source, new GUIContent(fieldName));
        //}
        //else {
        //    PropertyFieldExtended(position, target, fieldInfo, new GUIContent(fieldName), null);
        //}
    }

    //todo: style, label, layout options
    private static void DrawNonSerialized(object target, FieldInfo fieldInfo) {
        Type fieldType = fieldInfo.FieldType;
        string fieldName = Util.SplitAndTitlize(fieldInfo.Name);
        //PropertyDrawer drawer = Reflector.GetCustomPropertyDrawerFor(fieldType, typeof(EntitySystemWindow).Assembly);
        //if (drawer != null && drawer as ExtendedPropertyDrawer != null) {
        //    object source = fieldInfo.GetValue(target);
        //    if (source == null) {
        //        //todo make this iron clad
        //        source = Activator.CreateInstance(fieldType);
        //        fieldInfo.SetValue(target, source);
        //    }
        //    DrawExtendedPropertyLayout(drawer as ExtendedPropertyDrawer, source, new GUIContent(fieldName));
        //}
        //else {
        //    PropertyFieldExtendedLayout(target, fieldInfo, new GUIContent(fieldName));
        //}
    }

    ///<summary>
    ///Render child properties of a serialized property unless specified as an exception
    ///Uses CustomPropertyDrawers where needed
    ///</summary>
    public static void RenderChildrenExcept(SerializedProperty p, Type type, string[] exceptions, int indentDelta = 1) {
        EditorGUI.indentLevel += indentDelta;
        var endProperty = p.GetEndProperty();
        while (p.NextVisible(true) && !SerializedProperty.EqualContents(p, endProperty)) {

            if (exceptions != null && Array.IndexOf(exceptions, p.name) != -1) {
                continue;
            }

            DrawPropertyLayout(p, type);
        }
        EditorGUI.indentLevel -= indentDelta;
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

    public static void DrawPropertyLayout(SerializedProperty p, Type parentType) {

        FieldInfo fInfo = parentType.GetField(p.name);
        if (fInfo != null) {
            PropertyDrawer drawer = GetPropertyDrawerForField(fInfo);
            if (drawer != null) {
                DrawLayout(drawer, p);
            }
            else {
                EditorGUILayout.PropertyField(p, true);
            }
        }
    }

    public static void DrawProperty(SerializedProperty p, Type parentType) {
        throw new NotImplementedException();
    }

    public static void DrawLayout(PropertyDrawer drawer, SerializedProperty p, GUIContent label = null) {
        if (rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
        if (label == null) {
            label = new GUIContent(Util.SplitAndTitlize(p.name));
        }
        Rect position = EditorGUILayout.GetControlRect(true, drawer.GetPropertyHeight(p, label));
        rectField.SetValue(null, position);
        drawer.OnGUI(position, p, label);
    }

    public static void DrawExtendedPropertyLayout(ExtendedPropertyDrawer drawer, object source, GUIContent label) {
        if (rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
       // Rect position = EditorGUILayout.GetControlRect(true, drawer.GetPropertyHeight(source, label));
      //  rectField.SetValue(null, position);
       // drawer.OnGUI(position, source, label);
    }

    public static void DrawLayoutTexture(Texture2D texture, bool expand) {
        if (rectField == null) {
            rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
        }
        int height = texture.height;
        Rect position = EditorGUILayout.GetControlRect(false, height);
        if (!expand) {
            position = new Rect(position) {
                width = texture.width,
                height = texture.height
            };
        }
        rectField.SetValue(null, position);
        GUI.DrawTexture(position, texture);
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

    public static void DrawGUI(PropertyDrawer drawer, Rect position, SerializedProperty p, GUIContent label = null) {
        if (label == null) {
            label = new GUIContent(Util.SplitAndTitlize(p.name));
        }
        drawer.OnGUI(position, p, label);
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


