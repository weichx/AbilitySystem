using UnityEngine;

public enum ViewMode {
    Locked, Free
}

public class GameCamera : MonoBehaviour {
    public float xSpeed = 15;
    public float ySpeed = 15;
    public float zSpeed = 15;

    public float rotationSensitivity = 15f;

    public Transform focus;
    public ViewMode viewMode = ViewMode.Locked;

    public float minimumX = -360f;
    public float maximumX = 360f;

    public float minimumY = -90f;
    public float maximumY = 90f;

    float rotationX = 0f;
    float rotationY = 0f;

    private Camera cameraComponent;
    private Quaternion originalRotation;

    void Start() {
        //  viewMode = ViewMode.Free;
        //todo starting at a non zero rotation breaks the camera
        originalRotation = transform.localRotation;
        cameraComponent = GetComponentInChildren<Camera>();
        if (cameraComponent == null) {
            throw new System.Exception("GameCamera requires a child camera object");
        }
    }

    void Update() {
        DetermineFocus();
        CalculateRotation();
        CalculateTranslation();
    }

    public void DetermineFocus() {
        if (Input.GetKey(KeyCode.F)) {
            //Selection.GetCurrentSelectionTransform();
            if (focus) {
                viewMode = ViewMode.Locked;
            }
        }
    }

    private void CalculateRotation() {
        if (!Input.GetMouseButton(1)) return;
        // Read the mouse input axis
        rotationX += Input.GetAxis("Mouse X") * rotationSensitivity;
        rotationY += Input.GetAxis("Mouse Y") * rotationSensitivity;

        rotationX = Util.ClampAngle(rotationX, minimumX, maximumX);
        rotationY = Util.ClampAngle(rotationY, minimumY, maximumY);

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
        //todo if rotation starts non zero'd this is bad.
        transform.rotation = originalRotation * xQuaternion * yQuaternion;
    }

    private void CalculateTranslation() {

        if (viewMode == ViewMode.Locked) {
            if (focus == null) {
                viewMode = ViewMode.Free;
            }
            else {
                transform.position = focus.transform.position;
            }
        }

        float xChange = 0;
        float yChange = 0;
        float zChange = 0;

        if (Input.GetKey(KeyCode.W)) {
            zChange = zSpeed;
        }
        else if (Input.GetKey(KeyCode.S)) {
            zChange = -zSpeed;
        }

        if (Input.GetKey(KeyCode.D)) {
            xChange = xSpeed;
        }
        else if (Input.GetKey(KeyCode.A)) {
            xChange = -xSpeed;
        }

        if (Input.GetKey(KeyCode.E)) {
            yChange = ySpeed;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            yChange = -ySpeed;
        }

        if (zChange != 0 || xChange != 0 || yChange != 0) {
            viewMode = ViewMode.Free;
        }

        transform.Translate(new Vector3(xChange, yChange, zChange) * Time.deltaTime);
    }
}
