//using UnityEngine;

//namespace AbilitySystem {

//    public class PointAOEAbilityPrototype : Ability {

//        public GameObject spell;
//        public Projector aoeTargetSelectorPrefab;
//        protected Projector projector;

//        [HideInInspector] public Vector3 targetPoint;

//        public override void OnTargetSelectionStarted() {
//            Projector gameObject = Instantiate(aoeTargetSelectorPrefab) as Projector;
//            projector = gameObject.GetComponent<Projector>();
//        }

//        public override bool OnTargetSelectionUpdated() {
//            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
//            RaycastHit hit;
//            if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
//                float distSqrd = hit.point.DistanceToSquared(caster.transform.position);
//                float range = 999999;
//                AbilityAttribute rangeAttr = GetAttribute("Range");
//                if (rangeAttr != null) {
//                    range = rangeAttr.CachedValue;
//                }
//                if(range * range < distSqrd) {
//                    return false;
//                }
//                projector.transform.position = hit.point + Vector3.up * 3f;

//                if (Input.GetMouseButtonDown(0)) {
//                    targetPoint = hit.point;
//                    Debug.Log(hit.point);
//                    return true;
//                }
//            }

//            if (Input.GetMouseButtonDown(1)) {
//                CancelCast();
//            }
//            return false;

//        }

//        public override void OnTargetSelectionCompleted() {
//            if (projector != null) Destroy(projector.gameObject);
//        }

//        public override void OnTargetSelectionCancelled() {
//            if (projector != null) Destroy(projector.gameObject);
//        }

//        public override void OnCastCompleted() {
//            GameObject gameObject = Instantiate(spell, targetPoint, Quaternion.identity) as GameObject;
//            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
//            if (initializer != null) {
//                initializer.Initialize(this);
//            }
//        }
//    }

//}