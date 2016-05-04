using UnityEngine;
using UnityEditor;
using System.IO;

public static class StatusEffectMenuItem {

    [MenuItem("Assets/Status Effect")]
    public static StatusEffectWrapper CreateScriptableObject() {
        StatusEffectWrapper asset = ScriptableObject.CreateInstance<StatusEffectWrapper>();
        string assetpath = "Assets/Status Effects/Status Effect.asset";
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        var effect = new StatusEffect();
        effect.statusEffectId = Path.GetFileNameWithoutExtension(assetPathAndName);
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(effect);
        asset.statusSource = serializer.WriteToString();
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return asset;
    }

}