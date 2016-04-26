using UnityEngine;
using UnityEditor;
using EntitySystemUtil;
using System;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;

public enum ESWMode {
    Ability, StatusEffect, Behaviors, AIDebugger
}

public class EntitySystemWindow : EditorWindow {

    [MenuItem("Window/Entity System")]
    static void Init() {
        EntitySystemWindow window = GetWindow<EntitySystemWindow>();
    }
    private ESWMode mode;
    private AbilityPage abilityPage;

    public static ScriptableObject ExecuteCode(string code) {
        Dictionary<string, string> provOptions = new Dictionary<string, string>();

        provOptions.Add("CompilerVersion", "v2.0");
        CSharpCodeProvider provider = new CSharpCodeProvider(provOptions);
        CompilerParameters compilerParams = new CompilerParameters();
        compilerParams.GenerateExecutable = false;
        compilerParams.GenerateInMemory = true;
        compilerParams.IncludeDebugInformation = true;
        compilerParams.ReferencedAssemblies.Add(typeof(Ability).Assembly.CodeBase);
        compilerParams.ReferencedAssemblies.Add(typeof(Vector3).Assembly.CodeBase);

        CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, code);
        Debug.Log("Number of Errors: " + results.Errors.Count);
        foreach (CompilerError err in results.Errors) {
            Debug.Log(err.ErrorText);

        }
        
        Type t = results.CompiledAssembly.GetType("DummyScriptable");
        return ScriptableObject.CreateInstance(t);
    }

    void OnEnable() {
        abilityPage = new AbilityPage();
        abilityPage.Initialize();
    }

    void OnDisable() {
        if (abilityPage != null && abilityPage.dummy != null) {
            DestroyImmediate(abilityPage.dummy);
        }
    }

    void OnDestroy() {
        if (abilityPage != null && abilityPage.dummy != null) {
            DestroyImmediate(abilityPage.dummy);
        }
    }

    public void OnGUI() {
        Rect window = new Rect(0, 0, position.width, position.height)
            .ShrinkTopBottom(10f)
            .ShrinkLeftRight(20f);
        Rect header = new Rect(window) {
            height = 2f * EditorGUIUtility.singleLineHeight
        };

        RenderHeaderBar(header);

        Rect body = new Rect(window) {
            y = header.height + window.y,
            height = window.height - header.height
        };

        switch (mode) {
            case ESWMode.Ability:
                abilityPage.Render(body);
                break;
            default: break;
        }
    }

    void RenderHeaderBar(Rect rect) {

        HorizontalRectLayout d = new HorizontalRectLayout(rect, 4);

        if (GUI.Button(d, "Abilities")) {

        }
        else if (GUI.Button(d, "Status Effects")) {

        }
        else if (GUI.Button(d, "Behaviors")) {

        }
        else if (GUI.Button(d, "AI Debugger")) {

        }
    }

}