using UnityEngine;
using System.Collections.Generic;

//todo -- maybe put all of these in one asset and have a nice editor
//for working with them individually / searching through them
public class AbilityPrototype : ScriptableObject {

    public CastType castType = CastType.Cast;
    public float castTime = 1.0f;
    public float tickTime = 0.5f;
    public float cooldown = 6.0f;
    public float range = 40f;
    Dictionary<string, ModifiableAttribute> attributes;

    public virtual string Id {
        get { return Name; }
    }

    public virtual string Tooltip {
        get { return "Tooltip"; }
    }

    public virtual string Name {
        get { return name; }
    }

    public virtual Ability CreateAbility(Entity caster) {
        var ability = new Ability(caster);
        ability.castTime = new ModifiableAttribute(castTime);
        ability.cooldown = new ModifiableAttribute(cooldown);
        ability.range = new ModifiableAttribute(range);
        return ability;
    }

    public virtual void OnCastCompleted(Entity caster, Ability ability) {
        if(ability.GetAttribute("Speed").Value > 0) {
            //ability.caster.ApplyStatus("Slow");
        }
    }

}