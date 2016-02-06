using UnityEngine;

namespace AbilitySystem {
    public abstract class AbilityInitializer : MonoBehaviour {

        public abstract void Initialize(Ability ability, PropertySet properties);

    }
}