using UnityEngine;

namespace AbilitySystem {

    public class PointAOEAbilityPrototype : AbilityPrototype {

        public GameObject spell;
        public Projector aoeTargetSelector;

        public override void OnTargetSelectionStarted(Ability ability, PropertySet properties) {
            Projector gameObject = Instantiate(aoeTargetSelector) as Projector;
            var projector = gameObject.GetComponent<Projector>();
            properties.Set("Projector", projector);
        }

        public override bool OnTargetSelectionUpdated(Ability ability, PropertySet properties) {
            Projector projector = properties.Get<Projector>("Projector");
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
                float distSqrd = hit.point.DistanceToSquared(ability.caster.transform.position);
                float range = 999999;
                AbilityAttribute rangeAttr = ability.GetAttribute("Range");
                if (rangeAttr != null) {
                    range = rangeAttr.CachedValue;
                }
                if(range * range < distSqrd) {
                    return false;
                }
                projector.transform.position = hit.point + Vector3.up * 3f;

                if (Input.GetMouseButtonDown(0)) {
                    properties.Set("TargetPoint", hit.point);
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                ability.CancelCast();
            }
            return false;

        }

        public override void OnTargetSelectionCompleted(Ability ability, PropertySet properties) {
            Projector projector = properties.Get<Projector>("Projector");
            if (projector != null) Destroy(projector);
            properties.Delete<Projector>("Projector");
        }

        public override void OnTargetSelectionCancelled(Ability ability, PropertySet properties) {
            Projector projector = properties.Get<Projector>("Projector");
            if (projector != null) Destroy(projector);
            properties.Delete<Projector>("Projector");
        }

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            Vector3 targetPoint = properties.Get<Vector3>("TargetPoint");
            GameObject gameObject = Instantiate(spell, targetPoint, Quaternion.identity) as GameObject;
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(ability, properties);
            }
        }
    }

}