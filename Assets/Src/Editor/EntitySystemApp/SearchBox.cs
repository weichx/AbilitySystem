using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EntitySystemUtil;

public class SearchBox<T> : IRectDrawable {
    private Texture2D blue;
    private bool hasResults;
    private List<Type> results;
    private bool searching;
    private string searchString;
    private List<Type> searchSet;
    private GUIContent resultContent;
    private Action<Type> selected;

    public SearchBox(Texture2D resultIcon, Action<Type> selectedAction) {
        searchString = string.Empty;
        searchSet = Reflector.FindSubClasses<T>().FindAll((type) => {
            return type.IsSerializable;
        });
        results = new List<Type>();
        resultContent = new GUIContent();
        resultContent.image = resultIcon;
        selected = selectedAction;
        blue = new Texture2D(1, 1);
        blue.SetPixel(0, 0, Color.blue);
        blue.Apply();
    }

    public float GetHeight() {
        return 12f * 20f; //todo this is wrong
    }

    public void Render(Rect rect) {

        if (!searching && GUILayout.Button("Add Ability Component")) {
            searching = !searching;
        }

        if (searching) {

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Box("Ability Components", new GUIStyle(EditorStyles.helpBox) {
                font = EditorStyles.boldLabel.font,
                alignment = TextAnchor.UpperCenter,
            });

            EditorGUILayout.BeginHorizontal();
            searchString = GUILayout.TextField(searchString);
            if (GUILayout.Button("X", GUILayout.Width(25f))) {
                searching = false;
                searchString = string.Empty;
                results.Clear();
            }
            EditorGUILayout.EndHorizontal();

            results.Clear();
            results.AddRange(searchSet.FindAll((type) => {
                return type.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
            }));

            if (results.Count > 10) {
                results.RemoveRange(10, results.Count - 10);
            }
            GUIStyle itemStyle = new GUIStyle(GUI.skin.button);
            itemStyle.normal.background = null;
            itemStyle.hover.background = blue;
            itemStyle.active.background = blue;
            for (int i = 0; i < results.Count; i++) {
                resultContent.text = results[i].Name;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(resultContent, itemStyle)) {
                    selected(results[i]);
                    searching = false;
                    searchString = string.Empty;
                    results.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}

public static class SearchResult {


    public static bool SearchResultItem(Rect controlRect, GUIContent result) {

        int controlId = GUIUtility.GetControlID(FocusType.Native);

        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.Repaint:
                GUI.Label(controlRect, result);
                break;
            case EventType.MouseDown:
                if (controlRect.Contains(Event.current.mousePosition)) {
                    return true;
                }
                break;
        }

        return false;
    }
}