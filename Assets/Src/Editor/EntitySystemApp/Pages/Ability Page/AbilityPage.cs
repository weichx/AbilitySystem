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
	private int jobId;

	public override void Initialize() {
		jobId = -1;
		abilityInstance = new Ability(); //todo load from file
		abilityInstance.components.Add(new AddStatusEffect());//todo this temporary
		entries = new List<AbilityListEntry>();
		abilityListVerticalGroup = new VerticalLayoutGroup();
		//Ability[] abilities = EntitySystemLoader.Instance.CreateAll<Ability>();
		for(int i = 0; i < 100; i++) {
			entries.Add(new AbilityListEntry());
			entries[i].abilityId = "Ability " + i;// abilities[i].abilityId;
			abilityListVerticalGroup.AddDrawable(entries[i]);
		}
		searchBox = new SearchBox<AbilityComponent>(null, (Type componentType) => {
			abilityInstance.components.Add(Activator.CreateInstance(componentType) as AbilityComponent);
			CompileScriptableAbility();
		});
		CompileScriptableAbility();
	}

	public bool IsLoading {
		get { return jobId != -1; }
	}

	public void Update() {
		if(jobId != -1) {
			ScriptableObjectCompiler.CompileJobStatus status;
			string libraryPath;
			if(ScriptableObjectCompiler.TryGetJobResult(jobId, out status, out libraryPath)) {
				if(status == ScriptableObjectCompiler.CompileJobStatus.Succeeded) {
					ScriptableObjectCompiler.RemoveJob(jobId);
					Assembly assembly = Assembly.LoadFrom(libraryPath);
					CreateScritableAbility(assembly.GetType("GeneratedScriptable"));
					jobId = -1;
				}
			}
		}
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
	public void CompileScriptableAbility() {
		string code = "using UnityEngine;";
		code += "public class GeneratedScriptable : ScriptableObject {";
		code += "public Ability ability;";
		for(int i = 0; i < abilityInstance.components.Count; i++) {
			code += " public " + abilityInstance.components[i].GetType().Name + " component" + i + ";";
		}
		for(int i = 0; i < abilityInstance.requirements.Count; i++) {
			code += " public " + abilityInstance.requirements[i].GetType().Name + " requirement" + i + ";";
		}
		code += "}";

		string[] assemblies = new string[] {
			typeof(GameObject).Assembly.Location,
			typeof(Ability).Assembly.Location
		};
		jobId = ScriptableObjectCompiler.QueueCompileJob(code, assemblies);
	}

	private void CreateScritableAbility(Type type) {
		scriptable = ScriptableObject.CreateInstance(type);
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

		for(int i = 0; i < abilityInstance.components.Count; i++) {
			detailVerticalGroup.AddDrawable(new VerticalSpaceDrawable(10f));
			detailVerticalGroup.AddDrawable(new ComponentDrawable(obj.FindProperty("component" + i)));
			// detailVerticalGroup.AddDrawable(new HorizontalLineDrawable());
		}

		detailVerticalGroup.AddDrawable(searchBox);
	}

	public override void Render(Rect rect) {
		GUILayout.BeginArea(rect);
		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical(GUILayout.MaxWidth(200f));
		GUILayout.Space(10f);
		RenderMasterPane();
		GUILayout.EndVertical();

		GUILayout.Space(10f);

		GUILayout.BeginVertical(GUILayout.MaxHeight(100f));
		GUILayout.Space(10f);
		RenderNameSection();
		GUILayout.EndVertical();
		GUILayout.Space(10f);
		RenderGeneral();

		GUILayout.EndHorizontal();
		GUILayout.EndArea();

	}

	private void RenderGeneral() {

		SerializedProperty castMode = ability.FindPropertyRelative("castMode");
		SerializedProperty ignoreGCD = ability.FindPropertyRelative("IgnoreGCD");
		SerializedProperty castTime = ability.FindPropertyRelative("castTime").FindPropertyRelative("baseValue");
		SerializedProperty channelTime = ability.FindPropertyRelative("channelTime").FindPropertyRelative("baseValue");
		SerializedProperty channelTicks = ability.FindPropertyRelative("channelTicks").FindPropertyRelative("baseValue");
		SerializedProperty charges = ability.FindPropertyRelative("charges");

		GUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(castMode, false);
		EditorGUILayout.PropertyField(ignoreGCD, false);
		GUILayout.EndHorizontal();

		castTime.floatValue = EditorGUILayout.FloatField("Cast Time", castTime.floatValue);
		channelTime.floatValue = EditorGUILayout.FloatField("Channel Time", channelTime.floatValue);
		channelTicks.intValue = EditorGUILayout.IntField("Channel Ticks", channelTicks.intValue);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Charges");
		if(GUILayout.Button("+", GUILayout.Width(25f))) {
			charges.arraySize++;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel++;
		for (int i = 0; i < charges.arraySize; i++) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(charges.GetArrayElementAtIndex(i).FindPropertyRelative("cooldown"), new GUIContent("Charge " + i));
			GUI.enabled = charges.arraySize > 1;
			if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
				charges.DeleteArrayElementAtIndex(i);
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
		}
		EditorGUI.indentLevel--;
	}

	private void RenderMenuItems() {

		GUILayout.Label("Options", GUILayout.ExpandWidth(true));

		GUILayout.Button("General");

		GUILayout.Button("Attributes");

		GUILayout.Button("Components");

		GUILayout.Button("Requirements");

		GUILayout.Button("Context");

		if(GUILayout.Button("Save")) {
			Save();
		}

	}

	private void RenderDetailPane(Rect rect) {
		EditorRect r = new EditorRect(rect);
		if(IsLoading) {
			GUILayout.BeginArea(r);
			GUILayout.Label("Compiling");
			GUILayout.EndArea();
			return;
		}

		Rect menuPanel = r.HorizontalSlicePercent(0.25f);
		//menuPanel = new EditorRect(menuPanel).HeightMinus(25f);
		GUILayout.BeginArea(menuPanel);
		GUIStyle boxStyle = new GUIStyle();
		boxStyle.normal.background = EditorGUIUtility.whiteTexture;
		GUILayout.Box("", boxStyle, GUILayout.ExpandHeight(true));
		GUILayout.EndArea();

		GUILayout.BeginArea(menuPanel);
		RenderMenuButtons();
		GUILayout.EndArea();

		r.HorizontalSlice(10f);
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

	private void RenderMenuButtons() {


	}

	private void RenderNameSection() {
		SerializedProperty iconProp = ability.FindPropertyRelative("icon");
		SerializedProperty nameProp = ability.FindPropertyRelative("abilityId");
		GUILayout.BeginHorizontal();
		iconProp.objectReferenceValue = EditorGUILayout.ObjectField(iconProp.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));
		float labelWidth = EditorGUIUtility.labelWidth;
		GUILayout.BeginVertical();
		GUILayout.Space(20f);
		EditorGUIUtility.labelWidth = 100f;
		EditorGUILayout.PropertyField(nameProp, new GUIContent("Ability Name"));
		GUILayout.BeginHorizontal();
		EditorGUILayout.Popup("Categories", 0, new string[]{ "Fire", "Ice", "Melee"});
		EditorGUIUtility.labelWidth = labelWidth;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

	}

	private void RenderMasterPane() {
		
		GUILayout.TextField("Search");
		GUILayout.Space(5f);
		GUILayout.Button("New Ability");
		GUILayout.Space(5f);

		GUIStyle style = new GUIStyle(GUI.skin.box) {
			margin = new RectOffset() { top = 3 }
		};
		scrollPos = GUILayout.BeginScrollView(scrollPos);
//		abilityListVerticalGroup.Render();
		for(int i = 0; i < 100; i++) {
//			entries.Add(new AbilityListEntry());
//			entries[i].abilityId = "Ability " + i;// abilities[i].abilityId;
//			abilityListVerticalGroup.AddDrawable(entries[i]);
			GUILayout.Box("Ability " + i, style, GUILayout.ExpandWidth(true));
		}
		GUILayout.EndScrollView();
//		EditorRect r = new EditorRect(view);
//		GUI.TextField(r.VerticalSlice(LineHeight), "Search");
//
//		r.VerticalSlice(LineHeight);
//
//		GUI.Button(r.VerticalSlice(LineHeight), "New Ability");
//
//		r.VerticalSlice(LineHeight);
//
//		Rect sRect = r.VerticalSliceTo(LineHeight);
//
//		scrollPos = GUI.BeginScrollView(sRect, scrollPos, new Rect(view) {
//			width = 3,
//			height = abilityListVerticalGroup.GetHeight()
//		});
//
//		sRect = new Rect(sRect) {
//			width = sRect.width - GUI.skin.verticalScrollbar.fixedWidth
//		};
//
//		abilityListVerticalGroup.Render(sRect);
//		GUI.EndScrollView();
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
