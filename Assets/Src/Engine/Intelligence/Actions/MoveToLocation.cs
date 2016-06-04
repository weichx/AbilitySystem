using  UnityEngine;

namespace Intelligence {

    public class MoveToLocation : CharacterAction<PointContext> {

        public bool autobreak = true;
        private NavMeshAgent agent;

        public override void OnStart() {
            Vector3 location = context.point;
            agent = context.entity.GetComponent<NavMeshAgent>();
            agent.autoBraking = autobreak;
            agent.SetDestination(location);
        }

        public override bool OnUpdate() {
            return agent.hasPath && agent.remainingDistance <= 1f;
        }

        public override void OnComplete() {
            if (entity.vectors.Contains(context.point)) return;
            entity.vectors.Add(context.point);
        }


    }

}