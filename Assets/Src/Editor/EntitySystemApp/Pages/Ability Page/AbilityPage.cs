using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EntitySystemUtil;
using System.Reflection;

public class AbilityPage : Page {

    private List<AbilityListEntry> entries;
    private VerticalLayoutGroup abilityListVerticalGroup;
    private Vector2 scrollPos;
    private Vector2 detailScrollPos;
    private SerializedObject obj;
    private ScriptableObject scriptable;
    private SerializedProperty ability;
    private SearchBox<AbilityComponent> searchBox;
    private Ability abilityInstance;
    private VerticalLayoutGroup detailVerticalGroup;
    private int componentCount;

    public override void Initialize() {
        abilityInstance = new Ability(); // todo pass this in
        abilityInstance.components.Add(new AddStatusEffect());
        entries = new List<AbilityListEntry>();
        abilityListVerticalGroup = new VerticalLayoutGroup();
        //Ability[] abilities = EntitySystemLoader.Instance.CreateAll<Ability>();
        for (int i = 0; i < 100; i++) {
            entries.Add(new AbilityListEntry());
            entries[i].abilityId = "Ability " + i;// abilities[i].abilityId;
            abilityListVerticalGroup.AddDrawable(entries[i]);
        }
        searchBox = new SearchBox<AbilityComponent>(null, (Type componentType) => {
            abilityInstance.components.Add(Activator.CreateInstance(componentType) as AbilityComponent);
            GenerateScriptableAbility();
        });
        GenerateScriptableAbility();
    }

    ///<summary>This is totally cheating. We need to handle serialization seperately 
    ///from unity's system so we use our own asset file format. However we still need
    ///to render fields like the Unity inspector does so we need to use SerializableObject
    ///but only things that extend UnityEngine.Object are serializable, which we dont want
    ///want to do because it will truncate lists of subclasses and generics in general.
    ///Solution: cheat. Use editor-time in-memory code generation to create new subclasses
    ///of ScriptableObject and attach the properties we want to that. Then use that 
    ///instance to handle all our rendering, then save all the properties on the
    ///scriptable object into our regular class to be serialized and saved.
    ///</summary>
    public void GenerateScriptableAbility() {
        string code = "using UnityEngine;";
        code += "public class Generated : ScriptableObject {";
        code += "public Ability ability;";
        for(int i = 0; i < abilityInstance.components.Count; i++) {
            code += " public " + abilityInstance.components[i].GetType().Name + " component" + i + ";";
        }
        code += "}";
        scriptable = ScriptableObjectCompiler.Compile(code);
        Type type = scriptable.GetType();
        type.GetField("ability").SetValue(scriptable, abilityInstance);
        for(int i = 0; i < abilityInstance.components.Count; i++) {
            type.GetField("component" + i).SetValue(scriptable, abilityInstance.components[i]);
        }
        obj = new SerializedObject(scriptable);
        ability = obj.FindProperty("ability");
        detailVerticalGroup = new VerticalLayoutGroup();

        detailVerticalGroup.AddDrawable(new CastModeDrawable(ability.FindPropertyRelative("castMode"), ability.FindPropertyRelative("IgnoreGCD")));
        detailVerticalGroup.AddDrawable(new FloatAttributeDrawable(ability.FindPropertyRelative("castTime")));
        detailVerticalGroup.AddDrawable(new FloatAttributeDrawable(ability.FindPropertyRelative("channelTime")));
        detailVerticalGroup.AddDrawable(new IntAttributeDrawable(ability.FindPropertyRelative("channelTicks")));
        detailVerticalGroup.AddDrawable(new ChargesDrawable(ability.FindPropertyRelative("charges")));
        //detailVerticalGroup.AddDrawable(new HorizontalLineDrawable());

        for (int i = 0; i < abilityInstance.components.Count; i++) {
            detailVerticalGroup.AddDrawable(new VerticalSpaceDrawable(10f));
            detailVerticalGroup.AddDrawable(new ComponentDrawable(obj.FindProperty("component" + i)));
           // detailVerticalGroup.AddDrawable(new HorizontalLineDrawable());
        }
        
        detailVerticalGroup.AddDrawable(searchBox);
    }

    public override void Render(Rect rect) {
        if (ability == null) return;
        EditorRect r = new EditorRect(rect.ShrinkTop(10f));
        RenderMasterPane(r.HorizontalSlicePercent(0.20f));
        r.HorizontalSlice(20f);
        RenderDetailPane(r.ShrinkBottom(10f));
    }

    private void RenderDetailPane(Rect rect) {
        EditorRect r = new EditorRect(rect);
        r.Shrink(10f);
        Rect menuPanel = r.HorizontalSlicePercent(0.25f);

        EditorRect m = new EditorRect(menuPanel);
        GUI.Box(m, "", EditorStyles.helpBox);
        m.Shrink(10f);
        RenderNameSection(m.VerticalSlice(64));
        m.Shrink(10f);
        RenderMenuButtons(m);

        r.ShrinkLeftRight(20, 0);
        RenderDetailHeader(r.VerticalSlice(64f));

        r.VerticalSlice(LineHeight);
        RenderDetails(r);
    }

    private void RenderDetails(Rect rect) {
        GUI.Box(rect, "", EditorStyles.helpBox);
        EditorRect r = new EditorRect(rect, 0, LineHeight * 0.25f);
        r.Shrink(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = EditorGUIUtility.labelWidth * 0.75f;
        GUILayout.BeginArea(r);
        detailScrollPos = GUILayout.BeginScrollView(detailScrollPos);
        detailVerticalGroup.Render(r);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        EditorGUIUtility.labelWidth = labelWidth;
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

        r.VerticalSlice(LineHeight);
        if(GUI.Button(r.VerticalSlice(LineHeight), "Save")) {
            Save();
        }
    }

    private void RenderNameSection(Rect rect) {
        SerializedProperty iconProp = ability.FindPropertyRelative("icon");
        SerializedProperty nameProp = ability.FindPropertyRelative("abilityId");
        float remaining = rect.height - (2f * LineHeight);
        EditorRect r = new EditorRect(rect);
        Rect texRect = new Rect(r.HorizontalSlice(r.currentRect.height));
        iconProp.objectReferenceValue = EditorGUI.ObjectField(texRect, iconProp.objectReferenceValue, typeof(Texture2D), false);
        r.HorizontalSlice(10f);
        r.VerticalSlice(remaining * 0.33f);
        EditorGUI.PropertyField(r.VerticalSlice(LineHeight), nameProp, GUIContent.none);
        r.VerticalSlice(remaining * 0.33f);
        EditorGUI.PropertyField(r.VerticalSlice(LineHeight), nameProp, GUIContent.none);
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
            height = abilityListVerticalGroup.GetHeight()
        });

        sRect = new Rect(sRect) {
            width = sRect.width - GUI.skin.verticalScrollbar.fixedWidth
        };

        abilityListVerticalGroup.Render(sRect);
        GUI.EndScrollView();
    }

    public override float GetHeight() {
        return abilityListVerticalGroup.GetHeight();
    }

    private void Save() {
        obj.ApplyModifiedProperties();
        var serializer = new EntityAssetSerializer<Ability>(abilityInstance);
        serializer.Write(Application.dataPath + "/A/" + "hurr.txt");// abilityInstance.abilityId);
    }
}
