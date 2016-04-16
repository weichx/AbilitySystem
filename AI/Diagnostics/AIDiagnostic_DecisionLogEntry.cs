using System;
using System.Collections.Generic;

[Serializable]
public class AIDecisionLogEntry {
    public string deciscionName;
    public float score;
    public float timestamp;
    public List<AIDecisionEvaluatorLogEntry> decisions;

    public AIDecisionLogEntry() {
        decisions = new List<AIDecisionEvaluatorLogEntry>();
        timestamp = new Timer().Timestamp;
    }

    public AIDecisionEvaluatorLogEntry AddDecision(Decision decision, Context context) {
        AIDecisionEvaluatorLogEntry entry = new AIDecisionEvaluatorLogEntry(decision, context);
        decisions.Add(entry);
        return entry;
    }

    public void SetSelectedAction(AIDecisionResult result) {
        deciscionName = result.action.name;
        score = result.score;
    }
}



