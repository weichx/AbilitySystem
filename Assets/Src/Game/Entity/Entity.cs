using System;
using UnityEngine;

namespace EntitySystem {
		
	//todo make custom inspector and change back to FloatRange
	public partial class Entity {

	    public FloatRange Strength;
	    public FloatRange Agility;
	    public FloatRange Stamina;
	    public FloatRange Intelligence;
	    public FloatRange Spirit;

	    [UnitySerialized] [HideInInspector] public string source;
	    [NonSerialized] public bool initialized;
	    [NonSerialized] public Entity target;

	    public void Start() {
	        if (source != null && source != string.Empty) {
	            initialized = true;
	            new AssetDeserializer(source, false).DeserializeInto("__default__", this);
	        }
	        Strength.SetModifier("Protein Powder", FloatModifier.Percent(0.2f));

	    }
	}

}