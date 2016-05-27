using System;
using UnityEngine;

[TypeSerializer(typeof(Type))]
public class TypeTypeSerializer : TypeSerializer<Type> {

	public override void Serialize(Type type, IWriter serializer) {
		serializer.WriteField("qualifiedId", type.AssemblyQualifiedName);
	}

	public override void Deserialize(Type type, IReader deserializer) {
		deserializer.ReadDefault(); //todo fix this
//		string value = deserializer.GetFieldValue("qualifiedId");
		//deserializer.ReadField("qualifiedId", Type.GetType()
	}

}

