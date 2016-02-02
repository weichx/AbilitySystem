using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class ModifiableAttribute<T> : AbstractModifiableAttribute {

        protected MethodPointer<T, float, float> methodPointer;
        [SerializeField] protected List<AttributeModifier<T>> modifiers; 

        public ModifiableAttribute(string id, ModifiableAttribute<T> toClone) {
            this.id = id;
            baseValue = toClone.baseValue;
            methodPointer = new MethodPointer<T, float, float>(toClone.GetMethodPointer());
            value = toClone.value;
            modifiers = new List<AttributeModifier<T>>(toClone.modifiers.Count);
            for(int i = 0; i < modifiers.Count; i++) {
                modifiers[i] = new AttributeModifier<T>(toClone.modifiers[i].id, toClone.modifiers[i]);
            }
        }

        public MethodPointer<T, float, float> GetMethodPointer() {
            if(methodPointer == null) {
                methodPointer = new MethodPointer<T, float, float>(serializedMethodPointer);
            }
            return methodPointer;
        }

        public float UpdateValue(T arg0) {
            if(methodPointer.PointsToMethod) {
                value = methodPointer.Invoke(arg0, baseValue);
            }
            else {
                value = baseValue;
            }
            for(int i = 0; i < modifiers.Count; i++) {
                value += modifiers[i].ModifyValue(value, baseValue);
                value = modifiers[i].ClampValue(value, baseValue);
            }
 
            return value;
        }

        public virtual void AddModifier(AttributeModifier<T> modifier) {
            if (modifier == null) return;
            modifiers.Add(modifier);
        }

        public bool HasModifier(AttributeModifier<T> modifier) {
            return modifiers.Contains(modifier);
        }

        public virtual bool RemoveModifier(AttributeModifier<T> modifier) {
            return modifiers.Remove(modifier);
        }

        public void ClearModifiers() {
            value = baseValue;
            modifiers.Clear();
        }

        protected override void CreateTypedMethodPointer() {
            if (methodPointer == null) {
                methodPointer = new MethodPointer<T, float, float>(serializedMethodPointer);
            }
        }

    }

    [Serializable]
    public class ModifiableAttribute<T, U> : AbstractModifiableAttribute {

        protected MethodPointer<T, U, float, float> methodPointer;
        [SerializeField] protected List<AttributeModifier<T, U>> modifiers;

        public ModifiableAttribute(string id, ModifiableAttribute<T, U> toClone) {
            this.id = id;
            baseValue = toClone.baseValue;
            methodPointer = new MethodPointer<T, U, float, float>(toClone.GetMethodPointer());
            value = toClone.value;
            modifiers = new List<AttributeModifier<T, U>>(toClone.modifiers.Count);
            for (int i = 0; i < modifiers.Count; i++) {
                modifiers[i] = new AttributeModifier<T, U>(toClone.modifiers[i].id, toClone.modifiers[i]);
            }
        }

        public float UpdateValue(T arg0, U arg1) {
            if (methodPointer.PointsToMethod) {
                value = methodPointer.Invoke(arg0, arg1, baseValue);
            }
            else {
                value = baseValue;
            }
            return value;
        }

        public MethodPointer<T, U, float, float> GetMethodPointer() {
            if (methodPointer == null) {
                methodPointer = new MethodPointer<T, U, float, float>(serializedMethodPointer);
            }
            return methodPointer;
        }

        public virtual void AddModifier(AttributeModifier<T, U> modifier) {
            if (modifier == null) return;
            modifiers.Add(modifier);
        }

        public bool HasModifier(AttributeModifier<T, U> modifier) {
            return modifiers.Contains(modifier);
        }

        public virtual bool RemoveModifier(AttributeModifier<T, U> modifier) {
            return modifiers.Remove(modifier);
        }

        public void ClearModifiers() {
            value = baseValue;
            modifiers.Clear();
        }

        protected override void CreateTypedMethodPointer() {
            if (methodPointer == null) {
                methodPointer = new MethodPointer<T, U, float, float>(serializedMethodPointer);
            }
        }
    }

    [Serializable]
    public class ModifiableAttribute<T, U, V> : AbstractModifiableAttribute {

        protected MethodPointer<T, U, V, float, float> methodPointer;
        [SerializeField] protected List<AttributeModifier<T, U, V>> modifiers;

        public ModifiableAttribute(string id, ModifiableAttribute<T, U, V> toClone) {
            this.id = id;
            baseValue = toClone.baseValue;
            methodPointer = new MethodPointer<T, U, V, float, float>(toClone.GetMethodPointer());
            value = toClone.value;
            modifiers = new List<AttributeModifier<T, U, V>>(toClone.modifiers.Count);
            for (int i = 0; i < modifiers.Count; i++) {
                modifiers[i] = new AttributeModifier<T, U, V>(toClone.modifiers[i].id, toClone.modifiers[i]);
            }
        }

        public float UpdateValue(T arg0, U arg1, V arg2) {
            if (methodPointer.PointsToMethod) {
                value = methodPointer.Invoke(arg0, arg1, arg2, baseValue);
            }
            else {
                value = baseValue;
            }
            return value;
        }

        public MethodPointer<T, U, V, float, float> GetMethodPointer() {
            if (methodPointer == null) {
                methodPointer = new MethodPointer<T, U, V, float, float>(serializedMethodPointer);
            }
            return methodPointer;
        }

        public virtual void AddModifier(AttributeModifier<T, U, V> modifier) {
            if (modifier == null) return;
            modifiers.Add(modifier);
        }

        public bool HasModifier(AttributeModifier<T, U, V> modifier) {
            return modifiers.Contains(modifier);
        }

        public virtual bool RemoveModifier(AttributeModifier<T, U, V> modifier) {
            return modifiers.Remove(modifier);
        }

        public void ClearModifiers() {
            value = baseValue;
            modifiers.Clear();
        }

        protected override void CreateTypedMethodPointer() {
            if (methodPointer == null) {
                methodPointer = new MethodPointer<T, U, V, float, float>(serializedMethodPointer);
            }
        }
    }
    
}