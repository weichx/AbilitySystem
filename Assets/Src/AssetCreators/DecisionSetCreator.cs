
public class DecisionSetCreator : AssetCreator, IAssetCreator<DecisionSet> {
    
    public DecisionSet Create() {
        if (deserializer == null) {
            deserializer = new AssetDeserializer(source, false);
        }
        return deserializer.CreateItem<DecisionSet>();
    }

    public DecisionSet CreateForEditor() {
        return new AssetDeserializer(source, false).CreateItem<DecisionSet>();
    }

}