using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

namespace Intelligence {

    [RequireComponent(typeof(Entity))]
    public class AIIntelligenceController : MonoBehaviour {

        public DecisionPackageCreator packageCreator;
        public DecisionSelection decisionSelectionProfile;
        protected DecisionPackage package;
        protected Entity entity;
        protected List<Decision> decisions;
        protected List<DecisionContextPair> decisionContextPairs;
        protected CharacterAction currentAction;
        protected FastPriorityQueue<ScoreNode> scoreQueue;

        public void Awake() {

            entity = GetComponent<Entity>();
            decisions = new List<Decision>();
            scoreQueue = new FastPriorityQueue<ScoreNode>(1000);

            if (packageCreator != null) {
                package = packageCreator.Create();
                decisions.AddRange(package.decisions);
            }

        }

        public void Update() {
            currentAction = currentAction ?? GetDesiredAction();
            if (currentAction != null) {
                if (currentAction.OnUpdate()) {
                    currentAction.OnComplete();
                    currentAction = null;
                }
            }
        }

        protected CharacterAction GetDesiredAction() {
            DecisionContextPair dcp = ScoreAllDecisions();
            if (dcp.decision == null) {
                return null;
            }
            CharacterAction action = dcp.decision.action;
            action.Setup(dcp.context);
            return action;
        }

        //todo implement bonus: likely this is something like a momentum factor
        protected DecisionContextPair ScoreAllDecisions() {
            float cutoff = 0f;
            scoreQueue.Clear();
            if(decisions.Count == 0) return default(DecisionContextPair);
            for (int i = 0; i < decisions.Count; i++) {
                Decision decision = decisions[i];
                List<Context> contexts = decisions[i].contextCollector.Collect(decisions[i].action, entity);

                for (int j = 0; j < contexts.Count; j++) {
                    Context context = contexts[j];

                    float bonus = 0; //context.GetBonusFactor(lastContext); //I likely cant do bonus like this 
                    //if (bonus < cutoff) {
                    //    continue;
                    //}
                    float score = decision.evaluator.Score(context, bonus, cutoff);
                    ScoreNode node = new ScoreNode(new DecisionContextPair(decision, context));
                    scoreQueue.Enqueue(node, 1 - score); //queue uses a min heap so 1 - score is correct
                    if (score > cutoff) {
                        cutoff = score;
                    }

                }

            }

            switch (decisionSelectionProfile) {
                case DecisionSelection.AlwaysBest:
                    return scoreQueue.First.item;
                default:
                    return scoreQueue.First.item;
            }

        }

        //todo handle pooling these
        protected class ScoreNode : FastPriorityQueueNode {
            public DecisionContextPair item;

            public ScoreNode(DecisionContextPair item) {
                this.item = item;
            }
        }

        public enum DecisionSelection {
            AlwaysBest
        }
    }



}
