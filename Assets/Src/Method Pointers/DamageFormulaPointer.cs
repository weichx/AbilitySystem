using System;
using System.Reflection;

[Serializable]
public class DamageFormula : MethodPointer<OldContext, float, float> {

    public DamageFormula(MethodInfo info) : base(info) { }
    public DamageFormula(AbstractMethodPointer ptr) : base(ptr) { }

}