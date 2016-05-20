
public class AbilityCreator : AssetCreator, IAssetCreator<Ability> {

    public Ability Create() {
        if (deserializer == null) {
            deserializer = new AssetDeserializer(source, false);
        }
        return deserializer.CreateItem<Ability>();
    }

    ///<summary>
    ///Creates an Ability instance from source. Slower than Create() 
    ///but will behave properly when making changes the source
    ///</summary>
    public Ability CreateForEditor() {
        return new AssetDeserializer(source, false).CreateItem<Ability>();
    }
}