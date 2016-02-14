using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbilitySystem {

    public class StatusAction {

        public virtual void Initialize() { }

        public virtual void OnEffectApplied(StatusEffect status) {

        }

        public virtual void OnEffectUpdated(StatusEffect status) {

        }

        public virtual void OnEffectStackAdded(StatusEffect status) {

        }

        public virtual void OnEffectRefreshed(StatusEffect status) {

        }

        public virtual void OnEffectRemoved(StatusEffect status) {

        }

        public virtual void OnEffectExpired(StatusEffect status) {

        }

        public virtual bool OnDispelAttempted(StatusEffect status) {
            return true;
        }

        public virtual void OnEffectDispelled(StatusEffect status) {

        }

    }

    //public class SpawnThing : StatusAction {

    //    public GameObject thing;

    //    public override void OnEffectApplied(StatusEffect status) {
    //        UnityEngine.Object.Instantiate(thing);
    //        // status.GetActions<SpawnThing>(this);
    //    }
    //}

    //public class DamageOverTime : StatusAction {
    //    public string element;
    //    public float amount;
    //    public float interval;

    //    public override void OnEffectApplied(StatusEffect status) {
    //        status.properties.Set("Timer", new Timer(status.duration.CachedValue));
    //    }

    //    public override void OnEffectUpdated(StatusEffect status) {
    //        Timer timer = status.properties.Get<Timer>("Timer");
    //        if (timer.ReadyWithReset()) {
    //            //drain mana
    //        }
    //    }
    //}

    //public class DispelProtection : StatusAction {
    //    float amount;

    //    public override bool OnDispelAttempted(StatusEffect status) {
    //        return UnityEngine.Random.Range(0, 1) > amount;
    //    }
    //}

    //public class ExplodeOnDispel : StatusAction {
    //    GameObject prefab;

    //    public override void OnEffectDispelled(StatusEffect status) {
    //        UnityEngine.Object.Instantiate(prefab);
    //    }

    //}

    public class StatusEffectPrototype : ScriptableObject {

        public List<StatusAction> actions;
        public bool IsExpirable;
        public ModifiableAttribute<StatusEffect> duration;

        public StatusEffect CreateStatus(Ability ability, Entity target) {
            return new StatusEffect(name, ability, target, this);
        }

    }

    public class StatusEffect {

        public string name;
        protected List<StatusAction> actions;
        public readonly ModifiableAttribute<StatusEffect> duration;
        public bool IsExpirable;
        public bool IsDispellable;
        public bool IsRefreshable;
        public bool IsUnique;

        public Ability ability;
        public Entity caster;
        public Entity target;
        public StatusEffectPrototype prototype;
        public PropertySet properties;
        public StatusState state;
        public Timer timer;

        public StatusEffect(string name, Ability ability, Entity target, StatusEffectPrototype prototype) {
            this.name = name;
            this.ability = ability;
            this.target = target;
            this.prototype = prototype;
            duration = new ModifiableAttribute<StatusEffect>("Duration", prototype.duration);
            actions = new List<StatusAction>(prototype.actions);
            properties = new PropertySet();
            IsExpirable = prototype.IsExpirable;
            caster = ability.caster;
            timer = new Timer(duration.CachedValue);
        }

        public bool ReadyForRemoval {
            get { return state != StatusState.Active; }
        }

        public void Apply() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnEffectApplied(this);
            }
        }

        public void Update() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnEffectUpdated(this);
            }
            if (state == StatusState.Active && IsExpirable && timer.Ready) {
                Expire();
                state = StatusState.Expired;
            }
        }

        public void Dispel(/*source?*/) {
            if (state != StatusState.Active) return;
            bool isDispelled = true;
            for (int i = 0; i < actions.Count; i++) {
                bool actionResult = actions[i].OnDispelAttempted(this);
                if (!actionResult && isDispelled) {
                    isDispelled = actionResult;
                }
            }
            if (IsDispellable && isDispelled) {
                state = StatusState.Dispelled;
                for (int i = 0; i < actions.Count; i++) {
                    actions[i].OnEffectDispelled(this);
                }
            }
        }

        public void Expire() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnEffectExpired(this);
            }
        }

        public void Refresh() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnEffectRefreshed(this);
            }
        }

        public void Remove() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnEffectRemoved(this);
            }
        }


    }

}