using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class StatusPage : Page {

    public StatusListEntry activeEntry;
    private StatusPage_MasterView masterView;
    private StatusPage_DetailView detailView;
    private SerializedObject serialRoot;
    private ScriptableObject scriptableStatusEffect;
    private bool deletePending;

    public StatusPage() {
        masterView = new StatusPage_MasterView(this);
        detailView = new StatusPage_DetailView(this);
    }

    public override void Update() {
        if (serialRoot != null) {
            serialRoot.ApplyModifiedProperties();
        }
        if(deletePending) {
            deletePending = false;
            AssetDatabase.DeleteAsset(activeEntry.FilePath);
            AssetDatabase.Refresh();
            masterView.RemoveEntry(activeEntry);
            activeEntry = null;
            serialRoot = null;
            scriptableStatusEffect = null;
            detailView.SetTargetObject(null);
            masterView.SetTargetObject(null);
        }
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

    public void SetActiveStatusEffect(StatusListEntry listEntry) {
        if (activeEntry != null) {
            activeEntry.isSelected = false;
        }
        activeEntry = listEntry;
        if (activeEntry != null) {
            activeEntry.isSelected = true;
            LoadStatusEffect();
        }
    }

    public void Save() {
        if (serialRoot == null || activeEntry == null) return;
        serialRoot.ApplyModifiedProperties();
        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(activeEntry.statusEffect);
        activeEntry.Wrapper.statusSource = serializer.WriteToString();
        EditorUtility.SetDirty(activeEntry.Wrapper);
        AssetDatabase.SaveAssets();
        activeEntry.FilePath = AssetDatabase.RenameAsset(activeEntry.FilePath, activeEntry.Name);
        AssetDatabase.Refresh();
    }

    public void Delete() {
        deletePending = true;
    }

    public void Restore() {
        AssetDeserializer deserializer = new AssetDeserializer(activeEntry.Source, false);
        activeEntry.statusEffect = deserializer.CreateItem<StatusEffect>();
        Compile();
    }

    public void Compile() {
        string output;
        string code = GetCodeString(activeEntry.statusEffect);
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

    private void LoadStatusEffect() {
        if(activeEntry.statusEffect == null) {
            AssetDeserializer deserializer = new AssetDeserializer(activeEntry.Source, false);
            activeEntry.statusEffect = deserializer.CreateItem<StatusEffect>();
        }
        if (activeEntry.scriptableType == null) {
            Compile();
        }
        else {
            SetupObjects();
        }
    }

    private void SetupObjects() {
        scriptableStatusEffect = ScriptableObject.CreateInstance(activeEntry.scriptableType);
        activeEntry.scriptableType.GetField("statusEffect").SetValue(scriptableStatusEffect, activeEntry.statusEffect);
        for (int i = 0; i < activeEntry.statusEffect.components.Count; i++) {
            activeEntry.scriptableType.GetField("component" + i).SetValue(scriptableStatusEffect, activeEntry.statusEffect.components[i]);
        }
        serialRoot = new SerializedObject(scriptableStatusEffect);
        serialRoot.Update();
        detailView.SetTargetObject(serialRoot);
    }

    private static string GetCodeString(StatusEffect effect) {
        string code = "using UnityEngine;\n";
        code += "public class GeneratedScriptable : ScriptableObject {\n";
        code += "\tpublic StatusEffect statusEffect;\n";
        for (int i = 0; i < effect.components.Count; i++) {
            code += "\tpublic " + effect.components[i].GetType().Name + " component" + i + ";\n";
        }
        code += "}";
        return code;
    }

    private static string[] GetAssemblies() {
        return new string[] {
                typeof(GameObject).Assembly.Location,
                typeof(EditorGUIUtility).Assembly.Location,
                typeof(UnityEngine.UI.Image).Assembly.Location,
                typeof(Ability).Assembly.Location
            };
    }
}