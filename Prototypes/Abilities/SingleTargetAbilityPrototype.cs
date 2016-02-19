//using UnityEngine;

//namespace AbilitySystem {

//    //todo enforce these attributes
//    [RequireAbilityAttr("Projectile Speed")]
//    public class SingleTargetAbilityPrototype : Ability {

//        public GameObject projectile;
//        public Entity target;

//        public override void OnTargetSelectionStarted() {
//            target = caster.Target;
//            if(target == null) {
//                CancelCast();
//                return;
//            }
//        }

//        public override void OnCastCompleted() {
//            Transform transform = caster.transform;
//            GameObject gameObject = Instantiate(projectile, transform.position, transform.rotation) as GameObject;
//            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
//            if (initializer != null) {
//                initializer.Initialize(this);
//            }
//        }
//    }

//}