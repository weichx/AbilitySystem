using UnityEngine;

public class DecisionSetPage : Page<DecisionSet> {

    //private MasterView<DecisionSetItem, DecisionSet> masterView;
    //private DecisionSetPage_DetailView detailView;

    //public override void OnEnter(string itemId = null) {
    //    masterView = new MasterView<DecisionSetItem, DecisionSet>(SetActiveItem);
    //    detailView = new DecisionSetPage_DetailView();
    //    if (!string.IsNullOrEmpty(itemId)) {
    //        masterView.SelectItemById(itemId);
    //    }
    //}

    //public override void SetActiveItem(AssetItem<DecisionSet> newItem) {
    //    base.SetActiveItem(newItem);
    //    detailView.SetTargetObject(newItem);
    //}

    

    public override void Render(Rect rect) {
        //GUILayout.BeginArea(rect);
        //GUILayout.BeginHorizontal();
        //masterView.Render();
        //GUILayout.Space(10f);
        //detailView.Render();
        //GUILayout.EndHorizontal();
        //GUILayout.EndArea();
    }

}