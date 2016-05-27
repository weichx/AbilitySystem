using UnityEngine;

public interface IRectDrawable {
    void Render(Rect rect);
    float GetHeight();
}

public abstract class Page {
    public virtual void OnEnter() { }
    public virtual void Update() { }
    public virtual void OnExit() { }
    public abstract void Render(Rect rect);
}

public abstract class Page<T> : Page where T : EntitySystemBase {

    protected AssetItem<T> activeItem;

    public AssetItem<T> ActiveItem {
        get { return activeItem; }
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