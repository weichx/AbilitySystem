using UnityEngine;

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

public abstract class Page<T> : Page where T : EntitySystemBase {

    protected AssetItem<T> activeItem;

    public AssetItem<T> ActiveItem {
        get { return activeItem; }
    }

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