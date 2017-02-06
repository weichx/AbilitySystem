using UnityEngine;
using UnityEditor;
using EntitySystem;
using System.IO;

public static class InventoryItemMenuItem {

    [MenuItem("Assets/InventoryItems")]
    public static InventoryItemCreator CreateScriptableObject() {
        InventoryItemCreator asset = ScriptableObject.CreateInstance<InventoryItemCreator>();
        string assetpath = "Assets/InventoryItems/InventoryItem.asset";
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        InventoryItem item = new InventoryItem();
        item.Id = Path.GetFileNameWithoutExtension(assetPathAndName);
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(item);
        asset.source = serializer.WriteToString();
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return asset;
    }
}
