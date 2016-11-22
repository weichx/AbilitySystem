using UnityEngine;
using UnityEngine.UI;
using EntitySystem;

public class Nameplate : MonoBehaviour {

    protected Text nameText;
    protected Text castTimeText;
    protected Image castBar;
    protected Image healthBar;
    [Writable(false)] public Entity entity;

    public void Initialize(Entity entity) {
        this.entity = entity;
        WorldSpaceUIOverlay overlay = GetComponent<WorldSpaceUIOverlay>();
        overlay.SetTrackedObject(entity.transform);
        transform.SetParent(GameObject.FindGameObjectWithTag("UICanvas").transform);
        nameText = GetComponentInChildren<Text>();
        nameText.text = entity.name;
    }

    void Start () {
        castBar = transform.Find("Castbar").GetComponent<Image>();
        healthBar = transform.Find("Healthbar").GetComponent<Image>();
        castTimeText = castBar.GetComponentInChildren<Text>();
        castTimeText.text = "";

        if (entity != null) {
            WorldSpaceUIOverlay overlay = GetComponent<WorldSpaceUIOverlay>();
            overlay.SetTrackedObject(entity.transform);
        }
	}
	
	void Update () {
        if(entity == null) return;
        if(entity.abilityManager.IsCasting) {
            Ability currentAbility = entity.abilityManager.ActiveAbility;
            castTimeText.text = currentAbility.Id + " (" + currentAbility.ElapsedCastTime.ToString("0.00") + ")";
        }
        else {
            nameText.text = entity.name;
            castTimeText.text = "";
        }
		healthBar.fillAmount = 1.0f;//entity.GetAttribute("Health").NormalizedValue;
        castBar.fillAmount = entity.abilityManager.NormalizedElapsedCastTime;   
	}
}
