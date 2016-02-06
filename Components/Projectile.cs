using UnityEngine;

public class Projectile : MonoBehaviour {

    public float speed;

    void Update() {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}