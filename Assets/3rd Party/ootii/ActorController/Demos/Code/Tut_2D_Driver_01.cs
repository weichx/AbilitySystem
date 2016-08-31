using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;

namespace com.ootii.Demos
{
    public class Tut_2D_Driver_01 : MonoBehaviour
    {
        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject InputSourceOwner = null;

        /// <summary>
        /// Transform of the camera we can use to determine input direction
        /// </summary>
        public Transform CameraRig = null;

        /// <summary>
        /// Unity units per second the actor moves
        /// </summary>
        public float MovementSpeed = 5f;

        /// <summary>
        /// Degrees per second the actor rotates
        /// </summary>
        public float RotationSpeed = 360f;

        /// <summary>
        /// Amount of force to apply during a jump
        /// </summary>
        public float JumpForce = 10f;

        /// <summary>
        /// Provides access to the keyboard, mouse, etc.
        /// </summary>
        protected IInputSource mInputSource = null;

        /// <summary>
        /// Actor Controller being controlled
        /// </summary>
        protected ActorController mActorController = null;

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected virtual void Awake()
        {
            // Grab the actor controller so we can set it's values later
            mActorController = gameObject.GetComponent<ActorController>();

            // Object that will provide access to the keyboard, mouse, etc
            if (InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(InputSourceOwner); }

            // Default speed we'll use to rotate. To help smooth out the rotation,
            // We make it consistant even in low frame rates, by set the standard to be a
            // rotation if we're running at 60 frames per second. 
            mDegreesPer60FPSTick = RotationSpeed / 60f;
        }

        /// <summary>
        /// Called every frame so the driver can process input and
        /// update the actor controller.
        /// </summary>
        protected virtual void Update()
        {
            // Ensure we have everything we need
            if (mActorController == null) { return; }
            if (mInputSource == null || !mInputSource.IsEnabled) { return; }

            // Initialize some variables
            Vector3 lMovement = Vector3.zero;
            Quaternion lRotation = Quaternion.identity;

            // -----------------------------------------------------------------
            // INPUT
            // -----------------------------------------------------------------

            // This is the horizontal movement of the mouse or Xbox controller's right stick
            float lYaw = mInputSource.ViewX;

            // This is the WASD buttons or Xbox controller's left stick
            Vector3 lInput = new Vector3(mInputSource.MovementX, 0f, mInputSource.MovementY);

            // -----------------------------------------------------------------
            // ROTATE
            // -----------------------------------------------------------------

            // If the input source says we can, rotate based on the yaw.
            if (mInputSource.IsViewingActivated)
            {
                // The input from the mouse already takes the frame rate into account. By doing
                // the multiplication here, we keep the rotation consistant across frame rates.
                lRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);

                // Rotate our actor
                mActorController.Rotate(lRotation);
            }

            // -----------------------------------------------------------------
            // MOVE
            // -----------------------------------------------------------------

            // We get the tilt so we can add this up/down direction to the camera input. This helps
            // characters to not run off ramps since they are moving how they are facing (ie down a ramp)
            // vs. simply forward (off the ramp) 
            Quaternion lTilt = QuaternionExt.FromToRotation(Vector3.up, mActorController._Transform.up);

            // Move based on WASD we add the tilt
            lMovement = lTilt * lInput * MovementSpeed * Time.deltaTime;
            mActorController.Move(lMovement);

            // -----------------------------------------------------------------
            // JUMP
            // -----------------------------------------------------------------

            // Only jump if the button is pressed and we're on the ground
            if (mInputSource.IsJustPressed("Jump"))
            {
                if (mActorController.State.IsGrounded)
                {
                    mActorController.AddImpulse(mActorController._Transform.up * JumpForce);
                }
            }
        }
    }
}
