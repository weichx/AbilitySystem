public abstract class AbilityRequirement {

    public string FailureMessage { get; set; }

    //bool IsSupressed
    //bool IsStartCastSupressed
    //bool IsContinueCastSupressed
    //bool IsCompleteCastSupressed

    //CanStartTargeting, CanContinueTargeting, CanCompleteTargeting

    public virtual bool CanStartCast(Ability ability, Entity caster) {
        return true;
    }

    public virtual bool CanContinueCast(Ability ability, Entity caster) {
        return true;
    }

    //if we lose a target or run out of resources etc this can fail the action
    public virtual bool CanCompleteCast(Ability ability, Entity caster) {
        return true;
    }
}