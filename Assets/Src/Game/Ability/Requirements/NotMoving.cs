using Intelligence;
using UnityEngine;

public class NotMoving : AbilityRequirement {

    public override bool OnTest(Context context, RequirementType type) {
        return !context.entity.IsMoving;
    }

    public override void OnFailed(Context context, RequirementType type) {
        Debug.Log("Cannot move while casting");
    }
}