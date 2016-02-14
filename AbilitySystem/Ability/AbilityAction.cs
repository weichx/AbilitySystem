using System;
using UnityEngine;

namespace AbilitySystem {

    public class AbilityAction : MonoBehaviour {
        public string name2 = "yay";

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

    //public class DamageOverTime : AbilityAction { }
}