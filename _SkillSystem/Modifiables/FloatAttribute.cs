using UnityEngine;
using System.Collections.Generic;

public class FloatAttribute {

    protected Dictionary<string, FloatModifier> modifiers;
    protected float baseValue;
    protected float flatBonus;
    protected float percentBonus;
    protected float totalValue;
    protected float currentValue;

    public FloatAttribute() {
        baseValue = 0;
        currentValue = 0;
        totalValue = baseValue;
        modifiers = new Dictionary<string, FloatModifier>();
    }

    public FloatAttribute(float baseValue) {
        this.baseValue = Mathf.Max(0, baseValue);
        currentValue = baseValue;
        totalValue = baseValue;
        modifiers = new Dictionary<string, FloatModifier>();
    }

    public float GetFlatBonus(string id) {
        return modifiers.Get(id).flatBonus;
    }

    public float GetPercentBonus(string id) {
        return modifiers.Get(id).percentBonus;
    }

    public void SetFlatBonus(string id, float bonus) {
        FloatModifier mod = modifiers[id];
        mod.flatBonus = bonus;
        SetModifier(id, mod);
    }

    public void SetPercentBonus(string id, float bonus) {
        FloatModifier mod = modifiers[id];
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
        float totalFlat = baseValue + flatBonus;
        totalValue = totalFlat + (totalFlat * percentBonus);
        currentValue = currentNormalized * totalValue;
    }

    public virtual bool RemoveModifier(string id) {
        FloatModifier prev = modifiers.Get(id);
        flatBonus -= prev.flatBonus;
        percentBonus -= prev.percentBonus;

        float currentNormalized = NormalizedValue;
        float totalFlat = baseValue + flatBonus;
        totalValue = totalFlat + (totalFlat * percentBonus);
        currentValue = currentNormalized * totalValue;
        return modifiers.Remove(id);
    }

    public float BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
            float currentNormalized = NormalizedValue;
            float totalFlat = baseValue + flatBonus;
            totalValue = totalFlat + (totalFlat * percentBonus);
            currentValue = currentNormalized * totalValue;
        }
    }

    public virtual float Value {
        get { return currentValue; }
        set { currentValue = (value > totalValue) ? totalValue : value; }
    }

    public virtual float NormalizedValue {
        get {
            if (totalValue == 0) return 0;
            return currentValue / totalValue;
        }
        set {
            float percent = Mathf.Clamp01(value);
            currentValue = totalValue * percent;
        }
    }

    public float FlatBonus {
        get { return flatBonus; }
    }

    public float PercentBonus {
        get { return percentBonus; }
    }

    //extend this for 'bounded float attribute'
    //public virtual float MaxValue {
    //    get { return totalValue; }
    //}

    //public virtual float MinValue {

    //}

    //todo add method to show modifiers 
}

