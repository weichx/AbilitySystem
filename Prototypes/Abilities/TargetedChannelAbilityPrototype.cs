using UnityEngine;

namespace AbilitySystem {

    public class TargetedChannelAbilityPrototype : Ability {

        public GameObject spell;

        public override void OnTargetSelectionStarted(PropertySet properties) {
            Entity target = caster.Target;
            if (target == null) {
                CancelCast();
                return;
            }
            properties.Set("Target", target);
        }

        public override void OnCastStarted(PropertySet properties) {
            Transform transform = properties.Get<Entity>("Target").transform;
            GameObject gameObject = Instantiate(spell, caster.transform.position, Quaternion.identity) as GameObject;
            properties.Set("SpellInstanceGameObject", gameObject);
            IAbilityInitializer initializer = gameObject.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(this, properties);
            }
        }

        public override void OnCastCancelled(PropertySet properties) {
            base.OnCastCancelled(properties);
        }

        public override void OnCastCompleted(PropertySet properties) {
            DestroySpell(this, properties);
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