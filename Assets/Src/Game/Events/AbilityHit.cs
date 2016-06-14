using Intelligence;

public class AbilityHit : GameEvent {

    public readonly Context context;
    public readonly Ability ability;
    public readonly Entity caster;
    public readonly Entity target;

    public AbilityHit(Entity target, Context context) {
        this.target = target;
        this.context = context;
    }

}