using UnityEngine;
using System.Collections;

public class MeteorTestScript : MonoBehaviour {

    public void Start() {
        GetComponent<EventManager>().AddListenerOnce<AbilityHitEvent>(OnHit);
    }

    protected void OnHit(AbilityHitEvent evt) {
        Debug.Log("Hit");
    }

}
