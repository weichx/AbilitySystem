using UnityEngine;
using Intelligence;

public class HomingProjectile : MonoBehaviour {

    public float speed;
    public float collisionRange;

    [Writable(false)] public Entity target;
    protected SingleTargetContext context;

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
		Vector3 targetPoint = target.transform.position + Vector3.up; //swag it for now
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

    public void SetAbilityContext(SingleTargetContext context) {
        this.context = context;
        target = context.target;
		transform.position = context.entity.transform.position + Vector3.up; //swag it for now
    }
}
