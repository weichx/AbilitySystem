using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class AbilityPage : Page {

    private List<AbilityListEntry> entries;
    private Vector2 scrollPos;
    private Vector2 detailScrollPos;
    Vector2 detailScrollPosition;
    private SerializedObject obj;
    private ScriptableObject scriptable;
    private SerializedProperty abilityProperty;
    private SearchBox<AbilityComponent> componentSearchBox;
    private SearchBox<AbilityRequirement> requirementSearchBox;
    private Ability abilityInstance;
    private int componentCount;
    private int jobId;
    private AssetDeserializer deserializer;
    private AbilityListEntry activeEntry;
    private bool showComponents = true;
    private bool showRequirements = true;
    private bool showTags = true;
    private bool showAttributes = true;
    private bool showContexts = true;

    public override void OnEnter() {
        GUIUtility.keyboardControl = 0;
        jobId = -1;
        componentSearchBox = new SearchBox<AbilityComponent>(null, CreateComponent, "Add Ability Component", "Ability Components");
        requirementSearchBox = new SearchBox<AbilityRequirement>(null, CreateRequirement, "Add Ability Requirement", "Ability Requirements");

        string[] files = Directory.GetFiles(Application.dataPath + "/Abilities/", "*.ability", SearchOption.AllDirectories);
        entries = new List<AbilityListEntry>();
        for (int i = 0; i < files.Length; i++) {
            string id = Path.GetFileNameWithoutExtension(files[i]);
            AbilityListEntry entry = new AbilityListEntry(id);
            entries.Add(entry);
        }
        //if (entries.Count > 0) {
        //    activeEntry = entries[0];
        //    Load(entries[0].abilityId);
        //}
        SetAbility(abilityInstance);
    }

    private void CreateComponent(Type componentType) {
        abilityInstance.components.Add(Activator.CreateInstance(componentType) as AbilityComponent);
        SetAbility(abilityInstance, true);
    }

    private void CreateRequirement(Type requirementType) {
        abilityInstance.requirements.Add(Activator.CreateInstance(requirementType) as AbilityRequirement);
        SetAbility(abilityInstance, true);
    }

    public bool IsLoading {
        get { return jobId != -1; }
    }

    public override void Update() {
        if (jobId != -1) {
            ScriptableObjectCompiler.CompileJobStatus status;
            string libraryPath;
            if (ScriptableObjectCompiler.TryGetJobResult(jobId, out status, out libraryPath)) {
                if (status == ScriptableObjectCompiler.CompileJobStatus.Succeeded) {
                    ScriptableObjectCompiler.RemoveJob(jobId);
                    Assembly assembly = Assembly.LoadFrom(libraryPath);
                    CreateScritableAbility(assembly.GetType("GeneratedScriptable"));
                    jobId = -1;
                }
                else if (status == ScriptableObjectCompiler.CompileJobStatus.Failed) {
                    // Debug.Log("Failed to compile");
                }
            }
        }
        if (obj != null) {
            obj.ApplyModifiedProperties();
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
    public void SetAbility(Ability newAbility, bool dirty = false) {
        GUIUtility.keyboardControl = 0;
        abilityInstance = newAbility;
        if (abilityInstance == null) {
            if (activeEntry != null) {
                activeEntry.isSelected = false;
            }
            return;
        }
        if (activeEntry != null) {
            if (!dirty && activeEntry.Ability == newAbility) {
                return;
            }
            activeEntry.isSelected = false;
        }
        activeEntry = entries.Find((entry) => {
            return entry.abilityId == abilityInstance.abilityId;
        });
        if (activeEntry == null) {
            activeEntry = new AbilityListEntry(newAbility.abilityId);
            entries.Add(activeEntry);
        }
        activeEntry.isSelected = true;
        activeEntry.Ability = newAbility;
        if (activeEntry.scriptableType == null || dirty) {
            string code = "using UnityEngine;\n";
            code += "public class GeneratedScriptable : ScriptableObject {\n";
            code += "\tpublic Ability ability;\n";
            for (int i = 0; i < abilityInstance.components.Count; i++) {
                code += "\tpublic " + abilityInstance.components[i].GetType().Name + " component" + i + ";\n";
            }
            for (int i = 0; i < abilityInstance.requirements.Count; i++) {
                code += "\tpublic " + abilityInstance.requirements[i].GetType().Name + " requirement" + i + ";\n";
            }
            code += "}";
            string[] assemblies = new string[] {
                typeof(GameObject).Assembly.Location,
                typeof(EditorGUIUtility).Assembly.Location,
                typeof(UnityEngine.UI.Image).Assembly.Location,
                typeof(Ability).Assembly.Location
            };
            jobId = ScriptableObjectCompiler.QueueCompileJob(code, assemblies, true);

        }
        else {
            CreateScritableAbility(activeEntry.scriptableType);
        }
    }

    private void CreateScritableAbility(Type type) {
        scriptable = ScriptableObject.CreateInstance(type);
        type.GetField("ability").SetValue(scriptable, abilityInstance);
        for (int i = 0; i < abilityInstance.components.Count; i++) {
            type.GetField("component" + i).SetValue(scriptable, abilityInstance.components[i]);
        }
        for (int i = 0; i < abilityInstance.requirements.Count; i++) {
            type.GetField("requirement" + i).SetValue(scriptable, abilityInstance.requirements[i]);
        }
        obj = new SerializedObject(scriptable);
        abilityProperty = obj.FindProperty("ability");
        activeEntry.scriptableType = type;
    }

    public override void Render(Rect rect) {
        if (IsLoading) {
            return;
        }
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.MaxWidth(200f));
        GUILayout.Space(10f);
        RenderMasterPane();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        if (abilityProperty != null) {
            detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition);

            GUILayout.BeginVertical(GUILayout.MaxHeight(100f));
            GUILayout.Space(10f);
            RenderNameSection();
            GUILayout.EndVertical();
            GUILayout.Space(10f);
            RenderGeneral();
            GUILayout.Space(20f);

            showComponents = EditorGUILayout.Foldout(showComponents, "Components");

            if (showComponents) {
                RenderComponents();
                GUILayout.Space(20f);
            }
            showRequirements = EditorGUILayout.Foldout(showRequirements, "Requirements");
            if (showRequirements) {
                RenderRequirements();
                GUILayout.Space(20f);
            }

            showAttributes = EditorGUILayout.Foldout(showAttributes, "Attributes");

            showTags = EditorGUILayout.Foldout(showTags, "Tags");

            showContexts = EditorGUILayout.Foldout(showContexts, "Context");

            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void RenderGeneral() {
        if (IsLoading || abilityInstance == null || abilityProperty == null) return;

        SerializedProperty castMode = abilityProperty.FindPropertyRelative("castMode");
        SerializedProperty ignoreGCD = abilityProperty.FindPropertyRelative("IgnoreGCD");
        SerializedProperty castTime = abilityProperty.FindPropertyRelative("castTime").FindPropertyRelative("baseValue");
        SerializedProperty channelTime = abilityProperty.FindPropertyRelative("channelTime").FindPropertyRelative("baseValue");
        SerializedProperty channelTicks = abilityProperty.FindPropertyRelative("channelTicks").FindPropertyRelative("baseValue");
        SerializedProperty charges = abilityProperty.FindPropertyRelative("charges");

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(castMode, false);
        EditorGUILayout.PropertyField(ignoreGCD, false);
        GUILayout.EndHorizontal();

        castTime.floatValue = EditorGUILayout.FloatField("Cast Time", castTime.floatValue);
        channelTime.floatValue = EditorGUILayout.FloatField("Channel Time", channelTime.floatValue);
        channelTicks.intValue = EditorGUILayout.IntField("Channel Ticks", channelTicks.intValue);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Charges");
        if (GUILayout.Button("+", GUILayout.Width(25f))) {
            charges.arraySize++;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        for (int i = 0; i < charges.arraySize; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(charges.GetArrayElementAtIndex(i).FindPropertyRelative("cooldown"), new GUIContent("Charge " + i));
            GUI.enabled = charges.arraySize > 1;
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                charges.DeleteArrayElementAtIndex(i);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

    private void RenderComponents() {
        if (IsLoading || abilityInstance == null || abilityProperty == null) return;
        Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
        var indent = EditorGUI.indentLevel;
        var labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 250f;
        EditorGUI.indentLevel += 2;
        for (int i = 0; i < abilityInstance.components.Count; i++) {
            string type = obj.FindProperty("component" + i).type;
            EditorGUILayout.PropertyField(obj.FindProperty("component" + i), new GUIContent(type, icon), true);
        }
        GUILayout.Space(20f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        componentSearchBox.RenderLayout();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = indent;
        EditorGUIUtility.labelWidth = labelWidth;
    }

    private void RenderRequirements() {
        if (IsLoading || abilityInstance == null || abilityProperty == null) return;
        Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
        var indent = EditorGUI.indentLevel;
        var labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 250f;
        EditorGUI.indentLevel += 2;
        for (int i = 0; i < abilityInstance.requirements.Count; i++) {
            string type = obj.FindProperty("requirement" + i).type;
            EditorGUILayout.PropertyField(obj.FindProperty("requirement" + i), new GUIContent(type, icon), true);
        }
        GUILayout.Space(20f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        requirementSearchBox.RenderLayout();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = indent;
        EditorGUIUtility.labelWidth = labelWidth;
    }

    private void RenderNameSection() {
        if (IsLoading || abilityInstance == null || abilityProperty == null) return;
        SerializedProperty iconProp = abilityProperty.FindPropertyRelative("icon");
        SerializedProperty nameProp = abilityProperty.FindPropertyRelative("abilityId");
        GUILayout.BeginHorizontal();
        iconProp.objectReferenceValue = EditorGUILayout.ObjectField(iconProp.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));
        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 100f;
        EditorGUILayout.PropertyField(nameProp, new GUIContent("Ability Name"));
        EditorGUIUtility.labelWidth = labelWidth;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = false;
        if (GUILayout.Button("Restore")) {
            //Initialize();
            //load it again from serialized form
            Load(activeEntry.OriginalId);
        }
        GUI.enabled = true;
        if (GUILayout.Button("Delete")) {
            entries.Remove(activeEntry);
            if (File.Exists(GetSavePath(activeEntry.OriginalId))) {
                File.Delete(GetSavePath(activeEntry.OriginalId));
            }
            if (File.Exists(GetSavePath(activeEntry))) {
                File.Delete(GetSavePath(activeEntry));
            }
            activeEntry = null;
            SetAbility(null);
        }
        if (GUILayout.Button("Save")) {
            Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private string searchString = "";
    private List<AbilityListEntry> filteredEntries;

    private void RenderMasterPane() {
        if (IsLoading || entries == null) return;
        GUILayout.BeginHorizontal();
        GUIStyle toolbarStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
        string current = searchString;
        searchString = GUILayout.TextField(searchString, toolbarStyle, GUILayout.ExpandWidth(true));
        if (searchString != current) {
            filteredEntries = entries.FindAll((entry) => {
                return entry.abilityId.Contains(searchString);
            });
        }
        else if (searchString == "") {
            filteredEntries = entries;
        }
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
            searchString = "";
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        if (GUILayout.Button("New Ability")) {
            //todo - save prompt
            // Save();
            SetAbility(new Ability(GenerateAbilityId()));
            return;
        }
        GUILayout.Space(5f);

        GUIStyle style = new GUIStyle(GUI.skin.box) {
            margin = new RectOffset() { top = 3 }
        };
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        Color original = GUI.backgroundColor;
        for (int i = 0; i < filteredEntries.Count; i++) {
            if (filteredEntries[i].isSelected) {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button(filteredEntries[i].Name, style, GUILayout.ExpandWidth(true))) {
                Load(filteredEntries[i].abilityId);
            }
            GUI.backgroundColor = original;
        }
        GUILayout.EndScrollView();
    }

    private void Save() {
        if (IsLoading || abilityInstance == null || abilityProperty == null) return;
        if (obj != null) {
            obj.ApplyModifiedProperties();
        }
        if (string.IsNullOrEmpty(abilityInstance.abilityId)) {
            Debug.Log("Ability needs a unique name before it can be saved");
        }
        var serializer = new AssetSerializer();
        serializer.AddItem(abilityInstance);

        var existing = entries.Find((entry) => {
            return entry.abilityId == abilityInstance.abilityId;
        });
        if (existing != null && existing.Ability != abilityInstance) {
            Debug.Log("Name conflict, " + abilityInstance.abilityId + " already exists");
            return;
        }
        activeEntry.Save();
        serializer.WriteToFile(GetSavePath(activeEntry));
    }

    private void Load(string abilityId, bool isRefresh = false) {
        var entry = entries.Find((e) => {
            return e.abilityId == abilityId;
        });
        if (entry.Ability != null && !isRefresh) {
            SetAbility(entry.Ability);
        }
        else {
            var deserializer = new AssetDeserializer(GetLoadPath(abilityId));
            Ability loadedAbility = deserializer.CreateItem<Ability>();
            SetAbility(loadedAbility, true);
        }

    }

    private string GenerateAbilityId() {
        return "Ability " + entries.Count;
    }

    private static string GetLoadPath(string abilityId) {
        return Application.dataPath + "/Abilities/" + abilityId + ".ability";
    }

    private static string GetSavePath(string abilityId) {
        return Application.dataPath + "/Abilities/" + abilityId + ".ability";
    }

    private static string GetSavePath(AbilityListEntry entry) {
        return Application.dataPath + "/Abilities/" + entry.Ability.abilityId + ".ability";
    }
}
