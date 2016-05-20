
[TypeSerializer(typeof(MethodPointer))]
public class MethodPointerSerializer : TypeSerializer<MethodPointer> {

    public override void Deserialize(MethodPointer obj, IReader reader) {
        base.Deserialize(obj, reader);
        obj.OnAfterDeserialize();
    }
}

