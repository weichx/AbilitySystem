using UnityEngine;
using System.Collections;
using EntitySystem;

public class Actionbar : MonoBehaviour {

    public Entity player;
    public void Start() {
        PlayerSkillBook skillbook = player.GetComponent<PlayerSkillBook>();
        ActionbarSlot[] slots = GetComponentsInChildren<ActionbarSlot>();
        for (int i = 0; i < slots.Length; i++) {
            if (skillbook.skillBookEntries.Length == i) return;
            slots[i].SetItem(skillbook.skillBookEntries[i]);
        }
    }
}
