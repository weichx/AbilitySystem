using System;
using System.Collections.Generic;

public class StatusEffect : IDeserializable {

    public string statusEffectId;
    public StatusState state;
    public FloatAttribute duration;
    public TagCollection tags;
    public bool IsExpirable;
    public bool IsDispellable;
    public bool IsRefreshable;
    public bool IsUnique;

    public Entity caster;
    public Entity target;
    public List<StatusEffectComponent> components;
    protected Timer timer;
    public Context context;

    public FloatAttribute tickRate;
    public FloatAttribute ticks;

    public StatusEffect() {
        state = StatusState.Invalid;
        components = new List<StatusEffectComponent>();
        tags = new TagCollection();
        duration = new FloatAttribute();
        timer = new Timer();//can probably get rid of timer and just duration as a variable
    }

    public StatusEffectComponent AddStatusComponent<T>() where T : StatusEffectComponent, new() {
        T component = new T();
        component.statusEffect = this;
        component.caster = caster;
        component.context = context;
        components.Add(component);
        return component;
    }

    public void Apply(Entity target, Context context) {
        this.target = target;
        this.context = context;
        caster = context.entity;
        state = StatusState.Active;
        UpdateComponentContext();
    }

    public void UpdateComponents() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnEffectUpdated();
        }
        if (state == StatusState.Active && IsExpirable && timer.Ready) {
            Expire();
        }
    }

    public void Dispel(/*source?*/) {
        if (state != StatusState.Active) return;
        bool isDispelled = true;
        for (int i = 0; i < components.Count; i++) {
            bool actionResult = components[i].OnDispelAttempted(); //pass dispel context? prolly
            if (!actionResult && isDispelled) {
                isDispelled = actionResult;
            }
        }
        if (IsDispellable && isDispelled) {
            state = StatusState.Dispelled;
            for (int i = 0; i < components.Count; i++) {
                components[i].OnEffectDispelled();
            }
        }
    }

    public void Expire() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnEffectExpired();
        }
        state = StatusState.Expired;
    }

    public void Refresh(Context context) {
        this.context = context;
        UpdateComponentContext();
        for (int i = 0; i < components.Count; i++) {
            components[i].OnEffectRefreshed(context);
        }
    }

    public void Remove() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnEffectRemoved();
        }
    }

    public void OnDeserialized(Dictionary<string, object> table) {

    }

    public bool ReadyForRemoval {
        get { return state != StatusState.Active; }
    }

    private void UpdateComponentContext() {
        for (int i = 0; i < components.Count; i++) {
            components[i].caster = caster;
            components[i].target = target;
            components[i].context = context;
            components[i].OnEffectApplied();
        }
    }

    public virtual void AddStatusComponentsFromTemplate(StatusEffectTemplate template) {
        if (template.additionalComponents == null) return;
        for (int i = 0; i < template.additionalComponents.Length; i++) {
            StatusEffectComponent component = template.additionalComponents[i];
            components[i].caster = caster;
            components[i].target = target;
            components[i].context = context;
            components.Add(component);
        }
    }

    
}
