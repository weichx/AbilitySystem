using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Icon Slots/Equip Receiver", 46)]
	public class UIEquipReceiver : UIBehaviour, IEventSystemHandler, IDropHandler {
		
		public void OnDrop(PointerEventData eventData)
		{
			if (eventData.pointerPress == null)
				return;
			
			// Try getting slot base component from the selected object
			UISlotBase slotBase = eventData.pointerPress.gameObject.GetComponent<UISlotBase>();
			
			// Check if we have a slot
			if (slotBase == null)
				return;
			
			// Determine the type of slot we are dropping here
			if (slotBase is UIItemSlot)
			{
				UIItemSlot itemSlot = (slotBase as UIItemSlot);
				
				// Make sure the slot we are dropping is valid and assigned
				if (itemSlot != null && itemSlot.IsAssigned())
				{
					// Try finding a suitable slot to equip
					UIEquipSlot equipSlot = UIEquipSlot.GetSlotWithType(itemSlot.GetItemInfo().EquipType);
					
					if (equipSlot != null)
					{
						// Use the drop event to handle equip
						equipSlot.OnDrop(eventData);
						return;
					}
				}
			}
		}
	}
}
