using UnityEngine;
using System.Collections.Generic;
using Intelligence;
using System;


namespace EntitySystem {
    public class CharacterEquipment
    {
        public InventoryItemCreator head;
        public InventoryItemCreator shoulder;
        public InventoryItemCreator feet;
        public InventoryItemCreator body;
        public InventoryItemCreator legs;
        public InventoryItemCreator neck;
        public InventoryItemCreator gloves;
        public InventoryItemCreator waist;
        public InventoryItemCreator ring1;
        public InventoryItemCreator ring2;
        public InventoryItemCreator weapon;

        public InventoryItem this[EquipmentSlot slot] {
            get {
                if(!equiped.ContainsKey((int)slot)) return null;
                return equiped[(int)slot];
            }
            set { equiped[(int)slot] = value; }
        }

        public int EquipSlotCnt { get { return slots.Length; } }

        [NonSerializedAttribute] public Dictionary<int, InventoryItem> equiped = new Dictionary<int, InventoryItem>();
        [NonSerializedAttribute] public EquipmentSlot[] slots = new EquipmentSlot[] {
            EquipmentSlot.Head,
            EquipmentSlot.Shoulder,
            EquipmentSlot.Feet,
            EquipmentSlot.Body,
            EquipmentSlot.Legs,
            EquipmentSlot.Neck,
            EquipmentSlot.Gloves,
            EquipmentSlot.Waist,
            EquipmentSlot.Ring,
            EquipmentSlot.Ring,
            EquipmentSlot.Weapon
        };
    }

    public class CharacterParameters
    {
        public BaseParameters baseParameters;

        [NonSerialized] public IntRange armor;
        [NonSerialized] public IntRange attackBonus;
        [NonSerialized] public IntRange critChance;
        [NonSerialized] public IntRange critMultipiler;
        [NonSerialized] public IntRange counterAttack;

        [NonSerialized] public IntRange physicalSave;
        [NonSerialized] public IntRange magicalSave;
        [NonSerialized] public IntRange athleticSave;
}

    [System.Serializable]
    public class BaseParameters
    {
        [SerializeField]
        private int level;
        [SerializeField]
        public GameClass gameClass;


        public IntRange strength;
        public IntRange agility;
        public IntRange constitution;
        public IntRange intelligence;
        public IntRange wisdom;
        public IntRange luck;
    }

    public partial class Character : EntitySystemBase {
        public Sprite icon;
        public bool isPlayer;

        [SerializeField] public CharacterParameters parameters;
        [SerializeField] public CharacterEquipment equipment;

        [SerializeField] public List <InventoryItemCreator> items;
        [SerializeField] public List <AbilityCreator> abilities;

        public List<CharacterRequirement> requirements;
        public List<CharacterComponent> components;

        [NonSerialized] private Context context;
        public Type contextType;

        public Character() : this ("") {
            this.contextType = contextType ?? typeof(Context);
        }

        public Character(string id) {
            Id = id;
            components = new List<CharacterComponent>();
            requirements = new List<CharacterRequirement>();
        }

        public void SetEquiped (InventoryItem item, int id) {
            if (!item.isEquipable) return;
            if (item.GetInventoryItemComponent<Equipable>().equipSlot != equipment.slots[id]) return;
            if (item.itemState == InventoryItemState.InSlot) return;

            item.Equip();
        }

        public void Attack() {
            var weapon = equipment.equiped[(int)EquipmentSlot.Weapon];
            if(weapon != null) {
                weapon.Use();
            }
            else {
                Debug.Log("No weapon equiped, attacking with bare hands not supported yet");
            }
        }
    }
}
