using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[EntityDeserializerSkipConstructor]
public class FloatAttribute {
	//todo need a custom deserializer for this to run all modifiers
	[SerializeField] protected Dictionary<string, FloatModifier> modifiers;
    [SerializeField] protected float baseValue;
    [NonSerialized] protected float flatBonus;
    [NonSerialized] protected float percentBonus;
    [NonSerialized] protected float baseTotal;
    [NonSerialized] protected float currentValue;

    public FloatAttribute() {
        baseValue = 0;
        currentValue = 0;
        percentBonus = 0;
        baseTotal = baseValue;
        modifiers = new Dictionary<string, FloatModifier>();
    }

    public FloatAttribute(float baseValue) {
        this.baseValue = Mathf.Max(0, baseValue);
        currentValue = baseValue;
        baseTotal = baseValue;
        modifiers = new Dictionary<string, FloatModifier>();
    }

    public float GetFlatBonus(string id) {
        return modifiers.Get(id).flatBonus;
    }

    public float GetPercentBonus(string id) {
        return modifiers.Get(id).percentBonus;
    }

    public void SetFlatBonus(string id, float bonus) {
        FloatModifier mod = modifiers.Get(id);
        mod.flatBonus = bonus;
        SetModifier(id, mod);
    }

    public void SetPercentBonus(string id, float bonus) {
        FloatModifier mod = modifiers.Get(id);
        mod.percentBonus = bonus;
        SetModifier(id, mod);
    }

    public virtual void SetModifier(string id, FloatModifier modifier) {
        FloatModifier prev = modifiers.Get(id);

        flatBonus -= prev.flatBonus;
        percentBonus -= prev.percentBonus;

        flatBonus += modifier.flatBonus;
        percentBonus += modifier.percentBonus;

        modifiers[id] = modifier;

        float currentNormalized = NormalizedValue;
        float totalValue = baseValue + flatBonus;
        if(percentBonus != 0) {
            totalValue = totalValue + ((totalValue * percentBonus));
        }
        currentValue =  (currentNormalized * totalValue);
    }

    public virtual bool RemoveModifier(string id) {
        FloatModifier prev = modifiers.Get(id);
        flatBonus -= prev.flatBonus;
        percentBonus -= prev.percentBonus;

        float currentNormalized = NormalizedValue;
        float totalFlat = baseValue + flatBonus;
        baseTotal = totalFlat + (totalFlat * percentBonus);
        currentValue = (currentNormalized * baseTotal);
        return modifiers.Remove(id);
    }

    public float BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
            float currentNormalized = NormalizedValue;
            float totalFlat = baseValue + flatBonus;
            baseTotal = totalFlat + (totalFlat * percentBonus);
            currentValue = (currentNormalized * baseTotal);
        }
    }

    public virtual float Value {
        get { return currentValue; }
        set { 
			currentValue = (value > baseTotal) ? baseTotal : value;
		}
    }

    public virtual float NormalizedValue {
        get {
            if (baseTotal == 0) return 0;
            return currentValue / baseTotal;
        }
        set {
            float percent = Mathf.Clamp01(value);
            currentValue = baseTotal * percent;
        }
    }

    public float Max {
        get { return baseTotal; }
    }

    public float FlatBonus {
        get { return flatBonus; }
    }

    public float PercentBonus {
        get { return percentBonus; }
    }
}

