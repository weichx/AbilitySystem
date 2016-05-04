using System;
using UnityEngine;
using UnityEditor;

public class AbilityListEntry {

    public string abilityId;
    public bool isSelected;
    private Ability ability;
    public Type scriptableType;
    private string originalId;

    public AbilityListEntry(string abilityId) {
        this.abilityId = abilityId;
        originalId = abilityId;
        isSelected = false;
    }

    public Ability Ability {
        get { return ability; }
        set {
            ability = value;
            if (ability != null) {
                abilityId = ability.abilityId;
            }
        }
    }

    public bool NameChanged {
        get {
            if (ability != null) {
                return originalId != ability.abilityId;
            }
            else {
                return originalId != abilityId;
            }
        }
    }

    public string OriginalId {
        get { return originalId; }
    }

    public string Name {
        get {
            return ability != null ? ability.abilityId : abilityId;
        }
    }

    public void Save() {
        originalId = ability.abilityId;
    }

    public static GUIStyle style = new GUIStyle() {
        fontSize = 14,
        alignment = TextAnchor.MiddleCenter
    };

}