using UnityEngine;
using com.ootii.Input;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Timing;
using com.ootii.Utilities.Debug;

namespace com.ootii.Actors
{
    /// <summary>
    /// The SpiderActorDriver is an Animator based driver. While the spider animations
    /// don't use root-motion, we will set animator parameters at the end of the Update().
    /// 
    /// This version of the driver is for a 2.5D game where we only want the spider to go left
    /// or right. Since the animations don't have root motion, we need to make sure the direction
    /// of the spider and our input make sense.
    /// </summary>
    public class Spider2DDriver : AnimatorDriver
    {
        /// <summary>
        /// Transform of the camera we can use to determine input direction
        /// </summary>
        public Transform CameraTransform = null;

        /// <summary>
        /// Speed to move from ground to wall
        /// </summary>
        public float JumpToWallSpeed = 4f;

        /// <summary>
        /// Speed to rotate to the wall normal
        /// </summary>
        public float JumpToWallOrientSpeed = 0.75f;

        /// <summary>
        /// Height to test and jump to
        /// </summary>
        public float JumpToWallHeight = 1.5f;

        /// <summary>
        /// Max distance from wall we can jump to
        /// </summary>
        public float JumpToWallDistance = 1.5f;

        /// <summary>
        /// If we don't reach the target point within this time, stop
        /// </summary>
        public float JumpToWallTimeout = 1f;

        /// <summary>
        /// Determines how close until we are considered at the target
        /// </summary>
        public float JumpToWallArrivalDistance = 0.03f;

        /// <summary>
        /// Determines if we are in the middle of a jump
        /// </summary>
        protected bool mIsInJumpToWall = false;

        /// <summary>
        /// Point we're trying to reach
        /// </summary>
        protected Vector3 mJumpToWallPoint = Vector3.zero;

        /// <summary>
        /// Wall normal we're trying to cling to
        /// </summary>
        protected Vector3 mJumpToWallNormal = Vector3.zero;

        /// <summary>
        /// Time since we started jumping to the wall;
        /// </summary>
        protected float mJumpToWallElapsedTime = 0f;

        /// <summary>
        /// Holds the saved value so we can reset it
        /// </summary>
        protected float mSavedOrientToGroundSpeed = 1f;

        /// <summary>
        /// Called every frame so the driver can process input and
        /// update the actor controller.
        /// </summary>
        protected override void Update()
        {
            if (!_IsEnabled) { return; }
            if (mAnimator == null) { return; }
            if (mActorController == null) { return; }
            if (mInputSource == null || !mInputSource.IsEnabled) { return; }

            float lDeltaTime = TimeManager.SmoothedDeltaTime;

            Vector3 lMovement = Vector3.zero;
            Quaternion lRotation = Quaternion.identity;
            Vector3 lInput = new Vector3(mInputSource.MovementX, 0f, mInputSource.MovementY);

            // Convert the input to be relative to the camera
            Vector3 lCameraBasedInput = lInput;
            if (CameraTransform != null) { lCameraBasedInput = CameraTransform.rotation * lInput; }

            // If we're in the middle of the jump, move towards the wall and rotate if needed
            if (mIsInJumpToWall)
            {
                Vector3 lToPoint = mJumpToWallPoint - transform.position;

                // Increment our timer
                mJumpToWallElapsedTime = mJumpToWallElapsedTime + Time.deltaTime;

                // If we're close to the target or the time has elapsed, stop
                if (lToPoint.magnitude < JumpToWallArrivalDistance ||
                    (JumpToWallTimeout > 0f && mJumpToWallElapsedTime > JumpToWallTimeout))
                {
                    mIsInJumpToWall = false;
                    mJumpToWallPoint = Vector3.zero;
                    mJumpToWallNormal = Vector3.zero;
                    mJumpToWallElapsedTime = 0f;

                    mActorController.OrientToGroundSpeed = mSavedOrientToGroundSpeed;
                    mActorController.SetTargetGroundNormal(Vector3.zero);

                    // Reenable gravity once we're on
                    mActorController.IsGravityEnabled = true;
                }
                else
                {
                    mActorController.SetTargetGroundNormal(mJumpToWallNormal);

                    lMovement = lToPoint.normalized * Mathf.Min(JumpToWallSpeed * lDeltaTime, lToPoint.magnitude);
                    mActorController.Move(lMovement);

                    // Don't let gravity pull us down as we're trying to head to the target
                    mActorController.IsGravityEnabled = false;
                }
            }
            // Move like normal
            else
            {
                // Rotate based on the input (and root motion if there is any)
                Quaternion lUserRotation = Quaternion.identity;
                if (mInputSource.IsViewingActivated)
                {
                    float lYaw = mInputSource.ViewX;
                    lUserRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);
                }

                lRotation = mRootMotionRotation * lUserRotation;
                mActorController.Rotate(lRotation);

                // We do the tilt so we can add this up/down direction to the camera input. This helps
                // characters to not run off ramps since they are moving how they are facing (ie down a ramp)
                // vs. simply forward (off the ramp) 
                Quaternion lTilt = QuaternionExt.FromToRotation(Vector3.up, mActorController._Transform.up);

                // Move based on WASD (relative to the camera), we add the tilt
                lMovement = lTilt * lCameraBasedInput * _MovementSpeed * lDeltaTime;
                if (mRootMotionMovement.sqrMagnitude > 0f) { lMovement = mRootMotionMovement; }

                mActorController.Move(lMovement);
            }

