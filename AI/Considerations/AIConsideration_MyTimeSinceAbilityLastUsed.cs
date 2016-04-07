using System;
using AbilitySystem;
using UnityEngine;

public class AIConsideration_TimeSinceAbilityLastUsed : AIConsideration {

    public string abilityId;
    public float maxTime_ms;
    public float minTime_ms;

    public override float Score(AIDecisionContext context) {
        return 1f;
    }

}