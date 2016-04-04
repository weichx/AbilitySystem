
public abstract class AIRequirement {
    public string name;
    public abstract bool Check(AIDecisionContext context);
}