using System;

[AttributeUsage(AttributeTargets.Method)]
public class AbilityAttributeFormula : Attribute {}

public static class AttributeFormula {

        [AbilityAttributeFormula]
        public static float Formula1(Entity entity, float baseValue) { return 1f; }

        [AbilityAttributeFormula]
        public static float Formula2(Entity entity) { return 2f; }

        [AbilityAttributeFormula]
        public static float Formula3(Entity entity) { return 3f; }


}

