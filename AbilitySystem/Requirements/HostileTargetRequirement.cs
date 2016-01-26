public class HostileTargetRequirement : AbilityRequirement {

    public HostileTargetRequirement() {
        FailureMessage = "Invalid Target";
    }

    public override bool CanStartCast(Entity caster, AbstractAbility ability) {
        return caster.GetComponent<TargetManager>().currentTarget != null;
    }

}