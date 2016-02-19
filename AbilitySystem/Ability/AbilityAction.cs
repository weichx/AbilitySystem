using UnityEngine;

namespace AbilitySystem {

    public abstract class AbilityAction : AbilitySystemComponent {

        protected Entity caster;
        protected Ability ability;

        public void Initialize(Ability ability) {
            this.ability = ability;
            caster = ability.caster;
        }

        public virtual void OnTargetSelectionStarted() { }
        public virtual bool OnTargetSelectionUpdated() { return true; }
        public virtual void OnTargetSelectionCompleted() { }
        public virtual void OnTargetSelectionCancelled() { }

        public virtual void OnUse() { }

        public virtual void OnCastStarted() { }
        public virtual void OnCastCompleted() { }
        public virtual void OnCastFailed() { }
        public virtual void OnCastCancelled() { }
        public virtual void OnCastInterrupted() { }
        public virtual void OnChannelTick() { }
        public virtual void OnChargeConsumed() { }

    }

}