using UnityEngine;

namespace AbilitySystem {

    public class TargetedBlastPrototype : Ability {

        public GameObject spell;
        [Writable(false)] Entity target;

        public override void OnTargetSelectionStarted() {
             target = caster.Target;
            if (target == null) {
                CancelCast();
                return;
            }
        }

        public override void OnCastCompleted() {
            Vector3 toTarget = caster.transform.position.DirectionTo(target.transform.position);
            Vector3 blastPosition = target.transform.position - toTarget.normalized;
            GameObject gameObject = Instantiate(spell, blastPosition, Quaternion.identity) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(this);
            }
        }
    }

}