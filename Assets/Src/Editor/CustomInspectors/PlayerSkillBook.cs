using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSkillBook))]
public class PlayerSkillBookInspector : Editor {

    private SerializedObjectX root;

    public override void OnInspectorGUI() {
        PlayerSkillBook skillBook = target as PlayerSkillBook;

        if (root == null) {
            if (!string.IsNullOrEmpty(skillBook.source)) {
                new AssetDeserializer(skillBook.source, false).DeserializeInto("__default__", skillBook);
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