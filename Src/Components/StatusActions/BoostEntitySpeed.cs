using UnityEngine;
using AbilitySystem;

public class BoostEntitySpeed : StatusAction {

    public EntityAttribute speedBoost;
    private float oldSpeed;

    public override void OnEffectApplied() {
        var pcc = target.GetComponent<PlayerCharacterController>();
        oldSpeed = pcc.speed;
        pcc.speed = speedBoost.UpdateValue(caster);
    }

    public override void OnEffectUpdated() {
        var pcc = target.GetComponent<PlayerCharacterController>();
        pcc.speed = speedBoost.UpdateValue(caster);
    }

    public override void OnEffectRemoved() {
        var pcc = target.GetComponent<PlayerCharacterController>();
        pcc.speed = oldSpeed;
    }

    [Formula]
    public static float TestBoostSpeed(Entity entity, float baseValue) {
        return entity.abilityManager.ElapsedCastTime * 10f;
    }
}