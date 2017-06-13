using System.Collections.Generic;

public enum DiceBase {
    BASE_1d2,
    BASE_1d4,
    BASE_1d6,
    BASE_1d8,
    BASE_1d10,
    BASE_1d12,
    BASE_2d6,
    BASE_3d6
}

public struct DiceData {
    private int _maxValue;
    private int _minValue;
    private int _rollCount;
    private int[] _finalValue;

    public int MaxValue { get { return _maxValue; } }
    public int MinValue { get { return _minValue; } }
    public int RollCnt { get { return _rollCount; } }
    public int Result {
        get {
            int sum = 0;
            for(int i = 0; i < _finalValue.Length; i++) sum += _finalValue[i];
            return sum;
        }
    }

    public DiceData(int cnt, int max, int min = 1) {
        _maxValue = max;
        _minValue = min;
        _rollCount = cnt;
        _finalValue = new int[_rollCount];
    }

    public void AddRoll(int idx, int value) {
        if(_finalValue[idx] > 0) return;
        _finalValue[idx] = value;
    }
}
