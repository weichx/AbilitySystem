using UnityEngine;
using Intelligence;

public class EvaluatorPage : MasterDetailPage<DecisionScoreEvaluator> {

    public EvaluatorPage() : base() {
        detailView = new EvaluatorPage_DetailView();
    }

}
