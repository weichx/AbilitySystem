using System;
using UnityEngine;

namespace AbilitySystem {

    [Serializable]
    public class AbilityAction {

        public virtual void OnTargetSelectionStarted(Ability ability, PropertySet properties) { }
        public virtual bool OnTargetSelectionUpdated(Ability ability, PropertySet properties) { return true; }
        public virtual void OnTargetSelectionCompleted(Ability ability, PropertySet properties) { }
        public virtual void OnTargetSelectionCancelled(Ability ability, PropertySet properties) { }

        public virtual void OnUse(Ability ability, PropertySet properties) { }

        public virtual void OnCastStarted(Ability ability, PropertySet properties) { }
        public virtual void OnCastCompleted(Ability ability, PropertySet properties) { }
        public virtual void OnCastFailed(Ability ability, PropertySet properties) { }
        public virtual void OnCastCancelled(Ability ability, PropertySet properties) { }
        public virtual void OnCastInterrupted(Ability ability, PropertySet properties) { }
        public virtual void OnChannelTick(Ability ability, PropertySet properties) { }
        public virtual void OnChargeConsumed(Ability ability, PropertySet properties) { }
        
    }

}