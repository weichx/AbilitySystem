using System;
using System.Reflection;

[Serializable]
public class DamageFormula : MethodPointer<Context, float, float> {

    public DamageFormula(MethodInfo info) : base(info) { }
    public DamageFormula(AbstractMethodPointer ptr) : base(ptr) { }

}