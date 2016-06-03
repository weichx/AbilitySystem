using System;
using UnityEngine;
using UnityEditor;
using Intelligence;
using Texture2DExtensions;

public class EvaluatorPage_ConsiderationSection : ListSection<DecisionEvaluator> {

    private class ConsiderationRenderData : RenderData {
        public Texture2D graphTexture;
        public bool isCurveShown;
        public bool isInputDisplayed;
    }

    public EvaluatorPage_ConsiderationSection(float spacing) : base(spacing) {
        skipRenderingFields.Add("name");
        skipRenderingFields.Add("description");
        skipRenderingFields.Add("curve");
    }

    protected override string FoldOutLabel {
        get { return "Considerations (" + listRoot.ChildCount + ")"; }
    }

    protected override string ListRootName {
        get { return "considerations"; }
    }

    public override void SetTargetProperty(SerializedPropertyX rootProperty) {
        base.SetTargetProperty(rootProperty);
        searchBox = CreateSearchBox();
    }

    //todo if root context type changes this search set will be out of date
    protected override SearchBox CreateSearchBox() {
        if (rootProperty == null) return null;
        Type targetType = rootProperty["contextType"].GetValue<Type>();
        Type baseType = typeof(Consideration<>);
        Type genType = baseType.MakeGenericType(new Type[] {targetType});
        var searchSet = Reflector.FindSubClasses(typeof(Consideration));
        searchSet = searchSet.FindAll((considerationType) => {
            return genType.IsAssignableFrom(considerationType);
        });
        return new SearchBox(null, searchSet, AddListItem, "Add Consideration", "Considerations");
    }

    protected override RenderData CreateDataInstance(SerializedPropertyX property, bool isNewTarget) {
        ConsiderationRenderData data = new ConsiderationRenderData();
        data.isDisplayed = !isNewTarget;
        data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        data.isDisplayed = false;
        data.isCurveShown = false;
        return data;
    }

    protected override void RenderBody(SerializedPropertyX considerationProperty, RenderData _data, int index) {
        ConsiderationRenderData data = _data as ConsiderationRenderData;
        Consideration consideration = considerationProperty.GetValue<Consideration>();

        //manually render description and curve to make sure they come first
        DrawerUtil.PushLabelWidth(125);
        DrawerUtil.PushIndentLevel(1);
        consideration.description = EditorGUILayout.TextField(new GUIContent("Description"),
            consideration.description);
        GUIContent content = new GUIContent();
        content.text = Util.SplitAndTitlize(consideration.GetType().Name);
        EditorGUILayout.BeginHorizontal();
        data.isInputDisplayed = EditorGUILayout.Foldout(data.isInputDisplayed, content);
        EditorGUILayout.EndHorizontal();
        if (data.isInputDisplayed) {
            EditorGUI.indentLevel++;
            for (int i = 0; i < considerationProperty.ChildCount; i++) {
                SerializedPropertyX child = considerationProperty.GetChildAt(i);
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
        if (graphTexture == null) {
            data.graphTexture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        }
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
