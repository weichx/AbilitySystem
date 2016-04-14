using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour {

	public List<Ability2> abilities;
	public List<AbilityModifier> abilityModifiers;
	public event AbilityEvent OnUpdate;

	public void Awake() {
		abilities = new List<Ability2>();
		abilityModifiers = new List<AbilityModifier>();
	}

	public void Update() {
		OnUpdate(null);
	}

	public void AddAbilityModifier(AbilityModifier mod) {
		for(int i = 0; i < abilities.Count; i++) {
			mod.Apply(abilities[i]);
		}
	}

	public void RemoveAbilityModifier(AbilityModifier mod) {
		for(int i = 0; i < abilities.Count; i++) {
			mod.Remove(abilities[i]);
		}
	}

}