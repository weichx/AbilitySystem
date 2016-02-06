using UnityEngine;
using AbilitySystem;

[RequireAbilityAttr("Projectile Speed")]
[RequireAbilityAttr("Collision Range")]
public class HomingProjectile : AbilityInitializer {

    public float speed;
    public float baseCollisionRange;
    public Transform target;
    
    public override void Initialize(Ability ability, PropertySet properties) {
        target = properties.Get<Transform>("Target");
        if(target == null) {
            var targetEntity = properties.Get<Entity>("Target");
            if(targetEntity != null) {
                target = targetEntity.transform;
            }
        }
        speed = ability.GetAttributeValue("Projectile Speed");
        baseCollisionRange = ability.GetAttributeValue("Collision Range");
    }

    public void Update() {
        if (target == null) return;
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        transform.LookAt(target);
        transform.position += movement;
        float distSqr = transform.DistanceToSquared(target);
        if(distSqr <= baseCollisionRange * baseCollisionRange) {
            Debug.Log("HIT");
        }
    }
}
