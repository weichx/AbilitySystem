using UnityEngine;
using UnityEditor;
using EntitySystem;
using System.IO;

public static class CharacterMenuItem {

    [MenuItem("Assets/InventoryItems")]
    public static CharacterCreator CreateScriptableObject() {
        CharacterCreator  asset = ScriptableObject.CreateInstance<CharacterCreator>();
        string assetpath = "Assets/Characters/char.asset";
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        Character character = new Character();
        character.Id = Path.GetFileNameWithoutExtension(assetPathAndName);
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(character);
        asset.source = serializer.WriteToString();
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return asset;
    }
}
