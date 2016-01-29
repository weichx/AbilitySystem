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

    //todo should this take a matcher? maybe an optional one? if we end up storing these
    //on the entity and not the abilities themselves that would make sense, but id 
    //probably need to re-check the matchers occasinally. might be best to provide 
    //both per-attribute ability/status/whatever modifiers and entity level ones with a matcher
    //these might also want to take unique ids. probably dont need these to have any sense of
    //timeout. it might be interesting to have 'dynamic' modifers and 'static' modifers
    //so we can update the dynamic ones occasionally and not worry about the static ones.

    //might have comparison function for overwrites as well, i think this can safely come later
    //and just do a straight replace for now
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