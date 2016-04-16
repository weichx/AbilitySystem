using UnityEngine;
using System;
using System.Collections.Generic;

public interface IAbilityMatcher {
    bool Match(Ability ability);
}

public partial class Ability {

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
        for(int i = 0; i < components.Count; i++) {
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
}

