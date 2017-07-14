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

    public override void OnGUI(SerializedPropertyX source, GUIContent label) {
        Initialize(source);
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
            listRenderer.SetTargetProperty(rootProperty, ref listRoot);
            listRenderer.SetSearchBox(CreateSearchBox);
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
}
