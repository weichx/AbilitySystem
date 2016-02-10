using UnityEngine;

namespace AbilitySystem {

    public class TargetedChannelAbilityPrototype : Ability {

        public GameObject spellPrefab;
        [Writable(false)] public GameObject spellInstance;
        [Writable(false)] public Entity target;

        public override void OnTargetSelectionStarted() {
            target = caster.Target;
            if (target == null) {
                CancelCast();
                return;
            }
        }

        public override void OnCastStarted() {
            Transform targetTransform = target.transform;
            spellInstance = Instantiate(spellPrefab, caster.transform.position, Quaternion.identity) as GameObject;
            IAbilityInitializer initializer = spellInstance.GetComponent<IAbilityInitializer>();
            if (initializer != null) {
                initializer.Initialize(this);
            }
        }

        public override void OnCastCancelled() {
            DestroySpell(this);
        }

        public override void OnCastCompleted() {
            DestroySpell(this);
        }

        public void DestroySpell(Ability ability) {
            if (spellInstance != null) {
                IAbilityDestructor destructor = spellInstance.GetComponent<IAbilityDestructor>();
                if (destructor != null) {
                    destructor.Destruct(ability);
                }
                Destroy(spellInstance.gameObject);
            }
        }
    }

}