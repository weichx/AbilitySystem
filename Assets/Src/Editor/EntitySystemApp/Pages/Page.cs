using UnityEngine;
using EntitySystem;

public interface IRectDrawable {
    void Render(Rect rect);
    float GetHeight();
}

public abstract class Page {
    public virtual void OnEnter(string itemId = null) { }
    public virtual void Update() { }
    public virtual void OnExit() { }
    public abstract void Render(Rect rect);
    public abstract string GetActiveItemId();
}

public abstract class Page<T> : Page where T : EntitySystemBase, new() {

    protected AssetItem<T> activeItem;

    public override string GetActiveItemId() {
        if (activeItem == null) return "";
        return activeItem.Name;
    }

    public virtual void SetActiveItem(AssetItem<T> newItem) {
        if (activeItem != null) {
            activeItem.IsSelected = false;
        }
        activeItem = newItem;
        if (activeItem != null) {
            activeItem.IsSelected = true;
            activeItem.Load();
        }
        GUIUtility.keyboardControl = 0;
    }

}

public abstract class MasterDetailPage<T> : Page<T> where T : EntitySystemBase, new() {


    protected MasterView<T> masterView;
    protected DetailView<T> detailView;

    public MasterDetailPage() {
        masterView = new MasterView<T>(SetActiveItem);
        detailView = new DetailView<T>();
    }

    public override void OnEnter(string itemId = null) {
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

    public override void SetActiveItem(AssetItem<T> newItem) {
        base.SetActiveItem(newItem);
        detailView.SetTargetObject(newItem);
    }

    public override void Update() {
        if (activeItem != null) {
            activeItem.Update();
            if (activeItem.IsDeletePending) {
                detailView.SetTargetObject(null);
                masterView.RemoveItem(activeItem);
                activeItem.Delete();
            }
        }
    }

}