using System;
using UnityEditor;
using UnityEngine;
using Texture2DExtensions;

public class DecisionSetPage : Page<DecisionSet> {

    float slope = 1; //m
    float exp = 2; //k
    float hShift = 0; //c
    float vShift = 0; //b
    int width = 256;
    int height = 256;
    private Font font;
    private Texture2D fontTexture;
    private Texture2D graphTexture;
    private MasterView<DecisionSetItem, DecisionSet> masterView;
    private DecisionSetPage_DetailView detailView;

    public override void OnEnter(string itemId = null) {
        masterView = new MasterView<DecisionSetItem, DecisionSet>(SetActiveItem);
        detailView = new DecisionSetPage_DetailView();
        graphTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        font = AssetDatabaseExtensions.FindAsset<Font>("Roboto-Light");
        fontTexture = AssetDatabaseExtensions.FindAsset<Texture2D>("Roboto-Light_Texture");
        BuildTexture();
        if (!string.IsNullOrEmpty(itemId)) {
            masterView.SelectItemById(itemId);
        }
    }

    public override void SetActiveItem(AssetItem<DecisionSet> newItem) {
        base.SetActiveItem(newItem);
        detailView.SetTargetObject(newItem);
    }

    public override void Update() {
        if (activeItem != null) {
            activeItem.Update();
            if (activeItem.IsDeletePending) {
                detailView.SetTargetObject(null);
                masterView.RemoveItem(activeItem as DecisionSetItem);
                activeItem.Delete();
            }
        }
    }

    public override void Render(Rect rect) {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();
        masterView.Render();
        GUILayout.Space(10f);
        detailView.Render();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void BuildTexture() {
       // DrawGraphLines();
        graphTexture.DrawText("0.0", 0, height - 1, font, fontTexture, 14);
        graphTexture.DrawText("1.0", width - 25, height - 1, font, fontTexture, 14);
        graphTexture.FlipVertically();
        graphTexture.Apply();
    }

    private void MoveMe(Rect rect) {
        GUI.DrawTexture(new Rect(0, 0, width, height), graphTexture);
        GUILayout.BeginArea(new Rect(0, width, width, rect.height));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 50;
        EditorGUI.BeginChangeCheck();
        slope = EditorGUILayout.FloatField("Slope", slope);
        exp = EditorGUILayout.FloatField("Exp", exp);
        vShift = EditorGUILayout.FloatField("vShift", vShift);
        hShift = EditorGUILayout.FloatField("hShift", hShift);
        if (EditorGUI.EndChangeCheck()) {
            BuildTexture();
        }
        EditorGUIUtility.labelWidth = labelWidth;
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    
}