using UnityEngine;
using System.Collections.Generic;

///<summary>
///A modifiable variable that is bounded by a modifiable min and max 
///</summary>

namespace EntitySystem {
	
	public class FloatRange {

	    [HideInInspector] [SerializeField] protected List<FloatModifier> modifiers;

	    [HideInInspector] [SerializeField] protected float baseValue;
	    [HideInInspector] [SerializeField] protected float currentValue;
	    [HideInInspector] [SerializeField] protected float flatBonus;
	    [HideInInspector] [SerializeField] protected float percentBonus;

	    [SerializeField] protected FloatRangeBoundry min;
	    [SerializeField] protected FloatRangeBoundry max;

	    public FloatRange() : this(0, float.MinValue, float.MaxValue) { }

	    public FloatRange(float value, float minBase = float.MinValue, float maxBase = float.MaxValue) {
	        modifiers = new List<FloatModifier>();
	        min = new FloatRangeBoundry(this, minBase);
	        max = new FloatRangeBoundry(this, maxBase);
	    }

	    public void SetModifier(string id, FloatModifier modifier) {
	        modifier = new FloatModifier(id, modifier);

	        for (int i = 0; i < modifiers.Count; i++) {
	            if (modifiers[i].id == id) {
	                FloatModifier prev = modifiers[i];
	                flatBonus -= prev.flatBonus;
	                percentBonus -= prev.percentBonus;
	                break;
	            }
	        }

	        modifiers.Add(modifier);

	        flatBonus += modifier.flatBonus;
	        percentBonus += modifier.percentBonus;

	        BaseValue = BaseValue; //weird but works

	    }

	    public FloatModifier[] GetReadOnlyModiferList() {
	        return modifiers.ToArray();
	    }

	    public void RemoveModifier(string id) {
	        for (int i = 0; i < modifiers.Count; i++) {
	            if (modifiers[i].id == id) {
	                FloatModifier prev = modifiers[i];
	                flatBonus -= prev.flatBonus;
	                percentBonus -= prev.percentBonus;
	                BaseValue = BaseValue;
	                break;
	            }
	        }
	    }

	    public FloatRangeBoundry Min {
	        get { return min; }
	    }

	    public FloatRangeBoundry Max {
	        get { return max; }
	    }

	    public float BaseValue {
	        get { return baseValue; }
	        set {
	            if (baseValue != value) {
	                baseValue = value;
	                float minVal = min.Value;
	                float maxVal = max.Value;
	                float flatTotal = baseValue + flatBonus;
	                float total = flatTotal + (flatTotal * percentBonus);
	                currentValue = Mathf.Clamp(total, minVal, maxVal);
	            }
	        }
	    }

	    public float Value {
	        get {
	            float minVal = min.Value;
	            float maxVal = max.Value;
	            float flatTotal = baseValue + flatBonus;
	            float total = flatTotal + (flatTotal * percentBonus);
	            currentValue = Mathf.Clamp(total, minVal, maxVal);
	            return currentValue;
	        }
	        set {
	            currentValue = Mathf.Clamp(value, min.Value, max.Value);
	        }
	    }

	    //todo -- get
	    public float NormalizedValue {
	        set {
	            float val = Mathf.Clamp01(value);
	            float flatTotal = baseValue + flatBonus;
	            float total = flatTotal + (flatTotal * percentBonus);
	            currentValue = Mathf.Clamp(val * total, min.Value, max.Value);
	        }
	    }

	    public class FloatRangeBoundry : EntitySystem.FloatValue {

	        [HideInInspector] public FloatRange parent;

	        public FloatRangeBoundry() : base(0) {
	            parent = null;
	        }

	        public FloatRangeBoundry(FloatRange parent, float baseValue = 0f) : base(baseValue) {
	            this.parent = parent;
	        }

	        public override float BaseValue {
	            get { return base.BaseValue; }
	            set {
	                base.BaseValue = value;
	                parent.Value = parent.Value;
	            }
	        }

	    }
	}
}
