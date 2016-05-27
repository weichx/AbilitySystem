using UnityEditor;

public abstract class AbilityPage_SectionBase {

    protected AssetItem<Ability> targetItem;
    protected SerializedProperty abilityProperty;

    public virtual void SetTargetObject(AssetItem<Ability> targetItem) {
        this.targetItem = targetItem;
        if(targetItem == null) {
            abilityProperty = null;
            return;
        }
        if (serialRoot != null) {
            abilityProperty = serialRoot.FindProperty("ability");
        }
        else {
            abilityProperty = null;
        }
    }

    protected SerializedObject serialRoot {
        get { return targetItem == null ? null : targetItem.SerializedObject; }
    }

    public abstract void Render();

}
