namespace AbilitySystem {

    public class CastQueue {
        public Ability current;
        private Ability queued;
        private Timer gcdTimer;
        private Timer queuedExpire;

        public CastQueue() {
            gcdTimer = new Timer(); //todo gcd needs to be configurable
            queuedExpire = new Timer(0.2f);
        }

        public Ability UpdateCast() {
            Ability retn = current;

            if (queuedExpire.ReadyWithReset(0.2f)) {
                queued = null;
            }

            if (current == null) return null;
            
            CastState castState = current.Update();

            if (castState == CastState.Invalid) {
                current = null;
                queued = null;
            }
            else if (castState == CastState.Completed) {
                current = queued;
                queued = null;

                if (current == null) return retn;

                if (current.IsInstant && !current.IgnoreGCD) {
                    gcdTimer.Reset(GetGlobalCooldownTime(retn));
                }
                else {
                    current.Use();
                }

            }

            return retn;

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