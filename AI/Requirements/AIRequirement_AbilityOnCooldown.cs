using AbilitySystem;
using UnityEngine;

public class AIRequirement_AbilityOnCooldown : AIRequirement {

    public string abilityId;

    public override bool Check(Context context) {
        Ability ability = context.entity.abilityManager.GetAbility(abilityId);
        return ability != null && ability.OnCooldown;
    }

}