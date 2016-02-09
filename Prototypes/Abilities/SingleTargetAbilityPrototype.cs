using UnityEngine;

namespace AbilitySystem {

    //todo enforce these attributes
    [RequireAbilityAttr("Projectile Speed")]
    public class SingleTargetAbilityPrototype : Ability {

        public GameObject projectile;

        public override void OnTargetSelectionStarted(PropertySet properties) {
            Entity target = caster.Target;
            if(target == null) {
                CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastCompleted(PropertySet properties) {
            Transform transform = caster.transform;
            GameObject gameObject = Instantiate(projectile, transform.position, transform.rotation) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(this, properties);
            }
        }
    }

}