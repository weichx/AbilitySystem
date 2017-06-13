using Intelligence;
using EntitySystem;

public class SingleTargetDamage : AbilityComponent<SingleTargetContext> {

//    public float baseDamage;
    public DiceBase diceBae;
    public ElementType elementType;
//    public MethodPointer<SingleTargetContext, float, float> damageFormula;
    public MethodPointer<SingleTargetContext, DiceBase, int> damgeFormula;

    public override void OnCastCompleted() {
        // if (damageFormula == null) return;
        // float damage = damageFormula.Invoke(baseDamage, context);
        //todo do something with damage, damage aggregator component perhaps?
    }
}


// public class MultiTargetDamage : AbilityComponent<MultiPointContext> {
//     public MethodPointer<MultiPointContext, float float> damageFormula;

//     public override void OnCastCompleted() {
//         ability.Caster.character
//     }
// }