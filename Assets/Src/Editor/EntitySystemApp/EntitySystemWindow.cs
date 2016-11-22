using UnityEngine;
using UnityEditor;
using EntitySystemUtil;
using System;

public enum ESWMode {
    Ability, StatusEffect, Behaviors, AIDebugger
}

public class EntitySystemWindow : EditorWindow {

    [MenuItem("Window/Entity System")]
    static void Init() {
        GetWindow<EntitySystemWindow>();
    }

    public static GUIStyle CardStyle;

    private Page currentPage;

    void OnEnable() {
        CardStyle = new GUIStyle();
        CardStyle.normal.background = EditorGUIUtility.Load("bg_lighter.png") as Texture2D;
        CardStyle.border.left = 3;
        CardStyle.border.right = 3;
        CardStyle.border.top = 3;
        CardStyle.border.bottom = 3;
        CardStyle.padding.top = 5;
        CardStyle.padding.bottom = 5;
        CardStyle.padding.right = 10;
        CardStyle.margin.right = 10;
        CardStyle.margin.bottom = 5;
        titleContent = new GUIContent("Entity Engine");
        string typeString = EditorPrefs.GetString("ESWindow.CurrentPage");
        if (typeString != null) {
            switch (typeString) {
                case "AbilityPage":
                    currentPage = new AbilityPage();
                    break;
                case "StatusPage":
                    currentPage = new StatusPage();
                    break;
                case "DecisionPackagePage":
                    currentPage = new DecisionPackagePage();
                    break;
                case "EvaluatorPage":
                    currentPage = new EvaluatorPage();
                    break;
            }
        }
        if (currentPage == null) {
            currentPage = new AbilityPage();
        }
        currentPage.OnEnter(EditorPrefs.GetString("ESWindow.CurrentPage.SelectedItem"));
        Repaint();
    }

    void OnDisable() {
        if (currentPage != null) {
            EditorPrefs.SetString("ESWindow.CurrentPage", currentPage.GetType().Name);
            EditorPrefs.SetString("ESWindow.CurrentPage.SelectedItem", currentPage.GetActiveItemId());
        }
    }

    void OnDestroy() {
        EditorPrefs.DeleteKey("ESWindow.CurrentPage");
        EditorPrefs.DeleteKey("ESWindow.CurrentPage.SelectedItem");
    }

    public void Update() {
        ScriptableObjectCompiler.UpdateCompileJobs();
        if (currentPage != null) {
            currentPage.Update();
        }
    }

    public void OnGUI() {
        EditorGUIUtility.labelWidth = 0;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.wideMode = position.width >= 400;
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
        else if (GUI.Button(d, "Decision Packages", GetStyle(typeof(DecisionPackagePage)))) {
            currentPage = new DecisionPackagePage();
            currentPage.OnEnter();
        }
        else if (GUI.Button(d, "Decision Evaluators", GetStyle(typeof(EvaluatorPage)))) {
			currentPage = new EvaluatorPage();
			currentPage.OnEnter();
        }
    }

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