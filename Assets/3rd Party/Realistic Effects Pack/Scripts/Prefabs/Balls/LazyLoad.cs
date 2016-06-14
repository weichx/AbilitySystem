using UnityEngine;
using System.Collections;

public class LazyLoad : MonoBehaviour {

    public GameObject GO;
    public float TimeDelay = 0.3f;
    // Use this for initialization

    void Awake() {
        GO.SetActive(false);
    }

    // Update is called once per frame
    void LazyEnable() {
        GO.SetActive(true);
    }

    void OnEnable() {
        Invoke("LazyEnable", TimeDelay);
    }

    void OnDisable() {
        CancelInvoke("LazyEnable");
        GO.SetActive(false);
    }
}
