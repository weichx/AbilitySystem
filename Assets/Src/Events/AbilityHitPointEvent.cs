using UnityEngine;

public class AbilityHitPointEvent : GameEvent {

    public readonly OldContext context;
    public readonly Ability ability;
    public readonly Entity caster;
    public readonly Vector3 point;

    public AbilityHitPointEvent(Vector3 point, OldContext context) {
        this.point = point;
        this.context = context;
        ability = context.ability;
        caster = ability.caster;
    }

}