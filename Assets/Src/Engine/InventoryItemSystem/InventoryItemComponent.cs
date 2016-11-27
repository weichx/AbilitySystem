using System;
using Intelligence;

public abstract class InventoryItemComponent {
    [NonSerialized] public InventoryItem item;
    protected Context ctx;


    public virtual void SetContext(Context context) {
        this.ctx = context;
    }
    public virtual Type GetContextType() {
        return typeof(Context);
    }

    public virtual void OnUse() {}
    public virtual void OnEquip() {}
    public virtual void OnRemove() {}
    public virtual void OnDestroy() {}
    public virtual void OnBought() {}
    public virtual void OnSold() {}
    public virtual void OnGained() {}
    public virtual void OnLost() {}
    public virtual void OnSoulbound() {}
    public virtual void OnDrop() {}
}

public abstract class InventoryItemComponent<T> : InventoryItemComponent where T : Context {

    protected new T context;

    public virtual void SetContext(Context context) {
        this.ctx = context as T;
    }
    public virtual Type GetContextType() {
        return typeof(T);
    }

}