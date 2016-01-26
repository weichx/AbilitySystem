using System.Collections.Generic;
using AttributeSnapshot = System.Collections.Generic.Dictionary<string, float>;

public abstract class AbstractAbility {
    public static float RequirementCheckInterval = 0.1f;

    //todo maybe add some attachable callbacks
    protected Dictionary<string, ModifiableAttribute> attributes;
    protected Dictionary<string, ModifiableAttribute> resourceCosts;

    //public Dictionary<string, ResourceCost>(Mana, Fn|float, fn isApplicable) manaCost;
    public TagCollection tags;
    public CastState castState;
    public ModifiableAttribute castTime;
    public ModifiableAttribute cooldown;
    public ModifiableAttribute range;
    public bool IgnoreGCD = false;

    protected CastType castType;
    protected Timer castTimer;
    protected Timer requirementCheckTimer;
    protected List<AbilityRequirement> requirements;
    protected Entity caster;

    public AbstractAbility(Entity caster) {
        this.caster = caster;
        attributes = new Dictionary<string, ModifiableAttribute>(8);
        resourceCosts = new Dictionary<string, ModifiableAttribute>();
        requirements = new List<AbilityRequirement>();
        tags = new TagCollection();
        requirementCheckTimer = new Timer(0.1f);
        castTimer = new Timer();
        castType = CastType.Cast;
        castState = CastState.Invalid;
        range = attributes.AddAndReturn("Range", new ModifiableAttribute(40f));
        castTime = attributes.AddAndReturn("CastTime", new ModifiableAttribute(2f));
        cooldown = attributes.AddAndReturn("Cooldown", new ModifiableAttribute(0f));
        //attributes.AddAndReturn("TickTime", new TickTime(0.5f)); im not happy with how this is handled
    }

    public void Use() {
        OnTargetSelectionStarted();
        castTimer.Reset(castTime.Value);
        castState = CastState.Targeting;
        OnUse();
        Update();
    }

    public bool IsUsable() {
        for (int i = 0; i < requirements.Count; i++) {
            if (!requirements[i].CanStartCast(caster, this)) {
                //todo log this to game console when there is one
                UnityEngine.Debug.Log(requirements[i].FailureMessage);
                return false;
            }
        }
        return true;
    }

    public bool IsInstant { //not quite right for cast to channel spells
        get { return castTime.Value <= 0f; }
    }

    //temp
    public void UpdateAttributes() {
        //castTime.Update(this);
    }

    public CastState Update() {
        CastType actualCastMode = castType;
        if (requirementCheckTimer.ReadyWithReset(RequirementCheckInterval)) {
            for (int i = 0; i < requirements.Count; i++) {
                if (!requirements[i].CanContinueCast(caster, this)) {
                    //todo log this to game console when there is one
                    UnityEngine.Debug.Log(requirements[i].FailureMessage);
                    castState = CastState.Invalid;
                    return castState;
                }
            }
        }

        if (castState == CastState.Targeting) {
            if (OnTargetSelectionUpdated()) {
                OnTargetSelectionCompleted();
                OnCastStarted();
                castState = CastState.Casting;
                actualCastMode = (castTime.Value <= 0f) ? CastType.Instant : castType;
            }
        }

        if (castState == CastState.Casting) {
            switch (actualCastMode) {
                case CastType.Instant:
                    castState = CastState.Completed;
                    break;
                case CastType.Cast:
                    castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                    break;
                case CastType.Channeled:
                    //if (tickTimer.ReadyWithReset(tickTime)) OnChannelTick(); //todo maybe pass elapsed cast time and total cast time
                    castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                    break;
            }
        }

        if (castState == CastState.Completed) {
            OnCastCompleted();
        }

        return castState;
    }

    public bool IsCasting {
        get { return castState == CastState.Casting; }
    }

    //note: this is a cached value and maybe slightly out of sync
    //to get true updated cast time use castTime.UpdateValue();
    public float TotalCastTime {
        get { return castTime.Value; }
    }

