//using UnityEngine;

//namespace AbilitySystem {

//    public class PointListAbilityPrototype : Ability {

//        public GameObject spell;
//        public GameObject pointMarkerPrefab;
//        public Projector pointSelectorPrefab;
//        public int totalPoints;
//        public Projector projector;
//        public Vector3[] pointList;
//        public GameObject[] markers;
//        public int usedPoints;

//        public override void OnTargetSelectionStarted() {
//            Projector gameObject = Instantiate(pointSelectorPrefab) as Projector;
//            projector = gameObject.GetComponent<Projector>();
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
//                    markers[usedPoints] = SpawnAndInitialize(pointMarkerPrefab, this, hit.point);
//                    usedPoints++;
//                }
//            }

//            if (Input.GetMouseButtonDown(1)) {
//                CancelCast();
//            }
//            return false;

//        }

//        public void Cleanup() {
//            if (projector != null) Destroy(projector.gameObject);
//        }

//        public override void OnTargetSelectionCancelled() {
//            Cleanup();
//            for (int i = 0; i < markers.Length; i++) {
//                DestructAndDespawn(markers[i], this);
//            }
//        }

//        public override void OnTargetSelectionCompleted() {
//            Cleanup();
//        }

//        public override void OnCastCancelled() {
//            Cleanup();
//            for (int i = 0; i < markers.Length; i++) {
//                DestructAndDespawn(markers[i], this);
//            }
//        }

//        public override void OnCastCompleted() {
//            SpawnAndInitialize(spell, this);
//            for (int i = 0; i < markers.Length; i++) {
//                DestructAndDespawn(markers[i], this);
//            }
//        }
//    }

//}