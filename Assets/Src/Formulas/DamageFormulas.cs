
public static class DamageFormulas {
    [Pointable]
    public static float Fn(float f) {
        return 0;
    }

    [DamageFormula]
    public static float Slash(OldContext context, float baseValue) {
        return 10f;
    }

    [DamageFormula]
    public static float ShadowSlash(OldContext context, float baseValue) {
        return 11;
    }

}