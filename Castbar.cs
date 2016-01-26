using UnityEngine;
using UnityEngine.UI;

public class Castbar : MonoBehaviour {
    public Entity entity;
    private AbilityManager abilityManager;

    protected Text textComponent;

	void Start () {
        textComponent = GetComponent<Text>();
        abilityManager = entity.GetComponent<AbilityManager>();
    }

    void Update () {
	    if(abilityManager.IsCasting) {
            textComponent.text = abilityManager.ElapsedCastTime.ToString("0.0") + " / " + abilityManager.TotalCastTime.ToString("0.0");
        }
        else {
            textComponent.text = "";
        }
	}
}
