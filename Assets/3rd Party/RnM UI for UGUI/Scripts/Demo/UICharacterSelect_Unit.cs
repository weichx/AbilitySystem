using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace UnityEngine.UI
{
	public class UICharacterSelect_Unit : Toggle {
		
		[SerializeField][FormerlySerializedAs("avatarImageComponent")]
		private Image m_AvatarImageComponent;
		[SerializeField][FormerlySerializedAs("nameTextComponent")]
		private Text m_NameTextComponent;
		[SerializeField][FormerlySerializedAs("classTextComponent")]
		private Text m_ClassTextComponent;
		[SerializeField][FormerlySerializedAs("levelTextComponent")]
		private Text m_LevelTextComponent;
		
		[SerializeField]
		private Color m_NameNormalColor = Color.white;
		[SerializeField][FormerlySerializedAs("nameHighlightColor")]
		private Color m_NameHighlightColor = Color.white;
		
		[SerializeField]
		private Color m_ClassNormalColor = Color.white;
		[SerializeField][FormerlySerializedAs("classHighlightColor")]
		private Color m_ClassHighlightColor = Color.white;
		
		[SerializeField]
		private Color m_LevelNormalColor = Color.white;
		[SerializeField][FormerlySerializedAs("levelHighlightColor")]
		private Color m_LevelHighlightColor = Color.white;
		
		protected override void InstantClearState()
		{
			base.InstantClearState();
			this.DoStateTransition(SelectionState.Normal, true);
		}
		
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			
			// Check if the script is enabled
			if (!this.enabled || !this.gameObject.activeInHierarchy)
				return;
			
			switch (state)
			{
				case SelectionState.Normal:
					if (this.m_NameTextComponent != null) this.m_NameTextComponent.canvasRenderer.SetColor(this.m_NameNormalColor);
					if (this.m_ClassTextComponent != null) this.m_ClassTextComponent.canvasRenderer.SetColor(this.m_ClassNormalColor);
					if (this.m_LevelTextComponent != null) this.m_LevelTextComponent.canvasRenderer.SetColor(this.m_LevelNormalColor);
					break;
				case SelectionState.Highlighted:
					if (this.m_NameTextComponent != null) this.m_NameTextComponent.canvasRenderer.SetColor(this.m_NameHighlightColor);
					if (this.m_ClassTextComponent != null) this.m_ClassTextComponent.canvasRenderer.SetColor(this.m_ClassHighlightColor);
					if (this.m_LevelTextComponent != null) this.m_LevelTextComponent.canvasRenderer.SetColor(this.m_LevelHighlightColor);
					break;
			}
		}
		
		/// <summary>
		/// Sets the avatar of the unit.
		/// </summary>
		/// <param name="avatar">Avatar.</param>
		public void SetAvatar(Sprite avatar)
		{
			if (this.m_AvatarImageComponent != null)
				this.m_AvatarImageComponent.overrideSprite = avatar;
		}
		
		/// <summary>
		/// Sets the name of the unit.
		/// </summary>
		/// <param name="name">Name.</param>
		public void SetName(string name)
		{
			if (this.m_NameTextComponent != null)
				this.m_NameTextComponent.text = name;
		}
		
		/// <summary>
		/// Sets the level of the unit.
		/// </summary>
		/// <param name="level">Level.</param>
		public void SetLevel(int level)
		{
			if (this.m_ClassTextComponent != null)
				this.m_ClassTextComponent.text = "Level " + level.ToString();
		}
		
		/// <summary>
		/// Sets the class of the unit.
		/// </summary>
		public void SetClass(string mClass)
		{
			if (this.m_LevelTextComponent != null)
				this.m_LevelTextComponent.text = mClass;
		}
	}
}