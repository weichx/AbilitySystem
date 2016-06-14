using Intelligence;
using UnityEngine;

public class Facing : AbilityRequirement {

    public float maxDotToTarget;//todo make this a drawer a la http://forum.unity3d.com/threads/angle-property-drawer.236359/

    //todo -- I dont think is quite right
    public override bool OnTest(Context context, RequirementType type) {
        SingleTargetContext ctx = context as SingleTargetContext;
        Vector3 other = ctx.target.transform.position;
        Vector3 toTarget = ctx.entity.transform.position.DirectionTo(other);
        return ctx.entity.transform.forward.Dot(toTarget) < maxDotToTarget;
    }

}