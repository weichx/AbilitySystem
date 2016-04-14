using UnityEngine;
using System.Collections.Generic;

public partial class Ability2 {

	private Dictionary<string, FloatAttribute> floatAttributes;

	public bool AddAttribute(string id, FloatAttribute attr) {
		if(floatAttributes.ContainsKey(id)) {
			return false;
		}
		floatAttributes[id] = attr;
	}

	public void SetAttribute(string id, FloatAttribute attr) {
		floatAttributes[id] = attr;
	}

	public FloatAttribute GetAttribute(string id) {
		return floatAttributes.Get(id);
	}
}

