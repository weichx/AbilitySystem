using Intelligence;
using UnityEngine;

public class Facing : AbilityRequirement<SingleTargetContext> {

    public float maxDotToTarget;//todo make this a drawer a la http://forum.unity3d.com/threads/angle-property-drawer.236359/

    //todo -- I dont think is quite right
    public override bool OnTest(RequirementType type) {
        Vector3 other = context.target.transform.position;
        Vector3 toTarget = context.entity.transform.position.DirectionTo(other);
        return context.entity.transform.forward.Dot(toTarget) < maxDotToTarget;
    }

}