using UnityEngine;
using UnityEditor;
using Intelligence;
using System;
using Texture2DExtensions;
using System.Collections.Generic;

public class EvaluatorPage_ConsiderationSection2 : ListSection<DecisionScoreEvaluator> {

    private class ConsiderationRenderData : ListSection<DecisionScoreEvaluator>.ComponentRenderData {
        public Texture2D graphTexture;
        public bool isCurveShown;
        public bool isInputDisplayed;
    }

    public EvaluatorPage_ConsiderationSection2(float spacing) : base(spacing) {
        skipRenderingFields.Add("name");
        skipRenderingFields.Add("description");
        skipRenderingFields.Add("curve");
    }

    protected override string FoldOutLabel {
        get { return "Considerations"; }
    }

    protected override string ListRootName {
        get { return "considerations"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(Consideration), AddListItem, "Add Consideration", "Considerations");
    }

    protected override ComponentRenderData CreateDataInstance(bool isNewTarget) {
        ConsiderationRenderData data = new ConsiderationRenderData();
        data.isDisplayed = !isNewTarget;
        data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        data.isDisplayed = false;
        data.isCurveShown = false;
        return data;
    }

    protected override void RenderBody(SerializedPropertyX considerationProperty, ComponentRenderData _data, int index) {
        ConsiderationRenderData data = _data as ConsiderationRenderData;
        Consideration consideration = considerationProperty.GetValue<Consideration>();

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
            SerializedPropertyX property = listRoot.GetChildAt(index);
            for (int i = 0; i < property.ChildCount; i++) {
                SerializedPropertyX child = property.GetChildAt(i);
                if (skipRenderingFields.IndexOf(child.name) != -1) continue;
                EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
            }
            EditorGUI.indentLevel--;
        }
        RenderCurve(considerationProperty.FindProperty("curve"), data);
        DrawerUtil.PopLabelWidth();
        DrawerUtil.PopIndentLevel();
        
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
}

public class EvaluatorPage_ConsiderationSection : SectionBase<DecisionScoreEvaluator> {

    private class ConsiderationRenderData {
        public bool isDisplayed;
        public Texture2D graphTexture;
        public bool isCurveShown;
        internal bool isInputDisplayed;
    }

    private bool shown;
    private SearchBox searchBox;
    private static string[] ChildRenderExceptions = new string[] { "name", "description", "curve" };
    private List<ConsiderationRenderData> renderData;
    private SerializedPropertyX listRoot;

    public EvaluatorPage_ConsiderationSection(float spacing) : base(spacing) {
        shown = true;
        searchBox = new SearchBox(null, typeof(Consideration), AddConsideration, "Add Consideration", "Considerations");
        renderData = new List<ConsiderationRenderData>();
    }

    public override void SetTargetObject(AssetItem<DecisionScoreEvaluator> targetItem) {
        base.SetTargetObject(targetItem);
        listRoot = rootProperty["considerations"];
        int count = listRoot.ChildCount;
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
        ConsiderationRenderData data = new ConsiderationRenderData();
        data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        data.isDisplayed = true;
        data.isCurveShown = true;
        renderData.Add(data);
        listRoot.ArraySize++;
        listRoot.GetChildAt(listRoot.ArraySize - 1).Value = consideration;
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
        directon = Mathf.Clamp(directon, -1, 1);
        listRoot.SwapArrayElements(index, directon);
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
            listRoot.DeleteArrayElementAt(index);
            renderData.RemoveAt(index);
            return;
        }
        GUI.enabled = index != 0;
        if (GUILayout.Button("Up", miniMid)) {
            Swap(index, -1);
        }
        GUI.enabled = index != rootProperty.ChildCount - 1;
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
                SerializedPropertyX property = listRoot.GetChildAt(index);
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
        if(rootProperty == null) return;
        EditorGUILayout.BeginVertical();
        shown = EditorGUILayout.Foldout(shown, "Considerations");
        if (shown) {
            DrawerUtil.PushLabelWidth(250f);
            DrawerUtil.PushIndentLevel(1);
            for (int i = 0; i < listRoot.ChildCount; i++) {
                RenderConsideration(listRoot.GetChildAt(i), i);
                GUILayout.Space(5f);
            }

            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.Render();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            DrawerUtil.PopLabelWidth();
            DrawerUtil.PopIndentLevel();
        }
        EditorGUILayout.EndVertical();

    }

}