    public float ElapsedCastTime {
        get { return castTimer.ElapsedTime; }
    }

    public void AddResourceCost(string resourceName, ModifiableAttribute attr) {
        resourceCosts.Add(resourceName, attr);
    }

    public void SetResourceCost(string resourceName, ModifiableAttribute attr) {

    }

    public void RemoveResourceCost(string resourcename, ModifiableAttribute attr) {

    }

    public void AddRequirement<T>(T requirement) where T : AbilityRequirement {
        if (!requirements.Contains(requirement)) {
            requirements.Add(requirement);
        }
    }

    public bool HasRequirement<T>(T requirement) where T : AbilityRequirement {
        return requirements.Contains(requirement as AbilityRequirement);
    }

    public bool RemoveRequirement<T>(T requirement) where T : AbilityRequirement {
        return requirements.Remove(requirement as AbilityRequirement);
    }

    public void InterruptCast() {
        OnCastInterrupted();
    }

    public void CancelCast() {
        OnCastCancelled();
    }

    #region attribute accessors
    public bool AddAttribute(string attrName, ModifiableAttribute attr) {
        if (attr == null) return false;
        if (attributes.ContainsKey(attrName)) {
            return false;
        }
        attributes[attrName] = attr;
        return true;
    }

    public bool HasAttribute(string key) {
        return attributes.ContainsKey(key);
    }

    public ModifiableAttribute GetAttribute(string attrName) {
        return attributes.Get(attrName);
    }

    public float GetAttributeValue(string attrName) {
        var attr = attributes.Get(attrName);
        return (attr != null) ? attr.Value : 0f;
    }

    public bool GetAttribute(string key, out ModifiableAttribute attr) {
        if (key == null) {
            attr = null;
            return false;
        }
        attributes.TryGetValue(key, out attr);
        return attr != null;
    }

    public AttributeSnapshot GetAttributeSnapshot() {
        var keys = attributes.Keys;
        var snapshot = new Dictionary<string, float>(keys.Count);
        foreach (string key in keys) {
            snapshot[key] = attributes[key].Value;
        }
        return snapshot;
    }

    #endregion

    #region callbacks
    //todo everywhere there is an `On****` call caster.On*** so other systems can coordinate with abilities
    public abstract bool OnTargetSelectionUpdated();

    public virtual void OnTargetSelectionStarted() { }
    public virtual void OnTargetSelectionCompleted() { }
    public virtual void OnTargetSelectionCancelled() { }

    public virtual void OnCastStarted() { }
    public virtual void OnCastCompleted() { }
    public virtual void OnCastFailed() { }
    public virtual void OnCastCancelled() { }
    public virtual void OnCastInterrupted() { }
    public virtual void OnChannelTick() { }
    public virtual void OnUse() { }

    public virtual void OnSpendResources() {
        //resources.each ->
        //resourceCost.Update()
        //resourceManager.AdjustResource(resource, resourceCost.Update());
    }

    #endregion
}

//public class AttributeSnapshot {
//    protected Dictionary<string, float> attributes;

//    public AttributeSnapshot(Dictionary<string, ModifiableAttribute> inputAttributes) {
//        var keys = inputAttributes.Keys;
//        attributes = new Dictionary<string, float>(keys.Count);
//        foreach(string key in keys) {
//            attributes[key] = inputAttributes[key].Value;
//        }
//    }

//    public float GetValue(string key) {
//        if (key == null) return 0f;
//        float retn = 0f;
//        attributes.TryGetValue(key, out retn);
//        return retn;
//    }

//    public bool GetValue(string key, out float value) {
//        if (key == null) {
//            value = 0f;
//            return false;
//        }
//        return attributes.TryGetValue(key, out value);
//    }

//    public bool HasValue(string key) {
//        return attributes.ContainsKey(key);
//    }

//    public void SetValue(string key, float value) {
//        attributes[key] = value;
//    }

//    public bool AddValue(string key, float value) {
//        if (attributes.ContainsKey(key)) return false;
//        attributes[key] = value;
//        return true;
//    }
//}