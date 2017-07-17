using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;

public class ContextModifier : Modifier<Context> {
    public int test;
    public MethodPointer<Context, float> formula;
    public override void ApplyModifier(ref float value) {
        value *= 2;
    }
}

public class NoModifier : Modifier<SingleTargetContext> {
    public int test;
    public MethodPointer<SingleTargetContext, float, float> formula;
    public override void ApplyModifier(ref float value) {
        value += 10;
    }
}

public class OneModifier : Modifier<MultiPointContext> {
    public MethodPointer<MultiPointContext, float, float> formula;
}

public class TwoModifier : Modifier<DirectionalContext> {
    public MethodPointer <DirectionalContext, float, float, float> formula;
}

