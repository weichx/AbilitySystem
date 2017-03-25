using UnityEngine;

namespace EntitySystem {
    public class GameActor : Entity {
        [SerializeField] private CharacterCreator characterCreator;
        private Character character;
        private InventoryItemCreator[] equipTable;

        public override void Init() {
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

            for (int i = 0; i < equipTable.Length; i++) {
                if (equipTable[i] != null) {
                    var item = equipTable[i].Create();
                    item.isEquipable = true;
                    itemManager.EquipItem(item, (EquipmentSlot)i);
                }
            }

            character.parameters.baseParameters.strength.SetModifier("Protein Powder", FloatModifier.Percent(50.2f));
        }
    }
}
