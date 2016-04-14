using UnityEngine;

namespace AbilitySystem {

    [AddComponentMenu("Ability System/Targeting/Single Target")]
    public class SingleTargetStrategy : TargetingStrategy {

        [HideInInspector]
        public Entity target;

        public override void OnTargetSelectionStarted() {
            target = caster.Target;
            if (target == null) {
                ability.CancelCast();
                return;
            }
        }

    }

}