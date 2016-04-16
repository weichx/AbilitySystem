using System;
using System.Collections.Generic;

public abstract class AbilityModifier {

    protected List<Ability> modifiedAbilities;

    public IAbilityMatcher matcher;

    [NonSerialized]
    private bool initialized;

    public void Initialize() {
        if (initialized) return;
        initialized = true;
        modifiedAbilities = new List<Ability>();
        OnInitialize();
    }

    public void Apply(Ability ability) {
        if ((matcher == null || matcher.Match(ability)) && !modifiedAbilities.Contains(ability)) {
            modifiedAbilities.Add(ability);
            OnApply(ability);
        }
    }

    public void Update() {
        OnUpdate();
    }

    public void Remove(Ability ability) {
        if (modifiedAbilities.Remove(ability)) {
            OnRemove(ability);
        }
    }

    protected virtual void OnInitialize() {

    }

    protected virtual void OnApply(Ability ability) {

    }

    protected virtual void OnUpdate() {

    }

    protected virtual void OnRemove(Ability ability) {

    }

}
