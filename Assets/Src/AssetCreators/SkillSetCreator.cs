
public class SkillSetCreator : AssetCreator, IAssetCreator<SkillSet> {
    
    public SkillSet Create() {
        if (deserializer == null) {
            deserializer = new AssetDeserializer(source, false);
        }
        return deserializer.CreateItem<SkillSet>();
    }

    public SkillSet CreateForEditor() {
        return new AssetDeserializer(source, false).CreateItem<SkillSet>();
    }

}