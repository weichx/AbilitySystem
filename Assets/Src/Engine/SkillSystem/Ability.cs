using UnityEngine;
using System;
using System.Collections.Generic;
using Intelligence;

public partial class Ability : EntitySystemBase {

    [SerializeField] protected int nextCharge;
    [SerializeField] protected Charge[] charges;
    [NonSerialized] protected CastState castState;

    [NonSerialized] protected Entity caster;
    [NonSerialized] protected CastMode actualCastMode; //a cast time of <= 0 makes things instant. inverse possible as well
    [NonSerialized] protected Timer castTimer;
    [NonSerialized] protected Timer channelTimer;

    public Sprite icon;
    public FloatRange castTime;
    public FloatRange channelTime;
    public IntRange channelTicks;
    [SerializeField] protected CastMode castMode;

    public List<AbilityRequirement> requirements;
    public List<AbilityComponent> components;

    public TagCollection tags;
    public Type contextType;

    [NonSerialized] private Context context;

    public bool IgnoreGCD = false;

    public Ability() : this("") { }

    public Ability(string id) {
        Id = id;
        nextCharge = 0;
        charges = new Charge[1] {
            new Charge()
        };
        tags = new TagCollection();
        components = new List<AbilityComponent>();
        requirements = new List<AbilityRequirement>();
        castTime = new FloatRange(1f);
        castMode = CastMode.Cast;
        channelTicks = new IntRange(3);
        channelTime = new FloatRange(3f);
        castTimer = new Timer();
        channelTimer = new Timer();
    }

    public void SetCaster(Entity entity) {
        caster = entity;
    }

    public Entity Caster {
        get { return caster; }
    }

    public Context GetContext() {
        return context;
    }

    public T GetContext<T>() where T : Context {
        return context as T;
    }

    public AbilityComponent AddAbilityComponent<T>() where T : AbilityComponent, new() {
        AbilityComponent component = new T();
        component.ability = this;
        components.Add(component);
        //if casting, initialize
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

    public bool Usable(Context context) {
        return OffCooldown && CheckRequirements(context, RequirementType.CastStart);
    }

    public bool Use(Context context) {

        if (!Usable(context)) {
            return false;
        }

        this.context = context;
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
            components[i].ability = this;
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

}
