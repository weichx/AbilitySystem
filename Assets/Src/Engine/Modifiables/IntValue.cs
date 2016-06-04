using System.Collections.Generic;

public class IntValue {

    private int baseValue;
    private int flatBonus;
    private float percentBonus;

    private List<IntModifier> modifiers;

    public IntValue(int baseValue = 0) {
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
