using System;
using System.Collections.Generic;
using AbilitySystem;
using Intelligence;
namespace EntitySystem {
    public class InventoryItemManager {

        protected List<InventoryItem> items;
        protected Dictionary<int, InventoryItem> equiped;

        protected Entity entity;

        public InventoryItemManager(Entity entity) {
            this.entity = entity;
            items = new List<InventoryItem>();
            equiped = new Dictionary<int, InventoryItem>();
        }

        public void AddItem(string itemId) {

        }

        public void EquipItem(InventoryItem item, EquipmentSlot slot) {
            if (!item.isEquipable) return;
            if (item.GetInventoryItemComponent<Equipable>().equipSlot != slot) return;
            if (item.itemState == InventoryItemState.InSlot) return;
            item.Owner = entity;
            item.Equip();

            if (equiped.ContainsKey((int)slot)) {
                equiped[(int)slot].Unequip();
            }

            equiped[(int)slot] = item;
        }

        public void UnequipItem(EquipmentSlot slot) {}

        public List<InventoryItem> Items {
            get { return items; }
        }

        public Dictionary<int, InventoryItem> Equipment {
            get { return equiped; }
        }

        public void Update() {

        }
    }
}
