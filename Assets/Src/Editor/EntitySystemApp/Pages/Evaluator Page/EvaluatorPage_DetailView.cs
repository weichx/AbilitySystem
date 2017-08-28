using Intelligence;

public class EvaluatorPage_DetailView : DetailView<DecisionEvaluator> {

	public EvaluatorPage_DetailView() : base() {
		sections.Add(new EvaluatorPage_NameSection(10f)); 
        sections.Add(new EvaluatorPage_GeneralSection(10f));
        sections.Add(new EvaluatorPage_RequirementSection(10f));
        sections.Add(new EvaluatorPage_ConsiderationSection(10f));
    }

}