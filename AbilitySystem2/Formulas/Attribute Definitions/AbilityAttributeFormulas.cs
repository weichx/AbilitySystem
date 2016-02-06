using System;

namespace AbilitySystem {
    [AttributeUsage(AttributeTargets.Method)]
    public class AbilityAttributeFormula : Attribute { }

    public static class AttributeFormula {

        [Formula]
        public static float Formula1(Ability entity, float baseValue) {
            return baseValue;
        }

        [Formula]
        public static float Formula2(Ability entity, float baseValue) {
            return baseValue * 2f;
        }

        [Formula]
        public static float Cooldown(Ability entity, float baseValue) {
            return UnityEngine.Mathf.Sin(UnityEngine.Time.time);
        }

    }

}