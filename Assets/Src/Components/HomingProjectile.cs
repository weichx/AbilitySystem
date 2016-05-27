using UnityEngine;
using AbilitySystem;

public class HomingProjectile : MonoBehaviour, IAbilityContextAware {

    public float speed;
    public float collisionRange;

    [Writable(false)] public Entity target;
    protected OldContext context;

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
		Vector3 targetPoint = Vector3.zero;//target.CastTarget;
        transform.LookAt(targetPoint);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(targetPoint);
        if(distSqr <= collisionRange * collisionRange) {
            target = null;
            var evtManager = GetComponent<EventManager>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitEntityEvent(target, context));
            }
        }
    }

    public void SetAbilityContext(OldContext context) {
        this.context = context;
        target = context.Get<Entity>("target");
		transform.position = Vector3.zero;//context.entity.CastPoint;
    }
}
