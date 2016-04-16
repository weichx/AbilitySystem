using UnityEngine;
using System;
using System.Collections.Generic;

public partial class Ability : IDeserializable {

    public string abilityId;
    [NonSerialized]
    public Entity caster;
    private int nextCharge;
    private Charge[] charges;
    public CastState castState;
    public CastMode castMode;
    public CastMode actualCastMode; //a cast time of <= 0 makes things instant. inverse possible as well
    protected Timer castTimer;
    protected Timer channelTimer;

    public AbilityContextCreator contextCreator;

    public FloatAttribute castTime;
    public FloatAttribute channelTime;
    public IntAttribute channelTicks;

    public List<AbilityRequirement> requirements;
    public List<AbilityComponent> components;

    private Context context;

    public bool IgnoreGCD = false;

    public Ability() {
        nextCharge = 0;
        charges = new Charge[1] {
            new Charge()
        };
        components = new List<AbilityComponent>();
        requirements = new List<AbilityRequirement>();
        castTime = new FloatAttribute(1f);
        castMode = CastMode.Cast;
        channelTicks = new IntAttribute(3);
        channelTime = new FloatAttribute(3f);
        castTimer = new Timer();
        channelTimer = new Timer();
    }

    public AbilityComponent AddAbilityComponent<T>() where T : AbilityComponent, new() {
        AbilityComponent component = new T();
        component.ability = this;
        component.caster = caster;
        component.context = context;
        components.Add(component);
        return component;
    }

    public T GetAbilityComponent<T>() where T : AbilityComponent {
        Type type = typeof(T);
        for(int i = 0; i < components.Count; i++) {
            if (type == components[i].GetType()) return components[i] as T;
        }
        return null;
    }

    public bool RemoveAbilityComponent(AbilityComponent component) {
        return components.Remove(component);
    }

    public void OnDeserialized(Dictionary<string, object> properties) {
        int chargeCount = (int)properties.Get("chargeCount", 1);
        if (chargeCount <= 0) chargeCount = 1;

        charges = new Charge[chargeCount];

        float chargeCooldown = (float)properties.Get("chargeCooldown", 0f);

        for (int i = 0; i < chargeCount; i++) {
            charges[i] = new Charge(chargeCooldown);
        }

        if (castMode != CastMode.Instant) {
            if (castTime.Value <= 0) {
                castTime.BaseValue = 1f;
            }
        }

        SetComponentContext(null);
    }
}
