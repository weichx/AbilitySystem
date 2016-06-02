
public abstract class SectionBase<T> where T : EntitySystemBase, new() {

	protected AssetItem<T> targetItem;
	protected SerializedPropertyX rootProperty;
    protected float space;

    public SectionBase(float spacing = 0f) {
        space = spacing;
    }

	public void SetTargetObject(AssetItem<T> targetItem) {
        this.targetItem = targetItem;
	    if (targetItem != null) {
	        SetTargetProperty(targetItem.SerialObjectX.Root);
	    } else {
            SetTargetProperty(null);
        }
    }

    public virtual void SetTargetProperty(SerializedPropertyX rootProperty) {
        this.rootProperty = rootProperty;
    }

    public abstract void Render();

    public float Space {
        get { return space; }
    }
}
