using UnityEngine;
using System;

namespace AbilitySystem {
   
    public class RangeRequirementPrototype : RequirementPrototype {

        public AbilityAttribute attr;

        public override bool MeetsRequirement(Ability ability) {
            Entity caster = ability.caster;
            if (caster.Target == null) return false;
            return caster.Target.transform.DistanceToSquared(caster.transform) <= ability.GetAttributeValue("Range");
        }

    }

}