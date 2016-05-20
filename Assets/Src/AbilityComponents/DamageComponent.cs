using System;

[Serializable]
public class DamageComponent : AbilityComponent {

    public string element;
    public float baseValue;
    public DamageFormula formula;

    public float GetDamage(Context context) {
        if(formula == null) {
            return baseValue;
        }
        return formula.Invoke(context, baseValue);
    }

}
