using UnityEngine;
using System.Collections;

public class EnableDisableOnAbilityHitPoint : MonoBehaviour {

    public GameObject[] toEnable;
    public GameObject[] toDisable;

    void Start() {
        var evtManager = GetComponentInParent<EventEmitter>();
        if (evtManager != null) {
            evtManager.AddListener<AbilityHitPointEvent>(EnableDisableObjects);
        }
    }

    public void EnableDisableObjects(AbilityHitPointEvent evt) {
        if (toEnable != null) {
            for (int i = 0; i < toEnable.Length; i++) {
                if (toEnable[i] != null) toEnable[i].SetActive(true);
            }
        }
        if (toDisable != null) {
            for (int i = 0; i < toDisable.Length; i++) {
                if (toDisable[i] != null) toDisable[i].SetActive(false);
            }
        }
    }

}
