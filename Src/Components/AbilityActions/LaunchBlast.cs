using UnityEngine;
using AbilitySystem;


public class LaunchBlast : AbilityAction {

    public GameObject spellPrefab;

    public override void OnCastCompleted() {
        SingleTargetStrategy strategy = ability.TargetingStrategy as SingleTargetStrategy;
        var target = strategy.target;
        Vector3 toTarget = caster.transform.position.DirectionTo(target.transform.position);
        Vector3 blastPosition = target.transform.position - toTarget.normalized;
        Instantiate(spellPrefab, blastPosition, Quaternion.identity);
    }
}

