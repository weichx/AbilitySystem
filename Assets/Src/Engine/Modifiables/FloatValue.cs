using  System.Collections.Generic;

public class FloatValue {

    private float baseValue;
    private float flatBonus;
    private float percentBonus;

    private List<FloatModifier> modifiers;

    public FloatValue(float baseValue = 0f) {
        this.baseValue = baseValue;
        modifiers = new List<FloatModifier>();
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

    }

    public FloatModifier[] GetReadOnlyModiferList() {
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
