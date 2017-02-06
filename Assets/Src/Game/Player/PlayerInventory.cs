using UnityEngine;
using EntitySystem;

public class PlayerInventory : MonoBehaviour {
    [UnitySerialized] [HideInInspector] public string source;

    [SerializeField]
    public ItemEntry[] itemEntries;

    void Awake () {
        if(!string.IsNullOrEmpty(source)) {
            new AssetDeserializer(source, false).DeserializeInto("__default__", this);
        }
        Entity entity = GetComponent<Entity>();
        for (int i = 0; i < itemEntries.Length; i++) {
            itemEntries[i].Initialize(entity);
        }
    }
}
