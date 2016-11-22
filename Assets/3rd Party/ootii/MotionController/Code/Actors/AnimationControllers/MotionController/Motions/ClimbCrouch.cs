using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Handles the basic motion for getting the avatar into the
    /// crouch position and moving them from the crouch to idle.
    /// </summary>
    [MotionName("Climb Crouch")]
    [MotionDescription("Allows the avatar to move into a 'cat grab' parkour style position. When jumping or falling " +
                   "the avatar will attempt to grab a ledge. From there, this motions will allow them to move " +
                   "left or right or climb to the top of the ledge.")]
    public class ClimbCrouch : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 300;
        public const int PHASE_FROM_IDLE = 300;
        public const int PHASE_FROM_JUMP_RISE = 301;
        public const int PHASE_FROM_JUMP_TOP = 302;
        public const int PHASE_FROM_JUMP_FALL = 303;
        public const int PHASE_FROM_JUMP_IMPACT = 304;
        public const int PHASE_IDLE = 320;
        public const int PHASE_TO_TOP = 350;
        public const int PHASE_TO_FALL = 370;
        public const int PHASE_SHIMMY_LEFT = 380;
        public const int PHASE_SHIMMY_RIGHT = 385;

        /// <summary>
        /// Keeps us from having to reallocate over and over
        /// </summary>
        //private static RaycastHit sCollisionInfo = new RaycastHit();
        private static RaycastHit sCollisionUpdateInfo = new RaycastHit();

        /// <summary>
        /// Ensure when we're trying to climb with the crouch that
        /// the avatar is at a certain ground distance.
        /// Otherwise, it's odd to latch onto something when the avatar
        /// jumps 0.1m.
        /// </summary>
        public float _MinGroundDistance = 0.3f;
        public float MinGroundDistance
        {
            get { return _MinGroundDistance; }
            set { _MinGroundDistance = value; }
        }

        /// <summary>
        /// Minimum distance the new grab point must be from the last
        /// grab point in order for the grab to work.
        /// </summary>
        public float _MinRegrabDistance = 1.0f;
        public float MinRegrabDistance
        {
            get { return _MinRegrabDistance; }
            set { _MinRegrabDistance = value; }
        }

        /// <summary>
        /// The X distance from the grab position that the hands will
        /// be positions. If a value is set, we'll check to make sure there
        /// is something for them to grab or fail.
        /// </summary>
        public float _HandGrabOffset = 0.13f;
        public float HandGrabOffset
        {
            get { return _HandGrabOffset; }
            set { _HandGrabOffset = value; }
        }

        /// <summary>
        /// Target of the character's body from the grab position relative
        /// to the grab position in the direction of the body.
        /// </summary>
        public Vector3 _BodyTargetOffset = new Vector3(0f, -1.35f, -0.6f);
        public Vector3 BodyTargetOffset
        {
            get { return _BodyTargetOffset; }
            set { _BodyTargetOffset = value; }
        }

        /// <summary>
        /// Offset to the final position of the animation used to help the 
        /// character line up with the idle (or other) animation that will follow
        /// after it.
        /// </summary>
        public Vector3 _ExitPositionOffset = new Vector3(0f, 0.015f, 0f);
        public Vector3 ExitPositionOffset
        {
            get { return _ExitPositionOffset; }
            set { _ExitPositionOffset = value; }
        }

        /// <summary>
        /// Offset to add to the root motion velocity when the character starts
        /// to climb up to the top.
        /// </summary>
        public Vector3 _ToTopVelocity = Vector3.zero;
        public Vector3 ToTopVelocity
        {
            get { return _ToTopVelocity; }
            set { _ToTopVelocity = value; }
        }

        /// <summary>
        /// User layer id set for objects that are climbable.
        /// </summary>
        public int _ClimbableLayers = 1;
        public int ClimbableLayers
        {
            get { return _ClimbableLayers; }
            set { _ClimbableLayers = value; }
        }

        /// <summary>
        /// Deteremines the amount of room needed in order for
        /// the climbing character to move left or right.
        /// </summary>
        public float _MinSideSpaceForMove = 0.6f;

        [MotionDescription("Minimum space required for the avatar to shimmy to the left or right.")]
        public float MinSideSpaceForMove
        {
            get { return _MinSideSpaceForMove; }
            set { _MinSideSpaceForMove = value; }
        }

        /// <summary>
        /// Tracks the object that is being climbed
        /// </summary>
        protected GameObject mClimbable = null;

        /// <summary>
        /// Rotation it takes to get to facing the climbable's normal
        /// </summary>
        protected float mFaceClimbableNormalAngle = 0f;

        /// <summary>
        /// Amount of rotation that is already used
        /// </summary>
        protected float mFaceClimbableNormalAngleUsed = 0f;

        /// <summary>
        /// Keeps us from having to reallocate over and over
        /// </summary>
        protected RaycastHit mRaycastHitInfo = RaycastExt.EmptyHitInfo;

        /// <summary>
        /// Tracks the last grab position
        /// </summary>
        protected Vector3 mGrabPosition = Vector3.zero;

        /// <summary>
        /// Tracks the last grab position relative to the climbable
        /// </summary>
        protected Vector3 mLocalGrabPosition = Vector3.zero;

        /// <summary>
        /// Tracks the contact position from the avatar's perspective
        /// </summary>
        protected Vector3 mAvatarContactPosition = Vector3.zero;

        /// <summary>
        /// Normal extruding out of the climbable
        /// </summary>
        protected Vector3 mGrabPositionNormal = Vector3.zero;

        /// <summary>
        /// Tracks where we want the avatar to go
        /// </summary>
        protected Vector3 mTargetPosition = Vector3.zero;

        /// <summary>
        /// Tracks where we want the right hand to go
        /// </summary>
        protected Vector3 mRightHandTargetPosition = Vector3.zero;

        /// <summary>
        /// Tracks where we want the left hand to go
        /// </summary>
        protected Vector3 mLeftHandTargetPosition = Vector3.zero;

        /// <summary>
        /// Determines if the avatar arrived to the anchor spot
        /// </summary>
        protected bool mHasArrived = false;

        /// <summary>
        /// The speed at which we try to arrive at the anchor spot
        /// </summary>
        protected float mArrivalLerp = 0.25f;

        /// <summary>
        /// Motion we were in when we grabbed
        /// 0 = idle
        /// 1 = rise
        /// 2 = top
        /// 3 = fall
        /// </summary>
        protected int mGrabMotion = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClimbCrouch()
            : base()
        {
            _Priority = 20;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsNavMeshChangeExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "ClimbCrouch-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public ClimbCrouch(MotionController rController)
            : base(rController)
        {
            _Priority = 20;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsNavMeshChangeExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "ClimbCrouch-SM"; }
#endif
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable) { return false; }

            // Edge info
            bool lEdgeGrabbed = false;

            // If we're on the ground, we can't grab. However, we
            // want to clear our last grab position
            if (mMotionController.IsGrounded)
            {
                if (mGrabPosition.sqrMagnitude > 0f)
                {
                    mGrabPosition = Vector3.zero;
                }
            }
            // If we're in the air, we can test for a grab
            else
            {
                // If we're moving up, we can test for a grab
                if (mActorController.State.Velocity.y > 2f)
                {
                    mGrabMotion = 1;
                    lEdgeGrabbed = TestGrab(mMotionController.transform.position, mMotionController.transform.rotation, mMotionController.transform.forward, mActorController.BaseRadius, 1.20f, 1.35f);
                }
                // If we're at the peak
                else if (mActorController.State.Velocity.y > -2f)
                {
                    mGrabMotion = 2;
                    lEdgeGrabbed = TestGrab(mMotionController.transform.position, mMotionController.transform.rotation, mMotionController.transform.forward, mActorController.BaseRadius, 1.00f, 1.20f);
                }
                // When falling, we just test for a grab
                else
                {
                    mGrabMotion = 3;
                    lEdgeGrabbed = TestGrab(mMotionController.transform.position, mMotionController.transform.rotation, mMotionController.transform.forward, mActorController.BaseRadius, 1.00f, 1.42f);
                }

                // While going down, ensure that the height of the edge
                // passes our minimum grab standard
                if (lEdgeGrabbed)
                {
                    if (mActorController.State.GroundSurfaceDistance < _MinGroundDistance)
                    {
                        lEdgeGrabbed = false;
                    }
                }
            }

            // Ensure we meet the minimum distance from the last grab point
            if (lEdgeGrabbed)
            {
                if (Vector3.Distance(mRaycastHitInfo.point, mGrabPosition) < _MinRegrabDistance)
                {
                    lEdgeGrabbed = false;
                }
            }

            // Return the final result
            return lEdgeGrabbed;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Ensure we have good collision info
            if (mRaycastHitInfo.collider == null) { return false; }

            // Flag the motion as active
            mIsActive = true;
            mIsActivatedFrame = true;
            mIsStartable = false;
            mHasArrived = false;
            mArrivalLerp = 0.25f;

            mMotionController.AccumulatedVelocity = Vector3.zero;

            // Set the state
            mActorController.State.Stance = EnumControllerStance.CLIMB_CROUCH;

            // Set the control state
            MotionState lState = mMotionController.State;

            // Test if we're coming from a jump rise
            if (mGrabMotion == 1)
            {
                mPhase = ClimbCrouch.PHASE_FROM_JUMP_RISE;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_FROM_JUMP_RISE, true);
            }
            // Test if we're coming from the top of the jump
            else if (mGrabMotion == 2)
            {
                mPhase = ClimbCrouch.PHASE_FROM_JUMP_TOP;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_FROM_JUMP_TOP, true);
            }
            // Test if we're coming from a fall
            else if (mGrabMotion == 3)
            {
                mPhase = ClimbCrouch.PHASE_FROM_JUMP_FALL;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_FROM_JUMP_FALL, true);
            }

            mMotionController.State = lState;

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            Vector3 lClimbForward = Quaternion.AngleAxis(180, mActorController._Transform.up) * mRaycastHitInfo.normal;
            mFaceClimbableNormalAngle = mActorController._Transform.forward.HorizontalAngleTo(lClimbForward, mActorController._Transform.up);
            mFaceClimbableNormalAngleUsed = 0f;

            // Disable actor controller processing for a short time
            mActorController.IsGravityEnabled = false;
            mActorController.FixGroundPenetration = false;
            mActorController.SetGround(mClimbable.transform);

            // Set the grab position we're anchoring on
            mGrabPosition = mRaycastHitInfo.point;
            mLocalGrabPosition = Quaternion.Inverse(mClimbable.transform.rotation) * (mRaycastHitInfo.point - mClimbable.transform.position);

            // Set the grab normal coming out of the wall we're anchoring on
            mGrabPositionNormal = mRaycastHitInfo.normal;

            // Determine the target position given the grab info
            mTargetPosition = DetermineTargetPositions(ref mGrabPosition, ref mGrabPositionNormal);

            // Clear the avatar contact position so it can be reset
            mAvatarContactPosition = Vector3.zero;

            // Return
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            mIsActive = false;
            mIsStartable = true;
            mDeactivationTime = Time.time;
            mVelocity = Vector3.zero;
            mAngularVelocity = Vector3.zero;
            mGrabMotion = 0;

            // Track the object we're trying to climb and store it
            mClimbable = null;

            // Clear the stance
            if (mActorController.State.Stance == EnumControllerStance.CLIMB_CROUCH)
            {
                mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
            }

            // Re-enable actor controller processing
            mActorController.IsGravityEnabled = true;
            mActorController.IsCollsionEnabled = true;
            mActorController.FixGroundPenetration = true;
            mActorController.SetGround(null);

            // Get out of the climb motion
            if (mPhase == ClimbCrouch.PHASE_IDLE)
            {
                mPhase = ClimbCrouch.PHASE_UNKNOWN;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_UNKNOWN);
            }

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
            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // Some checks not needed since we baked the root motion
            if (lTransitionID == TRANS_EntryState_JumpRiseToClimbCrouch ||
                lTransitionID == TRANS_AnyState_JumpRiseToClimbCrouch ||
                lStateID == STATE_JumpFallToClimbCrouch ||
                lStateID == STATE_JumpTopToClimbCrouch ||
                lStateID == STATE_ClimbCrouchRecoverIdle
                )
            {
                rVelocityDelta.x = 0f;
                rVelocityDelta.z = 0f;
            }
            // Allow movement from the shimmy
            else if (IsInClimbShimmy)
            {
                rVelocityDelta.z = 0f;
                rVelocityDelta.y = 0f;
            }
            // However, when climbing up there is some forward motion
            else
            {
                rVelocityDelta.x = 0f;
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
            mVelocity = Vector3.zero;
            mMovement = Vector3.zero;
            mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;

            if (Time.deltaTime == 0f) { return; }

            int lStateID = mMotionLayer._AnimatorStateID;
            float lStateTime = mMotionController.State.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo.normalizedTime;

            // Ensure we're facing the wall directly
            if (lStateID == STATE_IdleToClimbCrouch ||
                lStateID == STATE_JumpFallToClimbCrouch ||
                lStateID == STATE_JumpRiseToClimbCrouch ||
                lStateID == STATE_JumpTopToClimbCrouch)
            {
                mRotation = GetReachRotation(0.0f, 0.5f, mFaceClimbableNormalAngle, ref mFaceClimbableNormalAngleUsed);
            }

            // Determine if we've reached the goal
            if (!mHasArrived)
            {
                float lDistance = Vector3.Distance(mMotionController.transform.position, mTargetPosition);
                if (!mHasArrived && lDistance < 0.01f)
                {
                    mHasArrived = true;
                }
            }

            // Orient ourselves towards the anchor point
            if (!mHasArrived)
            {
                // Convert the positions into velocity so they can be processed later
                Vector3 lNewPosition = Vector3.Lerp(mMotionController.transform.position, mTargetPosition, mArrivalLerp);
                mVelocity = (lNewPosition - mMotionController.transform.position) / Time.fixedDeltaTime;

                // We need to ensure our velocity doesn't exceed that of a normal jump
                mVelocity.y = Mathf.Min(mVelocity.y, 5.5f);

                // It's possible that we haven't arrived because we can't shift into the position to the
                // left or the right. This happens if something is blocking us. That said, if we don't require
                // any y movement, we could consider ourselves done
                if (mVelocity.y == 0f && (mVelocity.x != 0f || mVelocity.z != 0f))
                {
                    float lDistance = Vector3.Distance(mMotionController.transform.position, mTargetPosition);
                    if (lDistance < 0.1f)
                    {
                        mTargetPosition = mMotionController.transform.position;
                    }
                    else
                    {
                        mHasArrived = true;
                        mPhase = ClimbCrouch.PHASE_TO_FALL;
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_TO_FALL);
                    }
                }
            }
            else if (lStateID == STATE_JumpFallToClimbCrouch)
            {
                if (mHasArrived)
                {
                    mPhase = ClimbCrouch.PHASE_IDLE;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_IDLE);
                }
            }
            // If we're at the idle, allow the player to climb to the top
            else if (lStateID == STATE_ClimbCrouchPose)
            {
                // Test if we should shimmy left
                //if (mMotionController.UseInput && ootiiInputStub.IsPressed("MoveLeft"))
                if (mMotionController.State.InputFromAvatarAngle < -80f && mMotionController.State.InputFromAvatarAngle > -100f)
                {
                    if (TestShimmy(-_MinSideSpaceForMove))
                    {
                        mPhase = ClimbCrouch.PHASE_SHIMMY_LEFT;
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_SHIMMY_LEFT, true);
                    }
                }
                // Test if we should shimmy right
                //else if (mMotionController.UseInput && ootiiInputStub.IsPressed("MoveRight"))
                else if (mMotionController.State.InputFromAvatarAngle > 80f && mMotionController.State.InputFromAvatarAngle < 100f)
                {
                    if (TestShimmy(_MinSideSpaceForMove))
                    {
                        mPhase = ClimbCrouch.PHASE_SHIMMY_RIGHT;
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_SHIMMY_RIGHT, true);
                    }
                }
                // If the player is in control, test if we go to the top
                //else if (mMotionController.UseInput && (ootiiInputStub.IsJustPressed(mActionAlias) || ootiiInputStub.IsPressed("MoveUp")))
                else if ((mMotionController._InputSource != null && mMotionController._InputSource.IsJustPressed(_ActionAlias)) || 
                         (mMotionController.State.InputMagnitudeTrend.Value > 0.1f && mMotionController.State.InputFromAvatarAngle > -10f && mMotionController.State.InputFromAvatarAngle < 10f))
                {
                    // Start the movement to the top
                    mPhase = ClimbCrouch.PHASE_TO_TOP;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_TO_TOP);
                }
                // If the player is NOT in control, test if we should go to the top
                //else if (!mMotionController.UseInput && mMotionController.State.InputY > 0f)
                else if (mMotionController.State.InputY > 0f)
                {
                    // Start the movement to the top
                    mPhase = ClimbCrouch.PHASE_TO_TOP;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_TO_TOP);
                }
                // Test if we should drop
                //else if (mActorController.State.GroundSurfaceDistance < mMinGroundDistance || (mMotionController.UseInput && ootiiInputStub.IsJustPressed("Release")))
                else if (mActorController.State.GroundSurfaceDistance < _MinGroundDistance || (mMotionController.State.InputFromAvatarAngle < -170f || mMotionController.State.InputFromAvatarAngle > 170f))
                {
                    // Start the drop
                    mPhase = ClimbCrouch.PHASE_TO_FALL;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_TO_FALL);
                }
            }
            // If we're in the middle of a shimmy, ensure we're sticking to the wall
            else if (IsInClimbShimmy)
            {
                // Get the angular distance. We want there to be an initial 180-degree difference
                float lAngle = Vector3Ext.SignedAngle(mGrabPositionNormal, -mActorController._Transform.forward);
                mRotation = Quaternion.AngleAxis(lAngle, Vector3.up);

                // We need to keep a fixed distance from the wall. So we'll do the
                // raycast and adjust our forward velocity to move us into the right position
                float lDistance = Mathf.Abs(_BodyTargetOffset.z);

                mVelocity = Vector3.zero;
                

                //TT if (UnityEngine.Physics.Raycast(mMotionController.transform.position, mMotionController.transform.forward, out sCollisionUpdateInfo, lDistance * mMotionController.CharacterScale * 1.5f, lIsClimbableMask))
                if (RaycastExt.SafeRaycast(mMotionController.transform.position, mMotionController.transform.forward, out sCollisionUpdateInfo, lDistance * 1.5f, _ClimbableLayers, mActorController._Transform))
                {
                    mVelocity = (mMotionController.transform.forward * (sCollisionUpdateInfo.distance - lDistance)) / Time.deltaTime;
                }

                // If we're rotating, we need to adjust the contact position. This way
                // we rotate correctly with the platform
                mAvatarContactPosition = Quaternion.Inverse(mClimbable.transform.rotation) * (mMotionController.transform.position - mClimbable.transform.position);
            }
            // As we're climbing up to the top, we may want to add some extra velocity
            else if (lStateID == STATE_ClimbCrouchToTop)
            {
                mVelocity = _ToTopVelocity;

                if (lStateTime > 0.95f)
                {
                    mActorController.IsCollsionEnabled = true;
                }
                else if (lStateTime > 0.45f)
                {
                    mActorController.IsCollsionEnabled = false;
                }

                // As we climb, fake the contact position
                mAvatarContactPosition = Quaternion.Inverse(mClimbable.transform.rotation) * (mMotionController.transform.position - mClimbable.transform.position);
            }
            // Once we're at the top, we want to make sure there is no popping. So we'll force the
            // avatar to the right height
            else if (lStateID == STATE_ClimbCrouchRecoverIdle)
            {
                mActorController.IsCollsionEnabled = true;

                // As we exit the final animation, move towards the exact position that the
                // following animation (usually idle) will match to.
                Vector3 lTargetPosition = mMotionController.transform.position + (mMotionController.transform.rotation * _ExitPositionOffset);
                lTargetPosition.y = mGrabPosition.y + _ExitPositionOffset.y;

                // Check if we're at the destination
                if (Vector3.Distance(lTargetPosition, mMotionController.transform.position) > 0.01f)
                {
                    mTargetPosition = lTargetPosition;

                    // Convert the positions into velocity so they can be processed later
                    Vector3 lNewPosition = Vector3.Lerp(mMotionController.transform.position, mTargetPosition, 0.25f);
                    mVelocity = (lNewPosition - mMotionController.transform.position) / Time.fixedDeltaTime;
                }

                // Create a new contact position
                if (mAvatarContactPosition.sqrMagnitude == 0f)
                {
                    mAvatarContactPosition = Quaternion.Inverse(mClimbable.transform.rotation) * (mTargetPosition - mClimbable.transform.position);
                    //mAvatarContactPosition = Vector3.zero;
                }
            }
            // Drops us out of a climb so we can fall
            else if (lStateID == STATE_ClimbCrouchToJumpFall)
            {
                // If we're in the second half of the release, we can stop and let the fall take over.
                if (lStateTime > 0.5f && mPhase != ClimbCrouch.PHASE_UNKNOWN)
                {
                    // Stop, but do not clear the GrabPosition. This way we don't
                    // grab the same spot or a spot too close to it.
                    Deactivate();

                    // Reset the motion state so we can move to fall
                    mPhase = ClimbCrouch.PHASE_UNKNOWN;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_UNKNOWN);
                }
            }
            else if (lStateID == STATE_IdlePose)
            {
                Deactivate();

                mPhase = ClimbCrouch.PHASE_UNKNOWN;
            }
        }

        /// <summary>
        /// Shoot rays to determine if a horizontal edge exists that
        /// we may be able to grab onto. It needs to be within the range
        /// of the avatar's feelers.
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestGrab(Vector3 rPosition, Quaternion rRotation, Vector3 rForward, float rRadius, float rBottom, float rTop)
        {
            Vector3 lRayStart = Vector3.zero;

            // Max distance we'll use to ensure we can reach the edge
            float lTargetDistance = -_BodyTargetOffset.z * 1.5f;

            // Shoot forward and ensure above the edge is open
            lRayStart = rPosition + (mActorController._Transform.up * rTop);

            if (RaycastExt.SafeRaycast(lRayStart, rForward, out mRaycastHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Shoot forward and ensure below the edge is blocked
            lRayStart = rPosition + (mActorController._Transform.up * rBottom);

            if (!RaycastExt.SafeRaycast(lRayStart, rForward, out mRaycastHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Now that we know there is an edge, determine it's exact position.
            // First, we sink into the collision point a tad. Then, we use our 
            // collision point and start above it (where the top ray failed). Finally,
            // we shoot a ray down
            lRayStart = mRaycastHitInfo.point + (mActorController._Transform.up * (rTop - rBottom)) + (rForward * 0.01f);

            if (!RaycastExt.SafeRaycast(lRayStart, -mActorController._Transform.up, out mRaycastHitInfo, rTop, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Finally we shoot one last ray forward. We do this because we want the collision
            // data to be about the wall facing the avatar, not the wall facing the
            // last ray (which was shot down).
            Vector3 lLocalHitPoint = mActorController._Transform.InverseTransformPoint(mRaycastHitInfo.point);
            lRayStart = rPosition + (mActorController._Transform.up * (lLocalHitPoint.y - 0.01f));

            if (!RaycastExt.SafeRaycast(lRayStart, rForward, out mRaycastHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Test to make sure there's enough room between the grab point (at waist-ish level) and an object behind the player
            RaycastHit lBackHitInfo;
            if (RaycastExt.SafeRaycast(mRaycastHitInfo.point - mActorController._Transform.up, mRaycastHitInfo.normal, out lBackHitInfo, rRadius * 3f, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // If we have hand positions, ensure that they collide with something as well. Otherwise,
            // the hand will look like it's floating in the air.
            if (_HandGrabOffset > 0)
            {
                RaycastHit lHandHitInfo;

                // Check the right hand
                Vector3 lRightHandPosition = lRayStart + (rRotation * new Vector3(_HandGrabOffset, 0f, 0f));
                if (!RaycastExt.SafeRaycast(lRightHandPosition, rForward, out lHandHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
                {
                    return false;
                }

                // Check the left hand
                Vector3 lLeftHandPosition = lRayStart + (rRotation * new Vector3(-_HandGrabOffset, 0f, 0f));
                if (!RaycastExt.SafeRaycast(lLeftHandPosition, rForward, out lHandHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
                {
                    return false;
                }
            }

            // If we got here, we found an edge
            return true;
        }

        /// <summary>
        /// Performs a test to determine if the character is able to move to the left or right
        /// </summary>
        /// <param name="rOffset">Side distance to check. Left is negative and right is posative.</param>
        /// <returns>Boolean that determerines is the avatar can move</returns>
        private bool TestShimmy(float rOffset)
        {
            float lSideDistance = Mathf.Abs(rOffset);
            Transform lRoot = mMotionController.transform;

            // Shoot a ray to the left to see if there is room to move
            Vector3 lRayStart = lRoot.position;
            Vector3 lRayDirection = lRoot.rotation * new Vector3((rOffset < 0 ? -1 : 1), 0, 0);
            //TT if (UnityEngine.Physics.Raycast(lRayStart, lRayDirection, lSideDistance))
            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, lSideDistance))
            {
                return false;
            }

            // Shoot a ray forward from the future position to see if we can move there
            Vector3 lFuturePosition = lRoot.position + (lRoot.rotation * new Vector3(rOffset, 0, 0));
            if (!TestGrab(lFuturePosition, mMotionController.transform.rotation, mMotionController.transform.forward, mActorController.BaseRadius, 1.0f, 1.5f))
            {
                return false;
            }

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            // Set the grab position we're anchoring on
            mGrabPosition = mRaycastHitInfo.point;
            mLocalGrabPosition = Quaternion.Inverse(mClimbable.transform.rotation) * (mRaycastHitInfo.point - mClimbable.transform.position);

            // Set the grab normal coming out of the wall we're anchoring on
            mGrabPositionNormal = mRaycastHitInfo.normal;

            // Determine the target position given the grab info
            mTargetPosition = DetermineTargetPositions(ref mGrabPosition, ref mGrabPositionNormal);

            // If we got here, we can climb
            return true;
        }

        /// <summary>
        /// Raised when the animator's state has changed
        /// </summary>
        public override void OnAnimatorStateChange(int rLastStateID, int rNewStateID)
        {
            // Ensure we don't re-enter the 'any state'
            if (rNewStateID == STATE_JumpRiseToClimbCrouch || rNewStateID == STATE_JumpTopToClimbCrouch)
            {
                mPhase = ClimbCrouch.PHASE_IDLE;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_IDLE);
            }

            // If we've moved out the recovery state, reset the motion and state
            if (rLastStateID == STATE_ClimbCrouchRecoverIdle)
            {
                // Clear out the climb info
                Deactivate();

                // Clear out the old position
                mGrabPosition = Vector3.zero;

                // Reset the motion state so we can move to fall
                mPhase = ClimbCrouch.PHASE_UNKNOWN;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, ClimbCrouch.PHASE_UNKNOWN);
            }
        }

        /// <summary>
        /// Given the current grab position, calculates the avatar's target
        /// position
        /// </summary>
        /// <returns></returns>
        private Vector3 DetermineTargetPositions(ref Vector3 rGrabPosition, ref Vector3 rGrabNormal)
        {
            // If we're grounded (on the ground, climbing, etc), we may need to apply the velocity of the ground
            if (mActorController.State.Ground != null && (mActorController.State.Ground == mActorController.PrevState.Ground))
            {
                // Test if the support has rotated. Note that we may be a frame behind. Technically this is
                // best done in LateUpdate() after the support has updated, but we don't want to get ahead of the camera.
                if (Quaternion.Angle(mActorController.State.GroundRotation, mActorController.PrevState.GroundRotation) != 0f)
                {
                    // Rotate the avatar
                    Quaternion lDeltaRotation = mActorController.PrevState.GroundRotation.RotationTo(mActorController.State.GroundRotation);
                    rGrabNormal = lDeltaRotation * rGrabNormal;
                }
            }

            // Determine the hand position
            if (_HandGrabOffset > 0f)
            {
                mLeftHandTargetPosition = rGrabPosition - ((Quaternion.AngleAxis(-90, Vector3.up) * rGrabNormal) * _HandGrabOffset);
                mRightHandTargetPosition = rGrabPosition - ((Quaternion.AngleAxis(90, Vector3.up) * rGrabNormal) * _HandGrabOffset);
            }

            // Determine the new position
            Vector3 lTargetPosition = rGrabPosition;
            lTargetPosition = lTargetPosition + (mActorController._Transform.rotation * _BodyTargetOffset);

            return lTargetPosition;
        }

        /// <summary>
        /// Test to see if we're currently in the process
        /// of trying to grab onto something
        /// </summary>
        protected bool IsInClimbUpState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lTransitionID == TRANS_EntryState_JumpRiseToClimbCrouch ||
                    lTransitionID == TRANS_AnyState_JumpRiseToClimbCrouch ||
                    lStateID == STATE_IdleToClimbCrouch ||
                    lStateID == STATE_JumpRiseToClimbCrouch ||
                    lStateID == STATE_JumpTopToClimbCrouch ||
                    lStateID == STATE_JumpFallToClimbCrouch
                    )
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Test to see if we're currently in the idle state for the crouch
        /// </summary>
        protected bool IsInClimbIdleState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;

                if (lStateID == STATE_ClimbCrouchPose)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Test to see if we're currently in the climb state
        /// </summary>
        protected bool IsInClimbState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lTransitionID == TRANS_EntryState_JumpRiseToClimbCrouch ||
                    lTransitionID == TRANS_EntryState_IdleToClimbCrouch ||
                    lTransitionID == TRANS_EntryState_JumpTopToClimbCrouch ||
                    lTransitionID == TRANS_EntryState_JumpFallToClimbCrouch ||
                    lTransitionID == TRANS_AnyState_JumpRiseToClimbCrouch ||
                    lTransitionID == TRANS_AnyState_IdleToClimbCrouch ||
                    lTransitionID == TRANS_AnyState_JumpTopToClimbCrouch ||
                    lTransitionID == TRANS_AnyState_JumpFallToClimbCrouch ||
                    lStateID == STATE_IdleToClimbCrouch ||
                    lStateID == STATE_JumpRiseToClimbCrouch ||
                    lStateID == STATE_JumpTopToClimbCrouch ||
                    lStateID == STATE_JumpFallToClimbCrouch ||
                    lStateID == STATE_ClimbCrouchPose ||
                    lStateID == STATE_ClimbCrouchToTop ||
                    lStateID == STATE_ClimbCrouchRecoverIdle ||
                    lStateID == STATE_ClimbCrouchToJumpFall ||

                    lStateID == STATE_ClimbCrouchShimmyLeft ||
                    lStateID == STATE_ClimbCrouchShimmyRight ||
                    lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft ||
                    lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight ||
                    lTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose ||
                    lTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose
                    )
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determine if we're shimmy-ing left or right
        /// </summary>
        protected bool IsInClimbShimmy
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lStateID == STATE_ClimbCrouchShimmyLeft ||
                    lStateID == STATE_ClimbCrouchShimmyRight ||
                    lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft ||
                    lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight ||
                    lTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose ||
                    lTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose                    
                    )
                {
                    return true;
                }

                return false;
            }
        }


        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Allow the constraint to render it's own GUI
        /// </summary>
        /// <returns>Reports if the object's value was changed</returns>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers a climb."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            float lNewMinGroundDistance = EditorGUILayout.FloatField(new GUIContent("Min Distance", "Minimum ground distance inwhich the climb is valid."), MinGroundDistance, GUILayout.MinWidth(30));
            if (lNewMinGroundDistance != MinGroundDistance)
            {
                lIsDirty = true;
                MinGroundDistance = lNewMinGroundDistance;
            }

            Vector3 lNewBodyTargetOffset = EditorGUILayout.Vector3Field(new GUIContent("Target Offset", "When a ledge is grabbed, this defines the avatar position from the grab point. Change these values to ensure the avatar's hands fit the ledge."), BodyTargetOffset, GUILayout.MinWidth(30));
            if (lNewBodyTargetOffset != BodyTargetOffset)
            {
                lIsDirty = true;
                BodyTargetOffset = lNewBodyTargetOffset;
            }

            float lNewMinRegrabDistance = EditorGUILayout.FloatField(new GUIContent("Min Regrab Distance", "Minimum distance we can fall and try to regrab an edge."), MinRegrabDistance, GUILayout.MinWidth(30));
            if (lNewMinRegrabDistance != MinRegrabDistance)
            {
                lIsDirty = true;
                MinRegrabDistance = lNewMinRegrabDistance;
            }

            float lNewHandGrabOffset = EditorGUILayout.FloatField(new GUIContent("Hand Offset", "Position offset from the avatar's middle ledge grab where the left and right hands will be positioned."), HandGrabOffset, GUILayout.MinWidth(30));
            if (lNewHandGrabOffset != HandGrabOffset)
            {
                lIsDirty = true;
                HandGrabOffset = lNewHandGrabOffset;
            }

            float lNewMinSideSpaceForMove = EditorGUILayout.FloatField(new GUIContent("Min Side Space", "Minimum space that needs to exist to the side so the character can shimmy."), MinSideSpaceForMove, GUILayout.MinWidth(30));
            if (lNewMinSideSpaceForMove != MinSideSpaceForMove)
            {
                lIsDirty = true;
                MinSideSpaceForMove = lNewMinSideSpaceForMove;
            }

            Vector3 lNewExitPositionOffset = EditorGUILayout.Vector3Field(new GUIContent("Exit Offset", "When the avatar moves to the top of the ledge, an offset used to ensure the avatar lines up with the idle pose that follows."), ExitPositionOffset, GUILayout.MinWidth(30));
            if (lNewExitPositionOffset != ExitPositionOffset)
            {
                lIsDirty = true;
                ExitPositionOffset = lNewExitPositionOffset;
            }

            Vector3 lNewToTopVelocity = EditorGUILayout.Vector3Field(new GUIContent("To Top Veloctiy", "Additional velocity that can be added to help move the avatar to the top."), ToTopVelocity, GUILayout.MinWidth(30));
            if (lNewToTopVelocity != ToTopVelocity)
            {
                lIsDirty = true;
                ToTopVelocity = lNewToTopVelocity;
            }

            // Balance layer
            int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("Climbing Layers", "Layers that identies objects that can be climbed."), ClimbableLayers);
            if (lNewClimbableLayers != ClimbableLayers)
            {
                lIsDirty = true;
                ClimbableLayers = lNewClimbableLayers;
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
        public static int TRANS_EntryState_IdleToClimbCrouch = -1;
        public static int TRANS_AnyState_IdleToClimbCrouch = -1;
        public static int TRANS_EntryState_JumpRiseToClimbCrouch = -1;
        public static int TRANS_AnyState_JumpRiseToClimbCrouch = -1;
        public static int TRANS_EntryState_JumpTopToClimbCrouch = -1;
        public static int TRANS_AnyState_JumpTopToClimbCrouch = -1;
        public static int TRANS_EntryState_JumpFallToClimbCrouch = -1;
        public static int TRANS_AnyState_JumpFallToClimbCrouch = -1;
        public static int STATE_ClimbCrouchPose = -1;
        public static int TRANS_ClimbCrouchPose_ClimbCrouchToTop = -1;
        public static int TRANS_ClimbCrouchPose_ClimbCrouchToJumpFall = -1;
        public static int TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight = -1;
        public static int TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft = -1;
        public static int STATE_IdleToClimbCrouch = -1;
        public static int TRANS_IdleToClimbCrouch_ClimbCrouchToTop = -1;
        public static int TRANS_IdleToClimbCrouch_ClimbCrouchPose = -1;
        public static int STATE_ClimbCrouchToTop = -1;
        public static int TRANS_ClimbCrouchToTop_ClimbCrouchRecoverIdle = -1;
        public static int STATE_JumpRiseToClimbCrouch = -1;
        public static int TRANS_JumpRiseToClimbCrouch_ClimbCrouchPose = -1;
        public static int TRANS_JumpRiseToClimbCrouch_ClimbCrouchToTop = -1;
        public static int STATE_JumpFallToClimbCrouch = -1;
        public static int TRANS_JumpFallToClimbCrouch_ClimbCrouchPose = -1;
        public static int TRANS_JumpFallToClimbCrouch_ClimbCrouchToTop = -1;
        public static int STATE_ClimbCrouchToJumpFall = -1;
        public static int STATE_JumpTopToClimbCrouch = -1;
        public static int TRANS_JumpTopToClimbCrouch_ClimbCrouchPose = -1;
        public static int TRANS_JumpTopToClimbCrouch_ClimbCrouchToTop = -1;
        public static int STATE_ClimbCrouchRecoverIdle = -1;
        public static int TRANS_ClimbCrouchRecoverIdle_IdlePose = -1;
        public static int STATE_ClimbCrouchShimmyRight = -1;
        public static int TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose = -1;
        public static int TRANS_ClimbCrouchShimmyRight_ClimbCrouchToJumpFall = -1;
        public static int STATE_ClimbCrouchShimmyLeft = -1;
        public static int TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose = -1;
        public static int TRANS_ClimbCrouchShimmyLeft_ClimbCrouchToJumpFall = -1;
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

                if (lStateID == STATE_ClimbCrouchPose) { return true; }
                if (lStateID == STATE_IdleToClimbCrouch) { return true; }
                if (lStateID == STATE_ClimbCrouchToTop) { return true; }
                if (lStateID == STATE_JumpRiseToClimbCrouch) { return true; }
                if (lStateID == STATE_JumpFallToClimbCrouch) { return true; }
                if (lStateID == STATE_ClimbCrouchToJumpFall) { return true; }
                if (lStateID == STATE_JumpTopToClimbCrouch) { return true; }
                if (lStateID == STATE_ClimbCrouchRecoverIdle) { return true; }
                if (lStateID == STATE_ClimbCrouchShimmyRight) { return true; }
                if (lStateID == STATE_ClimbCrouchShimmyLeft) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpRiseToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpRiseToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpTopToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpTopToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpFallToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpFallToClimbCrouch) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchToTop) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchToJumpFall) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft) { return true; }
                if (lTransitionID == TRANS_IdleToClimbCrouch_ClimbCrouchToTop) { return true; }
                if (lTransitionID == TRANS_IdleToClimbCrouch_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchToTop_ClimbCrouchRecoverIdle) { return true; }
                if (lTransitionID == TRANS_JumpRiseToClimbCrouch_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_JumpRiseToClimbCrouch_ClimbCrouchToTop) { return true; }
                if (lTransitionID == TRANS_JumpFallToClimbCrouch_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_JumpFallToClimbCrouch_ClimbCrouchToTop) { return true; }
                if (lTransitionID == TRANS_JumpTopToClimbCrouch_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_JumpTopToClimbCrouch_ClimbCrouchToTop) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchRecoverIdle_IdlePose) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchToJumpFall) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose) { return true; }
                if (lTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchToJumpFall) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_ClimbCrouchPose) { return true; }
            if (rStateID == STATE_IdleToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchToTop) { return true; }
            if (rStateID == STATE_JumpRiseToClimbCrouch) { return true; }
            if (rStateID == STATE_JumpFallToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchToJumpFall) { return true; }
            if (rStateID == STATE_JumpTopToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchRecoverIdle) { return true; }
            if (rStateID == STATE_ClimbCrouchShimmyRight) { return true; }
            if (rStateID == STATE_ClimbCrouchShimmyLeft) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_ClimbCrouchPose) { return true; }
            if (rStateID == STATE_IdleToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchToTop) { return true; }
            if (rStateID == STATE_JumpRiseToClimbCrouch) { return true; }
            if (rStateID == STATE_JumpFallToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchToJumpFall) { return true; }
            if (rStateID == STATE_JumpTopToClimbCrouch) { return true; }
            if (rStateID == STATE_ClimbCrouchRecoverIdle) { return true; }
            if (rStateID == STATE_ClimbCrouchShimmyRight) { return true; }
            if (rStateID == STATE_ClimbCrouchShimmyLeft) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpRiseToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpRiseToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpTopToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpTopToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpFallToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpFallToClimbCrouch) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchToTop) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchToJumpFall) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft) { return true; }
            if (rTransitionID == TRANS_IdleToClimbCrouch_ClimbCrouchToTop) { return true; }
            if (rTransitionID == TRANS_IdleToClimbCrouch_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchToTop_ClimbCrouchRecoverIdle) { return true; }
            if (rTransitionID == TRANS_JumpRiseToClimbCrouch_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_JumpRiseToClimbCrouch_ClimbCrouchToTop) { return true; }
            if (rTransitionID == TRANS_JumpFallToClimbCrouch_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_JumpFallToClimbCrouch_ClimbCrouchToTop) { return true; }
            if (rTransitionID == TRANS_JumpTopToClimbCrouch_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_JumpTopToClimbCrouch_ClimbCrouchToTop) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchRecoverIdle_IdlePose) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchShimmyRight_ClimbCrouchToJumpFall) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose) { return true; }
            if (rTransitionID == TRANS_ClimbCrouchShimmyLeft_ClimbCrouchToJumpFall) { return true; }
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
            TRANS_EntryState_IdleToClimbCrouch = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbCrouch-SM.IdleToClimbCrouch");
            TRANS_AnyState_IdleToClimbCrouch = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbCrouch-SM.IdleToClimbCrouch");
            TRANS_EntryState_JumpRiseToClimbCrouch = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbCrouch-SM.JumpRiseToClimbCrouch");
            TRANS_AnyState_JumpRiseToClimbCrouch = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbCrouch-SM.JumpRiseToClimbCrouch");
            TRANS_EntryState_JumpTopToClimbCrouch = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbCrouch-SM.JumpTopToClimbCrouch");
            TRANS_AnyState_JumpTopToClimbCrouch = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbCrouch-SM.JumpTopToClimbCrouch");
            TRANS_EntryState_JumpFallToClimbCrouch = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbCrouch-SM.JumpFallToClimbCrouch");
            TRANS_AnyState_JumpFallToClimbCrouch = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbCrouch-SM.JumpFallToClimbCrouch");
            STATE_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_ClimbCrouchPose_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchPose -> Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            TRANS_ClimbCrouchPose_ClimbCrouchToJumpFall = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchPose -> Base Layer.ClimbCrouch-SM.ClimbCrouchToJumpFall");
            TRANS_ClimbCrouchPose_ClimbCrouchShimmyRight = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchPose -> Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyRight");
            TRANS_ClimbCrouchPose_ClimbCrouchShimmyLeft = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchPose -> Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyLeft");
            STATE_IdleToClimbCrouch = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.IdleToClimbCrouch");
            TRANS_IdleToClimbCrouch_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.IdleToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            TRANS_IdleToClimbCrouch_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.IdleToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            STATE_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            TRANS_ClimbCrouchToTop_ClimbCrouchRecoverIdle = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchToTop -> Base Layer.ClimbCrouch-SM.ClimbCrouchRecoverIdle");
            STATE_JumpRiseToClimbCrouch = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpRiseToClimbCrouch");
            TRANS_JumpRiseToClimbCrouch_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpRiseToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_JumpRiseToClimbCrouch_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpRiseToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            STATE_JumpFallToClimbCrouch = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpFallToClimbCrouch");
            TRANS_JumpFallToClimbCrouch_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpFallToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_JumpFallToClimbCrouch_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpFallToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            STATE_ClimbCrouchToJumpFall = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchToJumpFall");
            STATE_JumpTopToClimbCrouch = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpTopToClimbCrouch");
            TRANS_JumpTopToClimbCrouch_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpTopToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_JumpTopToClimbCrouch_ClimbCrouchToTop = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.JumpTopToClimbCrouch -> Base Layer.ClimbCrouch-SM.ClimbCrouchToTop");
            STATE_ClimbCrouchRecoverIdle = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchRecoverIdle");
            TRANS_ClimbCrouchRecoverIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchRecoverIdle -> Base Layer.ClimbCrouch-SM.IdlePose");
            STATE_ClimbCrouchShimmyRight = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyRight");
            TRANS_ClimbCrouchShimmyRight_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyRight -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_ClimbCrouchShimmyRight_ClimbCrouchToJumpFall = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyRight -> Base Layer.ClimbCrouch-SM.ClimbCrouchToJumpFall");
            STATE_ClimbCrouchShimmyLeft = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyLeft");
            TRANS_ClimbCrouchShimmyLeft_ClimbCrouchPose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyLeft -> Base Layer.ClimbCrouch-SM.ClimbCrouchPose");
            TRANS_ClimbCrouchShimmyLeft_ClimbCrouchToJumpFall = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.ClimbCrouchShimmyLeft -> Base Layer.ClimbCrouch-SM.ClimbCrouchToJumpFall");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.ClimbCrouch-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip mClimbCrouchPose = null;
        private AnimationClip mIdleToClimbCrouch = null;
        private AnimationClip mClimbCrouchToIdle = null;
        private AnimationClip mJumpRiseToClimbCrouch = null;
        private AnimationClip mJumpFallToClimbCrouch = null;
        private AnimationClip mClimbCrouchToJumpFall = null;
        private AnimationClip mJumpTopToClimbCrouch = null;
        private AnimationClip mClimbCrouchRecoverIdle = null;
        private AnimationClip mShimmyRight = null;
        private AnimationClip mShimmyLeft = null;
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

            UnityEditor.Animations.AnimatorState lClimbCrouchPose = lMotionStateMachine.AddState("ClimbCrouchPose", new Vector3(468, 96, 0));
            lClimbCrouchPose.motion = mClimbCrouchPose;
            lClimbCrouchPose.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToClimbCrouch = lMotionStateMachine.AddState("IdleToClimbCrouch", new Vector3(108, -108, 0));
            lIdleToClimbCrouch.motion = mIdleToClimbCrouch;
            lIdleToClimbCrouch.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbCrouchToTop = lMotionStateMachine.AddState("ClimbCrouchToTop", new Vector3(468, -72, 0));
            lClimbCrouchToTop.motion = mClimbCrouchToIdle;
            lClimbCrouchToTop.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpRiseToClimbCrouch = lMotionStateMachine.AddState("JumpRiseToClimbCrouch", new Vector3(108, -36, 0));
            lJumpRiseToClimbCrouch.motion = mJumpRiseToClimbCrouch;
            lJumpRiseToClimbCrouch.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpFallToClimbCrouch = lMotionStateMachine.AddState("JumpFallToClimbCrouch", new Vector3(108, 108, 0));
            lJumpFallToClimbCrouch.motion = mJumpFallToClimbCrouch;
            lJumpFallToClimbCrouch.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbCrouchToJumpFall = lMotionStateMachine.AddState("ClimbCrouchToJumpFall", new Vector3(468, 240, 0));
            lClimbCrouchToJumpFall.motion = mClimbCrouchToJumpFall;
            lClimbCrouchToJumpFall.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpTopToClimbCrouch = lMotionStateMachine.AddState("JumpTopToClimbCrouch", new Vector3(108, 36, 0));
            lJumpTopToClimbCrouch.motion = mJumpTopToClimbCrouch;
            lJumpTopToClimbCrouch.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbCrouchRecoverIdle = lMotionStateMachine.AddState("ClimbCrouchRecoverIdle", new Vector3(468, -156, 0));
            lClimbCrouchRecoverIdle.motion = mClimbCrouchRecoverIdle;
            lClimbCrouchRecoverIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbCrouchShimmyRight = lMotionStateMachine.AddState("ClimbCrouchShimmyRight", new Vector3(684, 312, 0));
            lClimbCrouchShimmyRight.motion = mShimmyRight;
            lClimbCrouchShimmyRight.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbCrouchShimmyLeft = lMotionStateMachine.AddState("ClimbCrouchShimmyLeft", new Vector3(252, 312, 0));
            lClimbCrouchShimmyLeft.motion = mShimmyLeft;
            lClimbCrouchShimmyLeft.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(720, -156, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToClimbCrouch);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 300f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpRiseToClimbCrouch);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 301f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpTopToClimbCrouch);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 302f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpFallToClimbCrouch);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 303f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lClimbCrouchPose.AddTransition(lClimbCrouchToTop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 350f, "L0MotionPhase");

            lStateTransition = lClimbCrouchPose.AddTransition(lClimbCrouchToJumpFall);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 370f, "L0MotionPhase");

            lStateTransition = lClimbCrouchPose.AddTransition(lClimbCrouchShimmyRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 1.5f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 385f, "L0MotionPhase");

            lStateTransition = lClimbCrouchPose.AddTransition(lClimbCrouchShimmyLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 1.5f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 380f, "L0MotionPhase");

            lStateTransition = lIdleToClimbCrouch.AddTransition(lClimbCrouchToTop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 350f, "L0MotionPhase");

            lStateTransition = lIdleToClimbCrouch.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 320f, "L0MotionPhase");

            lStateTransition = lClimbCrouchToTop.AddTransition(lClimbCrouchRecoverIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9253032f;
            lStateTransition.duration = 0.07885714f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lJumpRiseToClimbCrouch.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7965769f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 320f, "L0MotionPhase");

            lStateTransition = lJumpRiseToClimbCrouch.AddTransition(lClimbCrouchToTop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 350f, "L0MotionPhase");

            lStateTransition = lJumpFallToClimbCrouch.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8410774f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 320f, "L0MotionPhase");

            lStateTransition = lJumpFallToClimbCrouch.AddTransition(lClimbCrouchToTop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 350f, "L0MotionPhase");

            lStateTransition = lJumpTopToClimbCrouch.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8276094f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 320f, "L0MotionPhase");

            lStateTransition = lJumpTopToClimbCrouch.AddTransition(lClimbCrouchToTop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 350f, "L0MotionPhase");

            lStateTransition = lClimbCrouchRecoverIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3750001f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lClimbCrouchShimmyRight.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9501459f;
            lStateTransition.duration = 0.0498541f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lClimbCrouchShimmyRight.AddTransition(lClimbCrouchToJumpFall);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 370f, "L0MotionPhase");

            lStateTransition = lClimbCrouchShimmyLeft.AddTransition(lClimbCrouchPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.949408f;
            lStateTransition.duration = 0.1478843f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lClimbCrouchShimmyLeft.AddTransition(lClimbCrouchToJumpFall);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 370f, "L0MotionPhase");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mClimbCrouchPose = CreateAnimationField("ClimbCrouchPose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/ClimbCrouchPose.anim", "ClimbCrouchPose", mClimbCrouchPose);
            mIdleToClimbCrouch = CreateAnimationField("IdleToClimbCrouch", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/IdleToClimbCrouch.anim", "IdleToClimbCrouch", mIdleToClimbCrouch);
            mClimbCrouchToIdle = CreateAnimationField("ClimbCrouchToTop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/ClimbCrouchToIdle.anim", "ClimbCrouchToIdle", mClimbCrouchToIdle);
            mJumpRiseToClimbCrouch = CreateAnimationField("JumpRiseToClimbCrouch", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/JumpRiseToClimbCrouch.anim", "JumpRiseToClimbCrouch", mJumpRiseToClimbCrouch);
            mJumpFallToClimbCrouch = CreateAnimationField("JumpFallToClimbCrouch", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/JumpFallToClimbCrouch.anim", "JumpFallToClimbCrouch", mJumpFallToClimbCrouch);
            mClimbCrouchToJumpFall = CreateAnimationField("ClimbCrouchToJumpFall", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/ClimbCrouchToJumpFall.anim", "ClimbCrouchToJumpFall", mClimbCrouchToJumpFall);
            mJumpTopToClimbCrouch = CreateAnimationField("JumpTopToClimbCrouch", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/JumpTopToClimbCrouch.anim", "JumpTopToClimbCrouch", mJumpTopToClimbCrouch);
            mClimbCrouchRecoverIdle = CreateAnimationField("ClimbCrouchRecoverIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/ClimbCrouchRecoverIdle.anim", "ClimbCrouchRecoverIdle", mClimbCrouchRecoverIdle);
            mShimmyRight = CreateAnimationField("ClimbCrouchShimmyRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouchShimmy.fbx/ShimmyRight.anim", "ShimmyRight", mShimmyRight);
            mShimmyLeft = CreateAnimationField("ClimbCrouchShimmyLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouchShimmy.fbx/ShimmyLeft.anim", "ShimmyLeft", mShimmyLeft);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
