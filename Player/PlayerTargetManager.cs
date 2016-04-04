using UnityEngine;

namespace AbilitySystem {
    public class PlayerTargetManager : TargetManager {

        private Projector projector;

        public override void Start() {
            projector = GetComponentInChildren<Projector>();
            base.Start();
        }

        public void Update() {

            if (Input.GetKeyDown(KeyCode.Escape)) {
                SetTarget(null);
            }

            if(Input.GetKeyDown(KeyCode.Tab)) {
                
            }

            if (!Input.GetMouseButtonDown(0)) return;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Entity"))) {
                Entity target = hit.transform.GetComponent<Entity>();
                SetTarget(target);
            }
        }

        public override void SetTarget(Entity target) {
            base.SetTarget(target);
            if (currentTarget != null) {
                Highlight();
            }
        }

        private void Highlight() {
            if (currentTarget == null) {
                projector.gameObject.SetActive(false);
                projector.transform.parent = transform;
                projector.transform.localPosition = Vector3.zero;
            }
            else {
                projector.gameObject.SetActive(true);
                projector.transform.parent = currentTarget.transform;
                projector.transform.localPosition = new Vector3 {
                    x = 0, y = 1, z = 0
                };
                Color highlightColor = Color.green;
                projector.material.SetColor("_Color", highlightColor);
            }
        }

    }
}