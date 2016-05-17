using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public partial class Ability : EntitySystemBase {
    [NonSerialized]
    private CastEvent currentEvent;

    [NonSerialized]
    public Entity caster;
    [SerializeField] private int nextCharge;
    [SerializeField] private Charge[] charges;
    public CastState castState;
    public CastMode castMode;
    public CastMode actualCastMode; //a cast time of <= 0 makes things instant. inverse possible as well
    [NonSerialized] protected Timer castTimer;
    [NonSerialized] protected Timer channelTimer;
    public Texture2D icon;
    public AbilityContextCreator contextCreator;

    public FloatAttribute castTime;
    public FloatAttribute channelTime;
    public IntAttribute channelTicks;

    public List<AbilityRequirement> requirements;
    public List<AbilityComponent> components;

    public TagCollection tags;

    private Context context;

    public bool IgnoreGCD = false;

    public Ability() : this("") { }

    public Ability(string id) {
        Id = id;
        nextCharge = 0;
        charges = new Charge[1] {
            new Charge()
        };
        tags = new TagCollection();
       // attributes = new Dictionary<string, object>();
        components = new List<AbilityComponent>();
        requirements = new List<AbilityRequirement>();
        castTime = new FloatAttribute(1f);
        castMode = CastMode.Cast;
        channelTicks = new IntAttribute(3);
        channelTime = new FloatAttribute(3f);
        castTimer = new Timer();
        channelTimer = new Timer();
    }

    public CastEvent CurrentEvent {
        get {
            return currentEvent;
        }
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

    public bool Usable(Context context) {
        return OffCooldown && CheckRequirements(context, RequirementType.CastStart);
    }

    public bool Use(Context context) {

        if (!Usable(context)) {
            return false;
        }

        this.context = context;
        context.ability = this;

        SetComponentContext(context);
        OnUse();

        if (castState == CastState.Invalid) {
            if (castMode == CastMode.Channel) {
                float actualChannelTime = channelTime.Value;
                castTimer.Reset(actualChannelTime);
                channelTimer.Reset(actualChannelTime / channelTicks.Value);
            }
            else {
                float actualCastTime = castTime.Value;
                castTimer.Reset(actualCastTime);
                actualCastMode = (actualCastTime <= 0f) ? CastMode.Instant : castMode;
            }

            castState = CastState.Casting;
            OnCastStarted();
        }

        return true;
    }

    public CastState UpdateCast() {

        if (castState == CastState.Casting) {
            if (!CheckRequirements(context, RequirementType.CastUpdate)) {
                castState = CastState.Invalid;
                OnCastCancelled();
                return castState;
            }
            switch (actualCastMode) {
                case CastMode.Instant:
                    castState = CastState.Completed;
                    break;
                case CastMode.Cast:
                    castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                    break;
                case CastMode.Channel:
                    if (castTimer.Ready || channelTimer.ReadyWithReset()) {
                        Debug.Log("Tick: " + castTimer.ElapsedTime);
                        OnChannelTick();
                    }
                    castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                    break;
                case CastMode.CastToChannel:
                    break;
            }
        }

        if (castState == CastState.Completed) {
            if (CheckRequirements(context, RequirementType.CastComplete)) {
                OnCastCompleted();
                ExpireCharge();
                OnCastEnded();
                SetComponentContext(null);
                castState = CastState.Invalid;
                return CastState.Completed;
            }
            else {
                CancelCast();
                castState = CastState.Invalid;
            }
        }

        return castState;
    }

    public void CancelCast() {
        OnCastCancelled();
        OnCastEnded();
        SetComponentContext(null);
    }

    public void InterruptCast() {
        OnCastInterrupted();
        OnCastEnded();
        SetComponentContext(null);
    }

    private void SetComponentContext(Context context) {
        for (int i = 0; i < components.Count; i++) {
            components[i].context = context;
            components[i].ability = this;
            components[i].caster = caster;
        }
    }

    protected bool CheckRequirements(Context context, RequirementType reqType) {
        for (int i = 0; i < requirements.Count; i++) {
            if (!requirements[i].Test(context, reqType)) {
                return false;
            }
        }
        return true;
    }

    public bool IsCasting {
        get {
            return castState == CastState.Casting;
        }
    }

    public bool IsChanneled {
        get {
            return castMode == CastMode.Channel;
        }
    }

    public float ElapsedCastTime {
        get {
            return castState == CastState.Casting ? castTimer.ElapsedTime : 0f;
        }
    }

    public float NormalizedElapsedCastTime {
        get {
            return castState == CastState.Casting ? castTimer.ElapsedTime / TotalCastTime : 0f;
        }
    }

    public float TotalCastTime {
        get {
            if (IsCasting) {
                return castTimer.Timeout;
            }
            else if (castMode == CastMode.Channel) {
                return channelTicks.Value;
            }
            else if (castMode == CastMode.Cast) {
                return castTime.Value;
            }
            else {
                return 0f;
            }
        }
    }

    public bool IsInstant {
        get { return castMode == CastMode.Instant || castTime.Value <= 0f; }
    }

    public bool OnCooldown {
        get {
            return charges[nextCharge].OnCooldown;
        }
    }

    public bool OffCooldown {
        get {
            return !charges[nextCharge].OnCooldown;
        }
    }

    public int ChargeCount {
        get {
            return charges.Length - 1;
        }
    }

    public int GetCharges(ref Charge[] input, int count = -1) {
        if (count <= 0) {
            count = charges.Length - 1;
        }
        if (input.Length < charges.Length) {
            Array.Resize(ref input, charges.Length);
        }
        Array.Copy(charges, input, count);
        return count;
    }

    public Charge[] GetCharges() {
        Charge[] output = new Charge[charges.Length];
        Array.Copy(charges, output, charges.Length);
        return output;
    }


    public void AddCharge(float cooldown, bool ready = true) {
        Array.Resize(ref charges, charges.Length + 1);
        charges[charges.Length - 1] = new Charge(cooldown, ready);
    }

    public void SetChargeCooldown(float cooldown) {
        throw new System.NotImplementedException();
    }

    public bool SetChargeCooldown(int chargeIndex, float cooldown) {
        throw new System.NotImplementedException();
    }

    public bool RemoveCharge() {
        if (charges.Length == 1) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool RemoveCharge(int index) {
        if (index >= charges.Length) {
            return false;
        }
        return true;
    }

    public void ExpireCharge() {
        charges[nextCharge].Expire();
        nextCharge = (nextCharge + 1) % charges.Length;
    }

    public bool ExpireCharge(int chargeIndex) {
        throw new System.NotImplementedException();
    }

    protected void OnUse() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnUse();
        }
    }

    protected void OnChargeConsumed() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChargeConsumed();
        }
    }

    protected void OnCastStarted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastStarted();
        }
    }

    protected void OnCastUpdated() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastUpdated();
        }
    }

    protected void OnCastInterrupted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastInterrupted();
        }
    }

    protected void OnCastCompleted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastCompleted();
        }
    }

    protected void OnCastCancelled() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastCancelled();
        }
    }

    protected void OnCastFailed() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastFailed();
        }
    }

    protected void OnCastEnded() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastEnded();
        }
    }

    protected void OnChannelStart() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelStart();
        }
    }

    protected void OnChannelUpdated() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelUpdated();
        }
    }

    protected void OnChannelTick() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelTick();
        }
    }

    protected void OnChannelInterrupted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelInterrupted();
        }
    }

    protected void OnChannelCancelled() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelCancelled();
        }
    }

    protected void OnChannelEnd() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelEnd();
        }
    }

    //    private Dictionary<string, object> attributes;

    //    public T GetAttribute<T>(string attrName) {
    //        return (T)attributes.Get(attrName);
    //    }

    //    public object GetAttribute(string attrName) {
    //        return attributes.Get(attrName);
    //    }

    //    public void SetAttribute<T>(string attrName, T value) {
    //        attributes[attrName] = value;
    //    }

    //    public void SetAttribute(string attrName, object value) {
    //        attributes[attrName] = value;
    //    }

    //    public bool HasAttribute(string propertyName) {
    //        return attributes.ContainsKey(propertyName);
    //    }

    //    public object this[string attrName] {
    //        get {
    //            return attributes[attrName];
    //        }
    //        set {
    //            attributes[attrName] = value;
    //        }
    //    }
}
