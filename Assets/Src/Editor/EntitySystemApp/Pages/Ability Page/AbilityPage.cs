using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage : Page<Ability> {

    private MasterView<AbilityItem, Ability> masterView;
    private AbilityPage_DetailView detailView;

    public override void OnEnter() {
        masterView = new MasterView<AbilityItem, Ability>(SetActiveItem);
        detailView = new AbilityPage_DetailView();
        GUIUtility.keyboardControl = 0;
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

    public override void SetActiveItem(EntitySystemItem<Ability> newItem) {
        base.SetActiveItem(newItem);
        detailView.SetTargetObject(newItem);
    }

    public override void Update() {

        if(activeItem != null) {
            activeItem.Update();
            // if(activeItem.PendingDelete) {
            //    activeItem.Delete();
            //}
            detailView.SetTargetObject(null);
        }
        
    }
  
}
