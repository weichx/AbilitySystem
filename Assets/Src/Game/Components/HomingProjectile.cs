using UnityEngine;
using Intelligence;

public class HomingProjectile : MonoBehaviour, IContextAware {

    public float speed = 10;
    public float collisionRange = 1;

    [Writable(false)]
    public Entity target;
    protected SingleTargetContext context;

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        Vector3 targetPoint = target.transform.position + Vector3.up; //swag it for now
        transform.LookAt(targetPoint);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(targetPoint);
        if (distSqr <= collisionRange * collisionRange) {
            target = null;
            var evtManager = GetComponent<EventEmitter>();
            if (evtManager != null) {
                evtManager.TriggerEvent(new AbilityHit(target, context));
            }
            else {
                Destroy(gameObject);
            }
        }
    }

    public void SetContext(Context context) {
        this.context = context as SingleTargetContext;
        target = this.context.target;
        transform.position = context.entity.transform.position + Vector3.up; //swag it for now
        transform.rotation = Quaternion.LookRotation(target.transform.position.DirectionTo(transform.position));
    }
}
