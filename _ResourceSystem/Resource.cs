using System;
using UnityEngine;
using System.Collections.Generic;

//todo use an IModifiable<T> as a base interface for this and intattr / floatattr
//IAdjustable

public class Resource : FloatAttribute {

    protected List<ResourceAdjuster> adjusters;
    public AbilitySystem.TagCollection tags;

    public Resource() : base() {
        adjusters = new List<ResourceAdjuster>();
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
        currentValue = Mathf.Clamp(currentValue, float.MinValue, totalValue);
    }

    public void Decrease(float amount, Context context) {
        float adjustedValue = 0;
        for (int i = 0; i < adjusters.Count; i++) {
            adjustedValue -= adjusters[i].Adjust(amount, this, context);
        }
        currentValue -= adjustedValue;
    }

    public void IncreaseNormalized(float amount, Context context) {
        Increase(Mathf.Clamp01(amount / totalValue), context);
    }

    public void DecreaseNormalized(float amount, Context context) {
        Decrease(Mathf.Clamp01(amount / totalValue), context);
    }

}


public class ArmorAdjustor : ResourceAdjuster {

    public override float Adjust(float delta, Resource resource, Context context) {
        float armor = context.Get<float>("armor");
        return delta - armor;
    }
}