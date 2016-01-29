using System.Collections.Generic;
using Attributes = System.Collections.Generic.Dictionary<string, ModifiableAttribute>;

namespace AbilitySystem {

    public enum CastMode {
        Instant, Cast, Channel, CastToChannel
    }

    public class Ability {

        public readonly string name;
        public readonly Entity caster;
        public readonly CastMode castMode;
        public readonly TagCollection tags;
        public readonly AbilityPrototype prototype;

        public readonly List<AbilityRequirement> requirements;

        protected Timer castTimer;
        protected CastState castState;
        protected Attributes attributes;

        public Ability(Entity caster, AbilityPrototype prototype) {
            this.caster = caster;
            this.prototype = prototype;
            name = prototype.name;
            ValidatePrototype();
            ClonePrototypeAttributes();
            requirements = new List<AbilityRequirement>(prototype.requirements);
            castMode = prototype.castMode;
            SetAttribute("Range", prototype.range.Clone());
            SetAttribute("Cooldown", prototype.cooldown.Clone());
            SetAttribute("CastTime", prototype.castTime.Clone());
            castTimer = new Timer();
        }

        //called once per frame I think
        public CastState Update() {
            CastMode actualCastMode = castMode;
            //if (requirementCheckTimer.ReadyWithReset(RequirementCheckInterval)) {
                for (int i = 0; i < requirements.Count; i++) {
                    if (!requirements[i].CanContinueCast(this, caster)) {
                        //todo log this to game console when there is one
                        UnityEngine.Debug.Log(requirements[i].FailureMessage);
                        castState = CastState.Invalid;
                        return castState;
                    }
                }
           // }

            if (castState == CastState.Targeting) {
                if (prototype.OnTargetSelectionUpdated(this, caster)) {
                    prototype.OnTargetSelectionCompleted(this, caster);
                    prototype.OnCastStarted(this, caster);
                    castState = CastState.Casting;
                    actualCastMode = (GetAttribute("CastTime").Value <= 0f) ? CastMode.Instant : castMode;
                }
            }

            if (castState == CastState.Casting) {
                switch (actualCastMode) {
                    case CastMode.Instant:
                        castState = CastState.Completed;
                        break;
                    case CastMode.Cast:
                        castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                        break;
                    case CastMode.Channel:
                        //if (tickTimer.ReadyWithReset(tickTime)) OnChannelTick(); //todo maybe pass elapsed cast time and total cast time
                        //castState = castTimer.Ready ? CastState.Completed : CastState.Casting;
                        break;
                    case CastMode.CastToChannel:
                        break;
                }
            }

            if (castState == CastState.Completed) {
                prototype.OnCastCompleted(this, caster);
            }

            return castState;
        }

        public ModifiableAttribute GetAttribute(string attrName) {
            return attributes.Get(attrName);
        }

        public bool SetAttribute(string attrName, ModifiableAttribute attr, bool replace = true) {
            if (attrName == null || attr == null) return false;
            var existing = attributes.Get(attrName);
            if(existing != null) {
                if (replace) {
                    attributes[attrName] = attr;
                    return true;
                }
                else {
                    return false;
                }
            }
            attributes[attrName] = attr;
            return true;
        }

        public bool HasAttribute(string attrName) {
            return attributes.Get(attrName) != null;
        }

        public float GetAttributeValue(string attrName) {
            var attr = attributes.Get(attrName);
            if (attr == null) return 0f;
            return attr.Value;
        }

        private void ValidatePrototype() {
            if (prototype.range == null) prototype.range = new ModifiableAttribute("Range", 0f);
            if (prototype.cooldown == null) prototype.cooldown = new ModifiableAttribute("Cooldown", 0f);
            if (prototype.castTime == null) prototype.castTime = new ModifiableAttribute("CastTime", 0f);
            if (prototype.tags == null) prototype.tags = new TagCollection();
            if (prototype.requirements == null) prototype.requirements = new AbilityRequirement[0];
        }

        private void ClonePrototypeAttributes() {
            if (prototype.attributes == null) {
                attributes = new Attributes();
            }
            else {
                attributes = new Attributes(prototype.attributes.Count);
                foreach (var key in prototype.attributes.Keys) {
                    attributes[key] = prototype.attributes[key].Clone();
                }
            }
        }
    }


}