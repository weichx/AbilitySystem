using UnityEditor;
using UnityEngine;

namespace AbilitySystem {
    [CustomEditor(typeof(DebugAbility))]
    public class DebugAbilityInspector : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}