using System;

namespace AbilitySystem {

    [Serializable]
    public class EntityAttribute : ModifiableAttribute<Entity> {

        public EntityAttribute(string id, ModifiableAttribute<Entity> toClone) : base(id, toClone) { }

    }
}