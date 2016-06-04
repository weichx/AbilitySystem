using UnityEngine;
using System.Collections.Generic;

public class FloatRange {

    [SerializeField] protected List<FloatModifier> modifiers;

    [SerializeField] protected float baseValue;
    [SerializeField] protected float currentValue;
    [SerializeField] protected float flatBonus;
    [SerializeField] protected float percentBonus;

    [SerializeField] protected FloatRangeBoundry min;
    [SerializeField] protected FloatRangeBoundry max;

    public FloatRange() : this(0, float.MinValue, float.MaxValue) { }

    public FloatRange(float value, float minBase = float.MinValue, float maxBase = float.MaxValue) {
        modifiers = new List<FloatModifier>();
        min = new FloatRangeBoundry(this, minBase);
        max = new FloatRangeBoundry(this, maxBase);
    }

    public void SetModifier(string id, FloatModifier modifier) {
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

        BaseValue = BaseValue; //weird but works

    }

    public FloatModifier[] GetReadOnlyModiferList() {
        return modifiers.ToArray();
    }

    public void RemoveModifier(string id) {
        for (int i = 0; i < modifiers.Count; i++) {
            if (modifiers[i].id == id) {
                FloatModifier prev = modifiers[i];
                flatBonus -= prev.flatBonus;
                percentBonus -= prev.percentBonus;
                BaseValue = BaseValue;
                break;
            }
        }
    }

    public FloatRangeBoundry Min {
        get { return min; }
    }

    public FloatRangeBoundry Max {
        get { return max; }
    }

    public float BaseValue {
        get { return baseValue; }
        set {
            baseValue = value;
            if (max.Value == 0 && min.Value == 0) {
                currentValue = 0;
                return;
            }
            float currentPercent = (max.Value - currentValue) / (max.Value - min.Value);
            float flatTotal = baseValue + flatBonus;
            float total = flatTotal + (flatTotal * percentBonus);
            currentValue = Mathf.Clamp(currentPercent * total, min.Value, max.Value);
        }
    }

    public float Value {
        get { return currentValue; }
        set {
            currentValue = Mathf.Clamp(value, min.Value, max.Value);
        }
    }

    public float NormalizedValue {
        set {
            float val = Mathf.Clamp01(value);
            float flatTotal = baseValue + flatBonus;
            float total = flatTotal + (flatTotal * percentBonus);
            currentValue = Mathf.Clamp(val * total, min.Value, max.Value);
        }
    }

    public class FloatRangeBoundry : FloatValue {

        private FloatRange parent;

        public FloatRangeBoundry(FloatRange parent, float baseValue = 0f) : base(baseValue) {
            this.parent = parent;
        }

        public override float BaseValue {
            get { return base.BaseValue; }
            set {
                base.BaseValue = value;
                parent.Value = parent.Value;
            }
        }

    }
}

