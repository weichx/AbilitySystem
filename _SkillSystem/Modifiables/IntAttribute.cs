using System.Collections.Generic;

public class IntAttribute  {
    protected Dictionary<string, IntModifier> modifiers;
    protected float baseValue;
    protected float flatBonus;
    protected float percentBonus;
    protected float totalValue;

    public IntAttribute(float baseValue = 0) {
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

