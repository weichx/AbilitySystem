using UnityEngine;

namespace AbilitySystem {

    public class TargetedBlastPrototype : Ability {

        public GameObject spell;
        public StatusEffectPrototype status;

        public override void OnTargetSelectionStarted(PropertySet properties) {
            Entity target = caster.Target;
            if (target == null) {
                CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastCompleted(PropertySet properties) {
            Transform transform = properties.Get<Entity>("Target").transform;
            Vector3 toTarget = caster.transform.position.DirectionTo(transform.position);
            Vector3 blastPosition = transform.position - toTarget.normalized;
            GameObject gameObject = Instantiate(spell, blastPosition, Quaternion.identity) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(this, properties);
            }
        }
    }

}