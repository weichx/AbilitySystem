using Intelligence;

public class DecisionScoreEvaluatorCreator : AssetCreator, IAssetCreator<DecisionScoreEvaluator> {

	public DecisionScoreEvaluator Create() {
		if (deserializer == null) {
			deserializer = new AssetDeserializer(source, false);
		}
		return deserializer.CreateItem<DecisionScoreEvaluator>();
	}

	///<summary>
	///Creates an DSE instance from source. Slower than Create() 
	///but will behave properly when making changes the source
	///</summary>
	public DecisionScoreEvaluator CreateForEditor() {
		return new AssetDeserializer(source, false).CreateItem<DecisionScoreEvaluator>();
	}
}