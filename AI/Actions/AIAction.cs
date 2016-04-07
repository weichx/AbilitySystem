using AbilitySystem;
using System;
using UnityEngine;

public enum ActionStatus {
    Running, Failure, Success
}

[Serializable]
public abstract class AIAction {

    public string name;
    public Entity entity;
    public AIConsideration[] considerations;
    public AIRequirement[] requirements;

    protected AIDecisionContext context;
    private int nonBooleanConsiderationCount;

    private bool isInitialized = false;

    public void Initialize(Entity entity) {
        this.entity = entity;
    }

    public void Execute(AIDecisionContext context) {
        this.context = context;
        if (!isInitialized) {
            isInitialized = true;
            OnInitialize();
        }
        OnStart();
    }

    public virtual ActionStatus OnUpdate() {
        return ActionStatus.Success;
    }

    public virtual void OnInitialize() { }
    public virtual void OnStart() { }
    public virtual void OnSuccess() { }
    public virtual void OnInterrupt() { }
    public virtual void OnFailure() { }
    public virtual void OnEnd() { }

    public abstract AIDecisionContext[] GetContexts();

    //public AIDiagnostic_Action GetDiagnostics() {
    //    return null; 
    //}

    //public AIDiagnostic_Decision GetLastDecisionDiagnostic() {
    //    return null;
    //}

    public void AddConsideration(AIConsideration consideration) {
        Array.Resize(ref considerations, considerations.Length + 1);
        considerations[considerations.Length - 1] = consideration;
    }

    public void AddRequirement(AIRequirement requirement) {
        Array.Resize(ref requirements, requirements.Length + 1);
        requirements[requirements.Length - 1] = requirement;
    }

    public AIDecisionResult Score(AIDecisionContext context, AIActionLogEntry actionLog) {
        float modFactor = 1f - (1f / considerations.Length);
        float total = 1f;

        bool passedRequirements = true;

        if (requirements != null) {
            for (int i = 0; i < requirements.Length; i++) {
                var requirement = requirements[i];
                passedRequirements = requirement.Check(context);
                actionLog.RecordRequirement(requirement.name, passedRequirements);
                if (!passedRequirements) {
                    break;
                }
            }
        }

        if (passedRequirements) {
            //score and scale score according to total # of considerations
            for (int i = 0; i < considerations.Length; i++) {
                var consideration = considerations[i];
                var curve = consideration.curve;
                var input = considerations[i].Score(context);
                var score = curve.Evaluate(input);
                if (score == 0) {
                    total = 0;
                    actionLog.RecordConsideration(consideration, Mathf.Clamp01(input), score);
                    break;
                }
                float makeUpValue = (1 - score) * modFactor;
                float final = score + (makeUpValue * score);
                total *= final;
                actionLog.RecordConsideration(consideration, Mathf.Clamp01(input), score);
            }
        }

        AIDecisionResult result = new AIDecisionResult() {
            score = total,
            action = this,
            context = context
        };

        actionLog.RecordResult(total);
        return result;
    }
}



