using UnityEditor;

public abstract class AbilityPage_SectionBase : SectionBase<Ability> {

    protected SerializedProperty abilityProperty;

    public override void SetTargetObject(AssetItem<Ability> targetItem) {
        this.targetItem = targetItem;
        if (targetItem == null) {
            rootProperty = null;
            instanceRef = null;
            return;
        }
        instanceRef = targetItem.InstanceRef;
    }

}
