using UnityEngine;
using Intelligence;
using EntitySystem;
using System.Collections.Generic;

namespace EntitySystem {
    public class GameActor : Entity {
        [SerializeField] private CharacterCreator characterCreator;

        private PlayerCharacterAction skillAction;

        public override void Init() {
            character = characterCreator.Create();
            var equipTable = new InventoryItemCreator[] {
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

            for (int i = 0; i < character.abilities.Count; i++) {
                SkillBook.Add(character.abilities[i].Create());
                SkillBook[i].Caster = this;
            }

            for (int i = 0; i < equipTable.Length; i++) {
                if (equipTable[i] != null) {
                    var item = equipTable[i].Create();
                    item.isEquipable = true;
                    itemManager.EquipItem(item, (EquipmentSlot)i);
                }
            }

            ActiveEquipment[(int)EquipmentSlot.Weapon].Use();
        }
    }
}
