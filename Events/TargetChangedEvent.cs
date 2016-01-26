
public class TargetChangedEvent : GameEvent {

    public readonly Entity entity;
    public readonly Entity newTarget;
    public readonly Entity oldTarget;

    public TargetChangedEvent(Entity entity, Entity newTarget, Entity oldTarget) {
        this.entity = entity;
        this.newTarget = newTarget;
        this.oldTarget = oldTarget;
    }
}