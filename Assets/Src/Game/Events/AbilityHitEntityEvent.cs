using Intelligence;

public class AbilityHitEntityEvent : GameEvent {

    public readonly Context context;
    public readonly Ability ability;
    public readonly Entity caster;
    public readonly Entity target;

    public AbilityHitEntityEvent(Entity target, Context context) {
        this.target = target;
        this.context = context;
    }

}