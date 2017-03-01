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

            for (int i = 0; i < character.equipment.EquipSlotCnt; i++) {
                if (equipTable[i] != null) {
                    var item = equipTable[i].Create();
                    item.Owner = character;
                    item.isEquipable = true; // For debugging
                    character.SetEquiped(item, i);
                }
            }

            character.parameters.baseParameters.strength.SetModifier("Protein Powder", FloatModifier.Percent(50.2f));
            character.Attack();
        }

        public void Update () {
            //Debug.Log(character.equipment.equiped[(int)EquipmentSlot.Head].Id);
        }
    }
}
