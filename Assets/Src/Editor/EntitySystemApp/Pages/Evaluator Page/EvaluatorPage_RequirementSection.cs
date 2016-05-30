using UnityEngine;
using UnityEditor;
using Intelligence;
using System;
using System.Collections.Generic;

public class EvaluatorPage_RequirementSection : EvaluatorPage_SectionBase {

    private bool shown;
    private SearchBox<Requirement> searchBox;
    private List<RequirementRenderData> renderData;
    private static string[] ChildRenderExceptions = new string[] { "name", "description" };
    private Texture2D scriptIcon;

    private class RequirementRenderData {
        public bool isDisplayed;
        internal bool isInputDisplayed;
    }

    public EvaluatorPage_RequirementSection() {
        shown = true;
        renderData = new List<RequirementRenderData>();
        scriptIcon = EditorGUIUtility.FindTexture("cs Script Icon");
        searchBox = new SearchBox<Requirement>(null, AddRequirement, "Add Requirement", "Requirements");
    }

    public override void SetTargetObject(AssetItem<DecisionScoreEvaluator> targetItem) {
        base.SetTargetObject(targetItem);
        int count = instanceRef.requirements.Count;
        renderData = new List<RequirementRenderData>(count);
        for (int i = 0; i < count; i++) {
            RequirementRenderData data = new RequirementRenderData();
            data.isDisplayed = false;
            renderData.Add(data);
        }
    }

    private void AddRequirement(Type type) {
        Requirement requirement = Activator.CreateInstance(type) as Requirement;
        requirement.name = Util.SplitAndTitlize(type.Name);
        RequirementRenderData data = new RequirementRenderData();
        data.isDisplayed = true;
        renderData.Add(data);
        SerializedPropertyX property = targetItem.SerialObjectX.FindProperty("requirements");
        property.ArraySize++;
        property.GetChildAt(property.ArraySize - 1).Value = requirement;        
    }

    private void RenderRequirement(SerializedPropertyX property, int index) {
        Requirement requirement = property.Value as Requirement;//instanceRef.requirements[index];
        RequirementRenderData data = renderData[index];

        EditorGUILayout.BeginVertical(EntitySystemWindow.CardStyle);
        {//Render Foldout
            EditorGUILayout.BeginHorizontal();
            if (data.isDisplayed) {
                data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, "Name");
                SerializedPropertyX nameProp = property.FindProperty("name");
                nameProp.Value = EditorGUILayout.TextField(nameProp.Value as string);
            } else {
                data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, requirement.name);
            }
            GUILayout.FlexibleSpace();

            GUIStyle miniLeft = GUI.skin.GetStyle("minibuttonleft");
            GUIStyle miniMid = GUI.skin.GetStyle("minibuttonmid");
            GUIStyle miniRight = GUI.skin.GetStyle("minibuttonright");
            if (GUILayout.Button("Delete", miniLeft)) {
                targetItem.SerialObjectX.FindProperty("requirements").DeleteArrayElementAt(index);
                renderData.RemoveAt(index);
                return;
            }
            GUI.enabled = index != 0;
            if (GUILayout.Button("Up", miniMid)) {
                Swap(index, -1);
            }
            GUI.enabled = index != instanceRef.requirements.Count - 1;
            if (GUILayout.Button("Down", miniRight)) {
                Swap(index, 1);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        if (data.isDisplayed) {

            //manually render description to make sure it comes first
            DrawerUtil.PushLabelWidth(125);
            DrawerUtil.PushIndentLevel(1);

            {//Render Description
                GUIContent desc = new GUIContent("Description");
                SerializedPropertyX descProp = property.FindProperty("description");
                descProp.Value = EditorGUILayout.TextField(desc, descProp.Value as string);
            }
            {//Render Input Label
                GUIContent content = new GUIContent();
                content.text = requirement.GetType().Name;
                EditorGUILayout.BeginHorizontal();
                data.isInputDisplayed = EditorGUILayout.Foldout(data.isInputDisplayed, content);
                EditorGUILayout.EndHorizontal();
            }
            if (data.isInputDisplayed) {
                { //Render Children
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < property.ChildCount; i++) {
                        SerializedPropertyX child = property.GetChildAt(i);
                        if (Array.IndexOf(ChildRenderExceptions, child.name) != -1) continue;

                        EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            DrawerUtil.PopLabelWidth();
            DrawerUtil.PopIndentLevel();
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
        EditorGUILayout.BeginVertical();
        shown = EditorGUILayout.Foldout(shown, "Requirements");

        if (shown && targetItem != null) {
            DrawerUtil.PushLabelWidth(250f);
            DrawerUtil.PushIndentLevel(1);
            SerializedPropertyX reqProp = targetItem.SerialObjectX.FindProperty("requirements");
            for (int i = 0; i < reqProp.ChildCount; i++) {
                RenderRequirement(reqProp.GetChildAt(i), i);
                GUILayout.Space(5f);
            }

            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.RenderLayout();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            DrawerUtil.PopLabelWidth();
            DrawerUtil.PopIndentLevel();
        }

        EditorGUILayout.EndVertical();

    }

}
