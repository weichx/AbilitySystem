using UnityEngine;
using System.Collections.Generic;

public class FloatVariable {

	private float baseValue;
	private float baseBonus;
	private float totalValue;
	private float percentBonus;
	private float currentValue;

	private Dictionary<string, FloatModifier> modifiers;

	public FloatVariable(float baseValue) : this(baseValue, baseValue) {}

	public FloatVariable(float currentValue, float baseValue) {
		this.currentValue = currentValue;
		this.baseValue = baseValue;
		this.totalValue = baseValue;
		modifiers = new Dictionary<string, FloatModifier>();
	}

	public float CurrentValue {
		get { return currentValue; }
		set { currentValue = Mathf.Clamp(value, 0, totalValue); }
	}

	public float NormalizedCurrentValue {
		get { return currentValue / totalValue; }
		set { currentValue = (Mathf.Clamp01(value) / totalValue); }
	}

	public float Total {
		get { return totalValue; }
	}

	public float BaseTotal {
		get { return baseValue; }
		set {
			baseValue = value;
			float baseAndBonus = baseValue + baseBonus;
			totalValue = baseAndBonus + (baseAndBonus * percentBonus);
		}
	}

	public void SetModifier(string id, FloatModifier modifier) {
		FloatModifier prev;

		if(modifiers.TryGetValue(id, out prev)) {
			baseBonus -= prev.flatBonus;
			percentBonus -= prev.percentBonus;
		}

		if(modifier != null) {
			baseBonus += modifier.flatBonus;
			percentBonus += modifier.percentBonus;
		}

		modifiers[id] = modifier;
		float baseAndBonus = baseValue + baseBonus;
		totalValue = baseAndBonus + (baseAndBonus * percentBonus);
	}

}
