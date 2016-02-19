using UnityEngine;

public class NotBehavior : MonoBehaviour {

    public int value = 10;

    public void Awake() {
        Debug.Log("Yawn");
    }

    public void Start() {
        Debug.Log("here i am");
    }

    public void Update() {
        Debug.Log(":(");
    }
}