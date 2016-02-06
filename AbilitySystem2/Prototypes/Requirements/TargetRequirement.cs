using UnityEngine;
using System.Collections;
namespace AbilitySystem {
    public class TargetRequirement : RequirementPrototype {
        //todo replace with faction mask
        public string disposition = "replace with faction mask";

        public override bool MeetsRequirement(Ability ability) {
            return ability.caster.Target != null;
        }

        public override void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {
            Debug.Log("That ability requires a target.");
        }
    }
}