using UnityEngine;
using System.Collections.Generic;

public partial class Ability {

    private Dictionary<string, object> attributes;

    public T GetAttribute<T>(string attrName) {
        return (T)attributes.Get(attrName);
    }

    public object GetAttribute(string attrName) {
        return attributes.Get(attrName);
    }

    public void SetAttribute<T>(string attrName, T value) {
        attributes[attrName] = value;
    }

    public void SetAttribute(string attrName, object value) {
        attributes[attrName] = value;
    }

    public bool HasAttribute(string propertyName) {
        return attributes.ContainsKey(propertyName);
    }

    public object this[string attrName] {
        get {
            return attributes[attrName];
        }
        set {
            attributes[attrName] = value;
        }
    }
}

