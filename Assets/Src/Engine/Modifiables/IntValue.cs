using System.Collections.Generic;
using UnityEngine;

///<summary>
///A modifiable variable that is unbounded
///</summary>
public class IntValue {

    [SerializeField] protected int baseValue;
    [SerializeField] protected int flatBonus;
    [SerializeField] protected float percentBonus;

    [SerializeField] protected List<IntModifier> modifiers;

    public IntValue() {
        baseValue = 0;
        modifiers = new List<IntModifier>();
    }

    public IntValue(int baseValue) {
        this.baseValue = baseValue;
        modifiers = new List<IntModifier>();
    }

    public void SetModifier(string id, IntModifier modifier) {
        modifier = new IntModifier(id, modifier);

        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                IntModifier prev = modifiers[i];
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
        modifiers = modifiers ?? new List<IntModifier>();
        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                IntModifier prev = modifiers[i];
                flatBonus -= prev.flatBonus;
                percentBonus -= prev.percentBonus;
                modifiers.RemoveAt(i);
                break;
            }
        }
    }

    public IntModifier[] GetReadOnlyModiferList() {
        modifiers = modifiers ?? new List<IntModifier>();
        return modifiers.ToArray();
    }

    public virtual int BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
        }
    }

    public int Value {
        get {
            int flatTotal = baseValue + flatBonus;
            return (int)(flatTotal + (flatTotal * percentBonus));
        }
    }

}
