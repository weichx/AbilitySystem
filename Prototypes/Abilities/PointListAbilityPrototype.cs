using UnityEngine;

namespace AbilitySystem {

    public class PointListAbilityPrototype : AbilityPrototype {

        public GameObject spell;
        public GameObject pointMarkerPrefab;
        public Projector pointSelector;
        public int totalPoints;

        public override void OnTargetSelectionStarted(Ability ability, PropertySet properties) {
            Projector gameObject = Instantiate(pointSelector) as Projector;
            var projector = gameObject.GetComponent<Projector>();
            properties.Set("Projector", projector);
            properties.Set("PointList", new Vector3[totalPoints]);
            properties.Set("Markers", new GameObject[totalPoints]);
            properties.Set("UsedPoints", 0);
        }

        public override bool OnTargetSelectionUpdated(Ability ability, PropertySet properties) {
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
                    markers[usedPoints] = SpawnAndInitialize(pointMarkerPrefab, ability, properties, hit.point);
                    properties.Set("UsedPoints", ++usedPoints);
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                ability.CancelCast();
            }
            return false;

        }

        public void Cleanup(Ability ability, PropertySet properties) {
            Projector projector = properties.Get<Projector>("Projector");
            if (projector != null) Destroy(projector);
            properties.Delete<Projector>("Projector");    
        }

        public override void OnTargetSelectionCancelled(Ability ability, PropertySet properties) {
            Cleanup(ability, properties);
            GameObject[] markers = properties.Get<GameObject[]>("Markers");
            for (int i = 0; i < markers.Length; i++) {
                DestructAndDespawn(markers[i], ability, properties);
            }
            properties.Delete<GameObject[]>("Markers");
        }

        public override void OnTargetSelectionCompleted(Ability ability, PropertySet properties) {
            Cleanup(ability, properties);
        }

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            SpawnAndInitialize(spell, ability, properties);
            GameObject[] markers = properties.Get<GameObject[]>("Markers");
            for (int i = 0; i < markers.Length; i++) {
                DestructAndDespawn(markers[i], ability, properties);
            }
            properties.Delete<GameObject[]>("Markers");
        }
    }

}