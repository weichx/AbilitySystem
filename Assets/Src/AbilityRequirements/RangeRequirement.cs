using System;
using UnityEngine;

[Serializable]
public class RangeRequirement : AbilityRequirement {

    public FloatRange range;

    public override bool OnTest(OldContext context, RequirementType type) {
        var target = context.Get<Entity>("Target");
        var caster = context.entity;

        if(target == null) {
            return false;
        }

        return target.transform.DistanceToSquared(caster.transform) <= range.Value;
    }
}