using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityEngine.UI
{
	public class UIDurabilityPart : UIBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
	
		public Image targetImage;
		public Sprite hoverSprite;
		
		protected override void Awake()
		{
			if (this.targetImage == null)
				this.targetImage = this.GetComponent<Image>();
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.enabled)
			{
				if (this.targetImage != null)
					this.targetImage.overrideSprite = this.hoverSprite;
				
				UIDurabilityTooltip.SetDurability(Random.Range(0, 100).ToString());
				UIDurabilityTooltip.Show();
			}
		}
		
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this.enabled)
			{
				if (this.targetImage != null)
					this.targetImage.overrideSprite = null;
				
				UIDurabilityTooltip.Hide();
			}
		}
	}
}