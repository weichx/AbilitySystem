using UnityEngine;
using System.Collections.Generic;

namespace Intelligence {

    public class MultiPointContextCreator : PlayerAbilityContextCreator {

        public GameObject pointPrefab;
        public Projector targetSelectorPrefab;
        public int numPoints;
        public float maxDistance;
        
        private Projector targetSelector;
        private List<Vector3> pointList;
        private List<GameObject> pointMarkers;

        public override void Setup(Entity entity, Ability ability) {
            base.Setup(entity, ability);
            pointList = new List<Vector3>(12);
            pointMarkers = new List<GameObject>(12);
            targetSelector = Object.Instantiate(targetSelectorPrefab) as Projector;
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
                    GameObject marker = Object.Instantiate(pointPrefab) as GameObject;
                    marker.transform.position = hit.point;
                    marker.transform.rotation = Quaternion.identity;
                    pointMarkers.Add(marker);
                    pointList.Add(hit.point);
                    return pointMarkers.Count == numPoints ? ContextCreationStatus.Completed : ContextCreationStatus.Building;
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                return ContextCreationStatus.Cancelled;
            }
            return ContextCreationStatus.Building;
        }

        public override Context GetContext() {
            return new MultiPointContext(entity, pointList);
        }

        public override void Reset() {
            for (int i = 0; i < pointMarkers.Count; i++) {
                Object.Destroy(pointMarkers[i]);
            }
            pointMarkers = null;
            pointList = null;
            Object.Destroy(targetSelector.gameObject);
        }
    }

}