using UnityEngine;
using Intelligence;

public class HasTarget : AbilityRequirement {

    public override bool OnTest(Context context, RequirementType type) {
        SingleTargetContext stc = context as SingleTargetContext;
        if (stc == null) return false;
        return type == RequirementType.CastStart && stc.target != null;
    }

    public override void OnFailed(Context context, RequirementType type) {
        Debug.Log("That ability requires a target");
    }

}