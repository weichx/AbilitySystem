using Intelligence;

public class SingleTargetDamage : AbilityComponent {

    public float baseDamage;
    public ElementType elementType;
    public MethodPointer<float, SingleTargetContext, float> damageFormula;

    public override void OnCastCompleted() {
        if (damageFormula == null) return;
        float damage = damageFormula.Invoke(baseDamage, ability.GetContext<SingleTargetContext>());
        //todo do something with damage, damage aggregator component perhaps?
    }

}