using System;
using UnityEngine;
using System.Collections.Generic;

namespace AbilitySystem {

    [Serializable]
    public struct SpellSlotAssignment {
        public string spellId;
        public string slotId;
        public string actionBarId;
    }

    [RequireComponent(typeof(Entity))] //there might be a way to not require entity
    public class AbilityManager : MonoBehaviour {
        public float baseGlobalCooldown = 1.5f;

        public List<string> abilityNames;
        public List<SpellSlotAssignment> slotAssignments;

        [HideInInspector] public List<Ability> abilities;
        [HideInInspector] public Entity entity;

        protected Timer gcdTimer;
        protected CastQueue castQueue;

        void Start() {
            castQueue = new CastQueue();
            gcdTimer = new Timer(baseGlobalCooldown);
            entity = GetComponent<Entity>();
            LoadAbilities();
        }

        public void Cast(string abilityId) {
            Ability ability = GetAbility(abilityId);
            if (ability == null) throw new AbilityNotFoundException(abilityId);
            if (ability.IsUsable()) {
                castQueue.Enqueue(ability);
            }
        }

        public void Cast(Ability ability) {
            if (ability == null || ability.caster != entity) return;
            if (ability.IsUsable()) {
                castQueue.Enqueue(ability);
            }
        }

        public void Update() {
            //todo maybe only do this ever ~10 frames
            for(int i = 0; i < abilities.Count; i++) {
                abilities[i].UpdateAttributes();
            }
            castQueue.UpdateCast();
        }

        public bool InterruptCast() { //todo add parameter for cast interrupter class 
            if (IsCasting) {
                castQueue.current.InterruptCast();
            }
            castQueue.Clear();
            return true;
        }

        public bool CancelCast() {
            Ability current = castQueue.current;
            if (current != null) {
                current.CancelCast();
                castQueue.Clear();
            }
            return current != null;
        }

        public bool IsCasting {
            get {
                Ability ability = castQueue.current;
                return ability != null && ability.IsCasting;
            }
        }

        public float ElapsedCastTime {
            get { return (IsCasting) ? castQueue.current.ElapsedCastTime : 0; }
        }

        public float TotalCastTime {
            get { return (IsCasting) ? castQueue.current.TotalCastTime : 0f; }
        }

        public float CastProgress {
            get {
                if (IsCasting) {
                    return ElapsedCastTime / TotalCastTime;
                }
                return 0f;
            }
        }

        public bool OnGlobalCooldown {
            get { return !castQueue.gcdTimer.Ready; }
        }

        public float RemainingGCDTime {
            get { return castQueue.gcdTimer.TimeToReady; }
        }

        public Ability ActiveAbility {
            get { return castQueue.current; }
        }

        //public void AddAbility(string abilityId) {
        //    if (abilities.Get(abilityId) != null) return;
        //    AbilityPrototype prototype = masterAbilityDatabase.Get(abilityId);
        //    if (prototype == null) throw new AbilityNotFoundException(abilityId);
        //    abilities.Add(abilityId, prototype.CreateAbility(entity));
        //}

        public bool HasAbility(string abilityName) {
            for (int i = 0; i < abilities.Count; i++) {
                if (abilities[i].name == abilityName) return true;
            }
            return false;
        }

        public Ability GetAbility(string abilityId) {
            if(abilities == null) {
                LoadAbilities();
            }
            for (int i = 0; i < abilities.Count; i++) {
                if (abilities[i].name == abilityId) return abilities[i];
            }
            return null;
        }

        private void LoadAbilities() {
            LoadResources();

            abilityNames = abilityNames ?? new List<string>();
            abilities = abilities ?? new List<Ability>();
            entity = entity ?? GetComponent<Entity>();
            for (int i = 0; i < abilityNames.Count; i++) {
                if (string.IsNullOrEmpty(abilityNames[i])) continue;
                if (GetAbility(abilityNames[i]) != null) continue;
                AbilityPrototype proto = null;
                if (masterAbilityDatabase.TryGetValue(abilityNames[i], out proto)) {
                    abilities.Add(proto.CreateAbility(entity));
                }
                else {
                    throw new AbilityNotFoundException(abilityNames[i]);
                }
            }
        }

        private static Dictionary<string, AbilityPrototype> masterAbilityDatabase;
        private static void LoadResources() {
            if (masterAbilityDatabase != null) return;
            masterAbilityDatabase = new Dictionary<string, AbilityPrototype>();
            AbilityPrototype[] prototypes = Resources.LoadAll<AbilityPrototype>("Abilities");
            for (int i = 0; i < prototypes.Length; i++) {
                masterAbilityDatabase[prototypes[i].name] = prototypes[i];
            }
        }
    }

    public class AbilityNotFoundException : Exception {

        public AbilityNotFoundException(string abilityId) : base(abilityId) { }

    }
}