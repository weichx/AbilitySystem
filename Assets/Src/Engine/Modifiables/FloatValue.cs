using  System;
using  System.Collections.Generic;
using UnityEngine;

///<summary>
///A modifiable variable that is unbounded
///</summary>

public class FloatValue {

    [SerializeField] protected float baseValue;
    [SerializeField] protected float flatBonus;
    [SerializeField] protected float percentBonus;

    [SerializeField] protected List<FloatModifier> modifiers;

    public FloatValue() {
        baseValue = 0;
        modifiers = new List<FloatModifier>();
    }

    public FloatValue(float baseValue) {
        this.baseValue = baseValue;
        modifiers = new List<FloatModifier>();
    }

    public void SetModifier(string id, FloatModifier modifier) {
        modifiers = modifiers ?? new List<FloatModifier>();
        modifier = new FloatModifier(id, modifier);

        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                FloatModifier prev = modifiers[i];
                flatBonus -= prev.flatBonus;
                percentBonus -= prev.percentBonus;
                break;
            }
        }

        modifiers.Add(modifier);

        flatBonus += modifier.flatBonus;
        percentBonus += modifier.percentBonus;

    }

    public void ClearModifier(string id) {
        modifiers = modifiers ?? new List<FloatModifier>();
        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                FloatModifier prev = modifiers[i];
                flatBonus -= prev.flatBonus;
                percentBonus -= prev.percentBonus;
                modifiers.RemoveAt(i);
                break;
            }
        }
    }

    public FloatModifier[] GetReadOnlyModiferList() {
        modifiers = modifiers ?? new List<FloatModifier>();
        return modifiers.ToArray();
    }

    public virtual float BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
        }
    }

    public float Value {
        get {
            float flatTotal = baseValue + flatBonus;
            return flatTotal + (flatTotal * percentBonus);
        }
    }

}
