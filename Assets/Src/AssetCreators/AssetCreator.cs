using UnityEngine;

public class AssetCreator : ScriptableObject {

    [HideInInspector]
    public string source;
    protected AssetDeserializer deserializer;

}