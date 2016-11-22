using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[TypeSerializer(typeof(Dictionary<,>))]
public class DictionarySerializer : TypeSerializer<IDictionary> {

    public override void Serialize(IDictionary dict, IWriter writer) {
        if (dict != null) {
            //todo can optimize file size easily by not writing when empty
            //if (dict.Count == 0) {
            //    writer.WriteField("empty", -1);
            //}
            //else {
                ICollection keyCollection = dict.Keys;
                object[] keys = new object[keyCollection.Count];
                keyCollection.CopyTo(keys, 0);
                writer.WriteField("keys", keys);

                ICollection valueCollection = dict.Values;
                object[] values = new object[valueCollection.Count];
                valueCollection.CopyTo(values, 0);
                writer.WriteField("values", values);
            //}
        }
    }
}
