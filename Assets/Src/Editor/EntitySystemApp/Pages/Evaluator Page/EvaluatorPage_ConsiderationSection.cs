using UnityEngine;
using UnityEditor;
using Intelligence;
using System;
using Texture2DExtensions;
using System.Collections.Generic;

public class EvaluatorPage_ConsiderationSection : EvaluatorPage_SectionBase {

    private class ConsiderationRenderData {
        public bool isDisplayed;
        public Texture2D graphTexture;
        public bool isCurveShown;
        internal bool isInputDisplayed;
    }

    private bool shown;
    private SearchBox<Consideration> searchBox;
    private static string[] ChildRenderExceptions = new string[] { "name", "description", "curve" };
    private Texture2D scriptIcon;
    private List<ConsiderationRenderData> renderData;

    public EvaluatorPage_ConsiderationSection() {
        shown = true;
        scriptIcon = EditorGUIUtility.FindTexture("cs Script Icon");
        searchBox = new SearchBox<Consideration>(null, AddConsideration, "Add Consideration", "Considerations");
        renderData = new List<ConsiderationRenderData>();
    }

    public override void SetTargetObject(AssetItem<DecisionScoreEvaluator> targetItem) {
        base.SetTargetObject(targetItem);
        int count = instanceRef.considerations.Count;
        renderData = new List<ConsiderationRenderData>(count);
        for (int i = 0; i < count; i++) {
            ConsiderationRenderData data = new ConsiderationRenderData();
            data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
            data.isDisplayed = false;
            data.isCurveShown = false;
            renderData.Add(data);
        }
    }

    private void AddConsideration(Type type) {
        Consideration consideration = Activator.CreateInstance(type) as Consideration;
        consideration.name = Util.SplitAndTitlize(type.Name);
        targetItem.Rebuild();
        ConsiderationRenderData data = new ConsiderationRenderData();
        data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        data.isDisplayed = true;
        data.isCurveShown = true;
        renderData.Add(data);
        SerializedPropertyX property = targetItem.SerialObjectX.FindProperty("considerations");
        property.ArraySize++;
        property.GetChildAt(property.ArraySize - 1).Value = consideration;
    }

    private void RenderCurve(SerializedPropertyX curveProperty, ConsiderationRenderData data) {
        ResponseCurve curve = curveProperty.GetValue<ResponseCurve>();
        bool updateTexture = false;
        Texture2D graphTexture = data.graphTexture;
        EditorGUILayout.BeginHorizontal();
        data.isCurveShown = EditorGUILayout.Foldout(data.isCurveShown, "Curve(" + curve.curveType.ToString() + ")");
        if (!data.isCurveShown) {
            if (graphTexture.width != 64) {
                graphTexture.Resize(64, 32);

                Rect rect = new Rect() {
                    x = 0,
                    y = 0,
                    width = graphTexture.width,
                    height = graphTexture.height
                };
                GraphHelper.DrawGraphLines(rect, graphTexture, (float input) => {
                    return curve.Evaluate(input);
                });
                graphTexture.FlipVertically();
                graphTexture.Apply(true);
            }
            GUILayout.FlexibleSpace();
            GUIContent content = new GUIContent();
            content.text = curve.DisplayString;
            content.image = graphTexture;
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.MiddleLeft;
            GUILayout.Box(content, style);
        }

        EditorGUILayout.EndHorizontal();

        if (!data.isCurveShown) return;

        DrawerUtil.PushIndentLevel(1);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical(GUILayout.MaxWidth(400f));
            curve.curveType = (ResponseCurveType)EditorGUILayout.EnumPopup("Curve Type", curve.curveType);
            curve.slope = EditorGUILayout.FloatField("Slope", curve.slope);
            curve.exp = EditorGUILayout.FloatField("Exp", curve.exp);
            curve.vShift = EditorGUILayout.FloatField("Vertical Shift", curve.vShift);
            curve.hShift = EditorGUILayout.FloatField("Horizontal Shift", curve.hShift);
            curve.threshold = EditorGUILayout.FloatField("Threshold", curve.threshold);
            curve.invert = EditorGUILayout.Toggle(new GUIContent("Inverted"), curve.invert);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Reset")) {
                curve.Reset();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            updateTexture = EditorGUI.EndChangeCheck();
        }
        //draw the graph
        {
            if (updateTexture || graphTexture.width != 512) {
                curveProperty.Update();
                graphTexture.Resize(512, (int)(8.75f * EditorGUIUtility.singleLineHeight));
                Rect rect = new Rect() {
                    x = 0,
                    y = 0,
                    width = graphTexture.width,
                    height = graphTexture.height
                };
                GraphHelper.DrawGraphLines(rect, graphTexture, (float input) => {
                    return curve.Evaluate(input);
                });
                graphTexture.FlipVertically();
                graphTexture.Apply(true);
            }
            DrawerUtil.DrawLayoutTexture(graphTexture);
        }

