using UnityEngine;

namespace Intelligence {

    public class Idle : CharacterAction {

        private float timer;
        public float idleTime;

        public override void OnStart() {
            UnityEngine.AI.NavMeshAgent agent = entity.GetComponent<UnityEngine.AI.NavMeshAgent>();
            agent.ResetPath();
        }

        public override CharacterActionStatus OnUpdate() {
            timer += Time.deltaTime;
            return timer > idleTime ? CharacterActionStatus.Completed : CharacterActionStatus.Running;
        }

    }
}