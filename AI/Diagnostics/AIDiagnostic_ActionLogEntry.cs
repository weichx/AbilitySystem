using System;
using System.Collections.Generic;

[Serializable]
public class AIActionLogEntry {
    public string name;
    public string context;
    public float score;
    public float weight;
    public float time;
    public List<AIConsiderationLogEntry> considerations;
    public List<AIRequirementLogEntry> requirements;

    private Timer timer;

    public AIActionLogEntry(AIAction action, AIDecisionContext context) {
        timer = new Timer();
        name = action.name;
        this.context = context.ToString();
        considerations = new List<AIConsiderationLogEntry>();
        requirements = new List<AIRequirementLogEntry>();
    }

    public void RecordRequirement(string name, bool passed) {
        requirements.Add(new AIRequirementLogEntry(name, passed));
    }

    public void RecordConsideration(AIConsideration consideration, float input, float output) {
        considerations.Add(new AIConsiderationLogEntry(consideration, input, output));
    }

    public void RecordResult(float finalScore) {
        time = timer.ElapsedTime;
        score = finalScore;
    }
}