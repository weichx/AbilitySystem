
public class EvaluatorPage_DetailView : DetailView<DecisionScoreEvaluator> {

	public EvaluatorPage_DetailView() : base() {
		sections.Add(new EvaluatorPage_NameSection(10f)); 
        sections.Add(new EvaluatorPage_RequirementSection(10f));
        sections.Add(new EvaluatorPage_ConsiderationSection2(10f));
    }

}