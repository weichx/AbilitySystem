using UnityEngine;
using System.Collections;

namespace AbilitySystem {
    enum MovementState { Idle, Forward, Backward, Strafing, ForwardStrafing, Jumping }

    public class PlayerCharacterController : MonoBehaviour {

        public float speed = 3f;
        private TrackballCamera characterCamera;
        private CharacterController controller;
        private Animator animator;
        private MovementState state;

        void Start() {
            state = MovementState.Idle;
            characterCamera = Camera.main.GetComponent<TrackballCamera>();
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            controller.center = new Vector3(controller.center.x, controller.center.y + controller.skinWidth, controller.center.z);
        }

        void Update() {

            state = GetMovementState();

            switch (state) {
                case MovementState.Forward:
                    MoveForward();
                    break;
                case MovementState.Backward:
                    MoveBackward();
                    break;
                case MovementState.Idle:
                    Idle();
                    break;
            }

            CastStuff(); //temp until action bars are in
        }

        public void CastStuff() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                PlayerManager.playerEntity.GetComponent<AbilityManager>().Cast("Frostbolt");
            }
        }

        private void MoveForward() {
            controller.SimpleMove(transform.forward * speed);
            animator.SetFloat("Forward", 1f);
            if (Input.GetMouseButtonUp(1)) {
                characterCamera.ResetOrigin();
            }
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

        private void MoveBackward() {
            controller.SimpleMove(transform.forward * -speed);
            animator.SetFloat("Forward", -1f);
            if (Input.GetMouseButtonUp(1)) {
                characterCamera.ResetOrigin();
            }
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

        private void Idle() {
            animator.SetFloat("Forward", 0f);
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

        private MovementState GetMovementState() {
            if (PCInputManager.ForwardStrafing) {
                return MovementState.ForwardStrafing;
            }
            else if (PCInputManager.Forward) {
                return MovementState.Forward;
            }
            else if (PCInputManager.Strafing) {
                return MovementState.Strafing;
            }
            else if (PCInputManager.Backward) {
                return MovementState.Backward;
            }
            else {
                return MovementState.Idle;
            }
        }


    }

}