        EditorGUILayout.EndHorizontal();
        DrawerUtil.PopIndentLevel();
    }

    private void Swap(int index, int directon) {
        directon = (int)Mathf.Clamp(directon, -1, 1);
        var temp = instanceRef.considerations[index];
        instanceRef.considerations[index] = instanceRef.considerations[index + directon];
        instanceRef.considerations[index + directon] = temp;
        var tempData = renderData[index];
        renderData[index] = renderData[index + directon];
        renderData[index + directon] = tempData;
    }

    private void RenderConsideration(SerializedPropertyX considerationProperty, int index) {
        Consideration consideration = considerationProperty.GetValue<Consideration>();
        ConsiderationRenderData data = renderData[index];

        EditorGUILayout.BeginVertical(EntitySystemWindow.CardStyle);
        EditorGUILayout.BeginHorizontal();
        if (data.isDisplayed) {
            data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, "Name");
            consideration.name = EditorGUILayout.TextField(consideration.name);
        }
        else {
            data.isDisplayed = EditorGUILayout.Foldout(data.isDisplayed, consideration.name);
        }
        GUILayout.FlexibleSpace();

        GUIStyle miniLeft = GUI.skin.GetStyle("minibuttonleft");
        GUIStyle miniMid = GUI.skin.GetStyle("minibuttonmid");
        GUIStyle miniRight = GUI.skin.GetStyle("minibuttonright");
        if (GUILayout.Button("Delete", miniLeft)) {
            targetItem.SerialObjectX.FindProperty("considerations").DeleteArrayElementAt(index);
            renderData.RemoveAt(index);
            return;
        }
        GUI.enabled = index != 0;
        if (GUILayout.Button("Up", miniMid)) {
            Swap(index, -1);
        }
        GUI.enabled = index != instanceRef.considerations.Count - 1;
        if (GUILayout.Button("Down", miniRight)) {
            Swap(index, 1);
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        if (data.isDisplayed) {
            //manually render description and curve to make sure they come first
            DrawerUtil.PushLabelWidth(125);
            DrawerUtil.PushIndentLevel(1);

            consideration.description = EditorGUILayout.TextField(new GUIContent("Description"),
                consideration.description);
            GUIContent content = new GUIContent();
            content.text = consideration.GetType().Name;
            EditorGUILayout.BeginHorizontal();
            data.isInputDisplayed = EditorGUILayout.Foldout(data.isInputDisplayed, content);
            EditorGUILayout.EndHorizontal();
            if (data.isInputDisplayed) {
                EditorGUI.indentLevel++;
                SerializedPropertyX property = targetItem.SerialObjectX.FindProperty("considerations");
                property = property.GetChildAt(index);
                for (int i = 0; i < property.ChildCount; i++) {
                    SerializedPropertyX child = property.GetChildAt(i);
                    if (Array.IndexOf(ChildRenderExceptions, child.name) != -1) continue;
                    EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
                }
                EditorGUI.indentLevel--;
            }
            RenderCurve(considerationProperty.FindProperty("curve"), renderData[index]);
            DrawerUtil.PopLabelWidth();
            DrawerUtil.PopIndentLevel();
        }
        EditorGUILayout.EndVertical();
    }

    public override void Render() {
        EditorGUILayout.BeginVertical();
        shown = EditorGUILayout.Foldout(shown, "Considerations");
        if (shown && targetItem != null) {
            DrawerUtil.PushLabelWidth(250f);
            DrawerUtil.PushIndentLevel(1);
            SerializedPropertyX considerationProperty = targetItem.SerialObjectX.FindProperty("considerations");
            for (int i = 0; i < considerationProperty.ChildCount; i++) {
                RenderConsideration(considerationProperty.GetChildAt(i), i);
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
