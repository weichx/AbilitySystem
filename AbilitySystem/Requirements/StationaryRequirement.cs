public class StationaryRequirement : AbilityRequirement {

    public StationaryRequirement() {
        FailureMessage = "You cannot cast while moving."; 
    }

    public override bool CanStartCast(Entity caster, AbstractAbility currentAbility) {
        return !caster.IsMoving;
    }

    public override bool CanContinueCast(Entity caster, AbstractAbility ability) {
        return !caster.IsMoving;
    }

}