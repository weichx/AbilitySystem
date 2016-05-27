using UnityEngine;
using UnityEditor;
using System.IO;

public static class AbilityMenuItem {

    [MenuItem("Assets/Ability")]
    public static AbilityCreator CreateScriptableObject() {
        AbilityCreator asset = ScriptableObject.CreateInstance<AbilityCreator>();
        string assetpath = "Assets/Abilities/Ability.asset";
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        Ability ability = new Ability();
        ability.Id = Path.GetFileNameWithoutExtension(assetPathAndName);
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(ability);
        asset.source = serializer.WriteToString();
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return asset;
    }

}