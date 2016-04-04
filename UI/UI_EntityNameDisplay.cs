using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AbilitySystem;

public class UI_EntityNameDisplay : MonoBehaviour {

    private Entity entity;
    private Text textElement;

    void Start() {
        entity = GetComponentInParent<Entity>();
        textElement = GetComponent<Text>();
    }

    void Update() {
        textElement.text = entity.name;
    }
}
