using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Entity))] //there might be a way to not require entity
public class AbilityManager : MonoBehaviour {

    [HideInInspector] public Entity entity;

    public float baseGlobalCooldown = 1f;

    public Dictionary<string, Ability> abilities;
    public Timer gcdTimer;
    public CastQueue castQueue;

    void Start() {
        castQueue = new CastQueue();
        gcdTimer = new Timer(baseGlobalCooldown);
        entity = GetComponent<Entity>();
        var rm = GetComponent<ResourceManager>();
        if(rm != null) {
            //entity.eventManager.AddListener<(UseResources);
        }
        //todo array should be stored 
        //LoadAbilities(new string[] {
        //    "Frostbolt", "Fireball", "Fireblast"
        //});
    }

    public void Cast(string abilityId) {
        Ability ability = abilities.Get(abilityId);
        if (ability == null) throw new AbilityNotFoundException(abilityId);
        if (ability.IsUsable()) {
            castQueue.Enqueue(ability);
        }
        else {
            Debug.Log("That ability failed some requirements");
        }
    }

    public void Update() {
        var completedAbility = castQueue.UpdateCast();
        if(completedAbility != null) {
            entity.AbilityWasUsed(completedAbility);//.TriggerEvent(new AbilityUsedEvent(completedAbility));
        }
    }

    //entity.OnAbilityUsedWithTag(TagCollection, callback, once?
    //entity.OnAbilityUsed(abilityName)
    //entity.OnAbilityUsed<T>();

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

    public void AddAbility(string abilityId) {
        //if (abilities.Get(abilityId) != null) return;
        //AbilityPrototype prototype = masterAbilityDatabase.Get(abilityId);
        //if (prototype == null) throw new AbilityNotFoundException(abilityId);
        //abilities.Add(abilityId, prototype.CreateAbility(entity));
    }

    public void RemoveAbility(string abilityId) {
        abilities.Remove(abilityId);
    }

    public Ability GetAbility(string abilityId) {
        return abilities.Get(abilityId);
    }

    public bool IsCasting {
        get {
            Ability ability = castQueue.current;
            return ability != null && ability.castState == CastState.Casting;
        }
    }

    public float ElapsedCastTime {
        get { return (IsCasting) ? castQueue.current.ElapsedCastTime : 0; }
    }

    public float TotalCastTime {
        get { return (IsCasting) ? castQueue.current.TotalCastTime : 0f; }
    }

    //private void LoadAbilities(string[] abilityIds) {
    //    LoadResources();
    //    abilities = new Dictionary<string, Ability>();
    //    if (abilityIds != null) {
    //        for (int i = 0; i < abilityIds.Length; i++) {
    //            AbilityPrototype proto = null;
    //            string id = abilityIds[i];
    //            if (masterAbilityDatabase.TryGetValue(id, out proto)) {
    //                abilities[id] = proto.CreateAbility(entity);
    //            }
    //            else {
    //                throw new AbilityNotFoundException(id);
    //            }
    //            //todo apply any default or character modifiers that should go with this ability here
    //            //they are likely serialized somewhere else and will need to be loaded / created
    //        }
    //    }
    //}

    //private static Dictionary<string, AbilityPrototype> masterAbilityDatabase;
    //private static void LoadResources() {
    //    if (masterAbilityDatabase != null) return;
    //    masterAbilityDatabase = new Dictionary<string, AbilityPrototype>();
    //    AbilityPrototype[] prototypes = Resources.LoadAll<AbilityPrototype>("");
    //    for (int i = 0; i < prototypes.Length; i++) {
    //        masterAbilityDatabase[prototypes[i].Id] = prototypes[i];
    //    }
    //}
}

public class AbilityNotFoundException : Exception {

    public AbilityNotFoundException(string abilityId) : base(abilityId) { }

}
