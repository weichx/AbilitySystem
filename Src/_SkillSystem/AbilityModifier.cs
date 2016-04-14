using System;
using System.Collections.Generic;

public abstract class AbilityModifier {

	private List<Ability2> modifiedAbilities = new List<Ability2>();
	private Action remover = delegate {	};

	public IAbilityMatcher matcher;

	public void Apply(Ability2 ability) {
		if(matcher.Match(ability) && !modifiedAbilities.Contains(ability)) {
			modifiedAbilities.Add(ability);
			OnApply(ability);
		}
	}

	public void Remove(Ability2 ability) {
		if(modifiedAbilities.Remove(ability)) {
			OnRemove(ability);
		}
	}

	protected virtual void OnApply(Ability2 ability) {

	}

	protected virtual void OnRemove(Ability2 ability) {

	}

}
