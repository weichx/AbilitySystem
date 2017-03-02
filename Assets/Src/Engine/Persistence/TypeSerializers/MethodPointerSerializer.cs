
[TypeSerializer(typeof(AbstractMethodPointer))]
public class MethodPointerSerializer : TypeSerializer<AbstractMethodPointer> {

    public override void Serialize(AbstractMethodPointer ptr, IWriter writer) {
        writer.WriteField("ptr", ptr.signature);
    }

    public override void Deserialize(AbstractMethodPointer obj, IReader reader) {
        obj.signature = (string)reader.GetFieldValue("ptr");
    }
}
