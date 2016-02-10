using UnityEngine;
using AbilitySystem;

public class PointToPointProjectile : MonoBehaviour, IAbilityInitializer {

    public Vector3 targetPoint;
    public Vector3 originPoint;
    public float speed = 5;
    public float accelerationRate = 1f;
    public float maxSpeed = float.MaxValue;
    public float collisionRange = 0.25f;
    public float spawnHeight;
    public float originXZOffsetRadius;

    public void Initialize(Ability ability) {
        targetPoint = (ability as PointAOEAbilityPrototype).targetPoint;
        Vector2 offset = Random.insideUnitCircle * originXZOffsetRadius;
        originPoint = targetPoint + (Vector3.up * spawnHeight);
        originPoint.x += offset.x;
        originPoint.z += offset.y;
        transform.position = originPoint;
    }
	
	void Update () {
        speed += (accelerationRate * Time.deltaTime);
        speed = Mathf.Clamp(speed, 0, maxSpeed);
        if(transform.position.DistanceToSquared(targetPoint) < collisionRange * collisionRange) {
            var evtManager = GetComponent<EventManager>();
            if(evtManager != null) {
                evtManager.QueueEvent(new AbilityHitEvent());
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
    }
}
