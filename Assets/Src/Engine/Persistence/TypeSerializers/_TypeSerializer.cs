
[TypeSerializer(typeof(object))]
public class TypeSerializer {

    public virtual void Serialize(object obj, IWriter reader) {
        reader.WriteDefault();
    }

    public virtual void Deserialize(object obj, IReader reader) {
        reader.ReadDefault();
    }

    public virtual string GetAlias(string fieldName) {
        return fieldName;
    }

    public virtual string GetVersion() {
        return "v1.0";
    }

}

public abstract class TypeSerializer<T> : TypeSerializer {

    public override void Serialize(object obj, IWriter reader) {
        Serialize((T)obj, reader);
    }

    public virtual void Serialize(T obj, IWriter reader) {
        reader.WriteDefault();
    }

    public override void Deserialize(object obj, IReader reader) {
        Deserialize((T)obj, reader);
    }

    public virtual void Deserialize(T obj, IReader reader) {
        reader.ReadDefault();
    }

}

/*

    () {

    Serialize(T instance) {
        WriteField(id, type, instance);
        WriteKeyValue("", "");
        WriteField("id", instance);
        instance.thing = SetValue(thing);
        WriteField("values", string, "1, 1, 1");
        WriteReference();
    }

    T Deserialize(T instance) {
    
    }
}


*/