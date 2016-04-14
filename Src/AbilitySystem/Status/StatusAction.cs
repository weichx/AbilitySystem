
namespace AbilitySystem {

    public abstract class StatusAction : AbilitySystemComponent {

        protected StatusEffect status;
        protected Entity caster;
        protected Entity target;

        public void Initialize(Entity caster, Entity target) {
            this.caster = caster;
            this.target = target;
        }

        public virtual void OnEffectApplied() {

        }

        public virtual void OnEffectUpdated() {

        }

        public virtual void OnEffectStackAdded() {

        }

        public virtual void OnEffectRefreshed() {

        }

        public virtual void OnEffectRemoved() {

        }

        public virtual void OnEffectExpired() {

        }

        public virtual bool OnDispelAttempted() {
            return true;
        }

        public virtual void OnEffectDispelled() {

        }

    }
}