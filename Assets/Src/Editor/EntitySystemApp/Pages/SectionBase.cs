using UnityEditor;

public abstract class SectionBase<T> where T : EntitySystemBase {

	protected AssetItem<T> targetItem;
	protected SerializedObjectX rootProperty;
    protected float space;

    public SectionBase(float spacing) {
        space = spacing;
    }

	public virtual void SetTargetObject(AssetItem<T> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            rootProperty = null;
            return;
        }
	    rootProperty = targetItem.SerialObjectX;
    }

    public abstract void Render();

    public float Space {
        get { return space; }
    }
}
