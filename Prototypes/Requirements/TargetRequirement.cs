using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem {

    public class TargetRequirement : RequirementPrototype {

        public string disposition = "replace with faction mask";
        public RequirementLog requirementLog;

        public override bool MeetsRequirement(Ability ability) {
            return ability.caster.Target != null;
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