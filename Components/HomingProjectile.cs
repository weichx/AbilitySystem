using UnityEngine;
using AbilitySystem;

[RequireAbilityAttr("Projectile Speed")]
[RequireAbilityAttr("Collision Range")]
public class HomingProjectile : MonoBehaviour, IAbilityInitializer {

    public float speed;
    public float collisionRange;
    public Transform target;
    
    public virtual void Initialize(Ability ability, PropertySet properties) {
        target = properties.Get<Transform>("Target");
        if(target == null) {
            var targetEntity = properties.Get<Entity>("Target");
            if(targetEntity != null) {
                target = targetEntity.transform;
            }
        }
        speed = ability.GetAttributeValue("Projectile Speed");
        AbilityAttribute collisionRangeAttr = ability.GetAttribute("Collision Range");
        if(collisionRangeAttr != null) {
            collisionRange = collisionRangeAttr.CachedValue;
        }
    }

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        transform.LookAt(target);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(target);
        if(distSqr <= collisionRange * collisionRange) {
            target = null;
            var evtManager = GetComponent<EventManager>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitEvent());
            }
        }
    }
}
