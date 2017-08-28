using  UnityEngine;

namespace Intelligence.Actions {


    public class MoveToMeleeRange : CharacterAction<SingleTargetContext> {

        public bool autobreak = true;
        private UnityEngine.AI.NavMeshAgent agent;
        private Timer timer;
        
        public override void OnStart() {
            timer = new Timer(0.5f);
            //Vector3 location = context.entity.transform;
            agent = context.entity.GetComponent<UnityEngine.AI.NavMeshAgent>();
            agent.autoBraking = autobreak;
            //agent.SetDestination(location);

        }

        public override CharacterActionStatus OnUpdate() {

            if (timer.ReadyWithReset(0.5f)) {

                float entityRadius = context.entity.GetComponent<CapsuleCollider>().radius;
                float targetRadius = context.target.GetComponent<CapsuleCollider>().radius;
                float desiredOffset = entityRadius + targetRadius;
                Vector3 toTarget = Vector3.Normalize(context.entity.transform.position - context.target.transform.position);
                Vector3 desiredLocation = context.target.transform.position + (toTarget * 2);
                agent.SetDestination(desiredLocation);

            }
            return CharacterActionStatus.Running;
            //return agent.hasPath && agent.remainingDistance <= 1f ?
            //    CharacterActionStatus.Completed : CharacterActionStatus.Running;
        }

    }

}
