using System.Collections.Generic;
using UnityEngine;

public class Context {

    public readonly List<Entity> targets;
    public readonly List<Vector3> positions;

    public Ability ability;
    public Entity entity;
    protected Dictionary<string, object> properties;

    public Context() {
        targets = new List<Entity>();
        positions = new List<Vector3>();
        properties = new Dictionary<string, object>();
    }

    public void Clear() {
        entity = null;
        ability = null;
        targets.Clear();
        positions.Clear();
        targets.Clear();
    }

    public Entity Caster {
        get { return entity; }
    }

    public T Get<T>(string propertyName) {
        return (T)properties.Get(propertyName);
    }

    public object Get(string propertyName) {
        return properties.Get(propertyName);
    }

    public void Set<T>(string propertyName, T value) {
        properties[propertyName] = value;
    }

    public void Set(string propertyName, object value) {
        properties[propertyName] = value;
    }

    public bool Has(string propertyName) {
        return properties.ContainsKey(propertyName);
    }

    public bool MultiTarget {
        get { return targets.Count > 1; }
    }

    public bool MultiPoint {
        get { return positions.Count > 1; }
    }

    public object this[string propertyName] {
        get {
            return properties[propertyName];
        }
        set {
            properties[propertyName] = value;
        }
    }

}
