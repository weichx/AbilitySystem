using UnityEngine;
using UnityEditor;
using EntitySystemUtil;
using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;

public enum ESWMode {
    Ability, StatusEffect, Behaviors, AIDebugger
}

public class EntitySystemWindow : EditorWindow {

    [MenuItem("Window/Entity System")]
    static void Init() {
        GetWindow<EntitySystemWindow>();
    }

    private AbilityPage abilityPage;

    void OnEnable() {
        abilityPage = null;
        EditorUtility.UnloadUnusedAssetsImmediate(true);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        abilityPage = new AbilityPage();
        abilityPage.Initialize();
        Repaint();
    }

    void OnDisable() {
        EditorUtility.UnloadUnusedAssetsImmediate(true);
        CacheTools.CleanCache();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    void OnDestroy() {
        EditorUtility.UnloadUnusedAssetsImmediate(true);
        GC.Collect();
        GC.WaitForPendingFinalizers();
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

        abilityPage.Render(body);

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

        }//hello?
    }

}