using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {

    [Serializable] // just to let us draw an inspector
    public class AbilityRequirementSet {
        public List<AbilityRequirement> requirements = new List<AbilityRequirement>();
    }

    [Serializable]
    public class AbilityRequirementShell {
        public string id;
        public RequirementPrototype prototype;

        [SerializeField]
        private int type = 0; //cant use enum flags as type...

        public RequirementType RequirementType {
            get {
                switch (type) {
                    case 0: return RequirementType.CastStart;
                    case 1: return RequirementType.CastUpdate;
                    case 2: return RequirementType.CastComplete;
                    case 3: return RequirementType.StartAndUpdate;
                    case 4: return RequirementType.StartAndEnd;
                    case 5: return RequirementType.UpdateAndEnd;
                    case 6: return RequirementType.All;
                    default: return RequirementType.CastStart;
                }
            }
        }

        //public AbilityRequirement ToRequirement() {
        //    return new AbilityRequirement(id, prototype, RequirementType);
        //}

        public static string[] Options = {
            "Start", "Update", "Complete", "Start + Update", "Start + End", "Update + End", "All"
        };
    }
    
    [Serializable]
    public class AbilityRequirement : ISerializationCallbackReceiver {

        public string id;

        protected bool supressed;
        protected RequirementType appliesTo;
        [SerializeField] protected RequirementPrototype prototype;
        [SerializeField] private int type = 0; //cant use enum flags as type...

        public RequirementType RequirementType {
            get {
                switch (type) {
                    case 0: return RequirementType.CastStart;
                    case 1: return RequirementType.CastUpdate;
                    case 2: return RequirementType.CastComplete;
                    case 3: return RequirementType.StartAndUpdate;
                    case 4: return RequirementType.StartAndEnd;
                    case 5: return RequirementType.UpdateAndEnd;
                    case 6: return RequirementType.All;
                    default: return RequirementType.CastStart;
                }
            }
        }
     
        public static string[] Options = {
            "Start", "Update", "Complete", "Start + Update", "Start + End", "Update + End", "All"
        };

        //public AbilityRequirement(string id, RequirementPrototype prototype, RequirementType appliesTo) {
        //    this.id = id;
        //    this.prototype = prototype;
        //    this.appliesTo = appliesTo;
        //    if (prototype == null) {
        //        prototype = RequirementPrototype.Default();
        //    }
        //}

        public bool MeetsRequirement(Ability ability, RequirementType type) {
            if (prototype == null || supressed || (type & RequirementType) == 0) return true;

            bool requirementMet = prototype.MeetsRequirement(ability);

            if (requirementMet) {
                prototype.OnRequirementPassed(ability, this, type);
            }
            else {
                prototype.OnRequirementFailed(ability, this, type);
            }
            return requirementMet;
        }

        public void ApplyTo(RequirementType type) {
            appliesTo |= type;
        }

        public void DoNotApplyTo(RequirementType type) {
            appliesTo &= ~type;
        }

        public bool AppliesTo(RequirementType type) {
            return (type & appliesTo) != 0;
        }

        public void OnBeforeSerialize() {
            
        }

        public void OnAfterDeserialize() {
            appliesTo = RequirementType;
        }

        public bool IsSupressed {
            get { return supressed; }
            set { supressed = value; }
        }

    }

}