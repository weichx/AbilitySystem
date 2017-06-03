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

    // public void Randomize(int diceCnt, int maxNr) {
    //     var sum;
    //     for(int i=0; i < diceCnt; i++) {
    //         sum += random(1,maxNr);
    //     }
    // }

    // [Pointable]
    // public static float TwoHanded(MultiPointContext context, float baseValue) {
    //     return baseValue;
    // }

    // public static float WornModifier(float baseValue) {
    //     // do something
    //     return (baseValue - 4)
    // }

    // [Pointable]
    // public static float SwordBaseAttack(SingleTargetContext context) {
    //     var diceValue += damageTable[WeaponType.Sword];

    //     damage = WornModifier(damage);

    // }
}

