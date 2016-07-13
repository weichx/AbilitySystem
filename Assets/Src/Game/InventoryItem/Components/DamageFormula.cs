using UnityEditor;
using UnityEngine;
using Intelligence;

public class DamageFormula : InventoryItemComponent {

    public int baseDamage;
    public MethodPointer<float, Context, float> ptr;

}