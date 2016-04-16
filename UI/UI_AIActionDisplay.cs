using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_AIActionDisplay : MonoBehaviour {

    private AIEntity entity;
    private Text textElement;

	void Start () {
        entity = GetComponentInParent<AIEntity>();
        textElement = GetComponent<Text>();
    }
	
	void Update () {
        string name = entity.actionManager.GetCurrentActionName();
        int idx = name.IndexOf("AIAction_");
        
        if(idx != -1) {
            name = name.Substring("AIAction_".Length);
        }
        textElement.text = name;
	}
}
