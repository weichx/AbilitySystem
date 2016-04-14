using UnityEngine;
using System;

namespace AbilitySystem {

    public class FacingRequirementPrototype : RequirementPrototype {

        //[Range(0, 180)]
        public float dotRequirement = 0.75f;
        protected RequirementLog requirementLog;

        public override bool MeetsRequirement(Ability ability) {
            Entity caster = ability.caster;
            if (caster.Target == null) return false;
            Vector3 toTarget = caster.transform.position.DirectionTo(caster.Target.transform.position).normalized;
            float fdot = Vector3.Dot(toTarget, caster.transform.forward);
            return fdot >= dotRequirement;
        }

        public override void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {
            SetLogMessage();
        }

        private void SetLogMessage() {
            if (requirementLog == null) {
                requirementLog = GameObject.Find("RequirementLog").GetComponent<RequirementLog>();
            }
            if (requirementLog != null) {
                requirementLog.SetMessage("Target must be in front of you");
            }
        }
    }
}