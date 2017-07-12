using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ListRenderer {

    protected bool shown;
    protected SearchBox searchBox;
    protected List<RenderData> renderData;
    protected SerializedPropertyX listRoot;
    protected SerializedPropertyX rootProperty;

    protected List<string> skipRenderingFields;
    protected bool useFoldout;

    public ListRenderer(bool useFoldout = true) {
        this.useFoldout = useFoldout;
        this.listRoot = listRoot;
        this.rootProperty = rootProperty;
        shown = true;
        skipRenderingFields = new List<string>();
    }

    protected virtual string FoldOutLabel { get { return listRoot.name; } }

    public void SetSearchBox (Func<SearchBox> createSearchBox) {
        searchBox = createSearchBox();
    }

    public void SetTargetProperty(SerializedPropertyX rootProperty, SerializedPropertyX listRoot) {
        this.rootProperty = rootProperty;
        if (rootProperty == null) {
            listRoot = null;
            renderData = null;
            return;
        }

        this.listRoot = listRoot;
        int count = listRoot.ChildCount;
        renderData = new List<RenderData>(count);
        for (int i = 0; i < count; i++) {
            renderData.Add(CreateDataInstance(listRoot.GetChildAt(i), true));
        }
    }

    public void RenderListItem(SerializedPropertyX item, int index) {
        var indent = EditorGUI.indentLevel;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUI.indentLevel * 16f);
        EditorGUI.indentLevel = 0;

        EditorGUILayout.BeginVertical(EntitySystemWindow.CardStyle);

        RenderData data = renderData[index];
        RenderHeader(item, data, index);
        if (data.isDisplayed) {
            RenderBody(item, data, index);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = indent;

    }

    public string GetItemFoldoutLabel(SerializedPropertyX property, RenderData data) {
        return Util.SplitAndTitlize(property.Type.Name);
    }

    public void RenderHeader(SerializedPropertyX property, RenderData data, int index) {

        EditorGUILayout.BeginHorizontal();
        GUIStyle style;
            style = new GUIStyle(EditorStyles.foldout);

        style.normal.textColor = Color.white;
        style.active.textColor = Color.white;
        style.focused.textColor = Color.white;
        style.onActive.textColor = Color.white;
        style.onFocused.textColor = Color.white;
        style.onNormal.textColor = Color.white;
        style.onHover.textColor = Color.white;
        data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, GetItemFoldoutLabel(property, data), style);

        GUILayout.FlexibleSpace();

        GUIStyle miniLeft = GUI.skin.GetStyle("minibuttonleft");
        GUIStyle miniMid = GUI.skin.GetStyle("minibuttonmid");
        GUIStyle miniRight = GUI.skin.GetStyle("minibuttonright");
        if (GUILayout.Button("Delete", miniLeft)) {
            listRoot.DeleteArrayElementAt(index);
            renderData.RemoveAt(index);
            return;
        }
        GUI.enabled = index != 0;
        if (GUILayout.Button("Up", miniMid)) {
            Swap(index, -1);
        }
        GUI.enabled = index != listRoot.ChildCount - 1;
        if (GUILayout.Button("Down", miniRight)) {
            Swap(index, 1);
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

    }

    public void RenderBody(SerializedPropertyX property, RenderData data, int index) {
        EditorGUI.indentLevel++;
        if (Reflector.GetCustomPropertyDrawerFor(property) != null) {
            EditorGUILayoutX.PropertyField(property, property.label, property.isExpanded);
            return;
        }
        for (int i = 0; i < property.ChildCount; i++) {
            SerializedPropertyX child = property.GetChildAt(i);
            if (skipRenderingFields.IndexOf(child.name) != -1) continue;
            EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
        }
        EditorGUI.indentLevel--;
    }

    public RenderData CreateDataInstance(SerializedPropertyX property, bool isNewTarget) {
        RenderData data = new RenderData();
        data.isDisplayed = !isNewTarget;
        return data;
    }

    public void AddListItem(Type type) {
        object component = Activator.CreateInstance(type);
        listRoot.ArraySize++;
        SerializedPropertyX newChild = listRoot.GetChildAt(listRoot.ArraySize - 1);
        Debug.Log(newChild);
        Debug.Log(component);
        newChild.Value = component;
        renderData.Add(CreateDataInstance(newChild, true));
    }

    public void Render() {
        if (rootProperty == null) return;
        EditorGUILayout.BeginVertical();
        if (useFoldout) {
            shown = EditorGUILayout.Foldout(shown, FoldOutLabel);
        }
        else {
            shown = true;
        }
        if (shown) {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listRoot.ChildCount; i++) {
                SerializedPropertyX child = listRoot.GetChildAt(i);
                RenderListItem(child, i);
            }
            EditorGUI.indentLevel--;

            if (searchBox != null) {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                searchBox.Render();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

        }
        EditorGUILayout.EndVertical();

    }

    protected void Swap(int index, int directon) {
        directon = Mathf.Clamp(directon, -1, 1);
        listRoot.SwapArrayElements(index, directon);
        var tempData = renderData[index];
        renderData[index] = renderData[index + directon];
        renderData[index + directon] = tempData;
    }
}
