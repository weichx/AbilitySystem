using UnityEngine;

public class BoomCamera : MonoBehaviour {
    public float minDistance = 5;
    public float maxDistance = 50f;
    public float cameraSpeed = 25.0f;
    public float currentZ = 0;

    void Start() {
        //currentZ = transform.localPosition.z;
        float min = -maxDistance;
        float max = -minDistance;
        currentZ = Mathf.Clamp(currentZ, min, max);
        transform.localPosition = new Vector3(0, 0, currentZ);
    }

    void Update() {
        //todo add some momentum
        float min = -maxDistance;
        float max = -minDistance;

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta == 0) {
            if (Input.GetKey(KeyCode.Z)) {
                scrollDelta = cameraSpeed;
            }
            else if (Input.GetKey(KeyCode.X)) {
                scrollDelta = -cameraSpeed;
            }
        }
        if (scrollDelta == 0) return;
        float step = cameraSpeed * Time.deltaTime * Mathf.Sign(scrollDelta);
        float total = currentZ + step;
        if (total < max || total > min) {
            total = Mathf.Clamp(total, min, max);
        }
        currentZ = total;
        transform.localPosition = new Vector3(0, 0, total);
    }
}
