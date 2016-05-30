using  System;
using  UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class PropertyDrawerFor : Attribute {
    public Type type;

    public PropertyDrawerFor(Type type) {
        this.type = type;
    }
}

public abstract class ExtendedPropertyDrawer {

    public abstract void OnGUI(Rect position, SerializedPropertyX property, GUIContent label);

    public virtual float GetPropertyHeight(SerializedPropertyX property, GUIContent label) {
        return 16f;
    }

}

//public abstract class ExtendedPropertyDrawer<T> : ExtendedPropertyDrawer {

//    public override void OnGUI(Rect positionRect, object source, GUIContent label) {
//        OnGUI(positionRect, (T)source, label);
//    }

//    public abstract void OnGUI(Rect position, T source, GUIContent label);

//    public override float GetPropertyHeight(object source, GUIContent label) {
//        return GetPropertyHeight((T)source, label);
//    }

//    public virtual float GetPropertyHeight(T source, GUIContent label) {
//        return EditorGUIUtility.singleLineHeight;
//    }

//}