using UnityEngine;
using System.Collections;

public class TrackballCamera : MonoBehaviour {

    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = 0.5f;
    public float distanceMax = 15f;
    public float dollySpeed = 5f;

    public float x = 0.0f;
    private float y = 0.0f;
    public float originX;
    private float originY;

    void Start() {
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        transform.position = transform.rotation * negDistance + target.position + (Vector3.up * 2f);
        ResetOrigin();
    }

    //60 == 1 frame == instant alignment
    public void Align(Transform target, float speed = 60f) {
        float yTarget = y;
        if (y > yMaxLimit) yTarget = yMaxLimit;
        if (y < yMinLimit) yTarget = yMinLimit;
        x = Mathf.SmoothStep(x, originX, speed * Time.deltaTime);
        y = Mathf.SmoothStep(y, yTarget, speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(y, x, 0);
        SetPosition();
    }

    public void Orbit(Transform target) {

        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        x = Util.WrapAngle180Offset(x, originX);

        y = Util.ClampAngle(y, yMinLimit, yMaxLimit);
        transform.rotation = Quaternion.Euler(y, x, 0);
        SetPosition();
    }

    public void ResetOrigin() {
        originX = x;
    }

    public void SetPosition() {
        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * dollySpeed, distanceMin, distanceMax);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        transform.position = transform.rotation * negDistance + target.position + (Vector3.up * 2f);
    }
}