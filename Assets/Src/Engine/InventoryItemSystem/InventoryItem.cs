using System;
using UnityEngine;
using System.Collections.Generic;
using Intelligence;

namespace EntitySystem {
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
        public List<ItemType> isType;

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
            Id = id;
            requirements = new List<InventoryItemRequirement>();
            components = new List<InventoryItemComponent>();
            isType = new List<ItemType>();
        }

        public InventoryItemComponent AddInventoryItemComponent<T>() where T : InventoryItemComponent, new() {
            InventoryItemComponent component = new T();
            component.item = this;
            components.Add(component);

            return component;
        }

        public T GetInventoryItemComponent<T>() where T : InventoryItemComponent {
            Type type = typeof(T);
            for(int i = 0; i < components.Count; i++) {
                if (type == components[i].GetType()) return components[i] as T;
            }
            return null;
        }


        public void RemoveInventoryItemComponent(InventoryItemComponent component) {
            components.Remove(component);
        }

        public void Equip() {
            for(int i = 0 ; i < components.Count; i++) {
                components[i].item = this;
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
}
