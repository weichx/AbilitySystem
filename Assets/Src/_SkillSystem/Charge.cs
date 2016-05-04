using System;
using UnityEngine;

[Serializable]
[EntityDeserializerSkipConstructor]
public class Charge {

    private float lastUsed;
    public float cooldown;
    public FloatAttribute cooldownAttr;
    //todo this might also be responsible for cast time / channel time / tick time

    public Charge() {
        cooldownAttr = new FloatAttribute();
        lastUsed = -1;
    }

    public Charge(float cooldown, bool ready = true)  : this() {
        lastUsed = ready ? -1 : Timer.GetTimestamp;
        this.cooldown = cooldown;
    }

    public bool OnCooldown {
        get {
            if (lastUsed == -1) return false;
            return Timer.GetTimestamp - lastUsed < cooldown;
        }
    }

    public float Cooldown {
        get { return cooldown; }
        set { cooldown = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public void Expire() {
        lastUsed = Timer.GetTimestamp;
    }

    //public void OnDeserialized(Dictionary<string, object> table) {
    //    Debug.Log(cooldownAttr.Value + " -> attr value");
    //}
}