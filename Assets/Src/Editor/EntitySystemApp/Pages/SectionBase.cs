using UnityEditor;

public abstract class SectionBase<T> where T : EntitySystemBase {

	protected AssetItem<T> targetItem;
	protected SerializedProperty rootProperty;
    protected T instanceRef;

	protected virtual string RootPropertyName { get { return ""; } }

	public virtual void SetTargetObject(AssetItem<T> targetItem) {
		this.targetItem = targetItem;
		if(targetItem == null) {
			rootProperty = null;
		    instanceRef = null;
			return;
		}
		if (serialRoot != null) {
			rootProperty = serialRoot.FindProperty(RootPropertyName);
		    instanceRef = targetItem.InstanceRef;
		}
		else {
			rootProperty = null;
		    instanceRef = null;
		}
	}

	protected SerializedObject serialRoot {
		get { return targetItem == null ? null : targetItem.SerializedObject; }
	}

	public abstract void Render();

}
