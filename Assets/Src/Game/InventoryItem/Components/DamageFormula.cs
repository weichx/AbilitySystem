using UnityEditor;
using UnityEngine;
using Intelligence;
using EntitySystem;

public class DamageFormula : InventoryItemComponent {

    public int baseDamage;

    public MethodPointer<float, SingleTargetContext, float> damageFormula;

    public override void OnUse() {
    }
}