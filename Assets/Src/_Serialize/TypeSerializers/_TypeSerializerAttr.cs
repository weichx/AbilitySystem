using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TypeSerializerAttribute : Attribute {

    public readonly Type type;

    public TypeSerializerAttribute(Type type) {
        //todo see about arrays, might want to disclude them
        this.type = type;
    }
}