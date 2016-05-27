using UnityEditor;

public abstract class SectionBase<T> where T : EntitySystemBase {

	protected AssetItem<T> targetItem;
	protected SerializedProperty rootProperty;

	protected abstract string RootPropertyName { get; }

	public virtual void SetTargetObject(AssetItem<T> targetItem) {
		this.targetItem = targetItem;
		if(targetItem == null) {
			rootProperty = null;
			return;
		}
		if (serialRoot != null) {
			rootProperty = serialRoot.FindProperty(RootPropertyName);
		}
		else {
			rootProperty = null;
		}
	}

	protected SerializedObject serialRoot {
		get { return targetItem == null ? null : targetItem.SerializedObject; }
	}

	public abstract void Render();

}
