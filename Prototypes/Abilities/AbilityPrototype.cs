using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbilitySystem {

    public class AbilityPrototype : ScriptableObject {

        public Sprite icon;
        public CastMode castMode;

        [Visible("ShowIgnoreGCD")]
        public bool ignoreGCD;

        [Space()]
        [Visible("ShowCastTime")] public AbilityAttribute castTime;
        [Visible("ShowChannelTime")] public AbilityAttribute channelTime;
        [Visible("ShowChannelTime")] public AbilityAttribute channelTicks;
        public AbilityAttribute cooldown;
        public AbilityAttribute charges;
        public AbilityAttribute chargeUseCooldown;

        [Space()]

        public AbilityRequirementSet requirementSet;
        public TagCollection tags;

        public AbilityAttributeSet attributeSet;

        public void Reset() {
            //todo ensure name is unique and base attributes exist
            if (charges != null && charges.BaseValue == 0) charges.BaseValue = 1;
        }

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

        //User should never have to override this unless they choose to provide a custom 
        //class other than ability, which is hopefully rare since this is built to be easy
        public virtual Ability CreateAbility(Entity caster) {
            return new Ability(caster, this);
        }

        public static bool ShowCastTime(AbilityPrototype proto) {
            return proto.castMode == CastMode.Cast || proto.castMode == CastMode.CastToChannel;
        }

        public static bool ShowChannelTime(AbilityPrototype proto) {
            return proto.castMode == CastMode.Channel || proto.castMode == CastMode.CastToChannel;
        }

        public static bool ShowIgnoreGCD(AbilityPrototype proto) {
            return proto.castMode == CastMode.Instant;
        }

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