using System;

[Serializable]
public class AIRequirementLogEntry {
    public string name;
    public bool passed;

    public AIRequirementLogEntry(string name, bool passed) {
        this.name = name;
        this.passed = passed;
    }
}
