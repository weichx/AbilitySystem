using UnityEngine;
using AbilitySystem;

[AddComponentMenu("Ability System/Targeting/Point AOE")]
public class PointAoeTargetingStrategy : TargetingStrategy {

    public Projector targetSelectorPrefab;
    protected Projector targetSelector;
    [HideInInspector]
    public Vector3 targetPoint;

    public override void OnTargetSelectionStarted() {
        targetSelector = Instantiate(targetSelectorPrefab) as Projector;
        OnTargetSelectionUpdated();
    }

    public override bool OnTargetSelectionUpdated() {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
            float distSqrd = hit.point.DistanceToSquared(caster.transform.position);
            float range = 999999;
            AbilityAttribute rangeAttr = ability.GetAttribute("Range");
            if (rangeAttr != null) {
                range = rangeAttr.CachedValue;
            }
            if (range * range < distSqrd) {
                return false;
            }
            targetSelector.transform.position = hit.point + Vector3.up * 3f;

            if (Input.GetMouseButtonDown(0)) {
                targetPoint = hit.point;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            ability.CancelCast();
        }
        return false;

    }

    public override void OnTargetSelectionCompleted() {
        if (targetSelector != null) Destroy(targetSelector.gameObject);
    }

    public override void OnTargetSelectionCancelled() {
        if (targetSelector != null) Destroy(targetSelector.gameObject);
    }

}