using UnityEngine;

public class DisableObjectsOnTargetHit : MonoBehaviour {

    public GameObject[] toDisable;
    public HomingProjectile emitter;

    void OnEnable() {
        if(emitter == null) {
            emitter = GetComponent<HomingProjectile>();
        }
        if (emitter != null) {
            emitter.OnTargetHit += DisableObject;
        }
    }

    public void DisableObject() {
        if (toDisable == null) return;
        for (int i = 0; i < toDisable.Length; i++) {
            if (toDisable[i] != null) {
                toDisable[i].SetActive(false);
            }
        }
    }
}
