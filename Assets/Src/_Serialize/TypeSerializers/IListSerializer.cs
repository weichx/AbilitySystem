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
            Debug.Log("Yup");
            for(int i = 0; i < list.Count; i++) {
                writer.WriteField("-", list[i]);
            }
        }
        else {
            Debug.Log("Nope");
        }
    }
}

[TypeSerializer(typeof(Stack<>))]
public class StackSerializer : TypeSerializer<ICollection> {

    public override void Serialize(ICollection list, IWriter writer) {
        if (list != null) {
            Debug.Log("Yup");
            foreach(var item in list) {
                writer.WriteField("-", item);
            }
        }
        else {
            Debug.Log("Nope");
        }
    }

}
