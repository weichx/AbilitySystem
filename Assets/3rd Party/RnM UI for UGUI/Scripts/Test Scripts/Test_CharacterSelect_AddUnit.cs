using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class Test_CharacterSelect_AddUnit : MonoBehaviour {
		
		public UICharacterSelect_List listComponent;
		public int HowMany = 5;
		public bool EmptyOutList = true;
		
		protected void OnEnable()
		{
			string[] classes = new string[5] { "Blade Warrior", "Arcane Mage", "Boomstick Hunter", "Drunk Assassin", "Arch Bishop" };
			
			if (this.listComponent != null)
			{
				// Empty out the list
				if (this.EmptyOutList)
				{
					foreach (Transform trans in this.listComponent.transform)
						Destroy(trans.gameObject);
				}
				
				// Add the test units
				for (int i = 1; i <= this.HowMany; i++)
				{
					this.listComponent.AddCharacter("Test Character " + i, classes[Random.Range(0, 4)], Random.Range(1, 60), (i == 1));
				}
			}
		}
	}
}