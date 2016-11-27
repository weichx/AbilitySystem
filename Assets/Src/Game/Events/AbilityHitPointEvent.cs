using UnityEngine;
using EntitySystem;
using Intelligence;

public class AbilityHitPointEvent : GameEvent {

    public readonly PointContext context;
    public readonly Ability ability;
    public readonly Entity caster;
    public readonly Vector3 point;

    public AbilityHitPointEvent(Vector3 point, PointContext context) {
        this.point = point;
        this.context = context;
        //ability = context.ability;
        //caster = ability.caster;
    }

}