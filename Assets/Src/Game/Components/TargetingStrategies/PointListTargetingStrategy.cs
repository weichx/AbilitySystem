//using UnityEngine;

//namespace AbilitySystem {

//    [AddComponentMenu("Ability System/Targeting/Point List")]
//    public class PointListTargetingStrategy : TargetingStrategy {

//        public GameObject pointMarkerPrefab;
//        public Projector pointSelectorPrefab;
//        protected Projector projector;

//        public int totalPoints = 3;
//        public bool spawnAtCaster = false;
//        protected int usedPoints;
//        protected GameObject[] markers;

//        [HideInInspector]
//        public Vector3[] pointList;

//        public Vector3 FirstPosition {
//            get {
//                if (spawnAtCaster) {
//                    return caster.transform.position;
//                }
//                else {
//                    return pointList[0];
//                }
//            }
//        }

//        public override void OnTargetSelectionStarted() {
//            projector = Instantiate(pointSelectorPrefab) as Projector;
//            usedPoints = 0;
//            pointList = new Vector3[totalPoints];
//            markers = new GameObject[totalPoints];
//        }

//        public override bool OnTargetSelectionUpdated() {
//            if (totalPoints == usedPoints) return true;
//            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
//            RaycastHit hit;
//            if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
//                projector.transform.position = hit.point + Vector3.up * 3f;

//                if (Input.GetMouseButtonDown(0)) {
//                    //lift the point slightly off the ground
//                    pointList[usedPoints] = hit.point + (Vector3.up * 0.1f);
//                    markers[usedPoints] = Instantiate(pointMarkerPrefab, hit.point, Quaternion.identity) as GameObject;
//                    usedPoints++;
//                }
//            }

//            if (Input.GetMouseButtonDown(1)) {
//                ability.CancelCast();
//            }
//            return false;
//        }

//        public void Cleanup() {
//            if (projector != null) Destroy(projector.gameObject);
//            for (int i = 0; i < markers.Length; i++) {
//                if (markers[i] != null) Destroy(markers[i]);
//            }
//        }

//        public override void OnTargetSelectionCancelled() {
//            Cleanup();
//        }

//        public override void OnTargetSelectionCompleted() {
//            Cleanup();
//        }

//    }

//}