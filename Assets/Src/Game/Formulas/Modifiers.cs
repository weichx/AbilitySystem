using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;

public class ContextModifier : Modifier<Context> {
    public int test;
    public MethodPointer<float> formula;
    public override void ApplyModifier(ref float value) {
        value *= 2;
    }
}

public class NoModifier : Modifier<SingleTargetContext> {
    public int test;
    public MethodPointer<SingleTargetContext> formula;
    public override void ApplyModifier(ref float value) {
        value += 10;
    }
}

public class OneModifier : Modifier<MultiPointContext> {
    public MethodPointer<SingleTargetContext, float, float> formula;
}

public class TwoModifier : Modifier<DirectionalContext> {
    public MethodPointer <SingleTargetContext, float, float, float> formula;
}

