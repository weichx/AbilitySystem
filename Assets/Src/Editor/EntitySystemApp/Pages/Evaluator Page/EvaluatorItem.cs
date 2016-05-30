using Intelligence;

public class EvaluatorItem : AssetItem<DecisionScoreEvaluator> {

	public EvaluatorItem(AssetCreator creator) : base(creator) { }

    public override void Load() {
        if (instanceRef == null) {
            instanceRef = (creator as IAssetCreator<DecisionScoreEvaluator>).CreateForEditor();
        }
        if (serialRootObjectX == null) {
            serialRootObjectX = new SerializedObjectX(instanceRef);
        }
    }

    public override void Rebuild() {
        
    }

}