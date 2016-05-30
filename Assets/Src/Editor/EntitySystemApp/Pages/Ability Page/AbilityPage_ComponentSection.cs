using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AbilityPage_ComponentSection : AbilityPage_SectionBase {

    private bool shown;
    private SearchBox<AbilityComponent> searchBox;
    private List<ComponentRenderData> renderData;

    public class ComponentRenderData {
        public bool isDisplayed;
    }

    public AbilityPage_ComponentSection() {
        shown = true;
        searchBox = new SearchBox<AbilityComponent>(null, AddComponent, "Add Component", "Components");
        renderData = new List<ComponentRenderData>();
    }

    public override void SetTargetObject(AssetItem<Ability> targetItem) {
        base.SetTargetObject(targetItem);
        int count = instanceRef.components.Count;
        renderData = new List<ComponentRenderData>(count);
        for (int i = 0; i < count; i++) {
            ComponentRenderData data = new ComponentRenderData();
            data.isDisplayed = false;
            renderData.Add(data);
        }
    }

    private void AddComponent(Type type) {
        AbilityComponent component = Activator.CreateInstance(type) as AbilityComponent;
        ComponentRenderData data = new ComponentRenderData();
        data.isDisplayed = true;
        renderData.Add(data);
        SerializedPropertyX property = targetItem.SerialObjectX.FindProperty("components");
        property.ArraySize++;
        property.GetChildAt(property.ArraySize - 1).Value = component;
    }

    private void RenderAbilityComponent(SerializedPropertyX property, int index) {
        AbilityComponent component = property.Value as AbilityComponent;
        ComponentRenderData data = renderData[index];

        EditorGUILayout.BeginVertical(EntitySystemWindow.CardStyle);
        {//Render Foldout
            EditorGUILayout.BeginHorizontal();
            data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, component.GetType().Name);
            GUILayout.FlexibleSpace();

            GUIStyle miniLeft = GUI.skin.GetStyle("minibuttonleft");
            GUIStyle miniMid = GUI.skin.GetStyle("minibuttonmid");
            GUIStyle miniRight = GUI.skin.GetStyle("minibuttonright");
            if (GUILayout.Button("Delete", miniLeft)) {
                targetItem.SerialObjectX.FindProperty("components").DeleteArrayElementAt(index);
                renderData.RemoveAt(index);
                return;
            }
            GUI.enabled = index != 0;
            if (GUILayout.Button("Up", miniMid)) {
                Swap(index, -1);
            }
            GUI.enabled = index != targetItem.SerialObjectX.FindProperty("components").ChildCount - 1;
            if (GUILayout.Button("Down", miniRight)) {
                Swap(index, 1);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        if (data.isDisplayed) {

            DrawerUtil.PushLabelWidth(125);

            { //Render Children
                EditorGUI.indentLevel++;
                for (int i = 0; i < property.ChildCount; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                    EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
                }
                EditorGUI.indentLevel--;
            }

            DrawerUtil.PopLabelWidth();
        }
        EditorGUILayout.EndVertical();
    }

    private void Swap(int index, int directon) {
        directon = Mathf.Clamp(directon, -1, 1);
        SerializedPropertyX property = targetItem.SerialObjectX.FindProperty("requirements");
        property.SwapArrayElements(index, directon);
        var tempData = renderData[index];
        renderData[index] = renderData[index + directon];
        renderData[index + directon] = tempData;
    }

    public override void Render() {
        if (targetItem == null) return;

        shown = EditorGUILayout.Foldout(shown, "Components");

        if (shown) {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel++;
            SerializedPropertyX componentsProp = targetItem.SerialObjectX.FindProperty("components");
            for (int i = 0; i < componentsProp.ChildCount; i++) {
                SerializedPropertyX child = componentsProp.GetChildAt(i);
                RenderAbilityComponent(child, i);
                GUILayout.Space(5f);
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.RenderLayout();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}