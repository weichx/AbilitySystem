using UnityEngine;

public class AIAction_UseAbility : AIAction {

    public string abilityId;
    public float range;
    private Timer timer;

    public override void OnStart() {
        timer = new Timer(1f);
    }

    public override ActionStatus OnUpdate() {
        Debug.Log("Using ability [" + abilityId + "]");
        if (timer.Ready) {
            return ActionStatus.Success;
        }
        return ActionStatus.Running;
    }

    public override void OnSuccess() {
        Debug.Log("finished using ability [" + abilityId + "]");
    }

    public override AIDecisionContext[] GetContexts() {
        return AIDecisionContext.CreateFromEntityHostileList(entity, range);
    }
}