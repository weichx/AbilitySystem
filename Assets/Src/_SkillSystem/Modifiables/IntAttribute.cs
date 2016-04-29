using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class IntAttribute  {
    protected Dictionary<string, IntModifier> modifiers;
    [SerializeField] protected int baseValue;
    [NonSerialized] protected float flatBonus;
    [NonSerialized] protected float percentBonus;
    [NonSerialized] protected float totalValue;

    public IntAttribute(int baseValue = 0) {
        this.baseValue = baseValue;
        modifiers = new Dictionary<string, IntModifier>();
    }

    public void SetModifier(string id, IntModifier modifier) {
        IntModifier prev;

        if (modifiers.TryGetValue(id, out prev)) {
            flatBonus -= prev.flatBonus;
            percentBonus -= prev.percentBonus;
        }

        if (modifier != null) {
            flatBonus += modifier.flatBonus;
            percentBonus += modifier.percentBonus;
        }

        modifiers[id] = modifier;

        float totalFlat = baseValue + flatBonus;
        totalValue = totalFlat + (totalFlat * percentBonus);
    }

    public int Value {
        get { return (int)totalValue; }
    }
}

