using UnityEditor;
using Intelligence;

public abstract class EvaluatorPage_SectionBase : SectionBase<DecisionScoreEvaluator> {

    public override void SetTargetObject(AssetItem<DecisionScoreEvaluator> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            rootProperty = null;
            instanceRef = null;
            return;
        }
        instanceRef = targetItem.InstanceRef;
    }
}
