using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;

namespace com.ootii.Demos
{
    public class Tut_2D_Driver_03 : MonoBehaviour
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
        /// Target forward we want to turn towards over time
        /// </summary>
        protected Vector3 mTargetForward = Vector3.zero;

        /// <summary>
        /// Determines if we are in the middle of a jump
        /// </summary>
        protected bool mIsInToWall = false;

        /// <summary>
        /// Point we're trying to reach
        /// </summary>
        protected Vector3 mToWallPoint = Vector3.zero;

        /// <summary>
        /// Wall normal we're trying to cling to
        /// </summary>
        protected Vector3 mToWallNormal = Vector3.zero;

        /// <summary>
        /// Holds the saved value so we can reset it
        /// </summary>
        protected float mSavedOrientToGroundSpeed = 1f;

        /// <summary>
        /// Holds the saved value so we can reset it
        /// </summary>
        protected float mSavedMaxSlopeAngle = 0f;

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
            //float lYaw = mInputSource.ViewX;

            // This is the WASD buttons or Xbox controller's left stick
            Vector3 lInput = new Vector3(mInputSource.MovementX, 0f, mInputSource.MovementY);


            // -----------------------------------------------------------------
            // ROTATE
            // -----------------------------------------------------------------
            if (mIsInToWall)
            {
                Vector3 lToWallHit = mToWallPoint - transform.position;
                float lToWallHitNormal = Vector3.Angle(mActorController._Transform.up, mToWallNormal);

                // Move to the target and ensure we orient ourselves to the wall
                if (lToWallHit.magnitude > 0.03f || lToWallHitNormal > 0.5f)
                {
                    mActorController.SetTargetGroundNormal(mToWallNormal);

                    lMovement = lToWallHit.normalized * Mathf.Min(MovementSpeed * Time.deltaTime, lToWallHit.magnitude);
                    mActorController.Move(lMovement);
                }
                // Once we're there, clean up
                else
                { 
                    mIsInToWall = false;
                    mToWallPoint = Vector3.zero;
                    mToWallNormal = Vector3.zero;

                    mActorController.MaxSlopeAngle = mSavedMaxSlopeAngle;
                    mActorController.OrientToGroundSpeed = mSavedOrientToGroundSpeed;
                    mActorController.SetTargetGroundNormal(Vector3.zero);
                }
            }
            else
            {
                // -----------------------------------------------------------------
                // ROTATE
                // -----------------------------------------------------------------

                // Set the target based on the input. This works because we're looking down
                // the world's z-axis.
                if (lInput.x < 0f)
                {
                    mTargetForward = Vector3.left;
                }
                else if (lInput.x > 0f)
                {
                    mTargetForward = Vector3.right;
                }

                // If we have a target forward start rotating towards it and ignore input
                if (mTargetForward.sqrMagnitude > 0f)
                {
                    // Determine how much we need to rotate to get to the target
                    float lTargetAngle = Vector3Ext.SignedAngle(mActorController.Yaw.Forward(), mTargetForward);

                    // If there is no difference, we can turn off the target
                    if (lTargetAngle == 0f)
                    {
                        mTargetForward = Vector3.zero;
                    }
                    else
                    {
                        // Grab the actual rotation angle based on our speed. However, make sure we don't overshoot the
                        // angle. So, we do this logic to truncate it if we're only a tiny bit off.
                        float lRotationAngle = Mathf.Sign(lTargetAngle) * Mathf.Min(RotationSpeed * Time.deltaTime, Mathf.Abs(lTargetAngle));

                        // Since the rotate function deals with the actor's yaw, we just to a vector3.up (it's
                        // relative to the actor regardless of his orientation/tilt
                        lRotation = Quaternion.AngleAxis(lRotationAngle, Vector3.up);

                        // Rotate.
                        mActorController.Rotate(lRotation);
                    }
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

                // -----------------------------------------------------------------
                // TEST FOR WALL
                // -----------------------------------------------------------------

                RaycastHit lWallHitInfo;
                if (TestForWallCollision(lMovement, out lWallHitInfo))
                {
                    // Save the tilt values
                    mIsInToWall = true;
                    mToWallPoint = lWallHitInfo.point;
                    mToWallNormal = lWallHitInfo.normal;

                    // Save and reset some AC values that will help us to tilt
                    mSavedOrientToGroundSpeed = mActorController.OrientToGroundSpeed;
                    mActorController.OrientToGroundSpeed = 0.75f;

                    mSavedMaxSlopeAngle = mActorController.MaxSlopeAngle;
                    mActorController.MaxSlopeAngle = 0f;
                }
            }
        }

        /// <summary>
        /// Test if we hit a wall based on how we're moving.
        /// </summary>
        /// <param name="rMovement">Movement we're trying to make</param>
        /// <param name="rWallHitInfo">Returned information about the wall (if there is a hit)</param>
        /// <returns>Determines if we actually hit a wall</returns>
        protected bool TestForWallCollision(Vector3 rMovement, out RaycastHit rWallHitInfo)
        {
            Vector3 lRayStart = transform.position + (transform.up * 0.7f);
            Vector3 lRayDirection = rMovement.normalized;
            float lRayDistance = 0.8f;

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out rWallHitInfo, lRayDistance))
            {
                return true;
            }

            return false;
        }
    }
}
