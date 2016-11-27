using UnityEditor;
using UnityEngine;
using Intelligence;

public class DamageFormula : InventoryItemComponent {

    public int baseDamage;

    public MethodPointer<float, SingleTargetContext, float> damageFormula;

    public override void OnUse() {
    }
}