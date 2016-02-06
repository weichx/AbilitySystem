using UnityEngine;
using System.Collections.Generic;
using AbilitySystem;


public class Actionbar : MonoBehaviour {

    private ActionbarSlot[] slots;
    private AbilityManager abilityManager;

    void Start() {
        slots = GetComponentsInChildren<ActionbarSlot>();
        abilityManager = PlayerManager.playerEntity.GetComponent<AbilityManager>();
        for (int i = 0; i < slots.Length; i++) {
            slots[i].SetAbility(abilityManager.GetAbility(slots[i].abilityId));
        }
    }

    void Update() {
        for (int i = 0; i < slots.Length; i++) {
            ActionbarSlot slot = slots[i];
            if (!Input.GetKeyDown(slot.keyBind)) continue;
            Ability ability = slot.ability;
            if (ability != null) {
                abilityManager.Cast(ability);
                break;
            }
        }
    }
}
