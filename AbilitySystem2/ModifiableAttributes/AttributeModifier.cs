using UnityEngine;
using System;

namespace AbilitySystem {

    [Serializable]
    public class AttributeModifier {
        //todo these might want tags
        //todo not sure if these should be taking in arguments from their calling attribute...probably
        public string id;
        public float baseValue;
        public float min;
        public float max;

        public AttributeModifier(string id, float baseValue, float min = 0, float max = float.MaxValue) {
            this.id = id;
            this.baseValue = baseValue;
            this.min = min;
            this.max = max;
        }

        ///<summary>
        ///This method should return ONLY the adjustment for this modifier, not adjustment + current value
        ///</summary>
        public float ModifyValue(float currentAttributeValue, float baseAttributeValue) {
            return baseValue;
        }

        ///<summary>
        ///This method should clamp and return the total value of the attribute.
        ///This will run after all modifiers have had `ModifyValue` called.
        ///</summary>
        public float ClampValue(float currentAttributeValue, float baseAttributeValue) {
            return Mathf.Clamp(currentAttributeValue, min, max);
        }
    }

    [Serializable]
    public class AttributeModifier<T> : AttributeModifier {

        public AttributeModifier(string id, float baseValue, float min = 0, float max = float.MaxValue) : base(id, baseValue, min, max) { }
        public AttributeModifier(string id, AttributeModifier<T> toClone) : base(id, toClone.baseValue, toClone.min, toClone.max) { }

        ///<summary>
        ///This method should return ONLY the adjustment for this modifier, not adjustment + current value
        ///</summary>
        public float ModifyValue(float currentAttributeValue, T arg0, float baseAttributeValue) {
            return baseValue;
        }

        ///<summary>
        ///This method should clamp and return the total value of the attribute.
        ///This will run after all modifiers have had `ModifyValue` called.
        ///</summary>
        public float ClampValue(float currentAttributeValue, T arg0, float baseAttributeValue) {
            return Mathf.Clamp(currentAttributeValue, min, max);
        }
    }

    [Serializable]
    public class AttributeModifier<T, U> : AttributeModifier {

        public AttributeModifier(string id, float baseValue, float min = 0, float max = float.MaxValue) : base(id, baseValue, min, max) { }
        public AttributeModifier(string id, AttributeModifier<T, U> toClone) : base(id, toClone.baseValue, toClone.min, toClone.max) { }

        ///<summary>
        ///This method should return ONLY the adjustment for this modifier, not adjustment + current value
        ///</summary>
        public float ModifyValue(float currentAttributeValue, T arg0, U arg1, float baseAttributeValue) {
            return baseValue;
        }

        ///<summary>
        ///This method should clamp and return the total value of the attribute.
        ///This will run after all modifiers have had `ModifyValue` called.
        ///</summary>
        public float ClampValue(float currentAttributeValue, T arg0, U arg1, float baseAttributeValue) {
            return Mathf.Clamp(currentAttributeValue, min, max);
        }
    }

    [Serializable]
    public class AttributeModifier<T, U, V> : AttributeModifier {

        public AttributeModifier(string id, float baseValue, float min = 0, float max = float.MaxValue) : base(id, baseValue, min, max) { }
        public AttributeModifier(string id, AttributeModifier<T, U, V> toClone) : base(id, toClone.baseValue, toClone.min, toClone.max) { }

        ///<summary>
        ///This method should return ONLY the adjustment for this modifier, not adjustment + current value
        ///</summary>
        public float ModifyValue(float currentAttributeValue, T arg0, U arg1, V arg2, float baseAttributeValue) {
            return baseValue;
        }

        ///<summary>
        ///This method should clamp and return the total value of the attribute.
        ///This will run after all modifiers have had `ModifyValue` called.
        ///</summary>
        public float ClampValue(float currentAttributeValue, T arg0, U arg1, V arg2, float baseAttributeValue) {
            return Mathf.Clamp(currentAttributeValue, min, max);
        }
    }
}