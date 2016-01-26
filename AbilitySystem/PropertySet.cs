using System;
using System.Collections.Generic;

public class PropertySet {

    private Dictionary<Key, object> properties;

    public PropertySet() {
        properties = new Dictionary<Key, object>();
    }


    public T Get<T>(string id) {
        object o;
        if (properties.TryGetValue(new Key(typeof(T), id), out o)) {
            return (T)o;
        }
        return default(T);
    }

    public void Set<T>(string id, T value) {
        properties[new Key(typeof(T), id)] = value;
    }


    public bool TryGetValue<T>(string id, out T reference) {
        var key = new Key(typeof(T), id);
        if (properties.ContainsKey(key)) {
            reference = (T)properties[key];
            return true;
        }
        else {
            reference = default(T);
            return false;
        }
    }

    public bool Contains<T>(string id) {
        return properties.ContainsKey(new Key(typeof(T), id));
    }

    public bool Remove<T>(string id) {
        return properties.Remove(new Key(typeof(T), id));
    }

    public void Clear() {
        properties.Clear();
    }

    public void Merge(PropertySet other) {
        if (other == null) return;
        foreach(var key in other.properties.Keys) {
            properties[key] = other.properties[key];
        }
    }

    private struct Key {
        public Type type;
        public string id;

        public Key(Type type, string id) {
            this.type = type;
            this.id = id;
        }
    }
}