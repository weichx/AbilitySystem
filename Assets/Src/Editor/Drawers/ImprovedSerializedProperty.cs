using UnityEditor;
using UnityEngine;
using System;

//todo convert to extension method
public class ImprovedSerializedProperty {
    private SerializedProperty prop;

    public ImprovedSerializedProperty(SerializedProperty prop) {
        this.prop = prop;
    }

    public T[] GetArray<T>(string indexer) {
        SerializedProperty property = prop.FindPropertyRelative(indexer);
        if (property == null) return null;
        if (!property.isArray) return null;
        int arraySize = property.arraySize;
        T[] retn = new T[arraySize];
        for (int i = 0; i < arraySize; i++) {
            retn[i] = (T)(object)property.GetArrayElementAtIndex(i);
        }
        return retn;
    }

    public T Get<T>(string indexer) {
        return (T)Value<T>(prop.FindPropertyRelative(indexer));
    }

    public void Set<T>(string indexer, T _value) {
        Type type = typeof(T);
        var property = prop.FindPropertyRelative(indexer);
        if (property == null) throw new Exception("Null property in Set");
        object value = _value;

        if (type == typeof(string)) property.stringValue = value as string;
        else if (type == typeof(String)) property.stringValue = value as string;
        else if (type == typeof(int)) property.intValue = (int)value;
        else if (type == typeof(float)) property.floatValue = (float)value;
        else if (type == typeof(bool)) property.boolValue = (bool)value;
        else if (type == typeof(Vector2)) property.vector2Value = (Vector2)value;
        else if (type == typeof(Vector3)) property.vector3Value = (Vector3)value;
        else if (type == typeof(Vector4)) property.vector4Value = (Vector4)value;
        else if (type == typeof(Rect)) property.rectValue = (Rect)value;
        else if (type == typeof(Quaternion)) property.quaternionValue = (Quaternion)value;
        else if (type == typeof(double)) property.doubleValue = (double)value;
        else if (type == typeof(Color)) property.colorValue = (Color)value;
        else if (type == typeof(Bounds)) property.boundsValue = (Bounds)value;
        else throw new NotImplementedException();
    }

    private T Convert<T>(T type, object value) {
        return (T)value;
    }

    private object Value<T>(SerializedProperty property) {
        return (T)GetValue(typeof(T), property);
    }

    public static object GetValue(Type type, SerializedProperty property) {
        if (type == typeof(string)) return property.stringValue;
        if (type == typeof(int)) return property.intValue;
        if (type == typeof(float)) return property.floatValue;
        if (type == typeof(bool)) return property.boolValue;
        if (type == typeof(Vector2)) return property.vector2Value;
        if (type == typeof(Vector3)) return property.vector3Value;
        if (type == typeof(Vector4)) return property.vector4Value;
        if (type == typeof(Rect)) return property.rectValue;
        if (type == typeof(Quaternion)) return property.quaternionValue;
        if (type == typeof(double)) return property.doubleValue;
        if (type == typeof(Color)) return property.colorValue;
        if (type == typeof(Bounds)) return property.boundsValue;
        if (type == typeof(Rect)) return property.rectValue;
        if (type == typeof(long)) return property.longValue;
        if (type == typeof(AnimationCurve)) return property.animationCurveValue;
        return property.objectReferenceValue as System.Object;
    }

    public SerializedProperty FindPropertyRelative(string indexer) {
        return prop.FindPropertyRelative(indexer);
    }

    public ImprovedSerializedProperty Property(string indexer) {
        var p = prop.FindPropertyRelative(indexer);
        if (p == null) return null;
        return new ImprovedSerializedProperty(p);
    }
}