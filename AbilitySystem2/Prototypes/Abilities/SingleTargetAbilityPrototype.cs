using UnityEngine;

namespace AbilitySystem {

    //todo enforce these attributes
    [RequireAbilityAttr("Projectile Speed")]
    public class SingleTargetAbilityPrototype : AbilityPrototype {

        public AbilityInitializer projectile;

        public override void OnTargetSelectionStarted(Ability ability, PropertySet properties) {
            Entity caster = ability.caster;
            Entity target = caster.Target;
            if(target == null) {
                ability.CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            Transform transform = ability.caster.transform;
            GameObject gameObject = Instantiate(projectile, transform.position, transform.rotation) as GameObject;
            AbilityInitializer initializer = gameObject.GetComponent<AbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(ability, properties);
            }
        }
    }

}