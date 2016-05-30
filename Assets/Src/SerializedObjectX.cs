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
        rootProperty = new SerializedPropertyX(this, "__root__", value.GetType(), value);
    }

    public void Update() {
        
    }
    
    public void ApplyModifiedProperties() {
        Type type = root.GetType();
        for (int i = 0; i < rootProperty.ChildCount; i++) {
            SerializedPropertyX property = rootProperty.GetChildAt(i);
            property.ApplyModifiedProperties(rootProperty);
            type.GetField(property.name, BindFlags).SetValue(root, property.Value);
        }
        int z =1;
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

    public int childCount {
        get { return rootProperty.ChildCount; }
    }
}