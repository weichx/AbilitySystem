using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage : Page {

    public AbilityListEntry activeEntry;
    private AbilityPage_MasterView masterView;
    private AbilityPage_DetailView detailView;
    private SerializedObject serialRoot;
    private ScriptableObject scriptableAbility;
    private bool deletePending;

    public Ability Ability {
        get {
            return activeEntry.ability;
        }
    }

    public override void OnEnter() {
        masterView = new AbilityPage_MasterView(this);
        detailView = new AbilityPage_DetailView(this);
        GUIUtility.keyboardControl = 0;
    }

    public override void Render(Rect rect) {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();
        masterView.Render();
        GUILayout.Space(10f);
        detailView.Render();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public override void Update() {
        if (serialRoot != null) {
            serialRoot.ApplyModifiedProperties();
        }
        if (deletePending) {
            deletePending = false;
            AssetDatabase.DeleteAsset(activeEntry.FilePath);
            AssetDatabase.Refresh();
            masterView.RemoveEntry(activeEntry);
            activeEntry = null;
            serialRoot = null;
            scriptableAbility = null;
            detailView.SetTargetObject(null);
            masterView.SetTargetObject(null);
        }
    }

    ///<summary>This is totally cheating. We need to handle serialization seperately 
    ///from unity's system so we use our own asset file format. However we still need
    ///to render fields like the Unity inspector does so we need to use SerializableObject
    ///but only things that extend UnityEngine.Object are serializable, which we dont want
    ///want to do because it will truncate lists of subclasses and generics in general.
    ///Solution: cheat. Use editor-time in-memory code generation to create new subclasses
    ///of ScriptableObject and attach the properties we want to that. Then use that 
    ///instance to handle all our rendering, then save all the properties on the
    ///scriptable object into our regular class to be serialized and saved.
    ///</summary>
    public void Compile() {
        string output;
        string code = GetCodeString(activeEntry.ability);
        string[] assemblies = GetAssemblies();
        int jobId = ScriptableObjectCompiler.QueueCompileJob(code, assemblies, true);
        ScriptableObjectCompiler.CompileJobStatus result;
        ScriptableObjectCompiler.TryGetJobResult(jobId, out result, out output);
        if (result == ScriptableObjectCompiler.CompileJobStatus.Succeeded) {
            activeEntry.scriptableType = Assembly.LoadFrom(output).GetType("GeneratedScriptable");
            SetupObjects();
        }
        else {
            Debug.LogError("Failed to compile");
        }
    }

    public void SetActiveAbilityEntry(AbilityListEntry listEntry) {
        if (activeEntry != null) {
            activeEntry.isSelected = false;
        }
        activeEntry = listEntry;
        if (activeEntry != null) {
            activeEntry.isSelected = true;
            LoadAbility();
        }
    }

    private void LoadAbility() {
        if (activeEntry.ability == null) {
            AssetDeserializer deserializer = new AssetDeserializer(activeEntry.Source, false);
            activeEntry.ability = deserializer.CreateItem<Ability>();
        }
        if (activeEntry.scriptableType == null) {
            Compile();
        }
        else {
            SetupObjects();
        }
    }

    public void Save() {
        if (serialRoot == null || activeEntry == null) return;
        serialRoot.ApplyModifiedProperties();
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(activeEntry.ability);
        activeEntry.Wrapper.source = serializer.WriteToString();
        EditorUtility.SetDirty(activeEntry.Wrapper);
        AssetDatabase.SaveAssets();
        activeEntry.FilePath = AssetDatabase.RenameAsset(activeEntry.FilePath, activeEntry.Name);
        AssetDatabase.Refresh();
    }

    public void Restore() {
        AssetDeserializer deserializer = new AssetDeserializer(activeEntry.Source, false);
        activeEntry.ability = deserializer.CreateItem<Ability>();
        Compile();
    }

    public void Delete() {
        deletePending = true;
    }

    private void SetupObjects() {
        scriptableAbility = ScriptableObject.CreateInstance(activeEntry.scriptableType);
        activeEntry.scriptableType.GetField("ability").SetValue(scriptableAbility, activeEntry.ability);
        for (int i = 0; i < activeEntry.ability.components.Count; i++) {
            activeEntry.scriptableType.GetField("__component" + i).SetValue(scriptableAbility, activeEntry.ability.components[i]);
        }
        for (int i = 0; i < activeEntry.ability.requirements.Count; i++) {
            activeEntry.scriptableType.GetField("__requirement" + i).SetValue(scriptableAbility, activeEntry.ability.requirements[i]);
        }
        serialRoot = new SerializedObject(scriptableAbility);
        serialRoot.Update();
        detailView.SetTargetObject(serialRoot);
    }

    private static string[] GetAssemblies() {
        return new string[] {
                typeof(GameObject).Assembly.Location,
                typeof(EditorGUIUtility).Assembly.Location,
                typeof(UnityEngine.UI.Image).Assembly.Location,
                typeof(Ability).Assembly.Location
            };
    }

    private static string GetCodeString(Ability ability) {
        string code = "using UnityEngine;\n";
        code += "public class GeneratedScriptable : ScriptableObject {\n";
        code += "\tpublic Ability ability;\n";
        for (int i = 0; i < ability.components.Count; i++) {
            code += "\tpublic " + ability.components[i].GetType().Name + " __component" + i + ";\n";
        }
        for (int i = 0; i < ability.requirements.Count; i++) {
            code += "\tpublic " + ability.requirements[i].GetType().Name + " __requirement" + i + ";\n";
        }
        code += "}";
        return code;
    }

}
