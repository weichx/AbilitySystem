using UnityEngine;

namespace AbilitySystem {

    [DisallowMultipleComponent]
    public abstract class TargetingStrategy : AbilitySystemComponent {

        protected Entity caster;
        protected Ability ability;

        public void __Initialize(Ability ability) {
            this.ability = ability;
            caster = ability.caster;
            OnInitialize();
        }

        public virtual void OnInitialize() { }
        public virtual void OnTargetSelectionStarted() { }
        public virtual bool OnTargetSelectionUpdated() { return true; }
        public virtual void OnTargetSelectionCompleted() { }
        public virtual void OnTargetSelectionCancelled() { }

    }

}