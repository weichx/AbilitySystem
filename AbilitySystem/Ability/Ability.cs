using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    //[Serializable] serializing this causes problems with constructors not being called right
    //fail :( will need to figure this out soon
    public class Ability {

        public readonly string name;
        public readonly Entity caster;
        public readonly CastMode castMode;
        public readonly TagCollection tags;
        public readonly AbilityPrototype prototype;

        public readonly AbilityAttribute cooldown;
        public readonly AbilityAttribute castTime;
        public readonly AbilityAttribute channelTime;
        public readonly AbilityAttribute channelTicks;
        public readonly AbilityAttribute charges;
        public readonly AbilityAttribute chargeUseCD;

        public readonly List<AbilityRequirement> requirements;

        public bool IgnoreGCD;

        protected Timer castTimer;
        protected Timer channelTimer;
        protected CastState castState;

        protected AbilityAttributeSet attributes;
        protected PropertySet properties;
        protected List<Timer> chargeTimers;

        public Ability(Entity caster, AbilityPrototype prototype) {
            this.caster = caster;
            this.prototype = prototype;
            name = prototype.name;
            castMode = prototype.castMode;
            ClonePrototypeAttributes();
            castTimer = new Timer();
            channelTimer = new Timer();
            castTime = new AbilityAttribute("CastTime", prototype.castTime);
            channelTime = new AbilityAttribute("ChannelTime", prototype.channelTime);
            channelTicks = new AbilityAttribute("ChannelTicks", prototype.channelTicks);
            cooldown = new AbilityAttribute("Cooldown", prototype.cooldown);
            charges = new AbilityAttribute("Charges", prototype.charges);
            tags = new TagCollection(prototype.tags);
            requirements = prototype.requirementSet.CloneToList();
            chargeTimers = new List<Timer>();
            UpdateAttributes();
        }

        public bool Use() {
            if (!CheckRequirements(RequirementType.CastStart)) {
                return false;
            }
            properties = new PropertySet();
            castState = CastState.Targeting;
            prototype.OnUse(this, properties);
            prototype.OnTargetSelectionStarted(this, properties);
          //  Update();
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
            if(castState == CastState.Targeting) {
                prototype.OnTargetSelectionCancelled(this, properties);
            }
            castState = CastState.Invalid;
            prototype.OnCastCancelled(this, properties);
        }

        public void InterruptCast() {
            if (castState == CastState.Targeting) {
                prototype.OnTargetSelectionCancelled(this, properties);
            }
            castState = CastState.Invalid;
            prototype.OnCastInterrupted(this, properties);
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
                if(IsCasting) {
                    return castTimer.Timeout;
                }
                else if(castMode == CastMode.Channel) {
                    return channelTicks.CachedValue;
                }
                else if(castMode == CastMode.Cast) {
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
        public CastState Update() {
            CastMode actualCastMode = castMode;
            if (castState == CastState.Targeting) {
                if (prototype.OnTargetSelectionUpdated(this, properties)) {
                    if(castMode == CastMode.Channel) {
                        float actualChannelTime = channelTime.UpdateValue(this);
                        castTimer.Reset(actualChannelTime);
                        channelTimer.Reset(actualChannelTime / channelTicks.UpdateValue(this));
                    }
                    else {
                        float actualCastTime = castTime.UpdateValue(this);
                        castTimer.Reset(actualCastTime);
                        actualCastMode = (actualCastTime <= 0f) ? CastMode.Instant : castMode;
                    }
                    prototype.OnTargetSelectionCompleted(this, properties);
                    prototype.OnCastStarted(this, properties);
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
                            prototype.OnChannelTick(this, properties);
                        }
                        castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                        break;
                    case CastMode.CastToChannel:
                        break;
                }
            }

            if (castState == CastState.Completed) {
                if (CheckRequirements(RequirementType.CastComplete)) {
                    prototype.OnCastCompleted(this, properties);
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
            properties = new PropertySet();
            float cd = cooldown.UpdateValue(this);
            for (int i = 0; i < chargeTimers.Count; i++) {
                if (chargeTimers[i].ReadyWithReset(cd)) {
                    prototype.OnChargeConsumed(this, properties);
                    return;
                }
            }
        }

        protected bool CheckRequirements(RequirementType requirementType) {
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

        protected void ClonePrototypeAttributes() {
            if (prototype.attributeSet == null) {
                attributes = new AbilityAttributeSet();
            }
            else {
                attributes = new AbilityAttributeSet();
                for (int i = 0; i < prototype.attributeSet.Count; i++) {
                    var toClone = prototype.attributeSet.attrs[i];
                    attributes.attrs.Add(new AbilityAttribute(toClone.id, toClone));
                }
            }
        }
    }
}