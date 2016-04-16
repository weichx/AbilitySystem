using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Healthbar : MonoBehaviour {

    public AIEntity entity;
    public Image image;

    void Start() {
        entity = GetComponentInParent<AIEntity>();
        image = GetComponent<Image>();      
    }

    void Update() {
       // image.fillAmount = entity.health.Normalized;
    }
}
