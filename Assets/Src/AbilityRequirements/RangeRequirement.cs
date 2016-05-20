using System;
using UnityEngine;

[Serializable]
public class RangeRequirement : AbilityRequirement {

    public FloatAttribute range;

    public override bool OnTest(Context context, RequirementType type) {
        var target = context.Get<Entity>("Target");
        var caster = context.entity;

        if(target == null) {
            return false;
        }

        return target.transform.DistanceToSquared(caster.transform) <= range.Value;
    }
}