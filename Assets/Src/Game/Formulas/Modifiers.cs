using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponCategroy {
    Blunt = 0,
    Piercing = 1,
}

public class ContextModifier : Modifier<Context> {
    public int test;
    public WeaponCategroy cat;
    public MethodPointer<Context, float> formula;

    public override void ApplyModifier(ref float value) {
        value *= 2;
    }
}

public class StrModifier : Modifier<Context> {
    public float bonus;
    public override void ApplyModifier(ref float inValue) {
        inValue = inValue * bonus;
    }
}

public class OneHandedWeaponModifier : Modifier<SingleTargetContext> {
    public float test;
    public MethodPointer<SingleTargetContext, float, float> formula;
    public override void ApplyModifier(ref float inValue) {
        // while testing
        // if(debugMode) formula.OnAfterDeserialize();
        inValue = formula.Invoke((SingleTargetContext)this.context, inValue + test);
    }

}

public class SpellAttackBonusModifier : Modifier<SingleTargetContext> {
    public DiceBase diceLevelBonus;
    public DiceBase dice;
    public MethodPointer<SingleTargetContext, float, float> formula;
    public override void ApplyModifier(ref float inValue) {
        var characterLevel = context.entity.character.parameters.baseParameters.level;
    }
}

public class OneModifier : Modifier<MultiPointContext> {
    public MethodPointer<MultiPointContext, float, float> formula;
    public override void ApplyModifier(ref float inValue) {
    }
}

public class TwoModifier : Modifier<DirectionalContext> {
    public MethodPointer <DirectionalContext, float, float, float> formula;
    public override void ApplyModifier(ref float inValue) {
    }
}

