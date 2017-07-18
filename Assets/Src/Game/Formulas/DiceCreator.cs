using System.Collections.Generic;

public class DiceCreator {
    public readonly Dictionary<DiceBase, DiceData> DiceTable = new Dictionary<DiceBase, DiceData> {
        { DiceBase.BASE_1d2, new DiceData(1, 2) },
        { DiceBase.BASE_1d4, new DiceData(1, 4) },
        { DiceBase.BASE_1d6, new DiceData(1, 6) },
        { DiceBase.BASE_1d8, new DiceData(1, 8) },
        { DiceBase.BASE_1d10, new DiceData(1, 10) },
        { DiceBase.BASE_1d12, new DiceData(1, 12) },
        { DiceBase.BASE_2d6, new DiceData(2, 6) },
        { DiceBase.BASE_3d6, new DiceData(3, 6) },
    };

    public DiceData this[DiceBase baseValue] {
        get {
            DiceData dice = DiceTable[baseValue];
            var r = new System.Random();
            int[] results = new int[dice.RollCnt * 2];
            for(int i = 0; i < (dice.RollCnt * 2); i++) {
                results[i] = r.Next(dice.MinValue, dice.MaxValue + 1);
            }
            return dice.Final(results);
        }
    }

    public DiceData GenerateDiceResult(DiceData dice, int extraRoll = 0) {
        var r = new System.Random();
        int[] results = new int[dice.RollCnt * 2];
        for(int i = 0; i < (dice.RollCnt * 2) + extraRoll; i++) {
            results[i] = r.Next(dice.MinValue, dice.MaxValue + 1);
        }
        return dice.Final(results);
    }
}
