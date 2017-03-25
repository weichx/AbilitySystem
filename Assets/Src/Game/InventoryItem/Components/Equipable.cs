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

    }

    public override void OnRemove() {
    }
}