using UnityEngine;
using System;

namespace AbilitySystem {

    public class StationaryRequirementPrototype : RequirementPrototype {

        protected RequirementLog requirementLog;

        public override bool MeetsRequirement(Ability ability) {
            return !ability.caster.IsMoving;
        }

        public override void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {
            SetLogMessage();
        }

        private void SetLogMessage() {
            if (requirementLog == null) {
                requirementLog = GameObject.Find("RequirementLog").GetComponent<RequirementLog>();
            }
            if (requirementLog != null) {
                requirementLog.SetMessage("That ability requires a target");
            }
        }
    }

}