using UnityEditor;
using UnityEngine;
using Intelligence;
using EntitySystem;

public class Equipable : InventoryItemComponent {

    public EquipmentSlot equipSlot;
    public BaseParameters requiredParameters;
    public BaseParameters modifies;
    public BaseParameters setParamsTo;

    public override void OnEquip() {
        item.equipSlot = equipSlot;

        if(item.Owner.equipment[equipSlot] != null) {
            item.Owner.equipment[equipSlot].Unequip();
        }

        item.Owner.equipment[equipSlot] = item;
    }

    public override void OnRemove() {
    }
}