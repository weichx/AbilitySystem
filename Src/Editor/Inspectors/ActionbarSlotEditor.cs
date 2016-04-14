//using UnityEngine.UI;
//using UnityEngine;
//
//namespace UnityEditor.UI {
//
//    [CustomEditor(typeof(ActionbarSlot), false)]
//    public class ActionbarSlotEditor : UISlotBaseEditor {
//
//        public override void OnInspectorGUI() {
//            serializedObject.Update();
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityId"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyBind"));
//            serializedObject.ApplyModifiedProperties();
//            if (Application.isPlaying) {
//                if (GUILayout.Button("Refresh Ability")) {
//                    var slot = target as ActionbarSlot;
//                    slot.SetAbility(slot.abilityId);
//                }
//            }
//            base.OnInspectorGUI();
//
//        }
//    }
//}