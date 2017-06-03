
using UnityEngine;
using Intelligence;
using EntitySystem;
using System;

public class UsableAbility : InventoryItemComponent {

    [SerializeField] public AbilityCreator abilityCreator;
    [HideInInspector] public Ability ability;
    
    public override void OnUse() {
        ability =  abilityCreator.Create();
        ability.Caster = item.Owner;

        // ability.GetAbilityComponent<SingleTargetDamage>().baseDamage = item

        Debug.Log(ability.Id);

        // SetContext(ctx);
        // damageFormula.OnAfterDeserialize();
        // Debug.Log(damageFormula.Invoke((SingleTargetContext)ctx, baseDamage));

        // Debug.Log(damageFormula.Invoke(, (SingleTargetContext)ctx));
    }
}