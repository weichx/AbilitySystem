using UnityEngine;
using UnityEngine.UI;
using AbilitySystem;

public class Nameplate : MonoBehaviour {

    protected Text textComponent;
    protected Image castBar;
    [Writable(false)] public Entity entity;

    public void Initialize(Entity entity) {
        this.entity = entity;
        WorldSpaceUIOverlay overlay = GetComponent<WorldSpaceUIOverlay>();
        overlay.SetTrackedObject(entity.transform);
        transform.SetParent(GameObject.FindGameObjectWithTag("UICanvas").transform);
    }

    void Start () {
        textComponent = GetComponentInChildren<Text>();
        castBar = GetComponentInChildren<Image>();
        if(entity != null) {
            WorldSpaceUIOverlay overlay = GetComponent<WorldSpaceUIOverlay>();
            overlay.SetTrackedObject(entity.transform);
        }
	}
	
	void Update () {
        if(entity == null) return;
        if(entity.abilityManager.IsCasting) {
            Ability currentAbility = entity.abilityManager.ActiveAbility;
            textComponent.text = entity.name + " (" + currentAbility.ElapsedCastTime.ToString("0.00") + ")";
        }
        else {
            textComponent.text = entity.name;
        }
        castBar.fillAmount = entity.abilityManager.NormalizedElapsedCastTime;   
	}
}
