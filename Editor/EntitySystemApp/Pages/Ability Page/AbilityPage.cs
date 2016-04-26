using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EntitySystemUtil;

public class AbilityPage : Page {

    private List<AbilityListEntry> entries;
    private VerticalLayoutGroup layout;
    private Vector2 scrollPos;
    SerializedObject obj;
    public GameObject dummy;
    public List<string> generatedFieldNames;
    public int fieldIdx = 0;
    public ScriptableObject scriptable;

    public override void Initialize() {
        entries = new List<AbilityListEntry>();
        layout = new VerticalLayoutGroup();
        Ability[] abilities = EntitySystemLoader.Instance.CreateAll<Ability>();
        for (int i = 0; i < 100; i++) {
            entries.Add(new AbilityListEntry());
            entries[i].abilityId = "Ability " + i;// abilities[i].abilityId;
            layout.AddDrawable(entries[i]);
        }
        ReGenerateSerializedObject();
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
    public void ReGenerateSerializedObject() {

        generatedFieldNames = new List<string>();
        fieldIdx++;
        string code = @"
            using UnityEngine;
            
            public class DummyScriptable : ScriptableObject {
        
                public int field0;
                public AddStatusEffect field1;
                //public RemoveStatusEffect field2;
        ";

        code += "}";
        scriptable = EntitySystemWindow.ExecuteCode(code);
        obj = new SerializedObject(scriptable);
    }

    public override void Render(Rect rect) {
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

        r.ShrinkLeftRight(20, 10);
        RenderDetailHeader(r.VerticalSlice(64f));

        r.VerticalSlice(LineHeight);
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

        //todo -- im going to need to do some sort of recursive descent for
        //serializing things on sub-objects. I basically need to shadow
        //all properties all the way down to the leaves and then copy
        //changes back to the real object. 
        //components can have lists of things that are of polymorphic type
        //for example, or take references to things that are of
        //polymorphic type. For now I may be alright without 
        //this functionality for anything not on the top level.
        for (int i = 0; i < 2; i++) {
            SerializedProperty p = obj.FindProperty("field" + i);
            if(EditorGUI.PropertyField(cr.VerticalSlice(), p, true)) {
                //SetFieldValue(ability.components[i], value);
                //Save();
            }
        }
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
        EditorGUI.ObjectField(tex, null, typeof(Texture2D), false);
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

    private SearchBox<AbilityComponent> searchBox;
    private Texture2D abilityComponentTexture;
    private SerializedProperty property;

    public CEditor() {
        abilityComponentTexture = new Texture2D(1, 1);
        abilityComponentTexture.LoadImage(System.IO.File.ReadAllBytes(Application.dataPath + "/AbilityComponentIcon.jpg"));
        abilityComponentTexture.Apply();
        searchBox = new SearchBox<AbilityComponent>(abilityComponentTexture, AddAbilityComponent);
    }

    public void AddAbilityComponent(Type componentType) {
        Dummy dummy = property.serializedObject.targetObject as Dummy;
        Ability ability = dummy.ability;
        ability.components.Add(Activator.CreateInstance(componentType) as AbilityComponent);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        this.property = property;
        EditorRect r = new EditorRect(position);

        SerializedProperty componentsProp = property.FindPropertyRelative("components");
        int arraySize = componentsProp.arraySize;

        GUIContent content = new GUIContent();
        content.image = abilityComponentTexture;
        content.text = "Ability Component";
        //for (int i = 0; i < AbilityPage.fieldIdx; i++) {
        //    var prop = componentsProp.GetArrayElementAtIndex(i);

        //    EditorGUI.PropertyField(r.VerticalSlice(50f), componentsProp.GetArrayElementAtIndex(i), content, true);
        //}

        Rect searchRect = r.VerticalSlice(200f).WidthCentered(225f);
        searchBox.OnGUI(searchRect, "Add Component");
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 400f;// base.GetPropertyHeight(property, label);
    }
}


public class SearchBox<T> {

    private bool hasResults;
    private List<Type> results;
    private bool searching;
    private string searchString;
    private List<Type> searchSet;
    private Texture2D resultIcon;
    private GUIContent resultContent;
    private Action<Type> selected;

    public SearchBox(Texture2D resultIcon, Action<Type> selectedAction) {
        searchString = string.Empty;
        searchSet = Reflector.FindSubClasses<T>();
        results = new List<Type>();
        this.resultIcon = resultIcon;
        resultContent = new GUIContent();
        resultContent.image = resultIcon;
        selected = selectedAction;
    }

    public void OnGUI(Rect position, string buttonText) {
        int controlID = GUIUtility.GetControlID(FocusType.Native);
        switch (Event.current.GetTypeForControl(controlID)) {
            case EventType.Repaint:
                break;
            case EventType.MouseDown:
                break;
        }
        EditorRect r = new EditorRect(position);
        if (GUI.Button(r.VerticalSlice(24f), buttonText)) {
            searching = !searching;
        }

        if (searching) {

            searchString = GUI.TextField(r.VerticalSlice(), searchString);

            GUI.Box(r, "Ability Components", new GUIStyle(EditorStyles.helpBox) {
                font = EditorStyles.boldLabel.font,
                alignment = TextAnchor.UpperCenter,
            });

            r.VerticalSlice();
            results.Clear();
            results.AddRange(searchSet.FindAll((type) => {
                return type.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
            }));

            if (results.Count > 10) {
                results.RemoveRange(10, results.Count - 10);
            }

            for (int i = 0; i < results.Count; i++) {
                resultContent.text = results[i].Name;
                if (SearchResult.SearchResultItem(r.VerticalSlice(), resultContent)) {
                    selected(results[i]);
                    searching = false;
                    searchString = string.Empty;
                    results.Clear();
                }
            }
        }
    }

    public float GetHeight() {
        return 12f * 20f;
    }
}

public static class SearchResult {

    private class SearchResultState {
        public bool mouseOver;
    }

    public static bool SearchResultItem(Rect controlRect, GUIContent result) {

        int controlId = GUIUtility.GetControlID(FocusType.Native);
        var state = (SearchResultState)GUIUtility.GetStateObject(typeof(SearchResultState), controlId);

        switch (Event.current.GetTypeForControl(controlId)) {
            case EventType.Repaint:
                GUI.Label(controlRect, result);
                break;
            case EventType.MouseDown:
                if (controlRect.Contains(Event.current.mousePosition)) {
                    return true;
                }
                break;
        }

        return false;
    }
}