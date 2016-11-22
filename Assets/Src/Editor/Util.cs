using UnityEngine;
using UnityEditor;

public static class AssetDatabaseExtensions {

    public static T FindAsset<T>(string assetName) where T : Object {
        string[] guids = AssetDatabase.FindAssets(assetName);
        if (guids == null || guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

}