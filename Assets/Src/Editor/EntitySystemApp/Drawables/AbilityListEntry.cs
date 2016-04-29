using UnityEngine;
using UnityEditor;

public class AbilityListEntry : IRectDrawable {

    public string abilityId;

    public static GUIStyle style = new GUIStyle() {
        fontSize = 14,
        alignment = TextAnchor.MiddleCenter
    };

    public void Render(Rect rect) {
        GUI.Box(rect, "", EditorStyles.helpBox);
        GUI.Label(rect, abilityId, style);
    }

    public float GetHeight() {
        return EditorGUIUtility.singleLineHeight * 3f;
    }

}