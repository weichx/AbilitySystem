using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public abstract class ListSection<T> : SectionBase<T> where T : EntitySystemBase {

    public class ComponentRenderData {
        public bool isDisplayed;
    }

    protected bool shown;
    protected SearchBox searchBox;
    protected List<ComponentRenderData> renderData;
    protected SerializedPropertyX listRoot;

    protected List<string> skipRenderingFields;

    public ListSection(float spacing) : base(spacing) {
        searchBox = CreateSearchBox();
        shown = true;
        skipRenderingFields = new List<string>();
    }

    protected abstract SearchBox CreateSearchBox();
    protected abstract string FoldOutLabel { get; }
    protected abstract string ListRootName { get; }

    protected virtual void RenderListItem(SerializedPropertyX item, int index) {
        EditorGUILayout.BeginVertical(EntitySystemWindow.CardStyle);

        ComponentRenderData data = renderData[index];
        RenderHeader(item, data, index);
        if (data.isDisplayed) {
            RenderBody(item, data, index);
        }

        EditorGUILayout.EndVertical();
    }

    protected virtual void RenderHeader(SerializedPropertyX property, ComponentRenderData data, int index) {
        EditorGUILayout.BeginHorizontal();
        data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, property.Type.Name);
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

    protected virtual void RenderBody(SerializedPropertyX property, ComponentRenderData data, int index) {
        EditorGUI.indentLevel++;
        for (int i = 0; i < property.ChildCount; i++) {
            SerializedPropertyX child = property.GetChildAt(i);
            if (skipRenderingFields.IndexOf(child.name) != -1) continue;
            EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
        }
        EditorGUI.indentLevel--;
    }

    protected virtual ComponentRenderData CreateDataInstance(bool isNewTarget) {
        ComponentRenderData data = new ComponentRenderData();
        data.isDisplayed = !isNewTarget;
        return data;
    }

    public override void SetTargetObject(AssetItem<T> targetItem) {
        base.SetTargetObject(targetItem);
        listRoot = rootProperty[ListRootName];
        int count = listRoot.ChildCount;
        renderData = new List<ComponentRenderData>(count);
        for (int i = 0; i < count; i++) {
            renderData.Add(CreateDataInstance(true));
        }
    }

    protected void AddListItem(Type type) {
        object component = Activator.CreateInstance(type);
        renderData.Add(CreateDataInstance(true));
        listRoot.ArraySize++;
        listRoot.GetChildAt(listRoot.ArraySize - 1).Value = component;
    }

    public override void Render() {
        if (rootProperty == null) return;

        shown = EditorGUILayout.Foldout(shown, FoldOutLabel);

        if (shown) {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel++;
            for (int i = 0; i < listRoot.ChildCount; i++) {
                SerializedPropertyX child = listRoot.GetChildAt(i);
                RenderListItem(child, i);
                GUILayout.Space(5f);
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.Render();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    protected void Swap(int index, int directon) {
        directon = Mathf.Clamp(directon, -1, 1);
        listRoot.SwapArrayElements(index, directon);
        var tempData = renderData[index];
        renderData[index] = renderData[index + directon];
        renderData[index + directon] = tempData;
    }

}
