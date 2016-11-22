using UnityEngine;
using UnityEditor;
using EntitySystem;
using System;

public class AssetItem<T> where T : EntitySystemBase, new() {

    protected string assetguid;
    protected bool isDeletePending;
    protected SerializedObjectX serialRootObjectX;
    protected string name;

    public AssetItem(string assetguid) {
        this.assetguid = assetguid;
        string path = AssetDatabase.GUIDToAssetPath(assetguid);
        name = System.IO.Path.GetFileNameWithoutExtension(path);
    }

    protected AssetCreator<T> GetAssetCreator() {
        Type creatorType = typeof(EntitySystemBase).Assembly.GetType(typeof(T).Name + "Creator");
        string assetpath = AssetDatabase.GUIDToAssetPath(assetguid);
        AssetCreator<T> creator = AssetDatabase.LoadAssetAtPath(assetpath, creatorType) as AssetCreator<T>;
        return creator;
    }

    public bool IsSelected { get; set; }

    public string Name {
        get { return serialRootObjectX == null ? name : serialRootObjectX.FindProperty("id").GetValue<string>(); }
    }

    public SerializedObjectX SerialObjectX {
        get { return serialRootObjectX; }
    }

    public bool IsDeletePending {
        get { return isDeletePending; }
    }

    public virtual void Update() { }

    public virtual void Save() {
        serialRootObjectX.ApplyModifiedProperties();
        AssetCreator<T> creator = GetAssetCreator();
        creator.SetSourceAsset(serialRootObjectX.Root.Value as T);
        EditorUtility.SetDirty(creator);
        AssetDatabase.SaveAssets();
        string path = AssetDatabase.GUIDToAssetPath(assetguid);
        AssetDatabase.RenameAsset(path, serialRootObjectX["id"].GetValue<string>());
        AssetDatabase.Refresh();
    }

    public virtual void QueueDelete() {
        isDeletePending = true;
    }

    public virtual void Delete() {
        isDeletePending = false;
        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetguid));
        AssetDatabase.Refresh();
    }

    public virtual void Restore() {
        Load();
    }

    public virtual void Load() {
        serialRootObjectX = new SerializedObjectX(GetAssetCreator().Create());
    }

    //if (scriptableType == null) {
    //    ///<summary>This is totally cheating. We need to handle serialization seperately 
    //    ///from unity's system so we use our own asset file format. However we still need
    //    ///to render fields like the Unity inspector does so we need to use SerializableObject
    //    ///but only things that extend UnityEngine.Object are serializable, which we dont want
    //    ///want to do because it will truncate lists of subclasses and generics in general.
    //    ///Solution: cheat. Use editor-time in-memory code generation to create new subclasses
    //    ///of ScriptableObject and attach the properties we want to that. Then use that 
    //    ///instance to handle all our rendering, then save all the properties on the
    //    ///scriptable object into our regular class to be serialized and saved.
    //    ///</summary>
    //    string code = GetCodeString();
    //    string[] assemblies = GetAssemblies();
    //    scriptableType = ScriptableObjectCompiler.CreateScriptableType(code, assemblies);
    //}
    //if (scriptableType != null) {
    //    CreateScriptableObject();
    //}
    //else {
    //    Debug.LogError("Failed to compile");
    //}


    //protected void CreateScriptableObject() {
    //    scriptable = ScriptableObject.CreateInstance(scriptableType);
    //    InitializeScriptable();
    //    serialRoot = new SerializedObject(scriptable);
    //    serialRoot.Update();
    //}

    //protected virtual void InitializeScriptable() { }

    //protected virtual string GetCodeString() {
    //    string code = "using UnityEngine;\n";
    //    code += "public class GeneratedScriptable : ScriptableObject {\n";
    //    code += "\tpublic " + typeof(T).Name + " instance;\n";
    //    code += "}";
    //    return code;
    //}

    //protected virtual string[] GetAssemblies() {
    //    return new string[] {
    //            typeof(GameObject).Assembly.Location,
    //            typeof(EditorGUIUtility).Assembly.Location,
    //            typeof(UnityEngine.UI.Image).Assembly.Location,
    //            typeof(Ability).Assembly.Location
    //        };
    //}

}