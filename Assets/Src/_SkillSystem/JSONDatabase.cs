using System;
using System.Collections.Generic;
using UnityEngine;

//todo find a better interface restriction like IAssetLoadable
public class JSONDatabase<T> where T : class {

    private static JSONDatabase<T> instance;

    private Dictionary<string, string> assetMap;

    public JSONDatabase(string loadPath) {
        assetMap = new Dictionary<string, string>();
        UnityEngine.Object[] assets = Resources.LoadAll(loadPath);
        for (int i = 0; i < assets.Length; i++) {
            TextAsset asset = assets[i] as TextAsset;
            if (asset.GetType() == typeof(TextAsset)) {
                Add(asset.name, asset.text);
            }
        }
    }

    public T Create(string assetId) {
        var json = assetMap.Get(assetId);
        if (json == null) throw new JSONAssetMissingException(assetId);
        return MiniJSON.Json.Deserialize<T>(json);
    }

    public void Add(string assetId, string json) {
        if (assetMap.ContainsKey(assetId)) {
            throw new DuplicateAssetException(assetId);
        }
        assetMap[assetId] = json;
    }

}

class JSONAssetMissingException : Exception {
    public JSONAssetMissingException(string message) : base(message) { }
}

class DuplicateAssetException : Exception {

    public DuplicateAssetException(string message) : base(message) { }

}