using UnityEngine;
using AbilitySystem;

public class HomingProjectile : MonoBehaviour {

    public float speed;
    public float collisionRange;

    [Writable(false)] public Entity target;

    public void Initialize(Entity target) {
        this.target = target;
    }

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        transform.LookAt(target.transform);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(target.transform);
        if(distSqr <= collisionRange * collisionRange) {
            //snapshot.GetAttribute("Damage");
            //damage.GetDescriptor();
            //target.ApplyDamage(descriptor);
            target = null;
            var evtManager = GetComponent<EventManager>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitEvent());
            }
        }
    }
}
