using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

enum MovementState { Idle, Forward, Backward, Strafing, ForwardStrafing, Jumping }

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour {

    public float speed = 8f;
    public float jumpHeight = 2;
    public float jumpGravity = 1f;

    private TrackballCamera characterCamera;
    private CharacterController controller;
    private Animator animator;
    private MovementState state;
    private Vector3 movementVector;

    void Start() {
        state = MovementState.Idle;
        characterCamera = Camera.main.GetComponent<TrackballCamera>();
        characterCamera.target = transform;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        controller.center = new Vector3(controller.center.x, controller.center.y + controller.skinWidth, controller.center.z);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (Cursor.lockState == CursorLockMode.Confined) {
                Cursor.lockState = CursorLockMode.None;
            }
            else {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        if (Input.GetMouseButtonUp(1)) {
            characterCamera.ResetOrigin();
        }

        if (controller.isGrounded) {
            movementVector = Vector3.zero;
            if (PCInputManager.Forward) {
                movementVector += transform.forward;
            }
            else if (PCInputManager.Backward) {
                movementVector += transform.forward * -1;
            }

            if (PCInputManager.StrafingLeft) {
                movementVector += transform.right * -1;
            }
            else if (PCInputManager.StrafingRight) {
                movementVector += transform.right;
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                movementVector.y = jumpHeight;
            }

            movementVector = movementVector * speed;
        }

        movementVector.y -= jumpGravity * Time.deltaTime;
        controller.Move(movementVector * Time.deltaTime);

        //todo this needs work, probably needs to tie into a universal input system
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }

        if (movementVector != Vector3.zero) {
            // movementVector.Normalize();
            // controller.SimpleMove(movementVector * speed);
            if (Input.GetMouseButton(1)) { //turn character & align camera
                characterCamera.Orbit(transform);
                Vector3 flatCameraForward = characterCamera.transform.forward;
                flatCameraForward.y = 0f;// transform.position.y;
                transform.LookAt(transform.position + flatCameraForward);
            }
            else if (Input.GetMouseButton(0)) { //orbit & no alignment
                characterCamera.Orbit(transform);
            }
            else { //smoothstep camera back into alignment
                characterCamera.Align(transform, 7.5f);
            }
        }
        else {
            Idle();
        }

    }

    private void Idle() {
        if (PCInputManager.LeftMousePressed) { //orbit & no alignment
            characterCamera.Orbit(transform);
        }

        if (PCInputManager.RightMousePressed) {            //orbit & alignment
            characterCamera.Orbit(transform);
            Vector3 flatCameraForward = characterCamera.transform.forward;
            flatCameraForward.y = 0f;// transform.position.y;
            transform.LookAt(transform.position + flatCameraForward);
        }

        characterCamera.SetPosition();
    }

}

