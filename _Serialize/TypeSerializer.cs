using UnityEngine;
using System;
using System.Collections.Generic;

public class TypeSerializer<T> {

    public static Dictionary<Type, TypeSerializer<object>> typeSerializerMap = new Dictionary<Type, TypeSerializer<object>>();

    public virtual void Serialize(T obj, Serializer serializer) {
        serializer.WriteDefault(obj);
    }

    public virtual void OnDeserialize(T instance) {

    }

    public virtual string GetAlias(string fieldName) {
        return fieldName;
    }

    public virtual string GetVersion() {
        return "1.0";
    }
}

public class TransformSerializer : TypeSerializer<Transform> {

}

public class TypeSerializerAttribute : Attribute {

    Type type;
    public TypeSerializerAttribute(Type type) {
            this.type = type;
    }
}