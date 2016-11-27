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

    public int charges;

    public List<InventoryItemRequirement> requirements;
    public List<InventoryItemComponent> components;

    public EquipmentSlot equipSlot;
    public InventoryItemState itemState;

    protected Entity owner;
    private Context Context;

    public void OnUse() {}
    public void OnEquip() {}
    public void OnRemove() {}
    public void OnDestroy() {}
    public void OnBought() {}
    public void OnSold() {}
    public void OnLost() {}
    public void OnSoulbound() {}

    public Entity Owner {
        get { return owner; }
        set { owner = value; }
    }

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

    public void Equip() {
        if (itemState == InventoryItemState.InSlot) return;

        for(int i = 0 ; i < components.Count; i++) {
            components[i].OnEquip();
        }
    }

    public void Unequip() {
        for(int i = 0 ; i < components.Count; i++) {
            components[i].OnRemove();
        }
    }

    public void Use() {
        for(int i = 0 ; i < components.Count; i ++ ) {
            components[i].OnUse();
        }
    }
}