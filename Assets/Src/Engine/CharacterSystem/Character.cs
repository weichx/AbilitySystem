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

        [HideInInspector] public Dictionary<int, InventoryItem> equiped = new Dictionary<int, InventoryItem>();
        [HideInInspector] public Dictionary<int, EquipmentSlot> slots = new Dictionary<int, EquipmentSlot> {
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

        [SerializeField]
        public CharacterParameters parameters;
        [SerializeField]
        public CharacterEquipment equipment;

        [SerializeField]
        public List <InventoryItemCreator> items;
        [SerializeField]
        public List <AbilityCreator> abilities;

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
    }
}
