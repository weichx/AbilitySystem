using UnityEngine;
using Intelligence;

public class EvaluatorPage : Page<DecisionScoreEvaluator> {

	private MasterView<EvaluatorItem, DecisionScoreEvaluator> masterView;
	private EvaluatorPage_DetailView detailView;

	public override void OnEnter(string itemId = null) {
		masterView = new MasterView<EvaluatorItem, DecisionScoreEvaluator>(SetActiveItem);
		detailView = new EvaluatorPage_DetailView();
		GUIUtility.keyboardControl = 0;
        if (itemId != null) {
            masterView.SelectItemById(itemId);
        }
    }

	public override void Render(Rect rect) {
		GUILayout.BeginArea(rect);
		GUILayout.BeginHorizontal();
		masterView.Render();
		GUILayout.Space(10f);
		detailView.Render();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	public override void SetActiveItem(AssetItem<DecisionScoreEvaluator> newItem) {
		base.SetActiveItem(newItem);
		detailView.SetTargetObject(newItem);
	}

	public override void Update() {

		if (activeItem != null) {
			activeItem.Update();
			if (activeItem.IsDeletePending) {
				detailView.SetTargetObject(null);
				masterView.RemoveItem(activeItem as EvaluatorItem);
				activeItem.Delete();
			}
		}

	}

}
