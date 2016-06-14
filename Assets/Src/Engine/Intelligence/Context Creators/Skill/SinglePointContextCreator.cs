using UnityEngine;

namespace Intelligence {

    public class SinglePointContextCreator : PlayerAbilityContextCreator {

        public LayerMask layerMask;
        public float maxDistance = 1000f;
        public Projector targetSelectorPrefab;

        private Projector targetSelector;
        private Vector3 targetPoint;

        public override void Setup(Entity entity, Ability ability) {
            base.Setup(entity, ability);
            targetSelector = UnityEngine.Object.Instantiate(targetSelectorPrefab) as Projector;
            targetSelector.enabled = false;
        }

        public override ContextCreationStatus UpdateContext() {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //todo layer mask
            if (Physics.Raycast(mouseRay, out hit, maxDistance)) {
                float distSqrd = hit.point.DistanceToSquared(entity.transform.position);
                float range = 999999;

                FloatRange rangeAttr = null;//ability.GetRequirement<AbilityRange>();
                if (rangeAttr != null) {
                    range = rangeAttr.Value;
                }
                if (range * range < distSqrd) {
                    targetSelector.enabled = false;
                    return ContextCreationStatus.Building;
                }
                targetSelector.enabled = true;
                targetSelector.transform.position = hit.point + Vector3.up * 3f;

                if (Input.GetMouseButtonDown(0)) {
                    targetPoint = hit.point;
                    return ContextCreationStatus.Completed;
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                return ContextCreationStatus.Cancelled;
            }
            return ContextCreationStatus.Building;
        }

        public override Context GetContext() {
            return new PointContext(entity, targetPoint);
        }

        public override void Reset() {
            UnityEngine.Object.Destroy(targetSelector.gameObject);
        }
    }

}