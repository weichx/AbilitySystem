using UnityEngine;

namespace EntitySystem {
    public class GameActor : Entity {
        [SerializeField] private CharacterCreator characterCreator;
        private Entity entity;
        private Character character;

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

            for (int i = 0; i < character.equipment.slots.Count; i++) {
                if (equipTable[i] == null) continue;
                var item = equipTable[i].Create();
                item.Owner = entity;
                item.isEquipable = true; // For debugging
                SetEquiped(item, i);
            }

            character.parameters.baseParameters.strength.SetModifier("Protein Powder", FloatModifier.Percent(50.2f));
        }

        public void SetEquiped(InventoryItem item, int id) {
            if(!item.isEquipable) return;
            if(item.equipSlot != character.equipment.slots[id]) return;

            item.Equip();
            item.itemState = InventoryItemState.InSlot;

            if(character.equipment.equiped.ContainsKey(id)) {
                character.equipment.equiped[id].Unequip();
            }
            character.equipment.equiped[id] = item;
            Debug.Log(character.equipment.equiped[id].Id);
        }

        public void Update () {

        }
    }
}
