using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    public class Ability : AbilitySystemComponent, IAbilityRelated {
        public Sprite icon;
        public CastMode castMode;
        
        [Visible("ShowIgnoreGCD")]
        public bool IgnoreGCD;

        [Space()]
        [Visible("ShowCastTime")]
        public AbilityAttribute castTime;
        [Visible("ShowChannelTime")]
        public AbilityAttribute channelTime;
        [Visible("ShowChannelTime")]
        public AbilityAttribute channelTicks;
        public AbilityAttribute cooldown;
        public AbilityAttribute charges;
        //public AbilityAttribute chargeUseCooldown;

        [Space()]
        public TagCollection tags;
        public AbilityRequirementSet requirementSet;
        public AbilityAttributeSet attributes;

        protected List<AbilityAction> actionList;

        [HideInInspector]
        public Entity caster;

        protected Timer castTimer;
        protected Timer channelTimer;
        protected CastState castState;

        protected List<Timer> chargeTimers;

        public void Initialize(Entity caster) {
            this.caster = caster;
            castTimer = new Timer();
            channelTimer = new Timer();
            chargeTimers = new List<Timer>();
            UpdateAttributes();
            if(transform.childCount != 0) {
                Debug.LogError("Abilities should never have children or non-ability components.");
            }
            Component[] components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                if (components[i] != transform && (components[i] as AbilitySystemComponent) == null) {
                    DestroyImmediate(components[i], true);
                    Debug.LogError("Abilities should never have other components attached, removing " + components[i].GetType().Name + " from " + name);
                }
            }
            actionList = new List<AbilityAction>(GetComponents<AbilityAction>());
            Debug.Log(actionList.Count);
            for(int i = 0; i < actionList.Count; i++) {
                actionList[i].Initialize(this);
            }
        }

        //public virtual Ability CreateAbility(Entity caster) {
        //    var retn = Instantiate(this);
        //    retn.caster = caster;
        //    //retn.name = name;
        //    return retn;
        //}

        public bool Use() {
            if (!CheckRequirements(RequirementType.CastStart)) {
                return false;
            }
            castState = CastState.Targeting;
            OnUse();
            OnTargetSelectionStarted();
            return true;
        }

        public void UpdateAttributes() {
            attributes.UpdateAll(this);
            if (charges != null) {
                SetChargeCount((int)charges.UpdateValue(this));
            }
        }

        public void SetChargeCount(int chargeCount) {
            int oldChargeCount = chargeTimers.Count;
            if (chargeCount == oldChargeCount) return;
            if (chargeCount > oldChargeCount) {
                float value = cooldown.CachedValue;
                if (value <= 0) {
                    value = 0.0001f;
                }
                for (int i = oldChargeCount; i < chargeCount; i++) {
                    chargeTimers.Add(new Timer(value));
                }
            }
            else {
                chargeTimers.RemoveRange(chargeCount, oldChargeCount);
            }
        }

        public int AvailableCharges() {
            int available = 0;
            for (int i = 0; i < chargeTimers.Count; i++) {
                if (chargeTimers[i].Ready) available++;
            }
            return available;
        }

        public float NextChargeReadyTime() {
            float time = float.MaxValue;
            for (int i = 0; i < chargeTimers.Count; i++) {
                if (chargeTimers[i].Ready) {
                    return 0;
                }
                else if (chargeTimers[i].TimeToReady < time) {
                    time = chargeTimers[i].TimeToReady;
                }
            }
            return time;
        }

        ///<summary>
        ///Invokes a cooldown without consuming a charge or running any callbacks.
        ///The is intended to be used to synchronize ability cooldowns.
        ///</summary>
        public virtual int InvokeCooldown(int charges = 1) {
            int invoked = 0;
            for (int i = 0; i < chargeTimers.Count; i++) {
                if (chargeTimers[i].Ready) {
                    chargeTimers[i].Reset();
                    if (invoked++ == charges) return invoked;
                }
            }
            return invoked;
        }

        public int TotalCharges() {
            return (int)charges.UpdateValue(this);
        }

        public bool ChargeReady() {
            return AvailableCharges() > 0;
        }

        public bool IsUsable() {
            return ChargeReady() && CheckRequirements(RequirementType.CastStart);
        }

        public void CancelCast() {
            if (castState == CastState.Targeting) {
                OnTargetSelectionCancelled();
            }
            castState = CastState.Invalid;
            OnCastCancelled();
        }

        public void InterruptCast() {
            if (castState == CastState.Targeting) {
                OnTargetSelectionCancelled();
            }
            castState = CastState.Invalid;
            OnCastInterrupted();
        }

        public bool OnCooldown {
            get { return charges.UpdateValue(this) > 0 && AvailableCharges() == 0; }
        }

        public float RemainingCooldown {
            get {
                if (!OnCooldown) return 0;
                return NextChargeReadyTime();
            }
        }
        public bool IsCasting {
            get { return castState == CastState.Casting; }
        }

        public bool IsChanneled {
            get { return castMode == CastMode.Channel; }
        }

        public float ElapsedCastTime {
            get { return castState == CastState.Casting ? castTimer.ElapsedTime : 0f; }
        }

        public float TotalCastTime {
            get {
                if (IsCasting) {
                    return castTimer.Timeout;
                }
                else if (castMode == CastMode.Channel) {
                    return channelTicks.CachedValue;
                }
                else if (castMode == CastMode.Cast) {
                    return castTime.CachedValue;
                }
                else {
                    return 0f;
                }
            }
        }

        public bool IsInstant {
            get { return castMode == CastMode.Instant || castTime.UpdateValue(this) <= 0f; }
        }

        //todo this should be throttled to 60 fps
        public CastState UpdateCast() {
            CastMode actualCastMode = castMode;
            if (castState == CastState.Targeting) {
                if (OnTargetSelectionUpdated()) {
                    if (castMode == CastMode.Channel) {
                        float actualChannelTime = channelTime.UpdateValue(this);
                        castTimer.Reset(actualChannelTime);
                        channelTimer.Reset(actualChannelTime / channelTicks.UpdateValue(this));
                    }
                    else {
                        float actualCastTime = castTime.UpdateValue(this);
                        castTimer.Reset(actualCastTime);
                        actualCastMode = (actualCastTime <= 0f) ? CastMode.Instant : castMode;
                    }
                    OnTargetSelectionCompleted();
                    OnCastStarted();
                    castState = CastState.Casting;
                }
            }

            if (castState == CastState.Casting) {
                if (!CheckRequirements(RequirementType.CastUpdate)) {
                    castState = CastState.Invalid;
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
                if (CheckRequirements(RequirementType.CastComplete)) {
                    OnCastCompleted();
                    ConsumeCharge();
                    castState = CastState.Invalid;
                    return CastState.Completed;
                }
                else {
                    castState = CastState.Invalid;
                }
            }
            return castState;
        }

        protected void ConsumeCharge() {
            float cd = cooldown.UpdateValue(this);
            for (int i = 0; i < chargeTimers.Count; i++) {
                if (chargeTimers[i].ReadyWithReset(cd)) {
                    OnChargeConsumed();
                    return;
                }
            }
        }

        protected bool CheckRequirements(RequirementType requirementType) {
            List<AbilityRequirement> requirements = requirementSet.requirements;
            for (int i = 0; i < requirements.Count; i++) {
                if (!requirements[i].MeetsRequirement(this, requirementType)) {
                    return false;
                }
            }
            return true;
        }

        public AbilityAttribute GetAttribute(string attrName) {
            return attributes.Get(attrName);
        }

        public bool AddAttribute(AbilityAttribute attr, bool replace = true) {
            if (attr == null) return false;
            var existing = attributes.Get(attr.id);
            if (existing != null) {
                if (replace) {
                    attributes.Set(attr);
                    return true;
                }
                else {
                    return false;
                }
            }
            attributes.Set(attr);
            return true;
        }

        public bool HasAttribute(string attrName) {
            return attributes.Get(attrName) != null;
        }

        public float GetAttributeValue(string attrName) {
            var attr = attributes.Get(attrName);
            if (attr == null) return 0f;
            return attr.CachedValue;
        }

        public virtual void OnTargetSelectionStarted() {
            for (int i = 0; i < actionList.Count; i++) {
                actionList[i].OnTargetSelectionStarted();
            }
        }

        public virtual bool OnTargetSelectionUpdated() { return true; }
        public virtual void OnTargetSelectionCompleted() { }
        public virtual void OnTargetSelectionCancelled() { }

        public virtual void OnUse() {
            for (int i = 0; i < actionList.Count; i++) {
                actionList[i].OnUse();
            }
        }

        public virtual void OnCastStarted() {
            for (int i = 0; i < actionList.Count; i++) {
                actionList[i].OnCastStarted();
            }
        }

        public virtual void OnCastCompleted() {
            for (int i = 0; i < actionList.Count; i++) {
                actionList[i].OnCastCompleted();
            }
        }

        public virtual void OnCastFailed() { }
        public virtual void OnCastCancelled() { }
        public virtual void OnCastInterrupted() { }
        public virtual void OnChannelTick() { }
        public virtual void OnChargeConsumed() { }

        protected static GameObject SpawnAndInitialize(GameObject toSpawn, Ability ability, Vector3? position = null, Quaternion? rotation = null) {
            if (position == null) {
                position = ability.caster.transform.position;
            }
            if (rotation == null) {
                rotation = ability.caster.transform.rotation;
            }

            GameObject spawned = Instantiate(toSpawn, (Vector3)position, (Quaternion)rotation) as GameObject;
            IAbilityInitializer[] components = spawned.GetComponents<IAbilityInitializer>();
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    components[i].Initialize(ability);
                }
            }
            return spawned;
        }

        protected static void DestructAndDespawn(GameObject toDespawn, Ability ability) {
            if (toDespawn == null) return;
            IAbilityDestructor[] components = toDespawn.GetComponents<IAbilityDestructor>();
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    components[i].Destruct(ability);
                }
            }
            Destroy(toDespawn);
        }

        #region Editor Helpers
        public static bool ShowCastTime(Ability ability) {
            return ability.castMode == CastMode.Cast || ability.castMode == CastMode.CastToChannel;
        }

        public static bool ShowChannelTime(Ability ability) {
            return ability.castMode == CastMode.Channel || ability.castMode == CastMode.CastToChannel;
        }

        public static bool ShowIgnoreGCD(Ability ability) {
            return ability.castMode == CastMode.Instant;
        }
        #endregion
    }
}