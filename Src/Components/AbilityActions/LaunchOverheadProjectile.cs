using UnityEngine;
using AbilitySystem;

[RequireComponent(typeof(PointAoeTargetingStrategy))]
public class LaunchOverheadProjectile : AbilityAction {

    public PointToPointProjectile projectilePrefab;

    public override void OnCastCompleted() {
        Vector3 point = (ability.TargetingStrategy as PointAoeTargetingStrategy).targetPoint;
        var inst = Instantiate(projectilePrefab, point, Quaternion.identity) as PointToPointProjectile;
        inst.Initialize();
    }
}
