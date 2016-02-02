using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class BasicAttribute { }

    public abstract class AbstractModifiableAttribute : BasicAttribute, ISerializationCallbackReceiver {
        public string id;
        protected float value;

        [SerializeField] protected float baseValue;
        [SerializeField] protected MethodPointer serializedMethodPointer;

        public AbstractModifiableAttribute() {
            value = 0f;
            baseValue = 0f;
        }

        public AbstractModifiableAttribute(string id, float baseValue) {
            this.id = id;
            this.baseValue = baseValue;
            baseValue = value;
        }

        public float BaseValue {
            get { return baseValue; }
            set { baseValue = value; }
        }

        public float CachedValue {
            get { return value; }
        }

        //public virtual void AddModifier(AttributeModifier modifier) {
        //    if (modifier == null) return;
        //    modifiers.Add(modifier);
        //}

        //public bool HasModifier(AttributeModifier modifier) {
        //    return modifiers.Contains(modifier);
        //}

        //public virtual bool RemoveModifier(AttributeModifier modifier) {
        //    return modifiers.Remove(modifier);
        //}

        //public void ClearModifiers() {
        //    value = baseValue;
        //    modifiers.Clear();
        //}

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            CreateTypedMethodPointer();
        }

        protected abstract void CreateTypedMethodPointer();
    }
}