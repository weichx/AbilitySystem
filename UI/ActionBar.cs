//using UnityEngine;
//using System.Collections;

//public struct SlotDescriptor {
//    public string hotkeys;
//    public int index;
//    public string actionBarId;
//    public string abilityId;
//}

//public struct ActionBarDescriptor {
//    public string id;
//    public Vector2 screenPosition;
//    public SlotDescriptor[] slotDescriptors;
//}

//[System.Serializable]
//public class ActionBarConfiguration {
//    public string entityId;
//    public ActionBarDescriptor[] actionBarDescriptors;
//}

//public class ActionBar : MonoBehaviour {

//    [HideInInspector]
//    public RectTransform rectTransform;

//    private ActionbarSlot[] slots;

//	void Start () {
//        rectTransform = GetComponent<RectTransform>();
//	}
	
//    //todo
//    public int GetSlotFromMousePosition(Vector2 mousePosition) {
//        Rect rect = rectTransform.rect;
//        float mx = mousePosition.x;
//        float my = mousePosition.y;
//        return -1;
//    }

//    public void SetSlots(ActionbarSlot[] slots) {
//        rectTransform = GetComponent<RectTransform>();
//        this.slots = slots;
//        rectTransform.sizeDelta = new Vector2(slots.Length * 54, 54);
//        for(int i = 0; i < slots.Length; i++) {
//            var slot = slots[i];
//            var slotRectTransform = slot.GetComponent<RectTransform>();
//            slotRectTransform.parent = rectTransform;
//            //todo handle orientation
//            slotRectTransform.pivot = new Vector2(0f, 1f);
//            slotRectTransform.anchorMin = new Vector2(0f, 1f);
//            slotRectTransform.anchorMax = new Vector2(0f, 1f);
//            slotRectTransform.anchoredPosition = new Vector2(i * 54, 0);
//            slotRectTransform.sizeDelta = new Vector2(54, 54);
//        }
//    }
//}
