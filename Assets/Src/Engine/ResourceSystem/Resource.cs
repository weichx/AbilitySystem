using System;
using UnityEngine;
using System.Collections.Generic;
using Intelligence;

namespace EntitySystem.Resources {
	///<summary>
	///Responsible for things like armor, essentially a pipeline of adjustments just like modifiers
	///might have one to take double damage or half damage or negate armor or double xp rate etc.
	///</summary>
	public class Resource : FloatRange {
	    
	    public string resourceId; //todo depricate this
	    [NonSerialized] protected Action<float, float> watchers;

	    protected List<ResourceAdjuster> adjusters;
	    public TagCollection tags;
		public FloatRange regenerationRate;

	    public Resource() : base() {
	        adjusters = new List<ResourceAdjuster>();
	        watchers = delegate (float a, float b) { };
			regenerationRate = new FloatRange();
	    }

		public virtual void Tick() {
			float regenRate = regenerationRate.Value;
			Value += regenRate * Time.deltaTime; //todo -- tick rate
		}

	    public void AddAdjuster(ResourceAdjuster adjuster) {
	        adjusters.Add(adjuster);
	    }

	    public bool HasAdjuster(ResourceAdjuster adjuster) {
	        return adjusters.Contains(adjuster);
	    }

	    public bool HasAdjuster(string adjusterId) {
	        for (int i = 0; i < adjusters.Count; i++) {
	            if (adjusters[i].resourceId == adjusterId) return true;
	        }
	        return false;
	    }

	    public bool RemoveAdjuster(ResourceAdjuster adjuster) {
	        return adjusters.Remove(adjuster);
	    }

	    public bool RemoveAdjuster(string adjusterId) {
	        for (int i = 0; i < adjusters.Count; i++) {
	            if (adjusters[i].resourceId == adjusterId) {
	                adjusters.RemoveAt(i);
	                return true;
	            }
	        }
	        return false;
	    }

	    public void Increase(float amount, Context context) {
	        float adjustedValue = 0;
	        for (int i = 0; i < adjusters.Count; i++) {
	            adjustedValue += adjusters[i].Adjust(amount, this, context);
	        }
	        currentValue += adjustedValue;
	        //currentValue = Mathf.Clamp(currentValue, float.MinValue, baseTotal);
	    }

	    public void Decrease(float amount, Context context) {
	        float adjustedValue = 0;
	        for (int i = 0; i < adjusters.Count; i++) {
	            adjustedValue -= adjusters[i].Adjust(amount, this, context);
	        }
	        currentValue -= adjustedValue;
	    }

	    public void IncreaseNormalized(float amount, Context context) {
	        //Increase(Mathf.Clamp01(amount / baseTotal), context);
	    }

	    public void DecreaseNormalized(float amount, Context context) {
	       // Decrease(Mathf.Clamp01(amount / baseTotal), context);
	    }

	    public void AddWatcher(Action<float, float> watcher) {
	        watchers += watcher;
	    }

	    public void RemoveWatcher(Action<float, float> watcher) {
	        watchers -= watcher;
	    }

	}

}