using UnityEngine;
using System.Collections.Generic;
using AbilitySystem;
using System;

/*Infinite Axis Utility System is all about DECISION MAKING not ACTION TAKING*/
//this means I'll need to handle actions seperately from deciding when to do what
//this also means that we throw away the concept of 'planning' and just 'do whats best' 
//but planning can be done at a high level for group coordination, IE there can be a 
//high level planner (also controlled by utility most likely) that can issue orders
//to individuals. the individual agents can optionally have a goal
//(strictly defined as an action type, probably with considerations). Also, actions
//should be stateless 

//actions should all be done in isolation, long running actions should be given a bonus 
//score but still get re-evaluated. things like scripted actions can be either checked for
//or given very high weights


public class AIActionEvaluator {

    // todo divide into 'packages' that can be dynamically added / removed

    private Entity entity;
    private AIAction[] actions;
    private AIAction currentAction;
    public Timer nextDecisionTimer;
    private List<AIDecisionResult> actionResults;

    public AIDecisionLog decisionLog;

    public AIActionEvaluator(Entity entity, AIAction[] actions) {
        this.entity = entity;
        AddActionPackage(actions);
        nextDecisionTimer = new Timer(0.5f);
        actionResults = new List<AIDecisionResult>();
        decisionLog = new AIDecisionLog(entity.name);
    }

    //todo this should really be json so we dont re-use action instances across AI Entities
    public void AddActionPackage(AIAction[] actionPackage) {
        actions = actionPackage ?? new AIAction[0];
        for (int i = 0; i < actions.Length; i++) {
            actions[i].entity = entity;
        }
    }

    public void Update() {
        if (actions.Length == 0) return;
        if (currentAction == null || nextDecisionTimer.ReadyWithReset()) {
            SelectAction();
            return;
        }

        ActionStatus status = currentAction.OnUpdate();

        if (status == ActionStatus.Success) {
            currentAction.OnSuccess();
            currentAction.OnEnd();
            currentAction = null;
        }
        else if (status == ActionStatus.Failure) {
            currentAction.OnFailure();
            currentAction.OnEnd();
            currentAction = null;
        }
    }

    public void Unload() {
        if (currentAction != null) {
            currentAction.OnFailure();
            currentAction.OnEnd();
        }
        currentAction = null;
        actions = null;
    }

    //public AIDiagnostic_Action[] GetDiagnostics() {
    //    return null;
    //}

    public List<AIDecisionResult> GetLastDecisionResults() {
        return null;
    }

    //public AIDiagnostic_Action[] GetLastDecisionDiagnostic() {

    //}

    private void SelectAction() {
        if (actions == null) return;
        actionResults.Clear();

        AIDecisionLogEntry diagLog = decisionLog.AddEntry();

        for (int i = 0; i < actions.Length; i++) {
            var action = actions[i];
            var contexts = action.GetContexts();
            for (int j = 0; j < contexts.Length; j++) {
                AIDecisionContext context = contexts[j];
                AIActionLogEntry logEntry = diagLog.AddActionEntry(action, context);
                AIDecisionResult result = action.Score(context, logEntry);
                actionResults.Add(result);
            }
            //todo weight each result
        }

        if (actionResults.Count == 0) {
            return;
        }

        actionResults.Sort();
        AIDecisionResult best = actionResults[0];
        diagLog.SetSelectedAction(best);

        if (currentAction != null && best.action != currentAction) {
            currentAction.OnInterrupt();
            currentAction.OnEnd();
        }

        currentAction = best.action;
        currentAction.Execute(best.context);
    }

    public string GetCurrentActionName() {
        if (currentAction == null) return "No Current Action";
        return currentAction.GetType().Name;
    }
}

/*
Action spawns 1 decision per context evaluated
    each decision runs its considerations to get a final score
        considerations take an input which is parameterized
    decision result is added to list of executable actions

Action can be executed if selected

*/


/*
Actions are pre-created and generally shared between ai agents
Action considerations are added per ai-agent(can be shared in stuff like templates)
Actions should maybe have a base weight that is 1 - (their index / total actions considered) where no considerations are added

{
    "Actions": [
        {
            "Name": "Use Skill [Frostbolt]",
            "typeName": "",
            "AbilityId": "FrostBolt",
            "Considerations": [
                {
                    "name": "Not when I'm too close",
                    "typeName": "AIConsideration_DistanceToTarget"
                    "Range": [0, 1],
                    "Min": 0.2, 
                    "Tags": ["Tag"],
                    "Curve": {
                        "Type": "Linear", 
                        "M":0, 
                        "K":0, 
                        "B":0, 
                        "C":0
                    },
                },
                {
                    "Name": "Not for [n] seconds after [action]",
                    "Input": "AIInput_DelayAfterAction",
                    "Parameters": { "ActionName": "SomeAction", "Time": 10|Formula? }
                }
            ]
        }
    ]
}
*/
