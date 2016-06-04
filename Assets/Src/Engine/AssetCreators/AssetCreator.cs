using UnityEngine;

///<summary>Responsible for serializing / deserializing EntitySystemBase subclasses
/// Each subclass needs its own AssetCreator concrete non generic type so that
/// Unity can save them as ScriptableObjects. 
/// </summary>
public class AssetCreator<T> : ScriptableObject where T : EntitySystemBase, new() {

    [HideInInspector]
    public string source;

    [HideInInspector]
    public string typeName;
    protected AssetDeserializer deserializer;

    public virtual void SetSourceAsset(T asset) {
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(asset);
        source = serializer.WriteToString();
        typeName = asset.GetType().AssemblyQualifiedName;
    }

    ///<summary>
    ///Creates a T instance from source. 
    ///</summary>
    public T Create() {
#if UNITY_EDITOR
        return new AssetDeserializer(source, false).CreateItem<T>();
#else
        if (deserializer == null) {
            deserializer = new AssetDeserializer(source, false);
        }
        return deserializer.CreateItem<T>();
#endif
    }
}