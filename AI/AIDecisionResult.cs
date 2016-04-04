using System;

public struct AIDecisionResult : IComparable<AIDecisionResult> {
    public AIDecisionContext context;
    public AIAction action;
    public float score;

    public int CompareTo(AIDecisionResult other) {
        float otherScore = other.score;
        if (score < otherScore) return 1;
        if (score > otherScore) return -1;
        return 0;
    }
}