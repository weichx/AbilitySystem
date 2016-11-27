using UnityEngine;
using EntitySystem;

public class TargetManager : MonoBehaviour {

    public Entity currentTarget;
    protected Entity entity;

    public virtual void Start() {
        entity = GetComponent<Entity>();
    }

    public virtual void SetTarget(Entity newTarget) {
        Entity oldTarget = currentTarget;
        currentTarget = newTarget;

        if (newTarget != oldTarget) {
            //entity.eventManager.QueueEvent(new TargetChangedEvent(entity, currentTarget, oldTarget));
        }
    }
}