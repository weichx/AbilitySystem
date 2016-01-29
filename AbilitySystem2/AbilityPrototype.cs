using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttributeSet {
    public List<ModifiableAttribute> attrs = new List<ModifiableAttribute>();
}

namespace AbilitySystem {

    public class AbilityPrototype : ScriptableObject {

        public ModifiableAttribute cooldown;
        public ModifiableAttribute castTime;
        public ModifiableAttribute range;

        public AbilityRequirement[] requirements;
        public TagCollection tags;
        //public ResourceCost[] resourceCosts;

        public CastMode castMode;

        //todo this needs to be serialized
        public Dictionary<string, ModifiableAttribute> attributes;

        public AttributeSet attrSet;

        public void OnValidate() {
            //todo ensure name is unique and base attributes exist
        }

        public virtual void OnTargetSelectionStarted(Ability ability, Entity caster) { }
        public virtual bool OnTargetSelectionUpdated(Ability ability, Entity caster) { return true; }
        public virtual void OnTargetSelectionCompleted(Ability ability, Entity caster) { }
        public virtual void OnTargetSelectionCancelled(Ability ability, Entity caster) { }

        public virtual void OnUse(Ability ability, Entity caster) { }

        public virtual void OnCastStarted(Ability ability, Entity caster) { }
        public virtual void OnCastCompleted(Ability ability, Entity caster) { }
        public virtual void OnCastFailed(Ability ability, Entity caster) { }
        public virtual void OnCastCancelled(Ability ability, Entity caster) { }
        public virtual void OnCastInterrupted(Ability ability, Entity caster) { }
        public virtual void OnChannelTick(Ability ability, Entity caster) { }
        public virtual void OnSpendResources(Ability ability, Entity caster) { }

        //User should never have to override this unless they choose to provide a custom 
        //class other than ability, which is hopefully rare since this is build to be easy
        public virtual Ability CreateAbility(Entity caster, AbilityPrototype prototype) {
            return new Ability(caster, this);
        }
    }
}