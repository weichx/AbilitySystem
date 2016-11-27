using UnityEngine;

public class CombatTextScroll : MonoBehaviour {

    public float floatSpeed = 2f;
    public float maxY = 10f;

    public void Update() {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
      
        if (transform.position.y >= maxY) {
            Destroy(gameObject);
        }

    }
}
