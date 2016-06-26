using UnityEngine;
using UnityEditor;
using System.IO;

public static class ItemMenuItem {

    [MenuItem("Assets/Items")]
    public static ItemCreator CreateScriptableObject() {
        ItemCreator asset = ScriptableObject.CreateInstance<ItemCreator>();
        string assetpath = "Assets/Items/Item.asset";
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        Item item = new Item();
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
