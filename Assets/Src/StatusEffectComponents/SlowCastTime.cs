
public class SlowCastTime : StatusEffectComponent {

    public float power;
    public const string SlowCastId = "Slow Cast";
    protected AbilityModifier modifier;

    public override void OnEffectApplied() {
        modifier = new CastSpeedModifier(SlowCastId, power);
        target.abilityManager.AddAbilityModifier(modifier);
    }

    public override void OnEffectRemoved() {
        target.abilityManager.RemoveAbilityModifier(modifier);
    }

}

