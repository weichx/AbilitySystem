using UnityEngine;

namespace AbilitySystem {

    public class PointListAbilityPrototype : Ability {

        public GameObject spell;
        public GameObject pointMarkerPrefab;
        public Projector pointSelector;
        public int totalPoints;

        public override void OnTargetSelectionStarted(PropertySet properties) {
            Projector gameObject = Instantiate(pointSelector) as Projector;
            var projector = gameObject.GetComponent<Projector>();
            properties.Set("Projector", projector);
            properties.Set("PointList", new Vector3[totalPoints]);
            properties.Set("Markers", new GameObject[totalPoints]);
            properties.Set("UsedPoints", 0);
        }

        public override bool OnTargetSelectionUpdated(PropertySet properties) {
            int usedPoints = properties.Get<int>("UsedPoints");
            if (totalPoints == usedPoints) return true;
            Projector projector = properties.Get<Projector>("Projector");
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
                projector.transform.position = hit.point + Vector3.up * 3f;

                if (Input.GetMouseButtonDown(0)) {
                    Vector3[] points = properties.Get<Vector3[]>("PointList");
                    GameObject[] markers = properties.Get<GameObject[]>("Markers");
                    //lift the point slightly off the ground
                    points[usedPoints] = hit.point + (Vector3.up * 0.1f);
                    markers[usedPoints] = SpawnAndInitialize(pointMarkerPrefab, this, properties, hit.point);
                    properties.Set("UsedPoints", ++usedPoints);
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                CancelCast();
            }
            return false;

        }

        public void Cleanup(PropertySet properties) {
            Projector projector = properties.Get<Projector>("Projector");
            if (projector != null) Destroy(projector);
            properties.Delete<Projector>("Projector");    
        }

        public override void OnTargetSelectionCancelled(PropertySet properties) {
            Cleanup(properties);
            GameObject[] markers = properties.Get<GameObject[]>("Markers");
            for (int i = 0; i < markers.Length; i++) {
                DestructAndDespawn(markers[i], this, properties);
            }
            properties.Delete<GameObject[]>("Markers");
        }

        public override void OnTargetSelectionCompleted(PropertySet properties) {
            Cleanup(properties);
        }

        public override void OnCastCancelled(PropertySet properties) {
            Cleanup(properties);
            GameObject[] markers = properties.Get<GameObject[]>("Markers");
            for (int i = 0; i < markers.Length; i++) {
                DestructAndDespawn(markers[i], this, properties);
            }
            properties.Delete<GameObject[]>("Markers");
        }

        public override void OnCastCompleted(PropertySet properties) {
            SpawnAndInitialize(spell, this, properties);
            GameObject[] markers = properties.Get<GameObject[]>("Markers");
            for (int i = 0; i < markers.Length; i++) {
                DestructAndDespawn(markers[i], this, properties);
            }
            properties.Delete<GameObject[]>("Markers");
        }
    }

}