using UnityEngine;
using System;

[Serializable]
public class FaceTargetRequirement : AbilityRequirement {

    [Range(0f, 180f)]
    public float maximumAngleDifference;

    public override bool OnTest(OldContext context, RequirementType type) {
        Entity target = context.Get<Entity>("Target");
        Entity caster = context.Get<Entity>("Caster");
        if(target == null) {
            return true;
        }
        return Vector3.Angle(target.transform.position, caster.transform.forward) < maximumAngleDifference;
    }

}