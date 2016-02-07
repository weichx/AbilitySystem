namespace AbilitySystem {

    public class CastQueue {

        public Ability current;
        public Ability queued;
        public Timer gcdTimer;

        private Timer queuedExpire;

        public CastQueue() {
            gcdTimer = new Timer(0); 
            queuedExpire = new Timer(0.2f);
        }

        public Ability UpdateCast() {

            if (queuedExpire.ReadyWithReset(0.2f)) {
                queued = null;
            }

            if (current == null || !gcdTimer.Ready) return null;

            CastState castState = current.Update();

            if (castState == CastState.Invalid) {
                current = null;
                queued = null;
            }
            else if (castState == CastState.Completed) {

                if (current.IsInstant && !current.IgnoreGCD) {
                    gcdTimer.Reset(GetGlobalCooldownTime(current));
                }

                current = queued;
                queued = null;

                if (current != null) {
                    current.Use();
                }

            }

            return current;

        }

        private float GetGlobalCooldownTime(Ability ability) {
            float baseGCD = ability.caster.abilityManager.baseGlobalCooldown;
            return baseGCD + ability.GetAttributeValue("GCDAdjustment");
        }

        public void Enqueue(Ability ability) {
            if (current == null) {
                current = ability;
                current.Use();
            }
            else {
                queued = ability;
            }
        }

        public void Clear() {
            current = null;
            queued = null;
        }
    }
}