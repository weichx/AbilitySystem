using Intelligence;
using UnityEngine;
using EntitySystem;

public class NotMoving : AbilityRequirement<Context> {

    public override bool OnTest(RequirementType type) {
        return !context.entity.IsMoving;
    }

    public override void OnFailed(RequirementType type) {
        Debug.Log("Cannot move while casting");
    }
}