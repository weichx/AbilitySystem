using UnityEngine;
using UnityEditor;
using System;

public class AssetItem<T> where T : EntitySystemBase {

    protected string assetpath;
    protected Type scriptableType;
    protected ScriptableObject scriptable;
    protected AssetCreator creator;
    protected SerializedObject serialRoot;
    protected bool isDeletePending;
    protected SerializedObjectX serialRootObjectX;

    public AssetItem(AssetCreator creator) {
        this.creator = creator;
        assetpath = AssetDatabase.GetAssetPath(creator);
    }

    public bool IsSelected { get; set; }


    public string AssetPath {
        get { return assetpath; }
        set { assetpath = value; }
    }

    public string Name {
        get {
            return creator.name;
        }
    }

    public SerializedObjectX SerialObjectX {
        get {
            return serialRootObjectX;
        }
    }

    public SerializedObject SerializedObject {
        get {
            return serialRoot;
        }
    }

    public bool IsDeletePending {
        get { return isDeletePending; }
    }

    public virtual void Update() {}

    public virtual void Save() {
        serialRootObjectX.ApplyModifiedProperties();
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(serialRootObjectX.Root.Value);
        creator.source = serializer.WriteToString();
        EditorUtility.SetDirty(creator);
        AssetDatabase.SaveAssets();
        AssetPath = AssetDatabase.RenameAsset(AssetPath, serialRootObjectX.FindProperty("id").Value as string);
        AssetDatabase.Refresh();
    }

    public virtual void QueueDelete() {
        isDeletePending = true;
    }

    public virtual void Delete() {
        isDeletePending = false;
        AssetDatabase.DeleteAsset(AssetPath);
        AssetDatabase.Refresh();
        scriptable = null;
        scriptableType = null;
        serialRoot = null;
        creator = null;
    }

    public virtual void Restore() {
        serialRoot = null;
        Load();
    }

    public virtual void Load() {
            serialRootObjectX = new SerializedObjectX((creator as IAssetCreator<T>).CreateForEditor());
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