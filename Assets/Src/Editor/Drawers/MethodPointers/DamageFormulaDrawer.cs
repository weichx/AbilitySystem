using System;
using UnityEditor;

[CustomPropertyDrawer(typeof(DamageFormula))]
public class DamageFormulaDrawer : MethodPointerDrawer {

    protected override Type GetAttrType() {
        return typeof(DamageFormulaAttribute);
    }

    protected override Type[] GetSignature() {
        return new Type[] { typeof(Context), typeof(float) };
    }

}

