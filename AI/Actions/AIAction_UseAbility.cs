using AbilitySystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AIAction_UseAbility : AIAction {

    public string abilityId;

    public override void OnStart() {
        entity.abilityManager.Cast(abilityId, context);
    }

    public override ActionStatus OnUpdate() {
        if (entity.abilityManager.IsCasting) {
            return ActionStatus.Running;
        }
        return ActionStatus.Success;
    }

}
