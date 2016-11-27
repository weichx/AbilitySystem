using UnityEngine;
using Intelligence;

public class Consumable : InventoryItemComponent {
    public int maxCharges;

    public override void OnGained() {
        if(item.charges < maxCharges){
            item.charges += 1;
        }
    }

    public override void OnUse() {
        if(item.charges > 0){
            item.charges -= 1;
        }
    }
}