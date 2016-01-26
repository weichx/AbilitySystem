using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {

    public float delay;
    public GameObject obj;

    private Timer timer;

    void OnEnable() {
        timer = new Timer(delay);
    }

    void Update() {
        if(timer.Ready) {
            if (obj == null) obj = gameObject;
            Destroy(obj);
        }
    }
}