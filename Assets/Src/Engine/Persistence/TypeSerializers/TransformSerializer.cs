using UnityEngine;

[TypeSerializer(typeof(Transform))]
public class TransformSerializer : TypeSerializer<Transform> {

    public override void Serialize(Transform obj, IWriter serializer) {
        serializer.WriteField("position", typeof(Vector3), obj.position);
        serializer.WriteField("rotation", typeof(Quaternion), obj.rotation);
        serializer.WriteField("localScale", typeof(Vector3), obj.localScale);
        serializer.WriteField("parent", obj.parent);
    }

}

