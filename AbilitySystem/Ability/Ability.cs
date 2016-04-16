//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace AbilitySystem {

//    public class Ability : AbilitySystemComponent {
//        public Sprite icon;
//        public CastMode castMode;

//        [Visible("ShowIgnoreGCD")]
//        public bool IgnoreGCD;

//        [Space()]
//        [Visible("ShowCastTime")]
//        public AbilityAttribute castTime;
//        [Visible("ShowChannelTime")]
//        public AbilityAttribute channelTime;
//        [Visible("ShowChannelTime")]
//        public AbilityAttribute channelTicks;
//        public AbilityAttribute cooldown;
//        public AbilityAttribute charges;
//        public AbilityAttribute range;

//        //public AbilityAttribute chargeUseCooldown;

//        [Space()]
//        public TagCollection tags;
//        public AbilityRequirementSet requirementSet;

//        protected AbilityAction[] actionList;

//        [HideInInspector]
//        public Entity caster;

//        protected Timer castTimer;
//        protected Timer channelTimer;
//        protected CastState castState;

//        protected List<Timer> chargeTimers;
//        protected TargetingStrategy targetingStrategy;

//        public TargetingStrategy TargetingStrategy {
//            get { return targetingStrategy; }
//        }

//        public void Reset() {
//            transform.hideFlags = HideFlags.HideInInspector;
//        }

//        public void Initialize(Entity caster) {
//            this.caster = caster;
//            gameObject.isStatic = true;
//            transform.hideFlags = HideFlags.HideInInspector;
//            castTimer = new Timer();
//            channelTimer = new Timer();
//            chargeTimers = new List<Timer>();
//            UpdateAttributes();
//            if (transform.childCount != 0) {
//                Debug.LogError("Abilities should never have children or non-ability components.");
//            }
//            Component[] components = GetComponents<Component>();
//            for (int i = 0; i < components.Length; i++) {
//                if (components[i] != transform && (components[i] as AbilitySystemComponent) == null) {
//                    DestroyImmediate(components[i], true);
//                    Debug.LogError("Abilities should never have other components attached, removing " + components[i].GetType().Name + " from " + name);
//                }
//            }

//            targetingStrategy = GetComponent<TargetingStrategy>();
//            targetingStrategy.__Initialize(this);

//            actionList = GetComponents<AbilityAction>();
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].Initialize(this);
//            }
//        }

//        public bool Use() {
//            if (!CheckRequirements(RequirementType.CastStart)) {
//                return false;
//            }
//            castState = CastState.BuildingContext;
//            OnUse();
//            OnTargetSelectionStarted();
//            return true;
//        }

//        public void UpdateAttributes() {
//            range.UpdateValue(this);
//            cooldown.UpdateValue(this);
//            castTime.UpdateValue(this);
//            channelTime.UpdateValue(this);
//            channelTicks.UpdateValue(this);
//            if (charges != null) {
//                SetChargeCount((int)charges.UpdateValue(this));
//            }
//        }

//        public void SetChargeCount(int chargeCount) {
//            int oldChargeCount = chargeTimers.Count;
//            if (chargeCount == oldChargeCount) return;
//            if (chargeCount > oldChargeCount) {
//                for (int i = oldChargeCount; i < chargeCount; i++) {
//                    chargeTimers.Add(new Timer(0));
//                }
//            }
//            else {
//                chargeTimers.RemoveRange(chargeCount, oldChargeCount);
//            }
//        }

//        public int AvailableCharges() {
//            int available = 0;
//            for (int i = 0; i < chargeTimers.Count; i++) {
//                if (chargeTimers[i].Ready) available++;
//            }
//            return available;
//        }

//        public float NextChargeReadyTime() {
//            float time = float.MaxValue;
//            for (int i = 0; i < chargeTimers.Count; i++) {
//                if (chargeTimers[i].Ready) {
//                    return 0;
//                }
//                else if (chargeTimers[i].TimeToReady < time) {
//                    time = chargeTimers[i].TimeToReady;
//                }
//            }
//            return time;
//        }

