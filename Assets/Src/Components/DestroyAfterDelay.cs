using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {

    public float delay;
    public GameObject toDestroy;

    private Timer timer;

    void OnEnable() {
        timer = new Timer(delay);
    }

    void Update() {
        if(timer.Ready) {
            if (toDestroy == null) toDestroy = gameObject;
            Destroy(toDestroy);
        }
    }
}