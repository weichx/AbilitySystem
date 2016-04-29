
[TypeSerializer(typeof(object))]
public class TypeSerializer {

    public virtual void Serialize(object obj, IWriter serializer) {
        serializer.WriteDefault();
    }

    public virtual void Deserialize(object obj, IReader reader) {

    }

    public virtual string GetAlias(string fieldName) {
        return fieldName;
    }

    public virtual string GetVersion() {
        return "v1.0";
    }

}

public abstract class TypeSerializer<T> : TypeSerializer {

    public override void Serialize(object obj, IWriter serializer) {
        Serialize((T)obj, serializer);
    }

    public virtual void Serialize(T obj, IWriter serializer) {
        serializer.WriteDefault();
    }

    public override void Deserialize(object obj, IReader reader) {
        Deserialize((T)obj, reader);
    }

    public virtual void Deserialize(T obj, IReader reader) {
        reader.ReadDefault();
    }

}