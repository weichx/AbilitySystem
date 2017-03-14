using Intelligence;
using EntitySystem;

public static class DamageFormulas {
    [Pointable]
    public static float Fn(float f) {
        return 0;
    }

    [Pointable]
    public static float Slash(SingleTargetContext context, float baseValue) {
        return 10f;
    }

    [Pointable]
    public static float ShadowSlash(SingleTargetContext context, float baseValue) {
        return 11;
    }

    [Pointable]
    public static float Strike(SingleTargetContext context, float baseValue) {
        if (context.target.GetType() == typeof(Character)) {}
        return (baseValue + 10.0f);
    }
}