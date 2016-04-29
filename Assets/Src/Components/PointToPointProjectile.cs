using UnityEngine;
using AbilitySystem;
using System;

public class PointToPointProjectile : MonoBehaviour, IAbilityContextAware {

    public Vector3 targetPoint;
    public Vector3 originPoint;
    public float speed = 5;
    public float accelerationRate = 1f;
    public float maxSpeed = float.MaxValue;
    public float collisionRange = 0.25f;
    public float spawnHeight;
    public float originXZOffsetRadius;

    public Context context;

    void Update() {
        speed += (accelerationRate * Time.deltaTime);
        speed = Mathf.Clamp(speed, 0, maxSpeed);
        if (transform.position.DistanceToSquared(targetPoint) < collisionRange * collisionRange) {
            var evtManager = GetComponent<EventManager>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitPointEvent(targetPoint, context));
                enabled = false;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
    }

    public void SetAbilityContext(Context context) {
        this.context = context;
        targetPoint = transform.position;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * originXZOffsetRadius;
        originPoint = targetPoint + (Vector3.up * spawnHeight);
        originPoint.x += offset.x;
        originPoint.z += offset.y;
        transform.position = originPoint;
    }
}
