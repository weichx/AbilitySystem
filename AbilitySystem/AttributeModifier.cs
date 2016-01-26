using UnityEngine;

public class AttributeModifier {

    public float min;
    public float max;
    public float value;

    public AttributeModifier(float value) {
        this.value = value;
        this.min = 0f;
        this.max = float.MaxValue;
    }

    public AttributeModifier(float value = 0f, float min = 0, float max = float.MaxValue) {
        this.value = value;
        this.min = min;
        this.max = max;
    }

    public virtual float ModifyValue(float value, float baseValue) {
        return value;
    }

    public virtual float ClampValue(float value, float baseValue) {
        return Mathf.Clamp(value, min, max);
    }

}