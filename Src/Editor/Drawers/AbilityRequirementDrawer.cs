using UnityEngine;
using UnityEngineInternal;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;

namespace AbilitySystem {

    [CustomPropertyDrawer(typeof(AbilityRequirementSet))]
    public class AbilityRequirementDrawer : ReorderableDrawer {
        public override string LabelName {
            get { return "Requirements"; }
        }

        public override string ListPropertyName {
            get { return "requirements"; }
        }

        protected override void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused) {
            EditorRect rect = new EditorRect(baseRect);
            var prop = new ImprovedSerializedProperty(property.GetArrayElementAtIndex(index));
            var idRect = rect.Width(120f);
            prop.Set("id", EditorGUI.TextField(idRect, prop.Get<string>("id")));

            var prototype = prop.FindPropertyRelative("prototype");

            prototype.objectReferenceValue = EditorGUI.ObjectField(rect.WidthMinus(100f), prototype.objectReferenceValue, typeof(RequirementPrototype), false);

            int idx =prop.Get<int>("type");
            prop.Set("type", EditorGUI.Popup(rect, idx, AbilityRequirement.Options));
        }

        protected override void DrawHeader(Rect rect) {
            var editorRect = new EditorRect(rect);
            editorRect.Width(12f);
            EditorGUI.LabelField(editorRect.Width(120), "Requirement Id");
            EditorGUI.LabelField(editorRect.WidthMinus(100), "Prototype");
            EditorGUI.LabelField(editorRect.currentRect, "Applies To");
        }

        protected override void OnItemAdded(ReorderableList list) {
            var prop = AddItemToList(list);
            string name = "Requirement " + list.index;
            var proto = prop.FindPropertyRelative("prototype");
            if(proto != null && proto.objectReferenceValue != null) {
                name = proto.objectReferenceValue.name;
            }
            prop.FindPropertyRelative("id").stringValue = name;
        }
    }
}