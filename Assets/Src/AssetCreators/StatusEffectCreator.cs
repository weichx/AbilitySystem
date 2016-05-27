
public class StatusEffectCreator : AssetCreator, IAssetCreator<StatusEffect> {

    public StatusEffect Create() {
        if(deserializer == null) {
            deserializer = new AssetDeserializer(source, false);
        }
        StatusEffect status = deserializer.CreateItem<StatusEffect>();
        // status.Creator = this; -- unsure how i feel about this
        return status;
    }

    public StatusEffect CreateForEditor() {
        return new AssetDeserializer(source, false).CreateItem<StatusEffect>();
    }
}