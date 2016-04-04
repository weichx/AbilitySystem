
public class AIConsideration_MyTargetHealth : AIConsideration {

    public override float Score(AIDecisionContext context) {
        return context.target.health.Normalized;
    }

}