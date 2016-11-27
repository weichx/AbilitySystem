using UnityEngine;
using System.Collections.Generic;
using Intelligence;

[System.Serializable]
public class CharacterEquipment
{
    public List<InventoryItemCreator> equipedItem;

    public readonly Dictionary<int, EquipmentSlot> slot = new Dictionary<int, EquipmentSlot> {
        { 0, EquipmentSlot.Head },
        { 1, EquipmentSlot.Shoulder },
        { 2, EquipmentSlot.Feet },
        { 3, EquipmentSlot.Body },
        { 4, EquipmentSlot.Legs },
        { 5, EquipmentSlot.Neck },
        { 6, EquipmentSlot.Gloves },
        { 7, EquipmentSlot.Waist},
        { 8, EquipmentSlot.Ring },
        { 9, EquipmentSlot.Ring },
        { 10, EquipmentSlot.Weapon }
    };

    public Dictionary<int, InventoryItem> equiped = new Dictionary<int, InventoryItem>();
}

public class CharacterParameters
{
    public BaseParameters baseParameters;

    public IntRange armor;
    public IntRange attackBonus;
    public IntRange critChance;
    public IntRange critMultipiler;
    public IntRange counterAttack;

    public IntRange physicalSave;
    public IntRange magicalSave;
    public IntRange athleticSave;
}

[System.Serializable]
public class BaseParameters
{
    public int level;

    public IntRange strength;
    public IntRange agility;
    public IntRange constitution;
    public IntRange intelligence;
    public IntRange wisdom;
    public IntRange luck;
}

public class Character : Entity {
    void Start() {

        Entity entity = GetComponent<Entity>();
        
        if (equipment.equipedItem.Count > 0) {
            for (int i = 0; i < equipment.slot.Count; i++) {
                if (equipment.equipedItem[i] == null) continue;
                var item = equipment.equipedItem[i].Create();
                item.Owner = entity;
                SetEquipment(item, i);
            }
        }
    }

    public void ObtainItem() {

    }

    public void SetEquipment(InventoryItem item, int id) {

        if(!item.isEquipable) return;
        if(item.equipSlot != equipment.slot[id]) return;

        item.itemState = InventoryItemState.InSlot;
        item.Equip();

        if(equipment.equiped[id] != null) {
            equipment.equiped[id].Unequip();
        }

        equipment.equiped[id] = item;
    }
}
