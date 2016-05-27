
public class AbilityHitEntityEvent : GameEvent {

    public readonly OldContext context;
    public readonly Ability ability;
    public readonly Entity caster;
    public readonly Entity target;

    public AbilityHitEntityEvent(Entity target, OldContext context) {
        this.target = target;
        this.context = context;
        ability = context.ability;
        caster = ability.caster;
    }

}