using UnityEngine;
using System;
using System.Collections.Generic;
using AbilitySystem;

public interface IAbilityMatcher {
	bool Match(Ability2 ability);
}


public class AbilityContext {
	
	public readonly List<Entity> targets;
	public readonly List<Vector3> positions;

	protected Ability ability;
	protected Entity caster;
	protected Dictionary<string, object> properties;

	public AbilityContext() {
		targets = new List<Entity>();
		positions = new List<Vector3>();
		properties = new Dictionary<string, object>();
	}

	public void Initialize(Ability ability) {
		this.ability = ability;
		caster = ability.caster;
	}

	public void Clear() {
		targets.Clear();
		positions.Clear();
		targets.Clear();
	}

	public Ability Ability {
		get { return ability; }
	}

	public Entity Caster {
		get { return caster; }
	}

	public T Get<T>(string propertyName) {
		return (T)properties.Get(propertyName);
	}

	public object Get(string propertyName) {
		return properties.Get(propertyName);
	}

	public void Set<T>(string propertyName, T value) {
		properties[propertyName] = value;
	}

	public void Set(string propertyName, object value) {
		properties[propertyName] = value;
	}

	public bool Has(string propertyName) {
		return properties.ContainsKey(propertyName);
	}
		
}

public partial class Ability2 {

	public bool Usable() {
		
	}

	public bool Use(AbilityContext context) {
		if(!Usable()) {
			return false;
		}
	}

	public bool Use() {
	
	}

}

