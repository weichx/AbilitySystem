using UnityEngine;
using com.ootii.Cameras;
using com.ootii.Geometry;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Standing or running jump. The jump allows for control
    /// while in the air.
    /// </summary>
    [MotionName("Jump")]
    [MotionDescription("A physics based multi-part jump that allows the player to launch into the " +
                   "air and recover into the idle pose or a run. The jump is created so the avatar " +
                   "can jump as high as mass, gravity, and impulse allow.")]
    public class Jump : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;

        public const int PHASE_START = 251; 
        public const int PHASE_START_FALL = 250; 

        public const int PHASE_LAUNCH = 201;
        public const int PHASE_RISE = 202;
        public const int PHASE_RISE_TO_TOP = 203;
        public const int PHASE_TOP = 204;
        public const int PHASE_TOP_TO_FALL = 205;
        public const int PHASE_FALL = 206;
        public const int PHASE_LAND = 207;
        public const int PHASE_RECOVER_TO_IDLE = 208;
        public const int PHASE_RECOVER_TO_MOVE = 209;
        public const int PHASE_RUN = 210;
        public const int PHASE_WALK = 211;

        /// <summary>
        /// Stores hit info so we don't have to keep allocating
        /// </summary>
        //private static RaycastHit sHitInfo;

        /// <summary>
        /// The amount of force caused by the player jumping
        /// </summary>
        protected float _Impulse = 10f;
        public float Impulse
        {
            get { return _Impulse; }
            set { _Impulse = value; }
        }

        /// <summary>
        /// The physics jump creates a parabola that is typically based on the
        /// feet. However, if the animation has the feet move... that could be an issue.
        /// </summary>
        public bool _ConvertToHipBase = false;
        public bool ConvertToHipBase
        {
            get { return _ConvertToHipBase; }
            set { _ConvertToHipBase = value; }
        }

        /// <summary>
        /// Allows us to assign a hip bone for adjusting the jump height off of
        /// the foot position
        /// </summary>
        public string _HipBoneName = "";
        public string HipBoneName
        {
            get { return _HipBoneName; }

            set
            {
                _HipBoneName = value;
                if (mMotionController != null)
                {
                    mHipBone = mMotionController.gameObject.transform.FindChild(_HipBoneName);
                }
            }
        }

        /// <summary>
        /// Use the launch velocity throughout the jump
        /// </summary>
        public bool _IsMomentumEnabled = true;
        public bool IsMomentumEnabled
        {
            get { return _IsMomentumEnabled; }
            set { _IsMomentumEnabled = value; }
        }

        /// <summary>
        /// Determines if the player can control the avatar movement
        /// and rotation while in the air.
        /// </summary>
        public bool _IsControlEnabled = true;
        public bool IsControlEnabled
        {
            get { return _IsControlEnabled; }
            set { _IsControlEnabled = value; }
        }

        /// <summary>
        /// When in air, the player can still move the avatar. This
        /// value is the max speed the player can move the avatar by.
        /// </summary>
        public float _ControlSpeed = 2f;
        public float ControlSpeed
        {
            get { return _ControlSpeed; }
            set { _ControlSpeed = value; }
        }

        /// <summary>
        /// If the value is great than 0, we'll do a check to see if there
        /// is enough room to even attempt a jump. While in a jump, we'll cancel it
        /// if there isn't enough room
        /// </summary>
        public float _RequiredOverheadDistance = 0f;
        public float RequiredOverheadDistance
        {
            get { return _RequiredOverheadDistance; }
            set { _RequiredOverheadDistance = value; }
        }

        /// <summary>
        /// Forward the player was facing when they launched. It helps
        /// us control the total rotation that can happen in the air.
        /// </summary>
        protected Vector3 mLaunchForward = Vector3.zero;

        /// <summary>
        /// Velocity at the time the character launches. This helps us with momenumt
        /// </summary>
        protected Vector3 mLaunchVelocity = Vector3.zero;

        /// <summary>
        /// Transform for the hip to help adjust the height
        /// </summary>
        protected Transform mHipBone = null;

        /// <summary>
        /// Distance between the base and the hips
        /// </summary>
        protected float mLastHipDistance = 0f;

        /// <summary>
        /// Determines if the impulse has been applied or not
        /// </summary>
        protected bool mIsImpulseApplied = false;

        /// <summary>
        /// Connect to the move motion if we can
        /// </summary>
        protected WalkRunPivot mWalkRunPivot = null;
        protected WalkRunStrafe mWalkRunStrafe = null;
        protected WalkRunRotate mWalkRunRotate = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Jump()
            : base()
        {
            _Priority = 15;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Jump-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Jump(MotionController rController)
            : base(rController)
        {
            _Priority = 15;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Jump-SM"; }
#endif
        }

        /// <summary>
        /// Initialize is called after all the motions have been initialized. This allow us time to
        /// create references before the motions start working
        /// </summary>
        public override void Initialize()
        {
            if (mMotionController != null)
            {
                if (mWalkRunPivot == null) { mWalkRunPivot = mMotionController.GetMotion<WalkRunPivot>(); }
                if (mWalkRunStrafe == null) { mWalkRunStrafe = mMotionController.GetMotion<WalkRunStrafe>(); }
                if (mWalkRunRotate == null) { mWalkRunRotate = mMotionController.GetMotion<WalkRunRotate>(); }
            }
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            // If we're not startable, this is easy
            if (!mIsStartable)
            {
                return false;
            }

            // If we're not grounded, this is easy
            if (!mActorController.IsGrounded)
            {
                return false;
            }

            // Ensure we have input to test
            if (mMotionController._InputSource == null)
            {
                return false;
            }

            if (mMotionController._InputSource.IsJustPressed(_ActionAlias))
            {
                if (mActorController.State.Stance != EnumControllerStance.COMBAT_RANGED)
                {
                    // Perform an upward raycast to determine if something is overhead. If it is, we need
                    // to prevent or stop a jump
                    if (_RequiredOverheadDistance > 0)
                    {
                        if (RaycastExt.SafeRaycast(mActorController._Transform.position, mActorController._Transform.up, _RequiredOverheadDistance, mActorController._CollisionLayers, mActorController._Transform))
                        {
                            return false;
                        }
                    }

                    // If we're not in the middle of a jump, let it happen
                    if (IsInLandedState || !IsInMotionState)
                    {
                        return true;
                    }
                }
            }
                
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            if (mIsActivatedFrame)
            {
                return true;
            }

            if (mMotionLayer.ActiveMotionDuration > 5f)
            {
                return false;
            }

            // If we're not in a jump motion stake, we need to get out
            if (mIsAnimatorActive && !IsInMotionState)
            {
                // We also need to ensure we're not currently in a previous transition
                if (mMotionLayer._AnimatorTransitionID == 0)
                {
                    return false;
                }
            }

            // Perform an upward raycast to determine if something is overhead. If it is, we need
            // to prevent or stop a jump
            if (_RequiredOverheadDistance > 0)
            {
                if (RaycastExt.SafeRaycast(mMotionController.transform.position, Vector3.up, _RequiredOverheadDistance))
                {
                    return false;
                }
            } 
            
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Flag the motion as active
            mIsActivatedFrame = true;
            mIsStartable = false;

            // Force the camera to the default mode
            if (mMotionController.CameraRig != null)
            {
                mMotionController.CameraRig.Mode = 0;
            }

            // Attempt to find the hip bone if we have a name
            if (_ConvertToHipBase)
            {
                if (mHipBone == null)
                {
                    if (_HipBoneName.Length > 0)
                    {
                        mHipBone = mActorController._Transform.FindChild(_HipBoneName);
                    }

                    if (mHipBone == null)
                    {
                        Animator lAnimator = mMotionController.Animator;
                        if (lAnimator != null)
                        {
                            mHipBone = lAnimator.GetBoneTransform(HumanBodyBones.Hips);
                            if (mHipBone != null) { _HipBoneName = mHipBone.name; }
                        }
                    }
                }
            }

            // Reset the distance flag for this jump
            mLastHipDistance = 0f;

            // Clear out the impulse
            mIsImpulseApplied = false;

            // Grab the current velocities
            mLaunchForward = mActorController._Transform.forward;
            mLaunchVelocity = mActorController.State.Velocity;

            // Initialize the jump
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);

            // Report that we're good to enter the jump
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
        }

        /// <summary>
        /// Allows the motion to modify the root-motion velocities before they are applied. 
        /// 
        /// NOTE:
        /// Be careful when removing rotations as some transitions will want rotations even 
        /// if the state they are transitioning from don't.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rVelocityDelta">Root-motion linear velocity relative to the actor's forward</param>
        /// <param name="rRotationDelta">Root-motion rotational velocity</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rVelocityDelta, ref Quaternion rRotationDelta)
        {
            rRotationDelta = Quaternion.identity;

            if (!IsInLandedState)
            {
                rVelocityDelta = Vector3.zero;
            }
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            mMovement = Vector3.zero;

            bool lAllowSlide = false;
            float lHipDistanceDelta = 0f;

            // Since we're not doing any lerping or physics based stuff here,
            // we'll only process once per cyle even if we're running slow.
            if (rUpdateIndex != 1) { return; }

            // Grab the state info once
            MotionState lState = mMotionController.State;

            int lStateID = mMotionLayer._AnimatorStateID;
            float lStateTime = mMotionLayer._AnimatorStateNormalizedTime;

            int lTransitionID = mMotionLayer._AnimatorTransitionID;
            float lTransitionTime = mMotionLayer._AnimatorTransitionNormalizedTime;

            // We do the inverse tilt so we calculate the rotation in "natural up" space vs. "actor up" space. 
            Quaternion lInvTilt = QuaternionExt.FromToRotation(mActorController._Transform.up, Vector3.up);
            Vector3 lVelocity = lInvTilt * mActorController.State.Velocity;

            // If we have a hip bone, we'll adjust the jump based on the distance
            // that changes between the foot and the hips. This way, the jump is
            // "hip based" and not "foot based".
            if (_ConvertToHipBase && mHipBone != null)
            {
                Vector3 lLocalPosition = -mHipBone.InverseTransformPoint(mActorController._Transform.position);
                float lHipDistance = lLocalPosition.y;

                // As the distance gets smaller, we increase the shift
                lHipDistanceDelta = -(lHipDistance - mLastHipDistance);
                mLastHipDistance = lHipDistance;
            }

            // Blend that occurs before we jump
            if (lTransitionID == TRANS_EntryState_JumpRise)
            {
                if (!mIsImpulseApplied && lTransitionTime > 0.5f)
                {
                    mIsImpulseApplied = true;
                    mActorController.AddImpulse(mActorController._Transform.up * _Impulse);
                }
            }
            // This is the start of the jump. The animator will automatically move on after the node
            // has finished. However, it could move to the "JumpRisePose" or "JumpRiseToTop"
            else if (lStateID == STATE_JumpRise)
            {
                // We really shouldn't get here, but if the transition time is 
                // too short, we'll end up skipping past the previous impluse. This
                // happens with something like the idle pose
                if (!mIsImpulseApplied)
                {
                    mIsImpulseApplied = true;
                    mActorController.AddImpulse(mActorController._Transform.up * _Impulse);
                }
                // If our velocity is trailing off, move to the top position
                else if (lVelocity.y < 1.5f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RISE_TO_TOP);
                }
            }
            // This is the holding position for a super high jump. The pose gives us time
            // before the top occurs.
            else if (lStateID == STATE_JumpRisePose)
            {
                // If our velocity is trailing off, move to the top position
                if (lVelocity.y < 2.5f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RISE_TO_TOP);
                }

                //mMovement = (mActorController._Transform.rotation * new Vector3(0f, lHipDistanceDelta, 0f));
                mMovement = mActorController._Transform.up * lHipDistanceDelta;
            }
            // At this point, we're close to the peak of the jump and we need to transition
            // into the top position.
            else if (lStateID == STATE_JumpRiseToTop)
            {
                // If we slow down, start moving to the fall position
                if (lVelocity.y < -1.5f) // || lState.GroundDistance < 0.45f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_TOP_TO_FALL);
                }

                //mMovement = (mActorController._Transform.rotation * new Vector3(0f, lHipDistanceDelta, 0f));
                mMovement = mActorController._Transform.up * lHipDistanceDelta;
            }
            // We should be at the peak of the jump. We don't expect to wait here
            // long, but this gives us a "pose" to hold onto if needed
            else if (lStateID == STATE_JumpTopPose)
            {
                // We may have moved over something during the jump. If so, 
                // we can move straight to the recover
                if (mActorController.State.GroundSurfaceDistance < 0.5f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RECOVER_TO_IDLE);
                }
                // If we've reached the fall speed, transition
                else if (lVelocity.y < -0.25f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_TOP_TO_FALL);
                }
                // Otherwise, ensure we're in the right phase
                else
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_TOP);
                }

                mMovement = mActorController._Transform.up * lHipDistanceDelta;
            }
            // Here we come out of the top pose and start moving into the fall pose
            else if (lStateID == STATE_JumpTopToFall)
            {
                // If we got ontop of something, we may need to recover.
                if (mActorController.State.GroundSurfaceDistance < 0.15f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RECOVER_TO_IDLE);
                }
                // Look for the ground and prepare to transition
                else if (mActorController.State.GroundSurfaceDistance < 0.35f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_LAND);
                }

                mMovement = mActorController._Transform.up * lHipDistanceDelta;
            }
            // We could be falling for a while. This animation allows us to 
            // hold in the falling state until we hit the ground.
            else if (lStateID == STATE_JumpFallPose)
            {
                //mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, Jump_old.PHASE_FALL);

                // Look for the ground. In this case, we want a 
                // value that is slightly greater than our collider radius
                if (mActorController.State.GroundSurfaceDistance < 0.35f)
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_LAND);
                }

                mMovement = mActorController._Transform.up * lHipDistanceDelta;
            }
            // This is the first state in the jump where we hit the ground
            else if (lStateID == STATE_JumpLand)
            {
                if (mActorController.State.IsGrounded)
                {
                    // If there is no controller input, we can stop
                    if (lState.InputMagnitudeTrend.Value < 0.03f)
                    {
                        mLaunchVelocity = Vector3.zero;
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RECOVER_TO_IDLE);
                    }
                    // If the player is messing with the input, we need to think about
                    // what to transition to
                    else
                    {
                        if ((mWalkRunPivot != null && mWalkRunPivot.IsEnabled && mWalkRunPivot.IsRunActive) ||
                            (mWalkRunStrafe != null && mWalkRunStrafe.IsEnabled && mWalkRunStrafe.IsRunActive) ||
                            (mWalkRunRotate != null && mWalkRunRotate.IsEnabled && mWalkRunRotate.IsRunActive))
                        {
                            if (Mathf.Abs(lState.InputFromAvatarAngle) > 140) { mLaunchVelocity = Vector3.zero; }
                            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RECOVER_TO_MOVE);
                        }
                        else
                        {
                            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RECOVER_TO_IDLE);
                        }
                    }
                }
                else
                {
                    mMovement = mActorController._Transform.up * lHipDistanceDelta;
                }
            }
            // Called when the avatar starts to come out of the impact
            else if (lStateID == STATE_JumpRecoverIdle)
            {
                mIsStartable = true;

                // If we're moving forward, transition into the run/walk
                if (lState.InputMagnitudeTrend.Value >= 0.1f && Mathf.Abs(lState.InputFromAvatarAngle) < 20f)
                {
                    // Allow us to keep moving so we blend into the run
                    lAllowSlide = true;

                    // It may be time to move into the walk/run
                    if (lStateTime > 0.3f)
                    {
                        if (mWalkRunPivot != null && mWalkRunPivot.IsEnabled)
                        {
                            mWalkRunPivot.StartInRun = mWalkRunPivot.IsRunActive;
                            mWalkRunPivot.StartInWalk = !mWalkRunPivot.StartInRun;
                            mMotionController.ActivateMotion(mWalkRunPivot);
                        }
                        else if (mWalkRunStrafe != null && mWalkRunStrafe.IsEnabled)
                        {
                            mWalkRunStrafe.StartInRun = mWalkRunStrafe.IsRunActive;
                            mWalkRunStrafe.StartInWalk = !mWalkRunStrafe.StartInRun;
                            mMotionController.ActivateMotion(mWalkRunStrafe);
                        }
                        else if (mWalkRunRotate != null && mWalkRunRotate.IsEnabled)
                        {
                            mWalkRunRotate.StartInRun = mWalkRunRotate.IsRunActive;
                            mWalkRunRotate.StartInWalk = !mWalkRunRotate.StartInRun;
                            mMotionController.ActivateMotion(mWalkRunRotate);
                        }
                    }
                }
            }
            // Called when the avatar starts to come out of the impact
            else if (lStateID == STATE_JumpRecoverRun)
            {
                // Allow the animation to control movement again
                mLaunchVelocity = Vector3.zero;

                // It may be time to move into the walk/run
                if (mWalkRunPivot != null && mWalkRunPivot.IsEnabled)
                {
                    // Allow us to keep moving so we blend into the run
                    lAllowSlide = true;

                    if (lStateTime > 0.2f)
                    {
                        mWalkRunPivot.StartInRun = mWalkRunPivot.IsRunActive;
                        mWalkRunPivot.StartInWalk = !mWalkRunPivot.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunPivot);
                    }
                }
                else if (mWalkRunStrafe != null && mWalkRunStrafe.IsEnabled)
                {
                    // Allow us to keep moving so we blend into the run
                    lAllowSlide = true;

                    if (lStateTime > 0.2f)
                    {
                        mWalkRunStrafe.StartInRun = mWalkRunStrafe.IsRunActive;
                        mWalkRunStrafe.StartInWalk = !mWalkRunStrafe.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunStrafe);
                    }
                }
                else if (mWalkRunRotate != null && mWalkRunRotate.IsEnabled)
                {
                    // Allow us to keep moving so we blend into the run
                    lAllowSlide = true;

                    if (lStateTime > 0.2f)
                    {
                        mWalkRunRotate.StartInRun = mWalkRunRotate.IsRunActive;
                        mWalkRunRotate.StartInWalk = !mWalkRunRotate.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunRotate);
                    }
                }
                else
                {
                    Deactivate();
                }
            }
            // If there's no movement, we're done
            else if (lStateID == STATE_IdlePose)
            {
                Deactivate();
            }


            // Set the controller state with the modified values
            mMotionController.State = lState;

            // Determine the resulting velocity of this update
            mVelocity = DetermineVelocity(lAllowSlide);
        }

        /// <summary>
        /// Returns the current velocity of the motion
        /// </summary>
        protected Vector3 DetermineVelocity(bool rAllowSlide)
        {
            Vector3 lVelocity = Vector3.zero;
            int lStateID = mMotionLayer._AnimatorStateID;

            // TRT 11/20/15: If we're colliding with an object, we won't allow
            // any velocity. This helps prevent sliding while jumping
            // against an object.
            if (mActorController.State.IsColliding)
            {
                return lVelocity;
            }

            // If were in the midst of jumping, we want to add velocity based on 
            // the magnitude of the controller. 
            if ((lStateID != STATE_JumpRecoverIdle || rAllowSlide) && 
                (lStateID != STATE_JumpRecoverRun || rAllowSlide) &&
                IsInMotionState)
            {
                MotionState lState = mMotionController.State;

                // Speed that comes from momenum
                Vector3 lMomentum = mLaunchVelocity;
                float lMomentumSpeed = (_IsMomentumEnabled ? lMomentum.magnitude : 0f);

                // Speed that comes from the user
                float lControlSpeed = (_IsControlEnabled ? _ControlSpeed * lState.InputMagnitudeTrend.Value : 0f);

                // Speed we'll use as the character is jumping
                float lAirSpeed = Mathf.Max(lMomentumSpeed, lControlSpeed);

                // If we allow control, let the player determine the direction
                if (_IsControlEnabled)
                {
                    Vector3 lBaseForward = mActorController._Transform.forward;
                    if (mMotionController._InputSource != null && mMotionController._InputSource.IsEnabled)
                    {
                        if (mMotionController._CameraTransform != null)
                        {
                            lBaseForward = mMotionController._CameraTransform.forward;
                        }
                    }

                    // Create a quaternion that gets us from our world-forward to our actor/camera direction.
                    // FromToRotation creates a quaternion using the shortest method which can sometimes
                    // flip the angle. LookRotation will attempt to keep the "up" direction "up".
                    Quaternion lToBaseForward = Quaternion.LookRotation(lBaseForward, mActorController._Transform.up);

                    // Determine the avatar displacement direction. This isn't just
                    // normal movement forward, but includes movement to the side
                    Vector3 lMoveDirection = lToBaseForward * lState.InputForward;

                    // Apply the direction and speed
                    lVelocity = lVelocity + (lMoveDirection * lAirSpeed);
                }

                // If momementum is enabled, add it to keep the player moving in the direction of the jump
                if (_IsMomentumEnabled)
                {
                    lVelocity = lVelocity + lMomentum;
                }

                // Don't exceed our air speed
                if (lVelocity.magnitude > lAirSpeed)
                {
                    lVelocity = lVelocity.normalized * lAirSpeed;
                }
            }

            return lVelocity;
        }

        /// <summary>
        /// Test to see if we're currently in a jump state prior to landing
        /// </summary>
        protected bool IsInMidJumpState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                if (lStateID == STATE_JumpRise) { return true; }
                if (lStateID == STATE_JumpRisePose) { return true; }
                if (lStateID == STATE_JumpRiseToTop) { return true; }
                if (lStateID == STATE_JumpTopPose) { return true; }
                if (lStateID == STATE_JumpTopToFall) { return true; }
                if (lStateID == STATE_JumpFallPose) { return true; }
                if (lStateID == STATE_JumpLand) { return true; }

                int lTransitionID = mMotionLayer._AnimatorTransitionID;
                if (lTransitionID == TRANS_EntryState_JumpRise) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpRise) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpFallPose) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpFallPose) { return true; }

                return false;
            }            
        }

        /// <summary>
        /// Determines if we're in one of the landed states. Mostly this is so we
        /// can stop adding movement.
        /// </summary>
        protected bool IsInLandedState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                //if (lStateID == STATE_JumpRecoverIdle) { return true; }
                if (lStateID == STATE_JumpRecoverRun) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }

                return false;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers a climb."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            bool lNewConvertToHipBase = EditorGUILayout.Toggle(new GUIContent("Convert To Hip Base", "Determines if we apply the physics to the hip bone vs. feet."), ConvertToHipBase);
            if (lNewConvertToHipBase != ConvertToHipBase)
            {
                lIsDirty = true;
                ConvertToHipBase = lNewConvertToHipBase;
            }

            string lNewHipBoneName = EditorGUILayout.TextField(new GUIContent("Hip Bone", "Name of the hip bone for adjusting the jump root."), HipBoneName);
            if (lNewHipBoneName != HipBoneName)
            {
                lIsDirty = true;
                HipBoneName = lNewHipBoneName;
            }

            float lNewImpulse = EditorGUILayout.FloatField(new GUIContent("Impulse", "Strength of the jump as an instant force."), Impulse);
            if (lNewImpulse != Impulse)
            {
                lIsDirty = true;
                Impulse = lNewImpulse;
            }

            bool lNewIsMomentumEnabled = EditorGUILayout.Toggle(new GUIContent("Is Momentum Enabled", "Determines if the avatar's speed and direction before the jump are used to propel the avatar while in the air."), IsMomentumEnabled);
            if (lNewIsMomentumEnabled != IsMomentumEnabled)
            {
                lIsDirty = true;
                IsMomentumEnabled = lNewIsMomentumEnabled;
            }

            bool lNewIsControlEnabled = EditorGUILayout.Toggle(new GUIContent("Is Control Enabled", "Determines if the player can control the avatar while in the air."), IsControlEnabled);
            if (lNewIsControlEnabled != IsControlEnabled)
            {
                lIsDirty = true;
                IsControlEnabled = lNewIsControlEnabled;
            }

            float lNewControlSpeed = EditorGUILayout.FloatField(new GUIContent("Control Speed", "Speed of the avatar when in the air. This should roughly match the ground speed of the avatar."), ControlSpeed);
            if (lNewControlSpeed != ControlSpeed)
            {
                lIsDirty = true;
                ControlSpeed = lNewControlSpeed;
            }

            float lNewRequiredOverheadDistance = EditorGUILayout.FloatField(new GUIContent("Required Overhead Distance", "When greater than 0, a test will be made to determine if we can jump or can continue a jump."), RequiredOverheadDistance);
            if (lNewRequiredOverheadDistance != RequiredOverheadDistance)
            {
                lIsDirty = true;
                RequiredOverheadDistance = lNewRequiredOverheadDistance;
            }

            return lIsDirty;
        }

