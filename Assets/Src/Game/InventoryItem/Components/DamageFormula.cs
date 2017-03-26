
using UnityEngine;
using Intelligence;
using EntitySystem;
using System;

public class DamageFormula : InventoryItemComponent<SingleTargetContext> {

    public int baseDamage;

    public MethodPointer<SingleTargetContext, float, float> damageFormula;

    // For debugging
    public CharacterCreator TargetCharacter;

    public override void OnUse() {
        SetContext(ctx);
        damageFormula.OnAfterDeserialize();
        Debug.Log(damageFormula.Invoke((SingleTargetContext)ctx, baseDamage));

        //Debug.Log(damageFormula.Invoke(, (SingleTargetContext)ctx));
    }
}