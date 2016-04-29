using System;
using UnityEngine;

public class AIAction_PlayAnimation : AIAction {

    public string animationName;

    public override void OnStart() {
        //int animationId = AnimationManager.GetId(animationName);
        int animationId = 200;
        Animator animator = entity.GetComponent<Animator>();
        Debug.Assert(animator != null, "AIAction_PlayAnimation requires an animator, " + entity.name + " doesnt have one");
        animator.SetInteger("PhaseId", animationId);
    }

    public override ActionStatus OnUpdate() {
       var t = entity.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
       if(t.normalizedTime >= 0.98) {
            return ActionStatus.Success;
       }
        return ActionStatus.Running;
    }

    public override void OnEnd() {
        Animator animator = entity.GetComponent<Animator>();
        Debug.Assert(animator != null, "AIAction_PlayAnimation requires an animator, " + entity.name + " doesnt have one");
        animator.SetInteger("PhaseId", 0);
    }

}