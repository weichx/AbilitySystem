using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class Ability {

        public readonly string name;
        public readonly Entity caster;
        public readonly CastMode castMode;
        public readonly TagCollection tags;
        public readonly AbilityPrototype prototype;

        public readonly AbilityAttribute cooldown;
        public readonly AbilityAttribute castTime;
        public readonly AbilityAttribute channelTime;
        public readonly AbilityAttribute charges;
        public readonly AbilityAttribute chargeUseCooldown;

        public readonly List<AbilityRequirement> requirements;

        public bool IgnoreGCD;

        [SerializeField] protected Timer castTimer;
        [SerializeField] protected CastState castState;
        [SerializeField] protected AbilityAttributeSet attributes;

        protected List<Timer> chargeTimers;

        public Ability(Entity caster, AbilityPrototype prototype) {
            this.caster = caster;
            this.prototype = prototype;
            name = prototype.name;
            castMode = prototype.castMode;
            ClonePrototypeAttributes();
            castTimer = new Timer();
            castTime = new AbilityAttribute("CastTime", prototype.castTime);
            channelTime = new AbilityAttribute("ChannelTime", prototype.channelTime);
            cooldown = new AbilityAttribute("Cooldown", prototype.cooldown);
            charges = new AbilityAttribute("Charges", prototype.charges);
            tags = new TagCollection(prototype.tags);
            requirements = prototype.requirementSet.CloneToList();
            chargeTimers = new List<Timer>();
            UpdateAttributes();         
        }

        public void Use() {
            castState = CastState.Targeting;
            prototype.OnUse(this, caster);
            prototype.OnTargetSelectionStarted(this, caster);
            Update();
        }

        public void UpdateAttributes() {
            attributes.UpdateAll(this);
            SetChargeCount((int)charges.UpdateValue(this));
        }

        public void SetChargeCount(int chargeCount) {
            int oldChargeCount = chargeTimers.Count;
            if (chargeCount == oldChargeCount) return;
            if (chargeCount > oldChargeCount) {
                float value = cooldown.CachedValue;
                if(value <= 0) {
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
            castState = CastState.Invalid;
            prototype.OnCastCancelled(this, caster);
        }

        public void InterruptCast() {
            castState = CastState.Invalid;
            prototype.OnCastInterrupted(this, caster);
        }

        public float ElapsedCastTime {
            get { return castState == CastState.Casting ? castTimer.ElapsedTime : 0f; }
        }

        public bool IsInstant {
            get { return castMode == CastMode.Instant || castTime.UpdateValue(this) <= 0f; }
        }

        //todo this should be throttled to 60 fps
        public CastState Update() {
            CastMode actualCastMode = castMode;
            if (castState == CastState.Targeting) {
                if (prototype.OnTargetSelectionUpdated(this, caster)) {
                    prototype.OnTargetSelectionCompleted(this, caster);
                    prototype.OnCastStarted(this, caster);
                    float actualCastTime = castTime.UpdateValue(this);
                    castTimer.Reset(actualCastTime);
                    castState = CastState.Casting;
                    actualCastMode = (actualCastTime <= 0f) ? CastMode.Instant : castMode;
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
                        //if (tickTimer.ReadyWithReset(tickTime)) OnChannelTick(); //todo maybe pass elapsed cast time and total cast time
                        //castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                        break;
                    case CastMode.CastToChannel:
                        break;
                }
            }

            if (castState == CastState.Completed) {
                if (CheckRequirements(RequirementType.CastComplete)) {
                    prototype.OnCastCompleted(this, caster);
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
                    prototype.OnChargeConsumed(this, caster);
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