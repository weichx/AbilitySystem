using UnityEngine;
using System.Collections.Generic;

//this whole class is a prototype 

[RequireComponent(typeof(Entity))]
public class EntityProgression : MonoBehaviour {

	protected Entity entity;
	protected Dictionary<int, float> xpRequirements;

	void Start() {
		entity = GetComponent<Entity>();
		entity.resourceManager.GetResource("XP").AddWatcher(OnXPGained);
	}

	private void OnXPGained(float old, float total) {
		float requirement = xpRequirements.Get(10);
		if(total >= requirement) {
			//do level up -> run some script probably
			//entity.GetAttribute("Health").BaseValue = 200;
			//entity.EventManager.Trigger("LevelUp", new LevelUpEvent(entity));
		}
	}
}

/*

Entity
	Name
	Entity Definition
		Tags
		Resources
		Attributes
	Equipment Definition
	Intelligence Definition
		Skill Set
		Action Set
		Pathfinding Profile
	Progression Definition

entity:
	1: {
		"attributes": {
			Strength: 35,
			Agility: 40
		},
		"secondaryAttributes": {
		},
		"defensive": {
		},
		"offensive": {
		},
		"general": {
		}
		"resources": {
			Health: 400,
			Mana: 4000
		},
		"skillSet": {
			Backstab: true,
		},
		"formula": "ProgressionFormulaName"
	}
*/