//        ///<summary>
//        ///Invokes a cooldown without consuming a charge or running any callbacks.
//        ///The is intended to be used to synchronize ability cooldowns.
//        ///</summary>
//        public virtual int InvokeCooldown(int charges = 1) {
//            int invoked = 0;
//            for (int i = 0; i < chargeTimers.Count; i++) {
//                if (chargeTimers[i].Ready) {
//                    chargeTimers[i].Reset();
//                    if (invoked++ == charges) return invoked;
//                }
//            }
//            return invoked;
//        }

//        public int TotalCharges() {
//            return (int)charges.UpdateValue(this);
//        }

//        public bool ChargeReady() {
//            return AvailableCharges() > 0;
//        }

//        public bool IsUsable() {
//            return ChargeReady() && CheckRequirements(RequirementType.CastStart);
//        }

//        public void CancelCast() {
//            if (castState == CastState.Invalid) return;
//            if (castState == CastState.BuildingContext) {
//                OnTargetSelectionCancelled();
//            }
//            castState = CastState.Invalid;
//            OnCastCancelled();
//        }

//        public void InterruptCast() {
//            if (castState == CastState.Invalid) return;
//            if (castState == CastState.BuildingContext) {
//                OnTargetSelectionCancelled();
//            }
//            castState = CastState.Invalid;
//            OnCastInterrupted();
//        }

//        public bool OnCooldown {
//            get { return charges.UpdateValue(this) > 0 && AvailableCharges() == 0; }
//        }

//        public float RemainingCooldown {
//            get {
//                if (!OnCooldown) return 0;
//                return NextChargeReadyTime();
//            }
//        }

//        public bool IsCasting {
//            get { return castState == CastState.Casting; }
//        }

//        public bool IsChanneled {
//            get { return castMode == CastMode.Channel; }
//        }

//        public float ElapsedCastTime {
//            get { return castState == CastState.Casting ? castTimer.ElapsedTime : 0f; }
//        }

//        public float TotalCastTime {
//            get {
//                if (IsCasting) {
//                    return castTimer.Timeout;
//                }
//                else if (castMode == CastMode.Channel) {
//                    return channelTicks.CachedValue;
//                }
//                else if (castMode == CastMode.Cast) {
//                    return castTime.CachedValue;
//                }
//                else {
//                    return 0f;
//                }
//            }
//        }

//        public bool IsInstant {
//            get { return castMode == CastMode.Instant || castTime.UpdateValue(this) <= 0f; }
//        }

//        //todo this should be throttled to 60 fps
//        public CastState UpdateCast() {
//            CastMode actualCastMode = castMode;
//            if (castState == CastState.BuildingContext) {
//                if (OnTargetSelectionUpdated()) {
//                    if (castMode == CastMode.Channel) {
//                        float actualChannelTime = channelTime.UpdateValue(this);
//                        castTimer.Reset(actualChannelTime);
//                        channelTimer.Reset(actualChannelTime / channelTicks.UpdateValue(this));
//                    }
//                    else {
//                        float actualCastTime = castTime.UpdateValue(this);
//                        castTimer.Reset(actualCastTime);
//                        actualCastMode = (actualCastTime <= 0f) ? CastMode.Instant : castMode;
//                    }
//                    OnTargetSelectionCompleted();
//                    OnCastStarted();
//                    castState = CastState.Casting;
//                }
//            }

//            if (castState == CastState.Casting) {
//                if (!CheckRequirements(RequirementType.CastUpdate)) {
//                    castState = CastState.Invalid;
//                    OnCastCancelled();
//                    return castState;
//                }
//                switch (actualCastMode) {
//                    case CastMode.Instant:
//                        castState = CastState.Completed;
//                        break;
//                    case CastMode.Cast:
//                        castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
//                        break;
//                    case CastMode.Channel:
//                        if (castTimer.Ready || channelTimer.ReadyWithReset()) {
//                            Debug.Log("Tick: " + castTimer.ElapsedTime);
//                            OnChannelTick();
//                        }
//                        castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
//                        break;
//                    case CastMode.CastToChannel:
//                        break;
//                }
//            }

