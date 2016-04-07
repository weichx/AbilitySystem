using System;
using AbilitySystem;
using UnityEngine;
using System.Collections.Generic;

public class AIAction_CloseToMeleeRange : AIAction {

    private NavMeshAgent agent;
    public float arrivalDistance = 3f;

    public override void OnInitialize() {
        agent = entity.GetComponent<NavMeshAgent>();
    }

    public override void OnStart() {
        agent.SetDestination(context.target.transform.position);
        agent.stoppingDistance = arrivalDistance;
        Debug.Log("Going");
    }

    public override ActionStatus OnUpdate() {
        if(agent.remainingDistance <= arrivalDistance) {
            return ActionStatus.Success;
        }
        else {
            return ActionStatus.Running;
        }
    }

    public override void OnEnd() {
        agent.ResetPath();
    }

    public override AIDecisionContext[] GetContexts() {
        return AIDecisionContext.CreateFromEntityHostileList(entity, 250);
    }

}