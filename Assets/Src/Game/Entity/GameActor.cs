using System;
using System.Collections.Generic;
using UnityEngine;

namespace EntitySystem {
    public class EquipmentState {
        // [HideInInspector] public List<InventoryItem> equipedItem;
        [HideInInspector] public Dictionary<int, InventoryItem> equiped = new Dictionary<int, InventoryItem>();
        [HideInInspector] public Dictionary<int, EquipmentSlot> slot = new Dictionary<int, EquipmentSlot> {
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
    }

    //todo make custom inspector and change back to FloatRange
    public class GameActor : Entity {
        [SerializeField] private CharacterCreator characterCreator;
        private Entity entity;
        private Character character;

        public EquipmentState equipment;
        private InventoryItemCreator[] equipTable;

        public override void Init() {
            entity = GetComponent<Entity>();
            character = characterCreator.Create();
            equipTable = new InventoryItemCreator[] {
                character.equipment.head,
                character.equipment.shoulder,
                character.equipment.feet,
                character.equipment.body,
                character.equipment.legs,
                character.equipment.neck,
                character.equipment.gloves,
                character.equipment.waist,
                character.equipment.ring1,
                character.equipment.ring2,
                character.equipment.weapon,
            };
    
            for (int i = 0; i < equipment.slot.Count; i++) {
                if (equipTable[i] == null) continue;
                var item = equipTable[i].Create();
                item.Owner = entity;
                item.isEquipable = true; // For debugging
                SetEquiped(item, i);
            }

            Debug.Log(character.parameters.baseParameters.strength);
            character.parameters.baseParameters.strength.SetModifier("Protein Powder", FloatModifier.Percent(0.2f));
            Debug.Log(character.parameters.baseParameters.strength);
        }

        public void SetEquiped(InventoryItem item, int id) {
            if(!item.isEquipable) return;
            if(item.equipSlot != equipment.slot[id]) return;

            item.Equip();
            item.itemState = InventoryItemState.InSlot;

            if(equipment.equiped.ContainsKey(id)) {
                equipment.equiped[id].Unequip();
            }
            equipment.equiped[id] = item;
        }

        public void Update () {
            Debug.Log(equipment.equiped[(int)EquipmentSlot.Head].Id);
        }
    }
}