//            if (castState == CastState.Completed) {
//                if (CheckRequirements(RequirementType.CastComplete)) {
//                    OnCastCompleted();
//                    ConsumeCharge();
//                    castState = CastState.Invalid;
//                    return CastState.Completed;
//                }
//                else {
//                    OnCastCancelled();
//                    castState = CastState.Invalid;
//                }
//            }
//            return castState;
//        }

//        protected void ConsumeCharge() {
//            float cd = cooldown.UpdateValue(this);
//            for (int i = 0; i < chargeTimers.Count; i++) {
//                if (chargeTimers[i].ReadyWithReset(cd)) {
//                    OnChargeConsumed();
//                    return;
//                }
//            }
//        }

//        protected bool CheckRequirements(RequirementType requirementType) {
//            List<AbilityRequirement> requirements = requirementSet.requirements;
//            for (int i = 0; i < requirements.Count; i++) {
//                if (!requirements[i].MeetsRequirement(this, requirementType)) {
//                    return false;
//                }
//            }
//            return true;
//        }

//        //public AbilityAttribute GetAttribute(string attrName) {
//        //    return attributes.Get(attrName);
//        //}

//        //public bool AddAttribute(AbilityAttribute attr, bool replace = true) {
//        //    if (attr == null) return false;
//        //    var existing = attributes.Get(attr.id);
//        //    if (existing != null) {
//        //        if (replace) {
//        //            attributes.Set(attr);
//        //            return true;
//        //        }
//        //        else {
//        //            return false;
//        //        }
//        //    }
//        //    attributes.Set(attr);
//        //    return true;
//        //}

//        //public bool HasAttribute(string attrName) {
//        //    return attributes.Get(attrName) != null;
//        //}

//        //public float GetAttributeValue(string attrName) {
//        //    var attr = attributes.Get(attrName);
//        //    if (attr == null) return 0f;
//        //    return attr.CachedValue;
//        //}

//        protected void OnTargetSelectionStarted() {
//            targetingStrategy.OnTargetSelectionStarted();
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnTargetSelectionStarted();
//            }
//        }

//        protected bool OnTargetSelectionUpdated() {
//            bool retn = targetingStrategy.OnTargetSelectionUpdated();
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnTargetSelectionUpdated();
//            }
//            return retn;
//        }

//        protected void OnTargetSelectionCompleted() {
//            targetingStrategy.OnTargetSelectionCompleted();
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnTargetSelectionCompleted();
//            }
//        }

//        protected void OnTargetSelectionCancelled() {
//            targetingStrategy.OnTargetSelectionCancelled();
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnTargetSelectionCancelled();
//            }
//        }

//        protected void OnUse() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnUse();
//            }
//        }

//        protected void OnCastStarted() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnCastStarted();
//            }
//        }

//        protected void OnCastCompleted() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnCastCompleted();
//            }
//        }

//        protected void OnCastFailed() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnCastFailed();
//            }
//        }

//        protected void OnCastCancelled() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnCastCancelled();
//            }
//        }

//        protected void OnCastInterrupted() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnCastInterrupted();
//            }
//        }

//        protected void OnChannelTick() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnChannelTick();
//            }
//        }

//        protected void OnChargeConsumed() {
//            for (int i = 0; i < actionList.Length; i++) {
//                actionList[i].OnChargeConsumed();
//            }
//        }

//        #region Editor Helpers
//        public static bool ShowCastTime(Ability ability) {
//            return ability.castMode == CastMode.Cast || ability.castMode == CastMode.CastToChannel;
//        }

//        public static bool ShowChannelTime(Ability ability) {
//            return ability.castMode == CastMode.Channel || ability.castMode == CastMode.CastToChannel;
//        }

//        public static bool ShowIgnoreGCD(Ability ability) {
//            return ability.castMode == CastMode.Instant;
//        }
//        #endregion
//    }
//}