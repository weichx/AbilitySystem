using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[EntityDeserializerSkipConstructor]
public class FloatVariable {

	[SerializeField] protected FloatAttribute min;
	[SerializeField] protected FloatAttribute max;

	protected float current;

	public FloatVariable() {
		min = new FloatAttribute(float.MinValue);
		max = new FloatAttribute(float.MaxValue);
		current = 0;
	}

	public FloatVariable(float currentValue) {
		min = new FloatAttribute(float.MinValue);
		max = new FloatAttribute(float.MaxValue);
		current = currentValue;
	}

	public FloatVariable(float minValue, float maxValue, float currentValue) {
		min = new FloatAttribute(minValue);
		max = new FloatAttribute(maxValue);
		current = Mathf.Clamp(currentValue, min.Value, max.Value);
	}

	public float Value {
		get { return current; }
		set {
			current = Mathf.Clamp(value, min.Value, max.Value);
		}
	}
		
	public FloatAttribute Min {
		get { return min; }
		set { 
			min = value ?? min;
			current = Mathf.Clamp(current, min.Value, max.Value);
		}
	}

	public FloatAttribute Max {
		get { return max; }
		set { 
			max = value ?? max;
			current = Mathf.Clamp(current, min.Value, max.Value);
		}
	}
}

