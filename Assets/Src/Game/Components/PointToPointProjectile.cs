using UnityEngine;
using Intelligence;

public class PointToPointProjectile : MonoBehaviour, IContextAware {

    public Vector3 targetPoint;
    public Vector3 originPoint;
    public float speed = 5;
    public float accelerationRate = 1f;
    public float maxSpeed = float.MaxValue;
    public float collisionRange = 0.25f;
    public float spawnHeight;
    public float originXZOffsetRadius;

    private PointContext context;

    public void SetContext(Context context) {
        this.context = context as PointContext;
        targetPoint = this.context.point;//.position;
        Vector2 offset = Random.insideUnitCircle * originXZOffsetRadius;
        originPoint = targetPoint + (Vector3.up * spawnHeight);
        originPoint.x += offset.x;
        originPoint.z += offset.y;
        transform.position = originPoint;
    }

    void Update() {
        speed += (accelerationRate * Time.deltaTime);
        speed = Mathf.Clamp(speed, 0, maxSpeed);
        if (transform.position.DistanceToSquared(targetPoint) < collisionRange * collisionRange) {
            var evtManager = GetComponent<EventEmitter>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitPointEvent(targetPoint, context));
                enabled = false;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
    }
    
}
