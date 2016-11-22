using UnityEngine;
using EntitySystem;

public class PlayerSkillBook : MonoBehaviour {
    [UnitySerialized] [HideInInspector] public string source;

    public SkillBookEntry[] skillBookEntries;

    void Awake() {
        
        if (!string.IsNullOrEmpty(source)) {
            new AssetDeserializer(source, false).DeserializeInto("__default__", this);
        }
        Entity entity = GetComponent<Entity>();
        for (int i = 0; i < skillBookEntries.Length; i++) {
            skillBookEntries[i].Initialize(entity);
        }
    }
}