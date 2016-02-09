using UnityEngine;

namespace AbilitySystem {

    public class TargetedBlastPrototype : AbilityPrototype {

        public GameObject spell;
        public StatusEffectPrototype status;

        public override void OnTargetSelectionStarted(Ability ability, PropertySet properties) {
            Entity caster = ability.caster;
            Entity target = caster.Target;
            if (target == null) {
                ability.CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            Transform transform = properties.Get<Entity>("Target").transform;
            Vector3 toTarget = ability.caster.transform.position.DirectionTo(transform.position);
            Vector3 blastPosition = transform.position - toTarget.normalized;
            GameObject gameObject = Instantiate(spell, blastPosition, Quaternion.identity) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(ability, properties);
            }
        }
    }

}