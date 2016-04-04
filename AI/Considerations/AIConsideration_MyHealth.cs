using System;
using AbilitySystem;
using UnityEngine;

public class AIConsideration_MyHealth : AIConsideration {

    public override float Score(AIDecisionContext context) {
        return context.entity.health.Normalized;
    }

}