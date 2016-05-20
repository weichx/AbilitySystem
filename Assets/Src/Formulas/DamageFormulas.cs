
public static class DamageFormulas {

    [DamageFormula]
    public static float Slash(Context context, float baseValue) {
        return 10f;
    }

    [DamageFormula]
    public static float ShadowSlash(Context context, float baseValue) {
        return 11;
    }

}