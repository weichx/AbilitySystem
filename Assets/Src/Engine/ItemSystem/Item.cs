using UnityEngine;
using System.Collections.Generic;
using Intelligence;

public partial class Item : EntitySystemBase {

    public Sprite icon;

    public bool isUseable;
    public bool isDestructable;
    public bool isUnique;
    public bool isEquipable;
    public bool isSoulbound;

    public IntRange charges;
    public FloatRange cooldown;

    public List<ItemRequirement> requirements;
    public List<ItemComponent> components;


//    public EquipmentSlot equipSlot;

    public void OnUse() {}
    public void OnEquip() {}
    public void OnRemove() {}
    public void OnDestroy() {}
    public void OnBought() {}
    public void OnSold() {}
    public void OnGained() {}
    public void OnLost() {}
    public void OnSoulbound() {}

    public Item() {

    }

    public Item(string id) {
        requirements = new List<ItemRequirement>();
        components = new List<ItemComponent>();
    }

}