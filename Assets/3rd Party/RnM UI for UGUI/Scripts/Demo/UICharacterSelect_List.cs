using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UICharacterSelect_List : MonoBehaviour {
		
		private ToggleGroup toggleGroup;
		public GameObject characterPrefab;
		
		protected void Awake()
		{
			if (this.toggleGroup == null)
				this.toggleGroup = this.gameObject.GetComponent<ToggleGroup>();
		}
		
		/// <summary>
		/// Add a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		public void AddCharacter(string mName, string mClass, int mLevel)
		{
			this.AddCharacter(mName, mClass, mLevel, null, false);
		}
		
		/// <summary>
		/// Adds a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		/// <param name="mSelected">If set to <c>true</c> the character will be selected.</param>
		public void AddCharacter(string mName, string mClass, int mLevel, bool mSelected)
		{
			this.AddCharacter(mName, mClass, mLevel, null, mSelected);
		}
		
		/// <summary>
		/// Adds a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		/// <param name="mAvatar">Avatar.</param>
		public void AddCharacter(string mName, string mClass, int mLevel, Sprite mAvatar)
		{
			this.AddCharacter(mName, mClass, mLevel, mAvatar, false);
		}
		
		/// <summary>
		/// Add a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		/// <param name="mAvatar">Avatar.</param>
		/// <param name="mSelected">If set to <c>true</c> the character will be selected.</param>
		public void AddCharacter(string mName, string mClass, int mLevel, Sprite mAvatar, bool mSelected)
		{
			if (this.characterPrefab == null)
				return;
			
			// Instantiate the prefab	
			GameObject obj = (GameObject)Instantiate(this.characterPrefab);
			
			// Change parent
			obj.transform.SetParent(this.transform, false);
			
			// Get the unit component
			UICharacterSelect_Unit unit = obj.GetComponent<UICharacterSelect_Unit>();
			
			// Apply the toggle group
			if (this.toggleGroup != null)
				unit.group = this.toggleGroup;
			
			// Set the character details
			unit.SetName(mName);
			unit.SetClass(mClass);
			unit.SetLevel(mLevel);
			if (mAvatar != null) unit.SetAvatar(mAvatar);
			
			// Apply the selected flag
			unit.isOn = mSelected;
			
			// Fix the content size fitters
			foreach (ContentSizeFitter fitter in unit.transform.GetComponentsInChildren<ContentSizeFitter>())
			{
				fitter.SetLayoutHorizontal();
			}
		}
	}
}