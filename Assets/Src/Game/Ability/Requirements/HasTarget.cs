using UnityEngine;
using Intelligence;

public class HasTarget : AbilityRequirement<SingleTargetContext> {

    public override bool OnTest(RequirementType type) {
        return context.target != null;
    }

    public override void OnFailed(RequirementType type) {
        Debug.Log("That ability requires a target");
    }

}