using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbilitySystem {

    public class AbilityPrototype : ScriptableObject {

        public virtual void OnTargetSelectionStarted(PropertySet properties) { }
        public virtual bool OnTargetSelectionUpdated(PropertySet properties) { return true; }
        public virtual void OnTargetSelectionCompleted(PropertySet properties) { }
        public virtual void OnTargetSelectionCancelled(PropertySet properties) { }

        public virtual void OnUse(Ability ability, PropertySet properties) { }
       
        public virtual void OnCastStarted(PropertySet properties) { }
        public virtual void OnCastCompleted(PropertySet properties) { }
        public virtual void OnCastFailed(PropertySet properties) { }
        public virtual void OnCastCancelled(PropertySet properties) { }
        public virtual void OnCastInterrupted(PropertySet properties) { }
        public virtual void OnChannelTick(PropertySet properties) { }
        public virtual void OnChargeConsumed(PropertySet properties) { }

        protected static GameObject SpawnAndInitialize(GameObject toSpawn, Ability ability, PropertySet properties, Vector3? position = null, Quaternion? rotation = null) {
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
                    components[i].Initialize(ability, properties);
                }
            }
            return spawned;
        }

        protected static void DestructAndDespawn(GameObject toDespawn, Ability ability, PropertySet properties) {
            if (toDespawn == null) return;
            IAbilityDestructor[] components = toDespawn.GetComponents<IAbilityDestructor>();
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    components[i].Destruct(ability, properties);
                }
            }
            Destroy(toDespawn);
        }
    }
}