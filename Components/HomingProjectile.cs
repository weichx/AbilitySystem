using UnityEngine;
using AbilitySystem;

public class HomingProjectile : MonoBehaviour, IAbilityInitializer {

    public float speed;
    public float collisionRange;
    public Entity targetEntity;

    public virtual void Initialize(Ability ability) {
        targetEntity = ability.caster.Target;//todo make this better
        speed = ability.GetAttributeValue("Projectile Speed");
        AbilityAttribute collisionRangeAttr = ability.GetAttribute("Collision Range");
        if(collisionRangeAttr != null) {
            collisionRange = collisionRangeAttr.CachedValue;
        }
    }

    public void Update() {
        if (targetEntity == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        transform.LookAt(targetEntity.transform);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(targetEntity.transform);
        if(distSqr <= collisionRange * collisionRange) {
            targetEntity = null;
            var evtManager = GetComponent<EventManager>();
            if (evtManager != null) {
                evtManager.QueueEvent(new AbilityHitEvent());
            }
        }
    }
}
