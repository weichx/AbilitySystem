using AbilitySystem;

public class AIRequirement_AbilityOffCooldown : AIRequirement {

    public string abilityId;

    public override bool Check(AIDecisionContext context) {
        Ability ability = context.entity.abilityManager.GetAbility(abilityId);
        return ability == null || ability.OnCooldown;
    }

}