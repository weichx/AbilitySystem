using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EntitySystemUtil;

public class AbilityPage : Page {

    private List<AbilityListEntry> entries;
    private VerticalLayoutGroup layout;
    private Vector2 scrollPos;

    public override void Initialize() {
        entries = new List<AbilityListEntry>();
        layout = new VerticalLayoutGroup();
        Ability[] abilities = EntitySystemLoader.Instance.CreateAll<Ability>();
        for (int i = 0; i < 100; i++) {
            entries.Add(new AbilityListEntry());
            entries[i].abilityId = "Ability " + i;// abilities[i].abilityId;
            layout.AddDrawable(entries[i]);
        }
        dummy = new GameObject("Dummy");
        dummy.AddComponent<Dummy>();
        for(int i = 0; i < 1000; i++) {
            dummy.AddComponent<Dummy>();
        }
    }

    public GameObject dummy;

    public override void Render(Rect rect) {
        EditorRect r = new EditorRect(rect.ShrinkTop(10f));
        RenderMasterPane(r.HorizontalSlicePercent(0.20f));
        r.HorizontalSlice(20f);
        RenderDetailPane(r.ShrinkBottom(10f));
    }

    private void RenderDetailPane(Rect rect) {
        GUI.Box(rect, "", EditorStyles.helpBox);
        EditorRect r = new EditorRect(rect);
        r.Shrink(10f);
        Rect menuPanel = r.HorizontalSlicePercent(0.25f);

        EditorRect m = new EditorRect(menuPanel);
        RenderNameSection(m.VerticalSlice(64));
        m.Shrink(10f);
        RenderMenuButtons(m);

        r.ShrinkLeftRight(20, 10);
        RenderDetailHeader(r.VerticalSlice(64f));

        r.VerticalSlice(LineHeight);//h
        RenderDetails(r);
    }

    CastMode castMode;
    private void RenderDetails(Rect rect) {
        GUI.Box(rect, "", EditorStyles.helpBox);
        EditorRect r = new EditorRect(rect, 0, LineHeight * 0.25f);
        r.Shrink(20f);

        EditorRect cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        GUI.Label(cr.HorizontalSlice(LabelWidth), "Cast Mode");
        castMode = (CastMode)EditorGUI.EnumPopup(cr.HorizontalSlice(EditorGUIUtility.labelWidth), castMode);
        GUI.Toggle(cr, true, "Ignore GCD");

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        GUI.Label(cr.HorizontalSlice(LabelWidth), "Cast Time");
        EditorGUI.FloatField(cr.HorizontalSlice(LabelWidth), 1.0f);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        GUI.Label(cr.HorizontalSlice(LabelWidth), "Channel Time");
        EditorGUI.FloatField(cr.HorizontalSlice(LabelWidth), 1.0f);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        GUI.Label(cr.HorizontalSlice(LabelWidth), "Channel Ticks");
        EditorGUI.IntField(cr.HorizontalSlice(LabelWidth), 3);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        GUI.Label(cr.HorizontalSlice(LabelWidth), "Charges");
        EditorGUI.IntField(cr.HorizontalSlice(LabelWidth), 2);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        cr.HorizontalSlice(20f);
        GUI.Label(cr.HorizontalSlice(LabelWidth - 30f), "Charge Cooldown");
        EditorGUI.FloatField(cr.HorizontalSlice(LabelWidth), 1);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);
        cr.HorizontalSlice(20f);
        GUI.Label(cr.HorizontalSlice(LabelWidth - 30f), "Charge Cooldown");
        EditorGUI.FloatField(cr.HorizontalSlice(LabelWidth), 1);

        cr = r.VerticalSlice(LineHeight).EditorRect(10, LineHeight * 0.5f);

        SerializedObject obj = new SerializedObject(dummy.GetComponent<Dummy>());
        EditorGUI.PropertyField(cr, obj.FindProperty("ability"), true);

    }

    private void RenderDetailHeader(Rect rect) {
        GUI.Box(rect, "", EditorStyles.helpBox);
        GUI.Label(rect, "General", new GUIStyle() {
            alignment = TextAnchor.MiddleCenter
        });
    }

    private void RenderMenuButtons(Rect rect) {
        EditorRect r = new EditorRect(rect);

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "General");

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "Requirements");

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "Components");

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "Attributes");

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "Modifiers");

        r.VerticalSlice(LineHeight);
        GUI.Button(r.VerticalSlice(LineHeight), "Context");
    }

    private void RenderNameSection(Rect rect) {
        float remaining = rect.height - (2f * LineHeight);
        EditorRect r = new EditorRect(rect);
        Rect tex = new Rect(r.HorizontalSlice(r.currentRect.height));
        GUI.DrawTexture(tex, EditorGUIUtility.whiteTexture);
        r.HorizontalSlice(10f);
        r.VerticalSlice(remaining * 0.33f);
        GUI.TextField(r.VerticalSlice(LineHeight), "Ability Name");
        r.VerticalSlice(remaining * 0.33f);
        GUI.TextField(r.VerticalSlice(LineHeight), "Parent Name");
    }

    private void RenderMasterPane(Rect view) {

        EditorRect r = new EditorRect(view);
        GUI.TextField(r.VerticalSlice(LineHeight), "Search");

        r.VerticalSlice(LineHeight);

        GUI.Button(r.VerticalSlice(LineHeight), "New Ability");

        r.VerticalSlice(LineHeight);

        Rect sRect = r.VerticalSliceTo(LineHeight);

        scrollPos = GUI.BeginScrollView(sRect, scrollPos, new Rect(view) {
            width = 3,
            height = layout.GetHeight()
        });

        sRect = new Rect(sRect) {
            width = sRect.width - GUI.skin.verticalScrollbar.fixedWidth
        };

        layout.Render(sRect);
        GUI.EndScrollView();
    }

    public override float GetHeight() {
        return layout.GetHeight();
    }
}

[CustomPropertyDrawer(typeof(Ability))]
public class CEditor : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.Label(position, "SUCK IT UNITY");
    }

}