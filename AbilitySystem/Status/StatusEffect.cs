//using UnityEngine;
//using System;
//using System.Collections.Generic;

//namespace AbilitySystem {


//    //public class SpawnThing : StatusAction {

//    //    public GameObject thing;

//    //    public override void OnEffectApplied(StatusEffect status) {
//    //        UnityEngine.Object.Instantiate(thing);
//    //        // status.GetActions<SpawnThing>(this);
//    //    }
//    //}

//    //public class DamageOverTime : StatusAction {
//    //    public string element;
//    //    public float amount;
//    //    public float interval;

//    //    public override void OnEffectApplied(StatusEffect status) {
//    //        status.properties.Set("Timer", new Timer(status.duration.CachedValue));
//    //    }

//    //    public override void OnEffectUpdated(StatusEffect status) {
//    //        Timer timer = status.properties.Get<Timer>("Timer");
//    //        if (timer.ReadyWithReset()) {
//    //            //drain mana
//    //        }
//    //    }
//    //}

//    //public class DispelProtection : StatusAction {
//    //    float amount;

//    //    public override bool OnDispelAttempted(StatusEffect status) {
//    //        return UnityEngine.Random.Range(0, 1) > amount;
//    //    }
//    //}

//    //public class ExplodeOnDispel : StatusAction {
//    //    GameObject prefab;

//    //    public override void OnEffectDispelled(StatusEffect status) {
//    //        UnityEngine.Object.Instantiate(prefab);
//    //    }

//    //}

//public class StatusEffect {

//    [Writable(false)]
//    public StatusState state;

//    public ModifiableAttribute<StatusEffect> duration;
//    public bool IsExpirable;
//    public bool IsDispellable;
//    public bool IsRefreshable;
//    public bool IsUnique;

//    [HideInInspector]
//    public Entity caster;
//    [HideInInspector]
//    public Entity target;
//    protected PropertySet properties;
//    protected StatusAction[] actions;
//    protected Timer timer;

//    public void Initialize(Entity caster, Entity target) {
//        //hideFlags = HideFlags.HideInHierarchy;
//        //transform.hideFlags = HideFlags.HideInInspector;
//        //this.caster = caster;
//        //this.target = target;
//        //actions = GetComponents<StatusAction>();
//        //for (int i = 0; i < actions.Length; i++) {
//        //    actions[i].Initialize(caster, target);
//        //}           
//        //properties = new PropertySet();
//        //timer = new Timer(-1);
//    }

//    public bool ReadyForRemoval {
//        get { return state != StatusState.Active; }
//    }

//    public void Apply() {
//        state = StatusState.Active;
//        for (int i = 0; i < actions.Length; i++) {
//            actions[i].OnEffectApplied();
//        }
//    }

//    public void UpdateActions() {
//        for (int i = 0; i < actions.Length; i++) {
//            actions[i].OnEffectUpdated();
//        }
//        if (state == StatusState.Active && IsExpirable && timer.Ready) {
//            Expire();
//            state = StatusState.Expired;
//        }
//    }

//    public void Dispel(/*source?*/) {
//        if (state != StatusState.Active) return;
//        bool isDispelled = true;
//        for (int i = 0; i < actions.Length; i++) {
//            bool actionResult = actions[i].OnDispelAttempted();
//            if (!actionResult && isDispelled) {
//                isDispelled = actionResult;
//            }
//        }
//        if (IsDispellable && isDispelled) {
//            state = StatusState.Dispelled;
//            for (int i = 0; i < actions.Length; i++) {
//                actions[i].OnEffectDispelled();
//            }
//        }
//    }

//    public void Expire() {
//        for (int i = 0; i < actions.Length; i++) {
//            actions[i].OnEffectExpired();
//        }
//    }

//    public void Refresh() {
//        for (int i = 0; i < actions.Length; i++) {
//            actions[i].OnEffectRefreshed();
//        }
//    }

//    public void Remove() {
//        for (int i = 0; i < actions.Length; i++) {
//            actions[i].OnEffectRemoved();
//        }
//        //Destroy(gameObject);
//    }
//}

//}