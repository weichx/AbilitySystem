using System;
using UnityEngine;

[Serializable]
public class AbilityComponent {

    [NonSerialized] public Entity caster;
    [NonSerialized] public Ability ability;
    [NonSerialized] public OldContext context;

    public virtual void OnUse() { }
    public virtual void OnChargeConsumed() { }

    public virtual void OnCastStarted() { }
    public virtual void OnCastUpdated() { }
    public virtual void OnCastInterrupted() { }
    public virtual void OnCastCompleted() { }
    public virtual void OnCastCancelled() { }
    public virtual void OnCastFailed() { }
    public virtual void OnCastEnded() { }

    public virtual void OnChannelStart() { }
    public virtual void OnChannelUpdated() { }
    public virtual void OnChannelTick() { }
    public virtual void OnChannelInterrupted() { }
    public virtual void OnChannelCancelled() { }
    public virtual void OnChannelEnd() { }
}