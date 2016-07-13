using UnityEngine;
using System.Collections.Generic;
using Intelligence;

public partial class InventoryItem : EntitySystemBase {

    public Sprite icon;

    public bool isUsable;
    public bool isDestructable;
    public bool isUnique;
    public bool isEquipable;
    public bool isSoulbound;
    public bool isStackable;
    public bool isSocketable;

    public IntRange charges;
    public FloatRange cooldown;
    public FloatRange value;

    public List<InventoryItemRequirement> requirements;
    public List<InventoryItemComponent> components;

    public EquipmentSlot equipSlot;

    protected Entity owner;

    private Context Context;

    public void OnUse() {}
    public void OnEquip() {}
    public void OnRemove() {}
    public void OnDestroy() {}
    public void OnBought() {}
    public void OnSold() {}
    public void OnGained() {}
    public void OnLost() {}
    public void OnSoulbound() {}

    public InventoryItem() {

    }

    public InventoryItem(string id) {
        requirements = new List<InventoryItemRequirement>();
        components = new List<InventoryItemComponent>();
    }

    public InventoryItemComponent AddInventoryItemComponent<T>() where T : InventoryItemComponent, new() {
        InventoryItemComponent component = new T();
        component.item = this;
        components.Add(component);

        return component;
    }

    public void RemoveInventoryItemComponent(InventoryItemComponent component) {
        components.Remove(component);
    }

    public bool Use() {
        if (isUsable && charges.Value > 0) return true;
        return false;
    }

    public bool Equip(EquipmentSlot slot) {
        if (equipSlot != slot)
            return false;

        OnEquip();
        return true;
    }

    public void PickUp() {}

    public void Drop() {}

}