using System;
using UnityEngine;
using System.Collections.Generic;

//todo use an IModifiable<T> as a base interface for this and intattr / floatattr
//IAdjustable

public class Resource : FloatAttribute {

    public string name;
    public List<ResourceAdjuster> adjustors;

    public Resource() : base() {
        adjustors = new List<ResourceAdjuster>();
    }

    public void Increase(Context context, float amount) {
        float adjustedValue = 0;
        for (int i = 0; i < adjustors.Count; i++) {
            adjustedValue += adjustors[i].Adjust(amount, this, context);
        }
        currentValue += adjustedValue;
        currentValue = Mathf.Clamp(currentValue, float.MinValue, totalValue);
    }

    public void Decrease(Context context, float amount) {
        float adjustedValue = 0;
        for (int i = 0; i < adjustors.Count; i++) {
            adjustedValue -= adjustors[i].Adjust(amount, this, context);
        }
        currentValue -= adjustedValue;
    }

    public void IncreaseNormalized(Context context, float amount) {
        Increase(context, Mathf.Clamp01(amount / totalValue));
    }

    public void DecreaseNormalized(Context context, float amount) {
        Decrease(context, Mathf.Clamp01(amount / totalValue));
    }

}

//todo maybe this is an interface
public abstract class ResourceAdjuster {

    public string id;

    public abstract float Adjust(float delta, Resource resource, Context context);

}

public class ArmorAdjustor : ResourceAdjuster {

    public override float Adjust(float delta, Resource resource, Context context) {
        float armor = context.Get<float>("armor");
        return delta - armor;
    }
}