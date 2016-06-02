using System;
using Intelligence;
using UnityEngine;

public class DecisionEvaluatorCreator : AssetCreator<DecisionEvaluator> {
    [HideInInspector]
    public string contextTypeName;

    public override void SetSourceAsset(DecisionEvaluator asset) {
        base.SetSourceAsset(asset);
        contextTypeName = asset.contextType.AssemblyQualifiedName;
    }

    public Type GetContextType() {
        return Type.GetType(contextTypeName);
    }
}