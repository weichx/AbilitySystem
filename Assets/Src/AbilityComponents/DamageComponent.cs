using System;

[Serializable]
public class DamageComponent : AbilityComponent {

    public string element;
    public float baseValue;
    public MethodPointer<OldContext, float, float> formula;

    public float GetDamage(OldContext context) {
        if(formula == null) {
            return baseValue;
        }
        return formula.Invoke(context, baseValue);
    }

}
