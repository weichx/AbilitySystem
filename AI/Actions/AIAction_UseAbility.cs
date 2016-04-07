using UnityEngine;

public class AIAction_UseAbility : AIAction {

    public string abilityId;
    public float range;

    public override void OnStart() {
        entity.GetComponent<NavMeshAgent>().ResetPath();
        entity.Target = context.target;
        entity.abilityManager.Cast(abilityId);
        var animator = entity.GetComponent<Animator>();
        animator.SetInteger("PhaseId", 100);
    }

    public override ActionStatus OnUpdate() {
        if (entity.abilityManager.IsCasting) {
            return ActionStatus.Running;
        }
        return ActionStatus.Success;
    }

    public override AIDecisionContext[] GetContexts() {
        return AIDecisionContext.CreateFromEntityHostileList(entity, 200);
    }

}