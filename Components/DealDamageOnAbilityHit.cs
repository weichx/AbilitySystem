using UnityEngine;
using System.Collections;
using System;

public class DamageDescriptor {
    public FloatAttribute amount;
    public string element;
}

public class Rage : AbilityModifier {

    protected override void OnApply(Ability ability) {
        DamageDescriptor dmg = ability.GetAttribute("damage") as DamageDescriptor;
        dmg.amount.SetPercentBonus("Rage", 2f);
    }

    protected override void OnRemove(Ability ability) {
        DamageDescriptor dmg = ability.GetAttribute("damage") as DamageDescriptor;
        if (dmg == null) return;
        dmg.amount.RemoveModifier("Rage");
    }
}

public class DealDamageOnAbilityHit : MonoBehaviour {

    void Start() {
        EventManager evtManager = GetComponent<EventManager>();
        evtManager.AddListenerOnce<AbilityHitEntityEvent>(DealDamage);
    }

    void DealDamage(AbilityHitEntityEvent evt) {
        Entity target = evt.context["target"] as Entity;
        FloatAttribute damage = evt.ability["damage"] as FloatAttribute;
        target.resourceManager["health"].Decrease(damage.Value, evt.context);
    }

}

