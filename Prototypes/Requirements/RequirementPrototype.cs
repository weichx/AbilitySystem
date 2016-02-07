using UnityEditor;
using UnityEngine;

namespace AbilitySystem {

    public class RequirementPrototype : ScriptableObject {

        public virtual void OnEnabled(AbilityRequirement req) {

        }

        public virtual void OnDisabled(AbilityRequirement req) {

        }

        public virtual bool MeetsRequirement(Ability ability) {
            Debug.Log("BASE BAD");
            return true;
        }

        public virtual void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {

        }

        public virtual void OnRequirementPassed(Ability ability, AbilityRequirement req, RequirementType type) {

        }

        public static RequirementPrototype Default() {
            return CreateInstance<RequirementPrototype>();
        }
    }
}