using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;

public class ContextModifier : Modifier<Context> {
    public int test;
    public MethodPointer<float> formula;
}

public class NoModifier : Modifier<SingleTargetContext> {
    public int test;
    public MethodPointer<SingleTargetContext> formula;
}

public class OneModifier : Modifier<MultiPointContext> {
    public MethodPointer<SingleTargetContext, float, float> formula;
}

public class TwoModifier : Modifier<DirectionalContext> {
    public MethodPointer <SingleTargetContext, float, float, float> formula;
}

