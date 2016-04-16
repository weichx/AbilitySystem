using System;
using UnityEngine.EventSystems;
using AbilitySystem;
using UnityEngine;
using UnityEngine.UI;

public class ActionbarSlot : UISlotBase {

    public string abilityId;
    public KeyCode keyBind;

    private Image cooldownImage;
    private Text keybindText;
    private Text cooldownText;

    [NonSerialized]
    public Ability ability;
    public AbilityManager abilityManager;

    protected override void Start() {
        abilityManager = PlayerManager.playerEntity.abilityManager;    
        if (iconGraphic == null) {
            Transform iconTransform = transform.Find("Icon");
            iconGraphic = iconTransform.GetComponent<Image>();
        }
        if (hoverTargetGraphic == null) {
            hoverTargetGraphic = transform.Find("Hover").GetComponent<Image>();
        }
        base.Start();
        cooldownImage = transform.Find("Cooldown").GetComponent<Image>();
        cooldownText = transform.Find("CooldownText").GetComponent<Text>();
        keybindText = transform.Find("KeybindText").GetComponent<Text>();
        SetKeybind(keyBind);
    }

    public void SetKeybind(KeyCode key) {
        keybindText.text = KeyCodeToString(key);
    }

    public void Update() {
        DoCooldown();
    }

    public void SetAbility(Ability ability) {
        this.ability = ability;
        if (ability == null) {
            Unassign();
        }
        else {
            abilityId = ability.abilityId;
          //  SetIcon(ability.icon);
        }
    }

    public void DoCooldown() { 
        if(ability == null) {
            if (cooldownImage != null) cooldownImage.enabled = false;
            if (cooldownText != null) cooldownText.text = "";
        }
        else {
            //if (ability.OnCooldown) {
            //    cooldownImage.enabled = true;
            //    float remaining = ability.RemainingCooldown;
            //    cooldownImage.fillAmount = ability.RemainingCooldown / ability.cooldown.CachedValue;
            //    cooldownText.text = ability.RemainingCooldown.ToString("0.0");
            //}
            //else if (abilityManager.OnGlobalCooldown) {
            //    cooldownImage.enabled = true;
            //    cooldownImage.fillAmount = abilityManager.RemainingGCDTime;
            //}
            //else {
            //    cooldownImage.enabled = false;
            //    cooldownText.text = "";
            //}
        }

       
    }

    public void SetAbility(string abilityId) {
        var abilityManager = PlayerManager.playerEntity.abilityManager;
        SetAbility(abilityManager.GetAbility(abilityId));        
    }

    public override bool IsAssigned() {
        return ability != null;
    }

    public override bool Assign(UnityEngine.Object source) {
        ActionbarSlot otherSlot = source as ActionbarSlot;
        if (otherSlot == null) return false;

        string otherAbilityId = otherSlot.abilityId;
        Unassign();
        SetAbility(otherSlot.ability);
        return true;
    }

    public override void Unassign() {
        ability = null;
        abilityId = null;
        ClearIcon();
    }

    public override bool CanSwapWith(UnityEngine.Object target) {
        return target as ActionbarSlot != null;
    }

    public override bool PerformSlotSwap(UnityEngine.Object targetObject) {
        ActionbarSlot other = targetObject as ActionbarSlot;
        //string otherAbilityId = other.abilityId;
        Ability otherAbility = other.ability;
        other.SetAbility(ability);
        SetAbility(otherAbility);
        return true;
    }

    public override void OnPointerClick(PointerEventData eventData) {
        base.OnPointerClick(eventData);

        if (!IsAssigned()) return;

       // PlayerManager.playerEntity.abilityManager.Cast(abilityId);

        eventData.Use();

    }

    private static string KeyCodeToString(KeyCode key) {
        switch (key) {
            case KeyCode.None: return "";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Alpha0: return "0";
            case KeyCode.BackQuote: return "`";
            default: return key.ToString();
        }
    }
}