            // Tell the animator what to do next
            SetAnimatorProperties(lCameraBasedInput, lMovement, lRotation);
        }

        /// <summary>
        /// Provides a place to set the properties of the animator
        /// </summary>
        /// <param name="rInput">Vector3 representing the input</param>
        /// <param name="rMove">Vector3 representing the amount of movement taking place (in world space)</param>
        /// <param name="rRotate">Quaternion representing the amount of rotation taking place</param>
        protected override void SetAnimatorProperties(Vector3 rInput, Vector3 rMovement, Quaternion rRotation)
        {
            if (mInputSource == null || !mInputSource.IsEnabled) { return; }

            // Jump based on space
            bool lIsInJump = !mActorController.State.IsGrounded;

            if (mInputSource.IsJustPressed("Jump"))
            {
                if (!lIsInJump && !mIsInJumpToWall)
                {
                    lIsInJump = true;

                    // We need to check if we're actually jumping towards a wall
                    RaycastHit lHitInfo;
                    if (RaycastExt.SafeRaycast(transform.position + (transform.up * JumpToWallHeight), transform.forward, out lHitInfo, JumpToWallDistance))
                    {
                        mIsInJumpToWall = true;
                        mJumpToWallPoint = lHitInfo.point;
                        mJumpToWallNormal = lHitInfo.normal;
                        mJumpToWallElapsedTime = 0f;

                        mSavedOrientToGroundSpeed = mActorController.OrientToGroundSpeed;
                        mActorController.OrientToGroundSpeed = JumpToWallOrientSpeed;
                    }

                    // Perform the jump
                    mActorController.AddImpulse(transform.up * _JumpForce);
                }
            }

            // Direction of the camera
            float lDirection = 0f;

            // We do the inverse tilt so we calculate the rotation in "natural up" space vs. "actor up" space. 
            Quaternion lInvTilt = QuaternionExt.FromToRotation(mActorController._Transform.up, Vector3.up);

            // Forward direction of the actor in "natural up"
            Vector3 lControllerForward = lInvTilt * mActorController._Transform.forward;

            // Get the angular difference between the camera-based input and the spider's "natural-up" forward
            lDirection = NumberHelper.GetHorizontalAngle(lControllerForward, rInput);

            mAnimator.SetFloat("Direction", lDirection);
            mAnimator.SetFloat("Speed", rInput.magnitude);
            mAnimator.SetBool("Jump", lIsInJump);
        }
    }
}
