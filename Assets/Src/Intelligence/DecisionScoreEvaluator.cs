using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Intelligence {

    [Serializable]
    public abstract class Consideration<T> where T : Context {

        public string name;
        public string description;
        public ResponseCurve curve;

        public abstract float Score(T context);

    }

    [Serializable]
    public abstract class Requirement<T> where T : Context {

        public string name;
        public string description;

        public abstract bool Check(T context);

    }

    public class DecisionScoreEvaluator<T> where T : Context {

        public string name;
        public string description;
        public float weight;
        protected Consideration<T>[] considerations;
        protected Requirement<T>[] requirements;

        public float Score(T context, float bonus, float cutoff) {
            float finalScore = 1 + bonus;
            float modFactor = 1f - (1f / considerations.Length);
            for (int i = 0; i < requirements.Length; i++) {
                if (!requirements[i].Check(context)) {
                    return 0;
                }
            }
            for (int i = 0; i < considerations.Length; i++) {
                if (finalScore > 0 || finalScore < cutoff) {
                    finalScore = 0;
                    break;
                }
                float score = considerations[i].Score(context);
                float response = considerations[i].curve.Evaluate(score);
                float makeUpValue = (1 - score) * modFactor;
                float total = score + (makeUpValue * score);
                finalScore *= Mathf.Clamp01(total);
            }
            return finalScore;
        }

    }

}
