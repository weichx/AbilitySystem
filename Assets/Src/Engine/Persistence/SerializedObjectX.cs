using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SerializedObjectX {
    
    public bool IsDirty { get; set; }
    public readonly object root;
    private SerializedPropertyX rootProperty;
    private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public SerializedObjectX(object value) {
        root = value;
        if (value == null) throw new Exception("Target of a SerializedObjectX cannot be null or primitive");
        Type type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(string)) {
            throw new Exception("Target of a SerializedObjectX cannot be null or primitive"); 
        }
        rootProperty = new SerializedPropertyX("__root__", value.GetType(), value);
    }

    public void Update() {
        
    }
    
    public bool ApplyModifiedProperties() {
        Type type = root.GetType();
        bool changed = false;
        for (int i = 0; i < rootProperty.ChildCount; i++) {
            SerializedPropertyX property = rootProperty.GetChildAt(i);
            bool didChildChange = property.ApplyModifiedProperties();
            if (!changed && didChildChange) {
                changed = true;
            }
            type.GetField(property.name, BindFlags).SetValue(root, property.Value);
        }
        return changed;
    }

    public SerializedPropertyX GetChildAt(int idx) {
        return rootProperty.GetChildAt(idx);
    }

    public SerializedPropertyX FindProperty(string name) {
        return rootProperty.FindProperty(name);
    }

    public SerializedPropertyX Root {
        get { return rootProperty; }
    }

    public int ChildCount {
        get { return rootProperty.ChildCount; }
    }

    public SerializedPropertyX this[string path] {
        get { return rootProperty.FindProperty(path); }
    }
}