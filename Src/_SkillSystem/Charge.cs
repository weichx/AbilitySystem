using UnityEngine;

public class Charge {

	public float lastUsed;
	public float cooldown;
	public FloatAttribute cooldownAttr;

	public Charge(float cooldown = 0f, bool ready = true) {
		lastUsed = ready ? -1 : Timer.GetTimestamp;
		this.cooldown = cooldown;
		cooldownAttr = new FloatAttribute();
	}

	public bool OnCooldown {
		get { 
			return Timer.GetTimestamp - lastUsed >= cooldown; 
		}
	}

	public float Cooldown {
		get { return cooldown; }
		set { cooldown = Mathf.Clamp(value, 0, float.MaxValue); }
	}

}