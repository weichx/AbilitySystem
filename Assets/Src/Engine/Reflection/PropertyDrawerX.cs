using  System;
using  UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class PropertyDrawerFor : Attribute {
    public Type type;

    public PropertyDrawerFor(Type type) {
        this.type = type;
    }
}

public abstract class PropertyDrawerX {

    public abstract void OnGUI(SerializedPropertyX property, GUIContent label);

    public virtual float GetPropertyHeight(SerializedPropertyX property, GUIContent label) {
        return 16f;
    }

}