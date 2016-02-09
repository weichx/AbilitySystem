using UnityEngine;

namespace AbilitySystem {

    public class TargetedChannelAbilityPrototype : AbilityPrototype {

        public GameObject spell;

        public override void OnTargetSelectionStarted(Ability ability, PropertySet properties) {
            Entity caster = ability.caster;
            Entity target = caster.Target;
            if (target == null) {
                ability.CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastStarted(Ability ability, PropertySet properties) {
            Transform transform = properties.Get<Entity>("Target").transform;
            GameObject gameObject = Instantiate(spell, ability.caster.transform.position, Quaternion.identity) as GameObject;
            properties.Set("SpellInstanceGameObject", gameObject);
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(ability, properties);
            }
        }

        public override void OnCastCancelled(Ability ability, PropertySet properties) {
            base.OnCastCancelled(ability, properties);
        }

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            DestroySpell(ability, properties);
        }

        public void DestroySpell(Ability ability, PropertySet properties) {
            GameObject spellInstance = properties.Get<GameObject>("SpellInstanceGameObject");
            if (spellInstance != null) {
                IAbilityDestructor destructor = spellInstance.GetComponent<IAbilityDestructor>();
                if (destructor != null) {
                    destructor.Destruct(ability, properties);
                }
                Destroy(spellInstance);
            }
        }
    }

}