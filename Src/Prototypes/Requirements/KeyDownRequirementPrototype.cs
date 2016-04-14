using UnityEngine;
using System;

namespace AbilitySystem {

    public class KeyDownRequirementPrototype : RequirementPrototype {

        public KeyCode key;

        public override void OnRequirementFailed(Ability ability, AbilityRequirement req, RequirementType type) {
            Debug.Log("Failed, no key down");
        }

        public override bool MeetsRequirement(Ability ability) {
            return Input.GetKey(key);
        }

    }

}