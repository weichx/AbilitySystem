using UnityEngine;

namespace AbilitySystem {

    public class LaunchProjectileAction : AbilityAction {

        public HomingProjectile projectile;

        [Writable(false)]
        public Entity target;

        public override void OnTargetSelectionStarted() {
            target = caster.Target;
            if (target == null) {
                ability.CancelCast();
                return;
            }
        }

        public override void OnCastCompleted() {
            Transform transform = caster.transform;
            GameObject gameObject = Instantiate(projectile.gameObject, transform.position, transform.rotation) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(ability);
            }
        }
    }

}