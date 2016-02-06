using System.Collections.Generic;
using System;

namespace AbilitySystem {
    public class PropertySet {

        private Dictionary<Key, object> properties;

        public PropertySet() {
            properties = new Dictionary<Key, object>();
        }

        public void Set<T>(string id, T value) {
            properties.Add(new Key(typeof(T), id), value);
        }

        public T Get<T>(string id) {
            object o;
            if (properties.TryGetValue(new Key(typeof(T), id), out o)) {
                return (T)o;
            }
            return default(T);
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
            properties = new Dictionary<Key, object>();
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

}