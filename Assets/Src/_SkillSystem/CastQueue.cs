namespace AbilitySystem {

    public class CastQueue {

        private Ability currentAbility;
        private Ability queuedAbility;

        private OldContext currentContext;
        private OldContext queuedContext;

        public Timer gcdTimer;

        private Timer queuedExpire;

        public CastQueue() {
            gcdTimer = new Timer(0); 
            queuedExpire = new Timer(0.2f);
        }

        public Ability UpdateCast() {

            if (queuedExpire.ReadyWithReset(0.2f)) {
                queuedAbility = null;
            }

            if (currentAbility == null || !gcdTimer.Ready) return null;

            CastState castState = currentAbility.UpdateCast();

            if (castState == CastState.Invalid) {
                Clear();
            }
            else if (castState == CastState.Completed) {

                if (currentAbility.IsInstant && !currentAbility.IgnoreGCD) {
                    gcdTimer.Reset(GetGlobalCooldownTime(currentAbility));
                }

                currentAbility = queuedAbility;
                currentContext = queuedContext;
                queuedAbility = null;
                queuedContext = null;

                if (currentAbility != null && !currentAbility.Use(currentContext)) {
                    currentAbility = null;
                    currentContext = null;
                }

            }

            return currentAbility;

        }

        private float GetGlobalCooldownTime(Ability ability) {
            float baseGCD = 0;// ability.caster.abilityManager.baseGlobalCooldown;
            return baseGCD + 0;// ability.GetAttributeValue("GCDAdjustment");
        }

        public void Enqueue(Ability ability, OldContext context) {
            if (ability == null || context == null) return;
            if (currentAbility == null) {
                currentAbility = ability;
                currentContext = context;
                currentAbility.Use(context);
            }
            else {
                queuedAbility = ability;
                queuedContext = context;
            }
        }

        public void Clear() {
            currentAbility = null;
            currentContext = null;
            queuedAbility = null;
            queuedContext = null;
        }

        public OldContext CurrentContext {
            get { return currentContext; }
        }

        public Ability CurrentAbility {
            get { return currentAbility; }
        }
    }
}

//public class AttrMod {
//    public string id;
//    public string name;
//    public string[] tags;
//    public string category;
//    public string type;
//    public string source;
//    public float duration;
//    public float baseModifierValue;
//    public float baseClampValue;
//    public string modFnPointer;
//    public string clampFnPointer;
//}

//public class AbilAction {

//}