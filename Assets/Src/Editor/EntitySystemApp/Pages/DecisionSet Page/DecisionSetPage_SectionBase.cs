using UnityEditor;

public class DecisionSetPage_SectionBase {

    protected AssetItem<DecisionSet> targetItem;
    protected SerializedProperty decisionSetProperty;

    protected DecisionSet decisionSet {
        get {
            return targetItem.InstanceRef;
        }
    }

    public virtual void SetTargetObject(AssetItem<DecisionSet> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            decisionSetProperty = null;
            return;
        }
        if (serialRoot != null) {
            decisionSetProperty = serialRoot.FindProperty("decisionSet");
        }
        else {
            decisionSetProperty = null;
        }
    }

    protected SerializedObject serialRoot {
        get { return targetItem == null ? null : targetItem.SerializedObject; }
    }

}