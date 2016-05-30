using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage : MasterDetailPage<Ability> {

    public AbilityPage() : base() {
        detailView = new AbilityPage_DetailView();
    }
    //private MasterView<AbilityItem, Ability> masterView;
    //private AbilityPage_DetailView detailView;

    //public override void OnEnter(string itemId = null) {
    //    masterView = new MasterView<AbilityItem, Ability>(SetActiveItem);
    //    detailView = new AbilityPage_DetailView();
    //    GUIUtility.keyboardControl = 0;
    //    if (itemId != null) {
    //        masterView.SelectItemById(itemId);
    //    }
    //}

    //public override void Render(Rect rect) {
    //    GUILayout.BeginArea(rect);
    //    GUILayout.BeginHorizontal();
    //    masterView.Render();
    //    GUILayout.Space(10f);
    //    detailView.Render();
    //    GUILayout.EndHorizontal();
    //    GUILayout.EndArea();
    //}

    //public override void SetActiveItem(AssetItem<Ability> newItem) {
    //    base.SetActiveItem(newItem);
    //    detailView.SetTargetObject(newItem);
    //}

    //public override void Update() {

    //    if (activeItem != null) {
    //        activeItem.Update();
    //        if (activeItem.IsDeletePending) {
    //            detailView.SetTargetObject(null);
    //            masterView.RemoveItem(activeItem as AbilityItem);
    //            activeItem.Delete();
    //        }
    //    }

    //}

}
