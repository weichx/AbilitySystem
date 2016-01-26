//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;

//public class UIManager : MonoBehaviour {

//    public GameObject actionBarPrefab;
//    public GameObject actionBarSlotPrefab;

//	// Use this for initialization
//	void Start () {
//        CreateActionBars("Player");
//	}
	
//	// Update is called once per frame
//	void Update () {
	
//	}

//    //todo save / load these
//    public ActionBarConfiguration LoadConfig(string entityId) {
//        ActionBarConfiguration config = new ActionBarConfiguration();
//        var abDesc = new ActionBarDescriptor();
//        abDesc.id = "1";
//        abDesc.screenPosition = new Vector2();
//        abDesc.slotDescriptors = new SlotDescriptor[2];
//        var slotDesc = new SlotDescriptor();
//        slotDesc.abilityId = "Frostbolt";
//        slotDesc.actionBarId = "1";
//        slotDesc.index = 0;
//        slotDesc.hotkeys = "1";
//        var slotDesc2 = new SlotDescriptor();
//        slotDesc2.abilityId = "Frostbolt";
//        slotDesc.actionBarId = "1";
//        slotDesc.index = 1;
//        slotDesc.hotkeys = "2";
//        abDesc.slotDescriptors[0] = slotDesc;
//        abDesc.slotDescriptors[1] = slotDesc2;
//        config.actionBarDescriptors = new ActionBarDescriptor[1];
//        config.actionBarDescriptors[0] = abDesc;
//        return config;
//    }

//    //todo enable multi screen
//    public void CreateActionBars(string entityId) {
//        var config = LoadConfig(entityId);

//        for(int i = 0; i < config.actionBarDescriptors.Length; i++) {
//            var desc = config.actionBarDescriptors[i];
//            var actionBarRoot = Instantiate(actionBarPrefab) as GameObject;
//            var actionBar = actionBarRoot.GetComponent<ActionBar>();
//            actionBarRoot.name = "Action Bar " + desc.id;
//            actionBarRoot.transform.parent = transform;
//            var rectXForm = actionBar.GetComponent<RectTransform>();
//            rectXForm.pivot = new Vector2(0.5f, 0f);
//            rectXForm.anchorMin = new Vector2(0.5f, 0f);
//            rectXForm.anchorMax = new Vector2(0.5f, 0f);
//            rectXForm.anchoredPosition = new Vector2(0f, 5f); //todo multiple bars will stack right now
//            CreateActionBarSlots(actionBar, desc.slotDescriptors);
//        }
//    }

//    private void CreateActionBarSlots(ActionBar actionBar, SlotDescriptor[] slotDescriptors) {
//        ActionbarSlot[] slots = new ActionbarSlot[slotDescriptors.Length];
//        for (int i = 0; i < slotDescriptors.Length; i++) {
//            var slotDesc = slotDescriptors[i];
//            var slotRoot = Instantiate(actionBarSlotPrefab);
//            slotRoot.name = "Slot " + (i + 1);
//            var slot = slotRoot.GetComponent<ActionbarSlot>();
//            slot.SetAbility(slotDesc.abilityId);
//            slots[i] = slot;
//        }
//        actionBar.SetSlots(slots);
//    }
//}
