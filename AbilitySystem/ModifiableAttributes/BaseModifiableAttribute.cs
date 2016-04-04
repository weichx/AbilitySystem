using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class BasicAttribute { }

    [Serializable]
    public class VariableModifier {

        public string name;
        public string source;
        public virtual void ModifyValue(ref float value, float baseTotal) {}

    }

    [Serializable]
    public class ModifiableVariable {

        [SerializeField] private float currentValue;
        [SerializeField] private float baseTotalValue;
        //todo expose this as read only
        private float modifiedTotalValue;
        public MethodPointer methodPointer;
        public VariableModifier[] modifiers;

        public float Current {
            get { return currentValue; }
            set { currentValue = Mathf.Clamp(currentValue, 0, modifiedTotalValue); }
        }

        public float Normalized {
            get { return (modifiedTotalValue > 0) ? currentValue / modifiedTotalValue : 0; }
            set { currentValue = Mathf.Clamp01(modifiedTotalValue - (value * modifiedTotalValue)); }
        }

        public float Total {
            get { return modifiedTotalValue; }
        }

        public float BaseTotal {
            get { return baseTotalValue; }
        }

        public void Update() {
            float currentTotal = baseTotalValue;
            for(int i = 0; i < modifiers.Length; i++) {
                modifiers[i].ModifyValue(ref currentTotal, baseTotalValue);
            }
            modifiedTotalValue = currentValue;
            currentValue = Mathf.Clamp(currentValue, 0, modifiedTotalValue);
        }
    }

    public abstract class AbstractModifiableAttribute : BasicAttribute, ISerializationCallbackReceiver {
        public string id;
        protected float value;

        [SerializeField] protected float baseValue;
        [SerializeField] protected MethodPointer serializedMethodPointer;
        [SerializeField] protected List<AttributeModifier> serializedModifiers;

        public float current;
        public float total;

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

        public void OnBeforeSerialize() {
            //todo serialize modifiers
        }

        public void OnAfterDeserialize() {
            CreateTypedMethodPointer();
            if(serializedModifiers == null) {
                serializedModifiers = new List<AttributeModifier>();
            }
            DeserializeModifiers();
        }

        protected abstract void DeserializeModifiers();
        protected abstract void CreateTypedMethodPointer();
    }
}