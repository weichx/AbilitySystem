using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UIQuestRewardToggle : Toggle {
		
		[SerializeField] private Sprite m_ActiveSprite;
		[SerializeField] private Image m_TargetImage;
		
		protected override void Awake()
		{
			base.Awake();
			
			// Disable the selectable transition
			this.transition = Transition.None;
			
			// Disable the toggle transition
			this.toggleTransition = ToggleTransition.None;
			
			// Hook the on change event
			base.onValueChanged.AddListener(OnValueChanged);
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (Application.isPlaying && this.m_TargetImage != null)
			{
				this.m_TargetImage.overrideSprite = (this.isOn) ? this.m_ActiveSprite : null;
			}
		}
		
		protected virtual void OnValueChanged(bool state)
		{
			if (this.m_TargetImage == null)
				return;
			
			this.m_TargetImage.overrideSprite = (state) ? this.m_ActiveSprite : null;
		}
	}
}