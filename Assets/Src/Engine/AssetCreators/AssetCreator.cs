using UnityEngine;

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