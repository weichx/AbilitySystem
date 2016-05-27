using System;
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public sealed class UniqueIdComponent : MonoBehaviour, ISerializationCallbackReceiver {

    public static Dictionary<string, int> mapping;

    [Writable(false)] public string UniqueId;

    static UniqueIdComponent() {
        mapping = new Dictionary<string, int>();
    }

    void OnDestroy() {
        mapping.Remove(UniqueId);
    }

    public void OnBeforeSerialize() { }

    ///<summary>
    ///Generates a Unique Id that will never change even if the scene is reloaded. 
    ///Cloned objects get their own unque ids automatically.
    ///</summary>
    public void OnAfterDeserialize() {
        int instanceId = GetInstanceID();
        //checking for key and value match here because Unity seems to call this method twice.
        if (string.IsNullOrEmpty(UniqueId) || mapping.ContainsKey(UniqueId) && mapping[UniqueId] != instanceId) {
            UniqueId = Guid.NewGuid().ToString();
        }
        mapping[UniqueId] = instanceId;
    }

}
