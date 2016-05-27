using System;

[Serializable]
public class HasStatusRequirement : AbilityRequirement {

    public StatusEffectCreator[] statuses;

    public override bool OnTest(OldContext context, RequirementType type) {
        var caster = context.entity;
        for (int i = 0; i < statuses.Length; i++) {
            if (statuses[i] == null) continue;
            if (caster.statusManager.HasStatus(statuses[i])) {
                return true;
            }
        }
        return false;
    }
}