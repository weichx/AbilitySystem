using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;

public static class DamageFormulas {
    public static readonly Dictionary<DiceBase, DiceData> DiceTable = new Dictionary<DiceBase, DiceData> {
        { DiceBase.BASE_1d2, new DiceData(1, 2) },
        { DiceBase.BASE_1d4, new DiceData(1, 4) },
        { DiceBase.BASE_1d6, new DiceData(1, 6) },
        { DiceBase.BASE_1d8, new DiceData(1, 8) },
        { DiceBase.BASE_1d10, new DiceData(1, 10) },
        { DiceBase.BASE_1d12, new DiceData(1, 12) },
        { DiceBase.BASE_2d6, new DiceData(2, 6) },
        { DiceBase.BASE_3d6, new DiceData(3, 6) },
    };

    public static DiceData GenerateDiceResult(DiceData dice, int extraRoll = 0) {
        var r = new System.Random();
        int[] results = new int[dice.RollCnt * 2];
        for(int i = 0; i < (dice.RollCnt * 2) + extraRoll; i++) {
           results[i] = r.Next(dice.MinValue, dice.MaxValue + 1);
        }
        return dice.Final(results);
    }

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
        // if (context.target.GetType() == typeof(Character)) {}
        return (baseValue + 10.0f);
    }

    [Pointable] // sample
    public static int SwordAttack(SingleTargetContext context, DiceBase diceBase) {
        var diceRollFirst = GenerateDiceResult(DiceTable[diceBase]);
        var diceRollSecnond = GenerateDiceResult(DiceTable[diceBase]);
        return diceRollFirst.Result + diceRollSecnond.Result;
    }

    [Pointable]
    public static float Test(Context context) {
        return 0f;
    }
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

