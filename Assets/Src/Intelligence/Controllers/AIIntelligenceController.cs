using System.Collections.Generic;
using UnityEngine;

namespace Intelligence {

    public class ContextBuilder {

        public Context[] BuildContexts(Entity entity) {
            return null;
        }

    }

    public struct DecisionContextPair {

        public readonly Decision decision;
        public readonly Context context;

        public DecisionContextPair(Decision decision, Context context) {
            this.decision = decision;
            this.context = context;
        }

    }

    public class AIIntelligenceController : MonoBehaviour {

        protected List<Decision> decisions;
        protected List<DecisionContextPair> decisionContextPairs;
        protected Entity entity;

        public void Update() {
            PickDecision();
        }

        protected void UpdateDecisionPairs() {
            decisionContextPairs = new List<DecisionContextPair>(20);
            for (int i = 0; i < decisions.Count; i++) {
                Decision decision = decisions[i];
                Context[] contexts = decisions[i].contextBuilder.BuildContexts(entity); //todo caching this might be possible
                for (int j = 0; j < contexts.Length; j++) {
                    decisionContextPairs.Add(new DecisionContextPair(decision, contexts[j]));
                }
            }
        }

        protected void ScoreAllDecisions() {
            float cutoff = 0f;
            UpdateDecisionPairs();
            for (int i = 0; i < decisionContextPairs.Count; i++) {
                DecisionContextPair pair = decisionContextPairs[i];
                Decision decision = pair.decision;
                Context context = pair.context;
                float bonus = 0; //context.GetBonusFactor(lastContext); //I likely cant do bonus like this 
                if (bonus < cutoff) {
                    continue;
                }
                decision.score = decision.dse.Score(context, bonus, cutoff);
                if (decision.score > cutoff) {
                    cutoff = decision.score;
                }
            }
        }

        //todo this should be implemented by the intelligence definition
        protected Decision PickDecision() {
            /*return definition.GetDecision(decisions)*/
            //todo keeping these in a priority queue makes more sense
            decisionContextPairs.Sort();
            return decisionContextPairs[0].decision;
        }

    }

}
