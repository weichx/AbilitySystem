using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using EntitySystem;
using Intelligence;

[PropertyDrawerFor(typeof(DamageFormula))]
public class DamageFormulaDrawer : PropertyDrawerX {

    private SerializedPropertyX rootProperty;
    private SerializedPropertyX listRoot;
    private bool initialized;
    private ListRenderer listRenderer;
    private List<string> skipRenderingFields;

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {
        Initialize(property);
        EditorGUI.indentLevel++;
        for (int i = 0; i < property.ChildCount; i++) {
            SerializedPropertyX child = property.GetChildAt(i);
            child.isExpanded = true;
            if (skipRenderingFields.IndexOf(child.name) != -1) continue;
            EditorGUILayoutX.PropertyField(child, child.label, child.isExpanded);
        }
        EditorGUI.indentLevel--;
        CalculateButton(property);

        listRenderer.Render();
    }

    private void Initialize(SerializedPropertyX source) {
        if (!initialized) {
            var tmp = source.GetParent;
            while (tmp != null) {
                rootProperty = tmp;
                tmp = rootProperty.GetParent;
            }
            listRoot = source["modifiers"];

            listRenderer = new ListRenderer();
            listRenderer.Initialize();
            listRenderer.SetTargetProperty(rootProperty, listRoot);
            listRenderer.SetSearchBox(CreateSearchBox);

            skipRenderingFields = new List<string>();
            skipRenderingFields.Add("modifiers");
            skipRenderingFields.Add("contextType");
            initialized = true;
        }
    }

    private SearchBox CreateSearchBox() {
        Type targetType = rootProperty["contextType"].GetValue<Type>();
        var searchSet = Reflector.FindSubClasses(typeof(Modifier));
        searchSet = searchSet.FindAll((modifierType) => {
                var dummy = Activator.CreateInstance(modifierType) as Modifier;
                return dummy.GetContextType().IsAssignableFrom(targetType);
            });
        return new SearchBox(null, searchSet, listRenderer.AddListItem, "Add Modifier", "Modifiers");
    }

    private void CalculateButton(SerializedPropertyX property) {
        property.ApplyModifiedProperties();
        property.Update();

        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Calculate Formula", GUILayout.Width(150))) {
            DamageFormula d = property.GetValue<DamageFormula>();
            d.OnUse();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}

