using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[TypeSerializer(typeof(IList<>))]
[TypeSerializer(typeof(List<>))]
[TypeSerializer(typeof(Array))]
[TypeSerializer(typeof(Queue<>))]
public class IListSerializer : TypeSerializer<IList> {

    public override void Serialize(IList list, IWriter writer) {
        if (list != null) {
            if(!list.GetType().IsArray) {
                writer.WriteField("Length", list.Count);
            }
            for (int i = 0; i < list.Count; i++) {
                writer.WriteField("-", list[i]);
            }
        }
    }

    public override void Deserialize(IList obj, IReader reader) {
        if (obj.GetType().IsArray) {
            for (int i = 0; i < obj.Count; i++) {
                obj[i] = reader.GetFieldValueAtIndex(i);
            }
        }
        else {
            int length = reader.GetFieldValue<int>("Length");
            for (int i = 0; i < length; i++) {
                obj.Add(reader.GetFieldValueAtIndex(i + 1));
            }
        }
    }
}

[TypeSerializer(typeof(Stack<>))]
public class StackSerializer : TypeSerializer<ICollection> {

    public override void Serialize(ICollection list, IWriter writer) {
        if (list != null) {
            foreach (var item in list) {
                writer.WriteField("-", item);
            }
        }
    }
}
