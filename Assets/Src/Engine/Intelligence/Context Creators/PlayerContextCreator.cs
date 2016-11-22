using System;
using EntitySystem;
using Intelligence;

public enum ContextCreationStatus {
    Building, Completed, Cancelled
}

public abstract class PlayerContextCreator {

    [NonSerialized] protected Entity entity;

    public virtual void Initialize(Entity entity) {}

    public virtual ContextCreationStatus UpdateContext() {
        return ContextCreationStatus.Completed;
    }

    public abstract Context GetContext();

    public virtual void Reset() { }

    public virtual void Cancel() { }

}
