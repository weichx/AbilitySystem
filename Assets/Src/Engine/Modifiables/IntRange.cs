using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class IntRange  {
    [SerializeField] protected List<IntModifier> modifiers;

    [SerializeField] protected int baseValue;
    [SerializeField] protected int currentValue;
    [SerializeField] protected int flatBonus;
    [SerializeField] protected float percentBonus;

    [SerializeField] protected IntRangeBoundry min;
    [SerializeField] protected IntRangeBoundry max;

    public IntRange() : this(0, int.MinValue, int.MaxValue) { }

    public IntRange(int value = 0, int minBase = 0, int maxBase = 0) {
        modifiers = new List<IntModifier>();
        min = new IntRangeBoundry(this, minBase);
        max = new IntRangeBoundry(this, maxBase);
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

        BaseValue = BaseValue; //weird but works

    }

    public void RemoveModifier(string id) {
        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                IntModifier prev = modifiers[i];
                flatBonus -= prev.flatBonus;
                percentBonus -= prev.percentBonus;
                BaseValue = BaseValue;
                break;
            }
        }
    }

    public IntModifier[] GetReadOnlyModiferList() {
        return modifiers.ToArray();
    }

    public IntRangeBoundry Min {
        get { return min; }
    }

    public IntRangeBoundry Max {
        get { return max; }
    }

    public int BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
            if (max.Value == 0 && min.Value == 0) {
                currentValue = 0;
                return;
            }
            float currentPercent = (float)(max.Value - currentValue) / (float)(max.Value - min.Value);
            int flatTotal = baseValue + flatBonus;
            int total = (int)(flatTotal + (flatTotal * percentBonus));
            currentValue = (int)Mathf.Clamp((currentPercent * total), min.Value, max.Value);
        }
    }

    public int Value {
        get { return currentValue; }
        set {
            currentValue = (int)Mathf.Clamp(value, min.Value, max.Value);
        }
    }

    public float NormalizedValue {
        set {
            float val = Mathf.Clamp01(value);
            int flatTotal = baseValue + flatBonus;
            int total = (int)(flatTotal + (flatTotal * percentBonus));
            currentValue = (int)Mathf.Clamp(val * total, min.Value, max.Value);
        }
    }

    public class IntRangeBoundry : IntValue {

        private IntRange parent;

        public IntRangeBoundry(IntRange parent, int baseValue = 0) : base(baseValue) {
            this.parent = parent;
        }

        public override int BaseValue {
            get { return base.BaseValue; }
            set {
                base.BaseValue = value;
                parent.Value = parent.Value;
            }
        }

    }
}

