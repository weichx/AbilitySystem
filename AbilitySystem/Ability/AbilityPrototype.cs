using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbilitySystem {

    public class AbilityPrototype : ScriptableObject {

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

        protected static GameObject SpawnAndInitialize(GameObject toSpawn, Ability ability, Vector3? position = null, Quaternion? rotation = null) {
            if(position == null) {
                position = ability.caster.transform.position;
            }
            if(rotation == null) {
                rotation = ability.caster.transform.rotation;
            }

            GameObject spawned = Instantiate(toSpawn, (Vector3)position, (Quaternion)rotation) as GameObject;
            IAbilityInitializer[] components = spawned.GetComponents<IAbilityInitializer>();
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    components[i].Initialize(ability);
                }
            }
            return spawned;
        }

        protected static void DestructAndDespawn(GameObject toDespawn, Ability ability) {
            if (toDespawn == null) return;
            IAbilityDestructor[] components = toDespawn.GetComponents<IAbilityDestructor>();
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    components[i].Destruct(ability);
                }
            }
            Destroy(toDespawn);
        }
    }
}