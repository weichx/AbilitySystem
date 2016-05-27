using UnityEditor;

public abstract class StatusPage_SectionBase {

    protected AssetItem<StatusEffect> targetItem;
    protected SerializedProperty statusProperty;

    public virtual void SetTargetObject(AssetItem<StatusEffect> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            statusProperty = null;
            return;
        }
        if (serialRoot != null) {
            statusProperty = serialRoot.FindProperty("statusEffect");
        }
        else {
            statusProperty = null;
        }
    }

    protected SerializedObject serialRoot {
        get { return targetItem == null ? null : targetItem.SerializedObject; }
    }


    public abstract void Render();

}
