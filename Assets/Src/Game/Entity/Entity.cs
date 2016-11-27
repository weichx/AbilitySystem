using System;
using UnityEngine;

namespace EntitySystem {
		
    //todo make custom inspector and change back to FloatRange
    public partial class Entity {

    public CharacterParameters parameters;
    public CharacterEquipment equipment;
 
    [UnitySerialized] [HideInInspector] public string source;
    [NonSerialized] public bool initialized;
    [NonSerialized] public Entity target;

    public void Start() {
        if (source != null && source != string.Empty) {
            initialized = true;
            new AssetDeserializer(source, false).DeserializeInto("__default__", this);
        }
        parameters.baseParameters.strength.SetModifier("Protein Powder", FloatModifier.Percent(0.2f));

    }
}
