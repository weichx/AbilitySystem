using UnityEngine;
using UnityEditor;
using EntitySystemUtil;

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

    void OnEnable() {
        abilityPage = new AbilityPage();
        abilityPage.Initialize();
    }

    void OnDisable() {
        if (abilityPage.dummy != null) {
            DestroyImmediate(abilityPage.dummy);
        }
        Debug.Log("Disabling");
    }

    void OnDestroy() {
        if(abilityPage.dummy != null) {
            DestroyImmediate(abilityPage.dummy);
        }
        Debug.Log("Destroying");

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