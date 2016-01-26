using System;
using UnityEngine;
using System.Collections.Generic;

using AttributeSnapshot = System.Collections.Generic.Dictionary<string, float>;

public class HomingProjectile : MonoBehaviour {

    public float speed;
    public float collisionDistance;

    public Entity target;
    public Entity caster;
    public AttributeSnapshot snapshot;
    public Action OnTargetHit { get; set; }
    public List<IStatusPrototype> statusToApply;

    public virtual void Initialize(Entity caster, Entity target, AttributeSnapshot snapshot) {
        this.caster = caster;
        this.target = target;
        this.snapshot = snapshot;
        float defaultSpeed = speed;
        if(!snapshot.TryGetValue("ProjectileSpeed", out speed)) {
            speed = defaultSpeed;
        }
    }

    public virtual void OnDisable() {
        caster = null;
        target = null;
        snapshot = null;
    }

	public void Update () {
        if (target == null) return;
        transform.LookAt(target.CollisionPoint, Vector3.up);
        transform.position += transform.forward * speed * Time.deltaTime;
        if(transform.DistanceToSquared(target.CollisionPoint) <= collisionDistance * collisionDistance) {
            if (OnTargetHit != null) {
                OnTargetHit.Invoke();
                //target.ApplyDamage(snapshot.GetValue<Damage>());
                //target.AdjustResource<Health>(-3f);
                //target.ApplyStatus(new Status());
                //abilitySnapshot = {attr: value, attr: value, caster: entity, 
                //target.ApplyStatus(Status.Frozen, entity snapshot? should status be parameterized?);
                OnTargetHit = null;
            }
            enabled = false;
        }
    }

}
