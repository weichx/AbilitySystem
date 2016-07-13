using System;
using Intelligence;

public abstract class InventoryItemComponent {
    [NonSerialized] public InventoryItem item;

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