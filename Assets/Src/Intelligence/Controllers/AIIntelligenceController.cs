using System.Collections.Generic;
using UnityEngine;

namespace Intelligence {
	
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
				//Decision decision = decisions[i];
//				Context[] contexts = decisions[i].contextFactory.CreateContexts(entity); //todo caching this might be possible
//                for (int j = 0; j < contexts.Length; j++) {
//                    decisionContextPairs.Add(new DecisionContextPair(decision, contexts[j]));
//                }
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
				float score = decision.evaluator.Score(context, bonus, cutoff);
                if (score > cutoff) {
                    cutoff = score;
                }
				//todo save score, decision, context
            }
        }

        //todo this should be implemented by the intelligence definition
		protected void PickDecision() {
            /*return definition.GetDecision(decisions)*/
            //todo keeping these in a priority queue based on score makes more sense
            decisionContextPairs.Sort();
			//Decision d = decisionContextPairs[0].decision;
			//Context c = decisionContextPairs[0].context;
			//d.Run(c);
            //return decisionContextPairs[0].decision;
        }

    }

}
