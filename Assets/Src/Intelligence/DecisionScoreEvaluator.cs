using System;
using System.Collections.Generic;
using Intelligence;
using UnityEngine;


[Serializable]
public class DecisionScoreEvaluator : EntitySystemBase {

    public string name;
    public string description;
    public float weight;
    public List<Consideration> considerations;
    public List<Requirement> requirements;

    public DecisionScoreEvaluator() {
        considerations = new List<Consideration>();
        requirements = new List<Requirement>();
    }

    public float Score(Context context, float bonus, float cutoff) {
        float finalScore = 1 + bonus;
        float modFactor = 1f - (1f / considerations.Count);
        for (int i = 0; i < requirements.Count; i++) {
            if (!requirements[i].Check(context)) {
                return 0;
            }
        }
        for (int i = 0; i < considerations.Count; i++) {
            if (finalScore > 0 || finalScore < cutoff) {
                finalScore = 0;
                break;
            }
            float score = considerations[i].Score(context);
            float response = considerations[i].curve.Evaluate(score);
            float makeUpValue = (1 - response) * modFactor;
            float total = response + (makeUpValue * response);
            finalScore *= Mathf.Clamp01(total);
        }
        return finalScore;
    }

}

[Serializable]
public class NoOpDSE : DecisionScoreEvaluator {

    public NoOpDSE() : base() {
        name = " -- No Op --";
        description = "Does nothing";
        weight = 0;
    }


}