#endif

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int TRANS_EntryState_JumpRise = -1;
        public static int TRANS_AnyState_JumpRise = -1;
        public static int TRANS_EntryState_JumpFallPose = -1;
        public static int TRANS_AnyState_JumpFallPose = -1;
        public static int STATE_JumpRise = -1;
        public static int TRANS_JumpRise_JumpRiseToTop = -1;
        public static int TRANS_JumpRise_JumpRisePose = -1;
        public static int STATE_JumpLand = -1;
        public static int TRANS_JumpLand_JumpRecoverRun = -1;
        public static int TRANS_JumpLand_JumpRecoverIdle = -1;
        public static int STATE_JumpRisePose = -1;
        public static int TRANS_JumpRisePose_JumpRiseToTop = -1;
        public static int STATE_JumpFallPose = -1;
        public static int TRANS_JumpFallPose_JumpLand = -1;
        public static int STATE_JumpTopToFall = -1;
        public static int TRANS_JumpTopToFall_JumpLand = -1;
        public static int TRANS_JumpTopToFall_JumpFallPose = -1;
        public static int TRANS_JumpTopToFall_JumpRecoverIdle = -1;
        public static int TRANS_JumpTopToFall_JumpRecoverRun = -1;
        public static int STATE_JumpRiseToTop = -1;
        public static int TRANS_JumpRiseToTop_JumpTopToFall = -1;
        public static int TRANS_JumpRiseToTop_JumpTopPose = -1;
        public static int TRANS_JumpRiseToTop_JumpRecoverIdle = -1;
        public static int TRANS_JumpRiseToTop_JumpRecoverRun = -1;
        public static int STATE_JumpTopPose = -1;
        public static int TRANS_JumpTopPose_JumpTopToFall = -1;
        public static int TRANS_JumpTopPose_JumpRecoverIdle = -1;
        public static int STATE_JumpRecoverIdle = -1;
        public static int TRANS_JumpRecoverIdle_IdlePose = -1;
        public static int STATE_JumpRecoverRun = -1;
        public static int STATE_IdlePose = -1;

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsInMotionState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lStateID == STATE_JumpRise) { return true; }
                if (lStateID == STATE_JumpLand) { return true; }
                if (lStateID == STATE_JumpRisePose) { return true; }
                if (lStateID == STATE_JumpFallPose) { return true; }
                if (lStateID == STATE_JumpTopToFall) { return true; }
                if (lStateID == STATE_JumpRiseToTop) { return true; }
                if (lStateID == STATE_JumpTopPose) { return true; }
                if (lStateID == STATE_JumpRecoverIdle) { return true; }
                if (lStateID == STATE_JumpRecoverRun) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpRise) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpRise) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpFallPose) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpFallPose) { return true; }
                if (lTransitionID == TRANS_JumpRise_JumpRiseToTop) { return true; }
                if (lTransitionID == TRANS_JumpRise_JumpRisePose) { return true; }
                if (lTransitionID == TRANS_JumpLand_JumpRecoverRun) { return true; }
                if (lTransitionID == TRANS_JumpLand_JumpRecoverIdle) { return true; }
                if (lTransitionID == TRANS_JumpRisePose_JumpRiseToTop) { return true; }
                if (lTransitionID == TRANS_JumpFallPose_JumpLand) { return true; }
                if (lTransitionID == TRANS_JumpTopToFall_JumpLand) { return true; }
                if (lTransitionID == TRANS_JumpTopToFall_JumpFallPose) { return true; }
                if (lTransitionID == TRANS_JumpTopToFall_JumpRecoverIdle) { return true; }
                if (lTransitionID == TRANS_JumpTopToFall_JumpRecoverRun) { return true; }
                if (lTransitionID == TRANS_JumpRiseToTop_JumpTopToFall) { return true; }
                if (lTransitionID == TRANS_JumpRiseToTop_JumpTopPose) { return true; }
                if (lTransitionID == TRANS_JumpRiseToTop_JumpRecoverIdle) { return true; }
                if (lTransitionID == TRANS_JumpRiseToTop_JumpRecoverRun) { return true; }
                if (lTransitionID == TRANS_JumpTopPose_JumpTopToFall) { return true; }
                if (lTransitionID == TRANS_JumpTopPose_JumpRecoverIdle) { return true; }
                if (lTransitionID == TRANS_JumpRecoverIdle_IdlePose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_JumpRise) { return true; }
            if (rStateID == STATE_JumpLand) { return true; }
            if (rStateID == STATE_JumpRisePose) { return true; }
            if (rStateID == STATE_JumpFallPose) { return true; }
            if (rStateID == STATE_JumpTopToFall) { return true; }
            if (rStateID == STATE_JumpRiseToTop) { return true; }
            if (rStateID == STATE_JumpTopPose) { return true; }
            if (rStateID == STATE_JumpRecoverIdle) { return true; }
            if (rStateID == STATE_JumpRecoverRun) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_JumpRise) { return true; }
            if (rStateID == STATE_JumpLand) { return true; }
            if (rStateID == STATE_JumpRisePose) { return true; }
            if (rStateID == STATE_JumpFallPose) { return true; }
            if (rStateID == STATE_JumpTopToFall) { return true; }
            if (rStateID == STATE_JumpRiseToTop) { return true; }
            if (rStateID == STATE_JumpTopPose) { return true; }
            if (rStateID == STATE_JumpRecoverIdle) { return true; }
            if (rStateID == STATE_JumpRecoverRun) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpRise) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpRise) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpFallPose) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpFallPose) { return true; }
            if (rTransitionID == TRANS_JumpRise_JumpRiseToTop) { return true; }
            if (rTransitionID == TRANS_JumpRise_JumpRisePose) { return true; }
            if (rTransitionID == TRANS_JumpLand_JumpRecoverRun) { return true; }
            if (rTransitionID == TRANS_JumpLand_JumpRecoverIdle) { return true; }
            if (rTransitionID == TRANS_JumpRisePose_JumpRiseToTop) { return true; }
            if (rTransitionID == TRANS_JumpFallPose_JumpLand) { return true; }
            if (rTransitionID == TRANS_JumpTopToFall_JumpLand) { return true; }
            if (rTransitionID == TRANS_JumpTopToFall_JumpFallPose) { return true; }
            if (rTransitionID == TRANS_JumpTopToFall_JumpRecoverIdle) { return true; }
            if (rTransitionID == TRANS_JumpTopToFall_JumpRecoverRun) { return true; }
            if (rTransitionID == TRANS_JumpRiseToTop_JumpTopToFall) { return true; }
            if (rTransitionID == TRANS_JumpRiseToTop_JumpTopPose) { return true; }
            if (rTransitionID == TRANS_JumpRiseToTop_JumpRecoverIdle) { return true; }
            if (rTransitionID == TRANS_JumpRiseToTop_JumpRecoverRun) { return true; }
            if (rTransitionID == TRANS_JumpTopPose_JumpTopToFall) { return true; }
            if (rTransitionID == TRANS_JumpTopPose_JumpRecoverIdle) { return true; }
            if (rTransitionID == TRANS_JumpRecoverIdle_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            /// <summary>
            /// These assignments go inside the 'LoadAnimatorData' function so that we can
            /// extract and assign the hash values for this run. These are typically used for debugging.
            /// </summary>
            TRANS_EntryState_JumpRise = mMotionController.AddAnimatorName("Entry -> Base Layer.Jump-SM.JumpRise");
            TRANS_AnyState_JumpRise = mMotionController.AddAnimatorName("AnyState -> Base Layer.Jump-SM.JumpRise");
            TRANS_EntryState_JumpFallPose = mMotionController.AddAnimatorName("Entry -> Base Layer.Jump-SM.JumpFallPose");
            TRANS_AnyState_JumpFallPose = mMotionController.AddAnimatorName("AnyState -> Base Layer.Jump-SM.JumpFallPose");
            STATE_JumpRise = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRise");
            TRANS_JumpRise_JumpRiseToTop = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRise -> Base Layer.Jump-SM.JumpRiseToTop");
            TRANS_JumpRise_JumpRisePose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRise -> Base Layer.Jump-SM.JumpRisePose");
            STATE_JumpLand = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpLand");
            TRANS_JumpLand_JumpRecoverRun = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpLand -> Base Layer.Jump-SM.JumpRecoverRun");
            TRANS_JumpLand_JumpRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpLand -> Base Layer.Jump-SM.JumpRecoverIdle");
            STATE_JumpRisePose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRisePose");
            TRANS_JumpRisePose_JumpRiseToTop = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRisePose -> Base Layer.Jump-SM.JumpRiseToTop");
            STATE_JumpFallPose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpFallPose");
            TRANS_JumpFallPose_JumpLand = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpFallPose -> Base Layer.Jump-SM.JumpLand");
            STATE_JumpTopToFall = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopToFall");
            TRANS_JumpTopToFall_JumpLand = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopToFall -> Base Layer.Jump-SM.JumpLand");
            TRANS_JumpTopToFall_JumpFallPose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopToFall -> Base Layer.Jump-SM.JumpFallPose");
            TRANS_JumpTopToFall_JumpRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopToFall -> Base Layer.Jump-SM.JumpRecoverIdle");
            TRANS_JumpTopToFall_JumpRecoverRun = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopToFall -> Base Layer.Jump-SM.JumpRecoverRun");
            STATE_JumpRiseToTop = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRiseToTop");
            TRANS_JumpRiseToTop_JumpTopToFall = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRiseToTop -> Base Layer.Jump-SM.JumpTopToFall");
            TRANS_JumpRiseToTop_JumpTopPose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRiseToTop -> Base Layer.Jump-SM.JumpTopPose");
            TRANS_JumpRiseToTop_JumpRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRiseToTop -> Base Layer.Jump-SM.JumpRecoverIdle");
            TRANS_JumpRiseToTop_JumpRecoverRun = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRiseToTop -> Base Layer.Jump-SM.JumpRecoverRun");
            STATE_JumpTopPose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopPose");
            TRANS_JumpTopPose_JumpTopToFall = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopPose -> Base Layer.Jump-SM.JumpTopToFall");
            TRANS_JumpTopPose_JumpRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpTopPose -> Base Layer.Jump-SM.JumpRecoverIdle");
            STATE_JumpRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRecoverIdle");
            TRANS_JumpRecoverIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRecoverIdle -> Base Layer.Jump-SM.IdlePose");
            STATE_JumpRecoverRun = mMotionController.AddAnimatorName("Base Layer.Jump-SM.JumpRecoverRun");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Jump-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip mIdleToRise = null;
        private AnimationClip mFallToLand = null;
        private AnimationClip mRisePose = null;
        private AnimationClip mFall = null;
        private AnimationClip mTopToFall = null;
        private AnimationClip mRiseToTop = null;
        private AnimationClip mTopPose = null;
        private AnimationClip mLandToIdle = null;
        private AnimationClip mLandToRun2 = null;
        private AnimationClip mIdlePose = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;

            // If we find the sm with our name, remove it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                // Look for a sm with the matching name
                if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)
                {
                    // Allow the user to stop before we remove the sm
                    if (!UnityEditor.EditorUtility.DisplayDialog("Motion Controller", _EditorAnimatorSMName + " already exists. Delete and recreate it?", "Yes", "No"))
                    {
                        return;
                    }

                    // Remove the sm
                    lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);
                }
            }

            UnityEditor.Animations.AnimatorStateMachine lMotionStateMachine = lRootStateMachine.AddStateMachine(_EditorAnimatorSMName);

            // Attach the behaviour if needed
            if (_EditorAttachBehaviour)
            {
                MotionControllerBehaviour lBehaviour = lMotionStateMachine.AddStateMachineBehaviour(typeof(MotionControllerBehaviour)) as MotionControllerBehaviour;
                lBehaviour._MotionKey = (_Key.Length > 0 ? _Key : this.GetType().FullName);
            }

            UnityEditor.Animations.AnimatorState lJumpRise = lMotionStateMachine.AddState("JumpRise", new Vector3(-12, 132, 0));
            lJumpRise.motion = mIdleToRise;
            lJumpRise.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpLand = lMotionStateMachine.AddState("JumpLand", new Vector3(852, 132, 0));
            lJumpLand.motion = mFallToLand;
            lJumpLand.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpRisePose = lMotionStateMachine.AddState("JumpRisePose", new Vector3(132, 12, 0));
            lJumpRisePose.motion = mRisePose;
            lJumpRisePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpFallPose = lMotionStateMachine.AddState("JumpFallPose", new Vector3(660, 0, 0));
            lJumpFallPose.motion = mFall;
            lJumpFallPose.speed = 0.8f;

            UnityEditor.Animations.AnimatorState lJumpTopToFall = lMotionStateMachine.AddState("JumpTopToFall", new Vector3(552, 132, 0));
            lJumpTopToFall.motion = mTopToFall;
            lJumpTopToFall.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpRiseToTop = lMotionStateMachine.AddState("JumpRiseToTop", new Vector3(252, 132, 0));
            lJumpRiseToTop.motion = mRiseToTop;
            lJumpRiseToTop.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpTopPose = lMotionStateMachine.AddState("JumpTopPose", new Vector3(396, 12, 0));
            lJumpTopPose.motion = mTopPose;
            lJumpTopPose.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpRecoverIdle = lMotionStateMachine.AddState("JumpRecoverIdle", new Vector3(948, -84, 0));
            lJumpRecoverIdle.motion = mLandToIdle;
            lJumpRecoverIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpRecoverRun = lMotionStateMachine.AddState("JumpRecoverRun", new Vector3(936, 252, 0));
            lJumpRecoverRun.motion = mLandToRun2;
            lJumpRecoverRun.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(1176, -84, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpRise);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.5f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 251f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpFallPose);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.2f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 250f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lJumpRise.AddTransition(lJumpRiseToTop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9427966f;
            lStateTransition.duration = 0.07627118f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 203f, "L0MotionPhase");

            lStateTransition = lJumpRise.AddTransition(lJumpRisePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9455966f;
            lStateTransition.duration = 0.05858077f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lJumpLand.AddTransition(lJumpRecoverRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.882005f;
            lStateTransition.duration = 0.117995f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 209f, "L0MotionPhase");

            lStateTransition = lJumpLand.AddTransition(lJumpRecoverIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8636364f;
            lStateTransition.duration = 0.169278f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 208f, "L0MotionPhase");

            lStateTransition = lJumpRisePose.AddTransition(lJumpRiseToTop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 203f, "L0MotionPhase");

            lStateTransition = lJumpFallPose.AddTransition(lJumpLand);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.02201979f;
            lStateTransition.duration = 0.05033548f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 207f, "L0MotionPhase");

            lStateTransition = lJumpTopToFall.AddTransition(lJumpLand);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 207f, "L0MotionPhase");

            lStateTransition = lJumpTopToFall.AddTransition(lJumpFallPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.840604f;
            lStateTransition.duration = 0.159396f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lJumpTopToFall.AddTransition(lJumpRecoverIdle);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4982873f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 208f, "L0MotionPhase");

            lStateTransition = lJumpTopToFall.AddTransition(lJumpRecoverRun);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.5029359f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 209f, "L0MotionPhase");

            lStateTransition = lJumpRiseToTop.AddTransition(lJumpTopToFall);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.903662f;
            lStateTransition.duration = 0.1926761f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 205f, "L0MotionPhase");

            lStateTransition = lJumpRiseToTop.AddTransition(lJumpTopPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 203f, "L0MotionPhase");

            lStateTransition = lJumpRiseToTop.AddTransition(lJumpRecoverIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.07359297f;
            lStateTransition.duration = 1.948052f;
            lStateTransition.offset = 0.005134339f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 208f, "L0MotionPhase");

            lStateTransition = lJumpRiseToTop.AddTransition(lJumpRecoverRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 2.5f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 209f, "L0MotionPhase");

            lStateTransition = lJumpTopPose.AddTransition(lJumpTopToFall);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2013423f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 205f, "L0MotionPhase");

            lStateTransition = lJumpTopPose.AddTransition(lJumpRecoverIdle);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 1.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 208f, "L0MotionPhase");

            lStateTransition = lJumpRecoverIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8000917f;
            lStateTransition.duration = 0.1463132f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mIdleToRise = CreateAnimationField("JumpRise", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/IdleToRise.anim", "IdleToRise", mIdleToRise);
            mFallToLand = CreateAnimationField("JumpLand", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/FallToLand.anim", "FallToLand", mFallToLand);
            mRisePose = CreateAnimationField("JumpRisePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/RisePose.anim", "RisePose", mRisePose);
            mFall = CreateAnimationField("JumpFallPose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/Fall.anim", "Fall", mFall);
            mTopToFall = CreateAnimationField("JumpTopToFall", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/TopToFall.anim", "TopToFall", mTopToFall);
            mRiseToTop = CreateAnimationField("JumpRiseToTop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/RiseToTop.anim", "RiseToTop", mRiseToTop);
            mTopPose = CreateAnimationField("JumpTopPose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/TopPose.anim", "TopPose", mTopPose);
            mLandToIdle = CreateAnimationField("JumpRecoverIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/LandToIdle.anim", "LandToIdle", mLandToIdle);
            mLandToRun2 = CreateAnimationField("JumpRecoverRun", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/ootii_Jump.fbx/LandToRun2.anim", "LandToRun2", mLandToRun2);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
