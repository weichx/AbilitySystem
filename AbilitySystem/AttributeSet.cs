using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class AttributeSet<T> where T : AbstractModifiableAttribute {

        [SerializeField]
        public List<T> attrs = new List<T>();

        public T Get(string id) {
            for (int i = 0; i < attrs.Count; i++) {
                if (attrs[i].id == id) return attrs[i];
            }
            return null;
        }

        public void Set(T attr) {
            int idx = GetIndex(attr.id);
            if (idx != -1) {
                attrs[idx] = attr;
            }
            else {
                attrs.Add(attr);
            }
        }

        public int Count {
            get { return attrs.Count; }
        }

        private int GetIndex(string id) {
            for (int i = 0; i < attrs.Count; i++) {
                if (attrs[i].id == id) return i;
            }
            return -1;
        }

    }

    [Serializable]
    public class AbilityAttributeSet : AttributeSet<AbilityAttribute> {

        public void UpdateAll(Ability ability) {
            for (int i = 0; i < attrs.Count; i++) {
                attrs[i].UpdateValue(ability);
            }
        }
    }

}