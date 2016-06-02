using UnityEngine;
using Intelligence;

public class EvaluatorPage : MasterDetailPage<DecisionEvaluator> {

    public EvaluatorPage() : base() {
        detailView = new EvaluatorPage_DetailView();
    }

}
