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

        public IntRange strength;
        public IntRange agility;
        public IntRange constitution;
        public IntRange intelligence;
        public IntRange wisdom;
        public IntRange luck;

        public int[] RollParameters(int min, int max) {
            System.Random rand = new System.Random();
            int[] values = new int[6];
            int total;
            for (int i = 0; i < 6; i++) {
                values[i] = rand.Next(min,max);
            }

            strength.NormalizedValue = values[0];
            agility.NormalizedValue = values[1];
            constitution.NormalizedValue = values[2];
            intelligence.NormalizedValue = values[3];
            wisdom.NormalizedValue = values[4];
            luck.NormalizedValue = values[5];

            return values;
        }
    }

    public partial class Character : EntitySystemBase {

        public Sprite icon;
        public bool isPlayer;

        [SerializeField]
        public CharacterParameters parameters;
        [SerializeField]
        public CharacterEquipment equipment;

        [SerializeField]
        public List <InventoryItem> items;

        [SerializeField]
        public List <Ability> abilites;

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
