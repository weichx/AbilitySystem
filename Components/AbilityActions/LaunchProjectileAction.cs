using UnityEngine;

namespace AbilitySystem {

    public class LaunchProjectileAction : AbilityAction {

        public HomingProjectile projectilePrefab;
        protected Entity target;

        public override void OnTargetSelectionStarted() {
            target = caster.Target;
            if (target == null) {
                ability.CancelCast();
                return;
            }
        }

        public override void OnCastCompleted() {
            Transform transform = caster.transform;
            GameObject gameObject = Instantiate(projectilePrefab.gameObject, transform.position, transform.rotation) as GameObject;
            HomingProjectile projectile = gameObject.GetComponent<HomingProjectile>();
            projectile.Initialize(target);
            gameObject.transform.position += Vector3.up;
        }
    }

}