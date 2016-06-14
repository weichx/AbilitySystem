using System;
using UnityEngine;
using Intelligence;
using UnityEngine.UI;

public class SkillBookEntry : IActionbarItem {

    public PlayerAbilityContextCreator contextCreator;
    [NonSerialized] public PlayerCharacterAction action;
    [HideInInspector] public Ability ability;

    private bool initialized;
    [SerializeField]private AbilityCreator abilityCreator;

    public Sprite Icon {
        get { return ability.icon; }
    }

    public PlayerCharacterAction Action {
        get { return action; }
    }

    public void Initialize(Entity entity) {
        if (initialized) return;
        initialized = true;
        ability = abilityCreator.Create(); //ability does not have an entity right now
        ability.SetCaster(entity);
        action = new PlayerAction_UseSkill(ability, contextCreator);
    }
    
    public void PrepareToolTip() {
        //todo other stuff, maybe make abililty have a tooltip component to override this
        UITooltip.AddTitle(ability.Id);
    }
    
}