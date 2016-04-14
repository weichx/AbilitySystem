using UnityEngine;
using System.Collections.Generic;

public class FloatAttribute {

	private float baseValue;
	public Dictionary<string, FloatModifier> modifiers;
	private float flatBonus;
	private float percentBonus;
	private float totalValue;

	public FloatAttribute(float baseValue = 0) {
		this.baseValue = baseValue;
		modifiers = new Dictionary<string, FloatModifier>();
	}

	public void SetModifier(string id, FloatModifier modifier) {
		FloatModifier prev;

		if(modifiers.TryGetValue(id, out prev)) {
			flatBonus -= prev.flatBonus;
			percentBonus -= prev.percentBonus;
		}

		if(modifier != null) {
			flatBonus += modifier.flatBonus;
			percentBonus += modifier.percentBonus;
		}

		modifiers[id] = modifier;

		float totalFlat = baseValue + flatBonus;
		totalValue = totalFlat + (totalFlat * percentBonus);
	}
		
	public float Value {
		get { return totalValue; }
	}

	//todo add method to show modifiers 
}

