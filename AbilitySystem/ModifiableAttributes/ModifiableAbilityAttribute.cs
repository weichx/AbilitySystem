using System;

namespace AbilitySystem {

    [Serializable]
    public class AbilityAttribute : ModifiableAttribute<Ability> {

        public AbilityAttribute(string id, ModifiableAttribute<Ability> toClone) : base(id, toClone) { }

    }
}