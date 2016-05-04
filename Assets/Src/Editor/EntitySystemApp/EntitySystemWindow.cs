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

    private Page currentPage;


    void OnEnable() {
        string typeString = EditorPrefs.GetString("ESWindow.CurrentPage");
        if(typeString != null) {
            switch (typeString) {
                case "AbilityPage":
                    currentPage = new AbilityPage();
                    break;
                case "StatusPage":
                    currentPage = new StatusPage();
                    break;
            }
        }
        if (currentPage == null) {
            currentPage = new AbilityPage();
        }
        currentPage.OnEnter();
        Repaint();
    }

    void OnDisable() {
        EditorPrefs.SetString("ESWindow.CurrentPage", currentPage.GetType().Name);
    }

    void OnDestroy() {
        EditorPrefs.DeleteKey("ESWindow.CurrentPage");
    }

    public void Update() {
        ScriptableObjectCompiler.UpdateCompileJobs();
        //todo make this current page
        if (currentPage != null) {
            currentPage.Update();
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
            y = header.height + window.y + 5,
            height = window.height - header.height
        };

        currentPage.Render(body);

    }

    void RenderHeaderBar(Rect rect) {
        HorizontalRectLayout d = new HorizontalRectLayout(rect, 4);

        if (GUI.Button(d, "Abilities", GetStyle(typeof(AbilityPage)))) {
            currentPage = new AbilityPage();
            currentPage.OnEnter();
        }
        else if (GUI.Button(d, "Status Effects", GetStyle(typeof(StatusPage)))) {
            currentPage = new StatusPage();
            currentPage.OnEnter();
        }
        else if (GUI.Button(d, "Behaviors")) {

        }
        else if (GUI.Button(d, "AI Debugger")) {

        }
    }
    //
    private GUIStyle GetStyle(Type type) {
        GUIStyle normalStyle = new GUIStyle(GUI.skin.button);
        GUIStyle selectedStyle = new GUIStyle(GUI.skin.button);
        selectedStyle.fontStyle = FontStyle.Bold;
        if (currentPage.GetType() == type) {
            return selectedStyle;
        }
        else {
            return normalStyle;
        }
    }
}