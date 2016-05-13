using System;
using UnityEngine;

public abstract class StatusEffectComponent {

    [NonSerialized] public StatusEffect statusEffect;
    [NonSerialized] public Context context;
    [NonSerialized] public Entity caster;
    [NonSerialized] public Entity target;

    public virtual void OnEffectApplied() {

    }

    public virtual void OnEffectUpdated() {

    }

    public virtual void OnEffectStackAdded() {

    }

    public virtual void OnEffectTick() {

    }

    public virtual void OnEffectRefreshed(Context context) {

    }

    public virtual void OnEffectRemoved() {

    }

    public virtual void OnEffectExpired() {

    }

    public virtual bool OnDispelAttempted() {
        return true;
    }

    public virtual void OnEffectDispelled() {

    }

}