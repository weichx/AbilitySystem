using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem {

    public class RangeRequirementPrototype : RequirementPrototype {

        public RequirementLog requirementLog;

        public override void OnEnabled(AbilityRequirement req) {
            if (requirementLog == null) {
                requirementLog = GameObject.Find("RequirementLog").GetComponent<RequirementLog>();
            }
        }

        public override bool MeetsRequirement(Ability ability) {
            Entity caster = ability.caster;
            if (caster.Target == null) return false;
            float attr = ability.GetAttribute<FloatAttribute>("range").Value;
            attr *= attr;
            return caster.Target.transform.DistanceToSquared(caster.transform) <= attr;
        }

        public override void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {
            SetLogMessage();
        }

        private void SetLogMessage() {
            if (requirementLog == null) {
                requirementLog = GameObject.Find("RequirementLog").GetComponent<RequirementLog>();
            }
            if (requirementLog != null) {
                requirementLog.SetMessage("Target is out of range");
            }
        }

    }

}