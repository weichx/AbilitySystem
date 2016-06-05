using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity))]
public class EntityInspector : Editor {

    private SerializedObjectX root;

    public override void OnInspectorGUI() {
        Entity entity = target as Entity;

        if (root == null) {
            if (!string.IsNullOrEmpty(entity.source)) {
                new AssetDeserializer(entity.source, false).DeserializeInto("__default__", entity);
            }
            root = new SerializedObjectX(target);
        }
        EditorGUILayoutX.DrawProperties(root);
        bool didChange = root.Root.ApplyModifiedProperties();
        if (didChange) {
            AssetSerializer serializer = new AssetSerializer();
            serializer.AddItem(target);
            serializedObject.FindProperty("source").stringValue = serializer.WriteToString();
            serializedObject.ApplyModifiedProperties();
        }
    }

   
}