using UnityEditor;

public class SkillSetPage_SectionBase {

    protected AssetItem<SkillSet> targetItem;
    protected SerializedProperty skillSetProperty;

    protected SkillSet skillSet {
        get {
            return targetItem.InstanceRef;
        }
    }

    public virtual void SetTargetObject(AssetItem<SkillSet> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            skillSetProperty = null;
            return;
        }
        if (serialRoot != null) {
            skillSetProperty = serialRoot.FindProperty("skillSet");
        }
        else {
            skillSetProperty = null;
        }
    }

    protected SerializedObject serialRoot {
        get { return targetItem == null ? null : targetItem.SerializedObject; }
    }

}