using UnityEngine;

public class Decision {

    public string name;
    public AIAction action;
    public AIDecisionContextCreator contextCreator;
    public DecisionEvaluator evaluator;

    //this is the dse result, a dse is the combination of considerations + requirements and is
    //intrinsically encapsulated in this class
    public AIDecisionResult Score(Context context, AIDecisionEvaluatorLogEntry actionLog) {
        AIConsideration[] considerations = evaluator.considerations;
        AIRequirement[] requirements = evaluator.requirements;

        float modFactor = 1f - (1f / considerations.Length);
        float total = 1f;

        bool passedRequirements = true;

        if (requirements != null) {
            for (int i = 0; i < requirements.Length; i++) {
                var requirement = requirements[i];
                passedRequirements = requirement.Check(context);
                actionLog.RecordRequirement(requirement.name, passedRequirements);
                if (!passedRequirements) {
                    break;
                }
            }
        }

        if (passedRequirements) {
            //score and scale score according to total # of considerations
            for (int i = 0; i < considerations.Length; i++) {
                var consideration = considerations[i];
                var curve = consideration.curve;
                var input = considerations[i].Score(context);
                var score = curve.Evaluate(input);
                if (score == 0) {
                    total = 0;
                    actionLog.RecordConsideration(consideration, Mathf.Clamp01(input), score);
                    break;
                }
                float makeUpValue = (1 - score) * modFactor;
                float final = score + (makeUpValue * score);
                total *= final;
                actionLog.RecordConsideration(consideration, Mathf.Clamp01(input), score);
            }
        }

        AIDecisionResult result = new AIDecisionResult() {
            score = total,
            action = action,
            context = context
        };

        actionLog.RecordResult(total);
        return result;
    }

}