using UnityEngine;

public class DestroyAfterAbilityHit : MonoBehaviour {

    public float delay;
    public GameObject toDestroy;
    private Timer timer;
    private bool destroyed = false;

    void OnEnable() {
        EventManager evtManager = GetComponent<EventManager>();
        if(evtManager != null) {
            evtManager.AddListenerOnce<AbilityHitEntityEvent>(OnHit);
        }
    }

    void OnHit(AbilityHitEntityEvent evt) {
        if(delay > 0) {
            timer = new Timer(delay);
        }
        else {
            destroyed = true;
            if (toDestroy == null) toDestroy = gameObject;
            Destroy(toDestroy);
        }
    }

    void Update() {
        if (destroyed) return;

        if (timer != null && timer.Ready) {
            if (toDestroy == null) toDestroy = gameObject;
            Destroy(toDestroy);
            destroyed = true;
        }

    }
}