using System;
using System.Collections.Generic;
using AbilitySystem;

public class AbilityManager {

    protected List<Ability> abilities;
    protected List<int> abilityReferences;
    protected List<AbilityModifier> abilityModifiers;

    protected Timer gcdTimer;
    protected CastQueue castQueue;
    protected Entity entity;

    public static float BaseGlobalCooldown = 1.5f;

    public AbilityManager(Entity entity) {
        this.entity = entity;
        abilities = new List<Ability>();
        abilityReferences = new List<int>();
        abilityModifiers = new List<AbilityModifier>();
        castQueue = new CastQueue();
        gcdTimer = new Timer(BaseGlobalCooldown);
    }

    ///<summary>Add an ability by id and return the number of references to this ability</summary>
    public int AddAbility(string abilityId) {
        int idx = -1;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i].Id == abilityId) {
                idx = i;
                break;
            }
        }
        if (idx == -1) {
            Ability ability = EntitySystemLoader.Instance.Create<Ability>(abilityId);
            ability.caster = entity;
            ApplyModifiers(ability);
            abilities.Add(ability);
            abilityReferences.Add(1);
            return abilityReferences.Last();
        }
        else {
            abilityReferences[idx]++;
            return abilityReferences[idx];
        }
    }

    protected void ApplyModifiers(Ability ability) {
        for (int i = 0; i < abilityModifiers.Count; i++) {
            abilityModifiers[i].Apply(ability);
        }
    }

    protected void UpdateModifiers() {
        for(int i = 0; i < abilityModifiers.Count; i++) {
            abilityModifiers[i].Update();
        }
    }

    protected void RemoveModifiers(Ability ability) {
        for (int i = 0; i < abilityModifiers.Count; i++) {
            abilityModifiers[i].Remove(ability);
        }
    }

    ///<summary>Remove an ability, return remaining number of references</summary>
    public int RemoveAbility(string abilityId) {
        int idx = -1;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i].Id == abilityId) {
                idx = i;
                break;
            }
        }
        if (idx == -1) {
            return -1;
        }
        else {
            abilityReferences[idx]--;
            if (abilityReferences[idx] == 0) {
                Ability ability = abilities[idx];
                RemoveModifiers(ability);
                abilities.RemoveAt(idx);
                abilityReferences.RemoveAt(idx);
                return 0;
            }
            else {
                return abilityReferences[idx];
            }
        }
    }

    //todo -- extend this so player has ability pairs [Ability, Context] and something to build the context
    //PlayerContextBuilder maybe? this would manage life cycle

    //public bool Cast(string abilityId) {
    //    Ability ability = GetAbility(abilityId);
    //    AbilityContextCreator contextFactory = ability.GetContextFactory();
    //    if(contextFactory.IsContextReady) {

    //    }
    //    else {

    //    }
    //    return false;
    //}

    public bool Cast(Ability ability) {
        return false;
    }

    public bool Cast(string abilityId, OldContext context) {
        Ability ability = GetAbility(abilityId);
        if (ability == null) throw new AbilityNotFoundException(abilityId);

        if (ability.Usable(context)) {
            castQueue.Enqueue(ability, context);
            return true;
        }

        return false;
    }

    public void Update() {
        castQueue.UpdateCast();
        UpdateModifiers();
    }

    public Ability GetAbility(string abilityId) {
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i].Id == abilityId) return abilities[i];
        }
        return null;
    }

    public bool OnCooldown(string abilityId) {
        return GetAbility(abilityId).OnCooldown;
    }

    public bool InterruptCast() { //todo add parameter for cast interrupter class 
        if (IsCasting) {
            //castQueue.CurrentAbility.InterruptCast();
        }
        castQueue.Clear();
        return true;
    }

    public bool CancelCast() {
        Ability current = castQueue.CurrentAbility;
        if (current != null) {
          //  current.CancelCast();
            castQueue.Clear();
        }
        return current != null;
    }

    public bool IsCasting {
        get {
            Ability ability = castQueue.CurrentAbility;
            return ability != null && ability.IsCasting;
        }
    }

    public Ability ActiveAbility {
        get {
            return castQueue.CurrentAbility;
        }
    }

    public float CastProgress {
        get {
            if (IsCasting) {
                return ElapsedCastTime / TotalCastTime;
            }
            return 0f;
        }
    }

    public float ElapsedCastTime { get; internal set; }

    public float NormalizedElapsedCastTime {
        get {
            if (!IsCasting) return 0;
            return castQueue.CurrentAbility.NormalizedElapsedCastTime;
        }
    }

    public float TotalCastTime {
        get { return (IsCasting) ? castQueue.CurrentAbility.TotalCastTime : 0f; }
    }

    public void AddAbilityModifiers(IList<AbilityModifier> mods) {
        for (int i = 0; i < mods.Count; i++) {
            AddAbilityModifier(mods[i]);
        }
    }

    public void AddAbilityModifier(AbilityModifier mod) {
        mod.Initialize();
        for (int i = 0; i < abilities.Count; i++) {
            mod.Apply(abilities[i]);
        }
        abilityModifiers.Add(mod); //todo maybe ref count these as well? or break into stacks
    }

    public void RemoveAbilityModifier(AbilityModifier mod) { //todo maybe by id?
        for (int i = 0; i < abilities.Count; i++) {
            mod.Remove(abilities[i]);
        }
        abilityModifiers.Remove(mod);
    }

}

public class AbilityNotFoundException : Exception {

    public AbilityNotFoundException(string abilityId) : base(abilityId) { }

}