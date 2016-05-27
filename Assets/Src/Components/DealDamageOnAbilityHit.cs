using UnityEngine;
using System.Collections;
using System;

public class DealDamageOnAbilityHit : MonoBehaviour {

    void Start() {
        EventManager evtManager = GetComponent<EventManager>();
        evtManager.AddListenerOnce<AbilityHitEntityEvent>(DealDamage);
    }

    void DealDamage(AbilityHitEntityEvent evt) {
        Entity target = evt.context["target"] as Entity;
        //FloatAttribute damage = evt.ability["damage"] as FloatAttribute;
        target.resourceManager["health"].Decrease(10, evt.context);
    }

}

