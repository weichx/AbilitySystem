using System;
using Intelligence;

public abstract class AbilityComponent {

    [NonSerialized]
    public Ability ability;

    protected Context context;

    public virtual void SetContext(Context context) {
        this.context = context;
    }

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

    public virtual void OnInitialized(Ability ability) {
        this.ability = ability;
    }

    public virtual void OnRemoved() { }

    public virtual Type GetContextType() {
        return typeof(Context);
    }
}

public abstract class AbilityComponent<T> : AbilityComponent where T : Context {

    protected new T context;

    public override void SetContext(Context context) {
        this.context = context as T;
    }

    public override Type GetContextType() {
        return typeof(T);
    }
}