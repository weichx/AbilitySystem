using System;
using System.Collections.Generic;

[Serializable]
public class AIDecisionLogEntry {
    public string actionName;
    public float score;
    public float timestamp;
    public List<AIActionLogEntry> actions;

    public AIDecisionLogEntry() {
        actions = new List<AIActionLogEntry>();
        timestamp = new Timer().Timestamp;
    }

    public AIActionLogEntry AddActionEntry(AIAction action, AIDecisionContext context) {
        AIActionLogEntry entry = new AIActionLogEntry(action, context);
        actions.Add(entry);
        return entry;
    }

    public void SetSelectedAction(AIDecisionResult result) {
        actionName = result.action.name;
        score = result.score;
    }
}



