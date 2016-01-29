using UnityEngine;
using UnityEditor;
using AbilitySystem;
using System.IO;

public static class ScriptableObjectUtility {
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    [MenuItem("Assets/Scriptable Object")]
    public static void CreateScriptableObject() {
        CreateAsset<ScriptableObject>("Scriptable Object");

    }

    [MenuItem("Assets/Ability Prototype")]
    public static void CreateAbilityPrototype() {
        CreateAsset<AbilityPrototype>("Ability Prototype");
    }

    public static void CreateAsset<T>(string assetName = null) where T : ScriptableObject {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "") {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "") {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        assetName = assetName ?? typeof(T).ToString();
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}