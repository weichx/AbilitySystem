using UnityEngine;

public class DestroyAfterAbilityHit : MonoBehaviour {

    public float delay;
    public GameObject toDestroy;
    private Timer timer;
    private bool destroyed = false;
    private EventEmitter emitter;

    void OnEnable() {
        emitter = GetComponent<EventEmitter>();
        if (emitter != null) {
            emitter.AddListener<AbilityHit>(OnHit);
        }
    }

    void OnHit(AbilityHit evt) {
        if (delay > 0) {
            timer = new Timer(delay);
        }
        else {
            Destroy();
        }
    }

    void Update() {
        if (destroyed) return;

        if (timer != null && timer.Ready) {
            Destroy();
        }

    }

    private void Destroy() {
        if (toDestroy == null) toDestroy = gameObject;
        if (emitter != null) {
            emitter.RemoveListener<AbilityHit>(OnHit);
        }
        emitter = null;
        destroyed = true;
        Destroy(toDestroy);
    }
}