using Intelligence;
using EntitySystem;
using System.Collections.Generic;

public class SingleTargetDamage : AbilityComponent<SingleTargetContext> {

//    public float baseDamage;
    public DiceBase diceBase;
    public ElementType elementType;
    public MethodPointer<SingleTargetContext, float, float> damageFormula;
    // public MethodPointer<SingleTargetContext, DiceBase, int> damgeFormula;

    public List<Modifier> modifiers;
    public override void OnCastCompleted() {
        // if (damageFormula == null) return;
        // float damage = damageFormula.Invoke(baseDamage, context);
        //todo do something with damage, damage aggregator component perhaps?
    }

}

