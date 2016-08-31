using System.Collections.Generic;
using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Handles the basic motion for going up and down ladders
    /// </summary>
    [MotionName("Climb Ladder")]
    [MotionDescription("Allows for climbing up and down ladders. Looks good for a basic wall climb too. The default spacing for rungs is 0.32.")]
    public class ClimbLadder : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1500;
        public const int PHASE_START_JUMP = 1504;
        public const int PHASE_START_TOP = 1505;
        public const int PHASE_START_TOP_TURN = 1506;
        public const int PHASE_EXIT_TOP = 1520;
        public const int PHASE_EXIT_BOTTOM = 1510;

        /// <summary>
        /// Min horizontal distance the actor can be from the ladder in order to climb
        /// </summary>
        public float _MinDistance = 0.2f;
        public float MinDistance
        {
            get { return _MinDistance; }
            set { _MinDistance = value; }
        }

        /// <summary>
        /// Max horizontal distance the actor can be from the ladder in order to climb
        /// </summary>
        public float _MaxDistance = 0.75f;
        public float MaxDistance
        {
            get { return _MaxDistance; }
            set { _MaxDistance = value; }
        }

        /// <summary>
        /// Vertical distance from the character's root we'll use to test for the ladder's top.
        /// </summary>
        public float _TopExitTestHeight = 1.6f;
        public float TopExitTestHeight
        {
            get { return _TopExitTestHeight; }
            set { _TopExitTestHeight = value; }
        }

        /// <summary>
        /// Height from the root that we'll test to see if we're far enough on the surface to keep climbing.
        /// </summary>
        public float _DetachTestHeight = 1.0f;
        public float DetachTestHeight
        {
            get { return _DetachTestHeight; }
            set { _DetachTestHeight = value; }
        }

        /// <summary>
        /// Ensure when we're up high enough to catch. Otherwise, it's odd 
        /// to latch onto something when the avatar jumps 0.1m.
        /// </summary>
        public float _MinGroundDistance = 1.3f;
        public float MinGroundDistance
        {
            get { return _MinGroundDistance; }
            set { _MinGroundDistance = value; }
        }

        /// <summary>
        /// Minimum distance the new grab point must be from the last
        /// grab point in order for the grab to work.
        /// </summary>
        public float _MinRegrabDistance = 2.0f;
        public float MinRegrabDistance
        {
            get { return _MinRegrabDistance; }
            set { _MinRegrabDistance = value; }
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
        /// Reach offset value for the animation
        /// </summary>
        public float _ReachOffset1 = 0.35f;
        public float ReachOffset1
        {
            get { return _ReachOffset1; }
            set { _ReachOffset1 = value; }
        }

        public float _ReachOffset2 = -0.80f;
        public float ReachOffset2
        {
            get { return _ReachOffset2; }
            set { _ReachOffset2 = value; }
        }

        public float _ReachOffset3 = 0.25f;
        public float ReachOffset3
        {
            get { return _ReachOffset3; }
            set { _ReachOffset3 = value; }
        }

        public float _ReachOffset4 = -1.55f;
        public float ReachOffset4
        {
            get { return _ReachOffset4; }
            set { _ReachOffset4 = value; }
        }

        public float _ReachOffset5 = 0.30f;
        public float ReachOffset5
        {
            get { return _ReachOffset5; }
            set { _ReachOffset5 = value; }
        }

        public float _ReachOffset6 = 0.02f;
        public float ReachOffset6
        {
            get { return _ReachOffset6; }
            set { _ReachOffset6 = value; }
        }

        public float _ReachOffset7 = 0.30f;
        public float ReachOffset7
        {
            get { return _ReachOffset7; }
            set { _ReachOffset7 = value; }
        }
        
        /// <summary>
        /// Used to determine how we'll start the ladder climb
        /// </summary>
        protected int mStartState = PHASE_START;

        /// <summary>
        /// Tracks the object that is being climbed
        /// </summary>
        protected GameObject mClimbable = null;

        /// <summary>
        /// Store the climbable normal in case we need it later
        /// </summary>
        protected Vector3 mLocalClimbableNormal = Vector3.zero;

        /// <summary>
        /// Rotation it takes to get to facing the climbable's normal
        /// </summary>
        protected float mFaceClimbableNormalAngle = 0f;

        /// <summary>
        /// Amount of rotation that is already used
        /// </summary>
        protected float mFaceClimbableNormalAngleUsed = 0f;

        /// <summary>
        /// Track that last hit point for re-grabbing purposes
        /// </summary>
        protected Vector3 mLastGrabPoint = Vector3.zero;

        /// <summary>
        /// Used to determine if we've triggered the desired state. This is
        /// meant to be generic.
        /// </summary>
        protected bool mIsExitTriggered = false;

        /// <summary>
        /// Keeps us from having to reallocate over and over
        /// </summary>
        protected RaycastHit mRaycastHitInfo = RaycastExt.EmptyHitInfo;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClimbLadder()
            : base()
        {
            _Priority = 40;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "ClimbLadder-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public ClimbLadder(MotionController rController)
            : base(rController)
        {
            _Priority = 40;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsGroundedExpected = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "ClimbLadder-SM"; }
#endif
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable)
            {
                return false;
            }

            // If we're grounded, test for trigger. Then, we can test for the climb
            if (mMotionController.IsGrounded)
            {
                // Test if the trigger occured
                if (_ActionAlias.Length == 0 || (mMotionController._InputSource != null && mMotionController._InputSource.IsJustPressed(_ActionAlias)))
                {
                    // When we activate a test, we can clear out the grab point
                    mLastGrabPoint = Vector3.zero;

                    if (TestForClimbUp())
                    {
                        mStartState = PHASE_START;
                        return true;
                    }

                    // Test if there's a ladder for us to go down (behind us)
                    if (TestForClimbDownBack())
                    {
                        mStartState = PHASE_START_TOP;
                        return true;
                    }

                    // Test if there's a ladder for us to go down (in front of us)
                    if (TestForClimbDownForward())
                    {
                        mStartState = PHASE_START_TOP_TURN;
                        return true;
                    }
                }
            }
            // If we're in the air, we can test for a climb
            else
            {
                if (TestForClimbInAir())
                {
                    mStartState = PHASE_START_JUMP;
                    return true;
                }
            }
            // Return the final result
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
        {
            // Once we're at the top, we want to make sure there is no popping. So we'll force the
            // avatar to the right height
            if (mIsAnimatorActive && !IsInMotionState)
            {
                // Tell this motion to get out
                return false;
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
            // Ensure we have good collision info
            if (mRaycastHitInfo.collider == null) { return false; }

            // Reset the triggger
            mIsExitTriggered = false;

            // Set the state
            mActorController.State.Stance = EnumControllerStance.CLIMB_LADDER;

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            mLocalClimbableNormal = mClimbable.transform.InverseTransformDirection(mRaycastHitInfo.normal);

            Vector3 lClimbForward = Quaternion.AngleAxis(180, mActorController._Transform.up) * mRaycastHitInfo.normal;
            mFaceClimbableNormalAngle = mActorController._Transform.forward.HorizontalAngleTo(lClimbForward, mActorController._Transform.up);
            mFaceClimbableNormalAngleUsed = 0f;

            // Setup the reach data and clear any current values
            ClearReachData();
            if (mStartState == PHASE_START)
            {
                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_LadderBottomOn;
                lReachData.StartTime = 0.0f;
                lReachData.EndTime = 0.4f;
                lReachData.Power = 3;
                lReachData.ReachTarget = mRaycastHitInfo.point - (mActorController._Transform.up * mActorController._MaxStepHeight) + (mActorController._Transform.up * 0.0f) + (mRaycastHitInfo.normal * _ReachOffset1);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);
            }
            // Top with wall behind us
            else if (mStartState == PHASE_START_TOP)
            {
                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_LadderTopOn;
                lReachData.StartTime = 0.5f;
                lReachData.EndTime = 0.7f;
                lReachData.Power = 4;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mActorController._Transform.up * _ReachOffset2) + (mRaycastHitInfo.normal * _ReachOffset3);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_LadderTopOn;
                lReachData.StartTime = 0.75f;
                lReachData.EndTime = 0.95f;
                lReachData.Power = 3;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mActorController._Transform.up * _ReachOffset4) + (mRaycastHitInfo.normal * _ReachOffset5);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);
            }
            else if (mStartState == PHASE_START_TOP_TURN)
            {
                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_LadderTopOn;
                lReachData.StartTime = 0.5f;
                lReachData.EndTime = 0.7f;
                lReachData.Power = 4;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mActorController._Transform.up * _ReachOffset2) + (mRaycastHitInfo.normal * _ReachOffset3);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_LadderTopOn;
                lReachData.StartTime = 0.75f;
                lReachData.EndTime = 0.95f;
                lReachData.Power = 3;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mActorController._Transform.up * _ReachOffset4) + (mRaycastHitInfo.normal * _ReachOffset5);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);
            }
            else if (mStartState == PHASE_START_JUMP)
            {
                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.TransitionID = TRANS_EntryState_JumpToClimb;
                lReachData.StartTime = 0.0f;
                lReachData.EndTime = 0.95f;
                lReachData.Power = 2;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mRaycastHitInfo.normal * 0.55f) + (mActorController._Transform.up * -0.7f);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_JumpToClimb;
                lReachData.StartTime = 0.0f;
                lReachData.EndTime = 0.4f;
                lReachData.Power = 2;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mRaycastHitInfo.normal * 0.55f) + (mActorController._Transform.up * -0.65f);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_JumpToClimb;
                lReachData.StartTime = 0.5f;
                lReachData.EndTime = 0.7f;
                lReachData.Power = 4;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mRaycastHitInfo.normal * 0.55f) + (mActorController._Transform.up * -0.65f);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_JumpToClimb;
                lReachData.StartTime = 0.8f;
                lReachData.EndTime = 1.2f;
                lReachData.Power = 2;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * mActorController._MaxStepHeight) + (mRaycastHitInfo.normal * 0.35f) + (mActorController._Transform.up * -0.65f);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                //EditorApplication.isPaused = true;
            }

            // Disable actor controller processing for a short time
            mActorController.IsGravityEnabled = false;
            mActorController.FixGroundPenetration = false;
            mActorController.SetGround(mClimbable.transform);

            // Start the animations
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, mStartState, true);

            // Return
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            mClimbable = null;

            // Clear the stance
            if (mActorController.State.Stance == EnumControllerStance.CLIMB_LADDER)
            {
                mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
            }

            // Re-enable actor controller processing
            mActorController.IsGravityEnabled = true;
            mActorController.IsCollsionEnabled = true;
            mActorController.FixGroundPenetration = true;
            mActorController.SetGround(null);

            // Finish the deactivation process
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
            if (lStateID != STATE_IdleTurn180L)
            {
                rRotationDelta = Quaternion.identity;
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

            if (mClimbable == null) { return; }

            int lStateID = mMotionLayer._AnimatorStateID;
            float lStateTime = mMotionLayer._AnimatorStateNormalizedTime;

            // Reach data moves us the the specified position so we can line our animations up nicely.
            mMovement = GetReachMovement();
            
            // Check if we're jumping off
            if (lStateID == STATE_LadderPose ||
                lStateID == STATE_LadderUp ||
                lStateID == STATE_LadderDown)
            {
                if (_ActionAlias.Length > 0 && mMotionController._InputSource.IsJustPressed(_ActionAlias))
                {
                    mLastGrabPoint = mActorController._Transform.position;

                    Deactivate();

                    return;
                }
            }

            // Regrab our facing angle once the turn is done
            if (lStateID == STATE_IdleTurn180L)
            {
                if (lStateTime > 0.7f)
                {
                    Vector3 lClimbNormal = mClimbable.transform.TransformDirection(mLocalClimbableNormal);

                    Vector3 lClimbForward = Quaternion.AngleAxis(180, mActorController._Transform.up) * lClimbNormal;
                    mFaceClimbableNormalAngle = mActorController._Transform.forward.HorizontalAngleTo(lClimbForward, mActorController._Transform.up);
                    mFaceClimbableNormalAngleUsed = 0f;
                }
            }
            // Once we grab onto the ledge, we want to make sure we are facing the 'face of the wall'. So,
            // we may need to rotate
            else if (lStateID == STATE_LadderTopOn)
            {
                mRotation = GetReachRotation(0.6f, 0.9f, mFaceClimbableNormalAngle, ref mFaceClimbableNormalAngleUsed);

                if (lStateTime > 0.4f)
                {
                    mActorController.IsCollsionEnabled = false;
                }

                //if (lStateTime > 0.6f && lStateTime <= 0.9f)
                //{
                //    float lPercent = (lStateTime - 0.6f) / 0.3f;
                //    float lFrameYaw = (mFaceClimbableNormalAngle * lPercent) - mFaceClimbableNormalAngleUsed;

                //    mRotation = Quaternion.AngleAxis(lFrameYaw, Vector3.up);
                //    mFaceClimbableNormalAngleUsed = mFaceClimbableNormalAngle * lPercent;
                //}
            }
            else if (lStateID == STATE_LadderBottomOn)
            {
                mRotation = GetReachRotation(0.2f, 0.8f, mFaceClimbableNormalAngle, ref mFaceClimbableNormalAngleUsed);

                //if (lStateTime > 0.4f && lStateTime <= 0.55f)
                //{
                //    float lPercent = (lStateTime - 0.4f) / 0.15f;
                //    float lFrameYaw = (mFaceClimbableNormalAngle * lPercent) - mFaceClimbableNormalAngleUsed;

                //    mRotation = Quaternion.AngleAxis(lFrameYaw, Vector3.up);
                //    mFaceClimbableNormalAngleUsed = mFaceClimbableNormalAngle * lPercent;
                //}
            }
            // Test if we're at the top and can exit
            else if (lStateID == STATE_LadderUp)
            {
                if (!mIsExitTriggered && TestForTopExit())
                {
                    mIsExitTriggered = true;

                    MotionReachData lReachData = MotionReachData.Allocate();
                    lReachData.StateID = STATE_ClimbToIdle;
                    lReachData.StartTime = 0.40f;
                    lReachData.EndTime = 0.55f;
                    lReachData.Power = 3;
                    lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * _ReachOffset6) + (mActorController._Transform.forward * _ReachOffset7);
                    lReachData.ReachTargetGround = mActorController.State.Ground;
                    mReachData.Add(lReachData);

                    mActorController.IsCollsionEnabled = false;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_EXIT_TOP, true);
                }

                // Enable the collisions as soon as we can
                if (!mIsExitTriggered)
                {
                    mActorController.IsCollsionEnabled = true;

                    // Shoot forward and ensure we are still on a valid wall
                    Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.up * _DetachTestHeight);
                    if (!RaycastExt.SafeRaycast(lRayStart, mActorController._Transform.forward, out mRaycastHitInfo, _MaxDistance, _ClimbableLayers, mActorController._Transform))
                    {
                        Deactivate();
                        return;
                    }
                }
            }
            // We don't want to go lower than the ladder or into the floor
            else if (lStateID == STATE_LadderDown)
            {
                // Enable the collisions as soon as we can
                mActorController.IsCollsionEnabled = true;

                if (!mIsExitTriggered)
                {
                    if (mActorController.State.Ground != null && mActorController.State.GroundSurfaceDirectDistance > 0f && mActorController.State.GroundSurfaceDirectDistance < 0.2f)
                    {
                        mIsExitTriggered = true;
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_EXIT_BOTTOM, true);
                    }
                }

                // Shoot forward and ensure we are still on a valid wall
                if (!mIsExitTriggered)
                {
                    // Shoot forward and ensure we are still on a valid wall
                    Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.up * _DetachTestHeight);
                    if (!RaycastExt.SafeRaycast(lRayStart, mActorController._Transform.forward, out mRaycastHitInfo, _MaxDistance, _ClimbableLayers, mActorController._Transform))
                    {
                        RaycastExt.SafeRaycast(lRayStart, mActorController._Transform.forward, out mRaycastHitInfo, _MaxDistance, _ClimbableLayers, mActorController._Transform);
                        Deactivate();
                        return;
                    }
                }
            }
            // If we get off the ladder, end
            else if (lStateID == STATE_IdlePose)
            {
                Deactivate();
            }

            //Log.FileWrite(lStateTime + " " + StringHelper.ToString(mActorController._Transform.position - mStartPosition));
        }

        /// <summary>
        /// Determine if we're at the top of the ladder and we should exit
        /// </summary>
        /// <returns></returns>
        public virtual bool TestForTopExit()
        {
            // Shoot a ray forward above our head. If we want to exit, we can't hit anything
            Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.up * _TopExitTestHeight);
            Vector3 lRayDirection = mActorController._Transform.forward;
            float lRayDistance = 1f;

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, -1, mActorController._Transform))
            {
                return false;
            }

            // Shoot down to see if we hit a ledge. We want to hit the ledge.
            lRayStart = mActorController._Transform.position + (mActorController._Transform.up * _TopExitTestHeight) + (mActorController._Transform.forward * 1f);
            lRayDirection = -mActorController._Transform.up;
            lRayDistance = _TopExitTestHeight;

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, -1, mActorController._Transform))
            {
                // Shoot a ray forward to find the edge
                lRayStart = mRaycastHitInfo.point - (mActorController._Transform.up * 0.01f) - (mActorController._Transform.forward * 1f);
                lRayDirection = mActorController._Transform.forward;
                lRayDistance = 1.1f;

                if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, -1, mActorController._Transform))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Shoot a ray to determine if we found a wall while in the air.
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable wall</returns>
        public virtual bool TestForClimbInAir()
        {
            if (mActorController.State.GroundSurfaceDirectDistance > 0f && mActorController.State.GroundSurfaceDirectDistance < _MinGroundDistance) { return false; }

            // Shoot forward and ensure we can find a low wall
            Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.up * mActorController._MaxStepHeight);
            Vector3 lRayDirection = mMotionController._Transform.forward;
            float lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            float lDistance = Vector3.Distance(mActorController._Transform.position, mLastGrabPoint);
            if (lDistance < _MinRegrabDistance)
            {
                return false;
            }

            // If we get here, we've got a good wall
            return true;
        }

        /// <summary>
        /// Shoot a ray to determine if we found a ladder
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestForClimbUp()
        {
            // If there is active input pulling us away from the ladder, stop
            if (Mathf.Abs(mMotionController._InputSource.InputFromAvatarAngle) > 100f)
            {
                return false;
            }

            // Shoot forward and ensure we can find a ladder
            Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.up * mActorController._MaxStepHeight);
            Vector3 lRayDirection = mMotionController._Transform.forward;
            float lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Ensure we have a good distance
            if (mRaycastHitInfo.distance < _MinDistance || mRaycastHitInfo.distance > _MaxDistance)
            {
                return false;
            }

            // If we got here, we found an edge
            return true;
        }

        /// <summary>
        /// Shoot a ray to determine if we found a ladder behind us.
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestForClimbDownBack()
        {
            //float lTargetDistance = _MaxDistance;

            // Only climb those things that are on the climbable layer
            //int lIsClimbableMask = 1 << mClimbableLayers;

            // Go forward and see if we can shoot a ray down. If it hits, there's not a hole.
            Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.rotation * new Vector3(0f, mActorController._MaxStepHeight, -_MaxDistance));
            Vector3 lRayDirection = -mMotionController._Transform.up;
            float lRayDistance = mActorController._MaxStepHeight + 0.1f;

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, mActorController._CollisionLayers, mActorController._Transform))
            {
                return false;
            }

            // Shoot a ray from the back to the front to see if there's a ladder
            lRayStart = mActorController._Transform.position + (mActorController._Transform.rotation * new Vector3(0f, -mActorController._MaxStepHeight, -_MaxDistance));
            lRayDirection = mMotionController._Transform.forward;
            lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Ensure we have a good distance
            if (mRaycastHitInfo.distance > _MaxDistance)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shoot a ray to determine if we found a ladder
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestForClimbDownForward()
        {
            //float lTargetDistance = _MaxDistance;

            // Only climb those things that are on the climbable layer
            //int lIsClimbableMask = 1 << mClimbableLayers;

            // Go forward and see if we can shoot a ray down. If it hits, there's not a hole
            Vector3 lRayStart = mActorController._Transform.position + (mActorController._Transform.rotation * new Vector3(0f, mActorController._MaxStepHeight, _MaxDistance));
            Vector3 lRayDirection = -mMotionController._Transform.up;
            float lRayDistance = mActorController._MaxStepHeight + 0.1f;

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, mActorController._CollisionLayers, mActorController._Transform))
            {
                return false;
            }

            // Shoot a ray from the front to the back to see if there's a ladder
            lRayStart = mActorController._Transform.position + (mActorController._Transform.rotation * new Vector3(0f, -mActorController._MaxStepHeight, _MaxDistance));
            lRayDirection = -mMotionController._Transform.forward;
            lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Ensure we have a good distance
            if (mRaycastHitInfo.distance > _MaxDistance)
            {
                return false;
            }

            return true;
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        // Used to hide/show the offset section
        private bool mEditorShowOffsets = false;

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

            float lNewMinDistance = EditorGUILayout.FloatField(new GUIContent("Min Distance", "Minimum distance inwhich the climb is valid."), MinDistance, GUILayout.MinWidth(30));
            if (lNewMinDistance != MinDistance)
            {
                lIsDirty = true;
                MinDistance = lNewMinDistance;
            }

            float lNewMaxDistance = EditorGUILayout.FloatField(new GUIContent("Max Distance", "Maximum distance at which the climb is valid."), MaxDistance, GUILayout.MinWidth(30));
            if (lNewMaxDistance != MaxDistance)
            {
                lIsDirty = true;
                MaxDistance = lNewMaxDistance;
            }

            float lNewTopExitTestHeight = EditorGUILayout.FloatField(new GUIContent("Exit Test Height", "Height from actor's root to test if it's time to exit the top of the climb."), TopExitTestHeight, GUILayout.MinWidth(30));
            if (lNewTopExitTestHeight != TopExitTestHeight)
            {
                lIsDirty = true;
                TopExitTestHeight = lNewTopExitTestHeight;
            }

            float lNewDetachTestHeight = EditorGUILayout.FloatField(new GUIContent("Detach Test Height", "Height from actor's root to test if it's time to drop off the wall because of no surface."), DetachTestHeight, GUILayout.MinWidth(30));
            if (lNewDetachTestHeight != DetachTestHeight)
            {
                lIsDirty = true;
                DetachTestHeight = lNewDetachTestHeight;
            }

            float lNewMinGroundDistance = EditorGUILayout.FloatField(new GUIContent("Min Ground Distance", "When in a jump, the minimum ground distance before we allow a grab."), MinGroundDistance, GUILayout.MinWidth(30));
            if (lNewMinGroundDistance != MinGroundDistance)
            {
                lIsDirty = true;
                MinGroundDistance = lNewMinGroundDistance;
            }

            float lNewMinRegrabDistance = EditorGUILayout.FloatField(new GUIContent("Min Regrab Distance", "When in a jump, the minimum distance between the last grab."), MinRegrabDistance, GUILayout.MinWidth(30));
            if (lNewMinRegrabDistance != MinRegrabDistance)
            {
                lIsDirty = true;
                MinRegrabDistance = lNewMinRegrabDistance;
            }

            // Balance layer
            int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("Ladder Layers", "Layers that identies objects that can be climbed."), ClimbableLayers);
            if (lNewClimbableLayers != ClimbableLayers)
            {
                lIsDirty = true;
                ClimbableLayers = lNewClimbableLayers;
            }

            EditorGUI.indentLevel++;
            mEditorShowOffsets = EditorGUILayout.Foldout(mEditorShowOffsets, new GUIContent("Reach Offsets"));
            if (mEditorShowOffsets)
            {
                float lNewReachOffset1 = EditorGUILayout.FloatField(new GUIContent("Bottom start edge forward"), _ReachOffset1);
                if (lNewReachOffset1 != _ReachOffset1)
                {
                    lIsDirty = true;
                    _ReachOffset1 = lNewReachOffset1;
                }

                float lNewReachOffset2 = EditorGUILayout.FloatField(new GUIContent("Top start actor up"), _ReachOffset2);
                if (lNewReachOffset2 != _ReachOffset2)
                {
                    lIsDirty = true;
                    _ReachOffset2 = lNewReachOffset2;
                }

                float lNewReachOffset3 = EditorGUILayout.FloatField(new GUIContent("Top start edge forward"), _ReachOffset3);
                if (lNewReachOffset3 != _ReachOffset3)
                {
                    lIsDirty = true;
                    _ReachOffset3 = lNewReachOffset3;
                }

                float lNewReachOffset4 = EditorGUILayout.FloatField(new GUIContent("Top mid actor up"), _ReachOffset4);
                if (lNewReachOffset4 != _ReachOffset4)
                {
                    lIsDirty = true;
                    _ReachOffset4 = lNewReachOffset4;
                }

                float lNewReachOffset5 = EditorGUILayout.FloatField(new GUIContent("Top mid edge forward"), _ReachOffset5);
                if (lNewReachOffset5 != _ReachOffset5)
                {
                    lIsDirty = true;
                    _ReachOffset5 = lNewReachOffset5;
                }

                float lNewReachOffset6 = EditorGUILayout.FloatField(new GUIContent("Top end actor up"), _ReachOffset6);
                if (lNewReachOffset6 != _ReachOffset6)
                {
                    lIsDirty = true;
                    _ReachOffset6 = lNewReachOffset6;
                }

                float lNewReachOffset7 = EditorGUILayout.FloatField(new GUIContent("Top end edge forward"), _ReachOffset7);
                if (lNewReachOffset7 != _ReachOffset7)
                {
                    lIsDirty = true;
                    _ReachOffset7 = lNewReachOffset7;
                }
            }
            EditorGUI.indentLevel--;

            return lIsDirty;
        }

#endif

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int TRANS_EntryState_LadderBottomOn = -1;
        public static int TRANS_AnyState_LadderBottomOn = -1;
        public static int TRANS_EntryState_IdleTurn180L = -1;
        public static int TRANS_AnyState_IdleTurn180L = -1;
        public static int TRANS_EntryState_LadderTopOn = -1;
        public static int TRANS_AnyState_LadderTopOn = -1;
        public static int TRANS_EntryState_JumpToClimb = -1;
        public static int TRANS_AnyState_JumpToClimb = -1;
        public static int STATE_LadderBottomOn = -1;
        public static int TRANS_LadderBottomOn_LadderUp = -1;
        public static int STATE_LadderUp = -1;
        public static int TRANS_LadderUp_LadderDown = -1;
        public static int TRANS_LadderUp_LadderPose = -1;
        public static int TRANS_LadderUp_ClimbToIdle = -1;
        public static int STATE_LadderDown = -1;
        public static int TRANS_LadderDown_LadderBottomOff = -1;
        public static int TRANS_LadderDown_LadderUp = -1;
        public static int TRANS_LadderDown_LadderPose = -1;
        public static int STATE_LadderBottomOff = -1;
        public static int TRANS_LadderBottomOff_IdlePose = -1;
        public static int STATE_LadderPose = -1;
        public static int TRANS_LadderPose_LadderDown = -1;
        public static int TRANS_LadderPose_LadderUp = -1;
        public static int STATE_IdlePose = -1;
        public static int STATE_ClimbToIdle = -1;
        public static int TRANS_ClimbToIdle_IdlePose = -1;
        public static int STATE_LadderTopOn = -1;
        public static int TRANS_LadderTopOn_LadderDown = -1;
        public static int STATE_IdleTurn180L = -1;
        public static int TRANS_IdleTurn180L_LadderTopOn = -1;
        public static int STATE_JumpToClimb = -1;
        public static int TRANS_JumpToClimb_LadderPose = -1;

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

                if (lStateID == STATE_LadderBottomOn) { return true; }
                if (lStateID == STATE_LadderUp) { return true; }
                if (lStateID == STATE_LadderDown) { return true; }
                if (lStateID == STATE_LadderBottomOff) { return true; }
                if (lStateID == STATE_LadderPose) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_ClimbToIdle) { return true; }
                if (lStateID == STATE_LadderTopOn) { return true; }
                if (lStateID == STATE_IdleTurn180L) { return true; }
                if (lStateID == STATE_JumpToClimb) { return true; }
                if (lTransitionID == TRANS_EntryState_LadderBottomOn) { return true; }
                if (lTransitionID == TRANS_AnyState_LadderBottomOn) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_EntryState_LadderTopOn) { return true; }
                if (lTransitionID == TRANS_AnyState_LadderTopOn) { return true; }
                if (lTransitionID == TRANS_EntryState_JumpToClimb) { return true; }
                if (lTransitionID == TRANS_AnyState_JumpToClimb) { return true; }
                if (lTransitionID == TRANS_LadderBottomOn_LadderUp) { return true; }
                if (lTransitionID == TRANS_LadderUp_LadderDown) { return true; }
                if (lTransitionID == TRANS_LadderUp_LadderPose) { return true; }
                if (lTransitionID == TRANS_LadderUp_ClimbToIdle) { return true; }
                if (lTransitionID == TRANS_LadderDown_LadderBottomOff) { return true; }
                if (lTransitionID == TRANS_LadderDown_LadderUp) { return true; }
                if (lTransitionID == TRANS_LadderDown_LadderPose) { return true; }
                if (lTransitionID == TRANS_LadderBottomOff_IdlePose) { return true; }
                if (lTransitionID == TRANS_LadderPose_LadderDown) { return true; }
                if (lTransitionID == TRANS_LadderPose_LadderUp) { return true; }
                if (lTransitionID == TRANS_ClimbToIdle_IdlePose) { return true; }
                if (lTransitionID == TRANS_LadderTopOn_LadderDown) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_LadderTopOn) { return true; }
                if (lTransitionID == TRANS_JumpToClimb_LadderPose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_LadderBottomOn) { return true; }
            if (rStateID == STATE_LadderUp) { return true; }
            if (rStateID == STATE_LadderDown) { return true; }
            if (rStateID == STATE_LadderBottomOff) { return true; }
            if (rStateID == STATE_LadderPose) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_ClimbToIdle) { return true; }
            if (rStateID == STATE_LadderTopOn) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_JumpToClimb) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_LadderBottomOn) { return true; }
            if (rStateID == STATE_LadderUp) { return true; }
            if (rStateID == STATE_LadderDown) { return true; }
            if (rStateID == STATE_LadderBottomOff) { return true; }
            if (rStateID == STATE_LadderPose) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_ClimbToIdle) { return true; }
            if (rStateID == STATE_LadderTopOn) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_JumpToClimb) { return true; }
            if (rTransitionID == TRANS_EntryState_LadderBottomOn) { return true; }
            if (rTransitionID == TRANS_AnyState_LadderBottomOn) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_EntryState_LadderTopOn) { return true; }
            if (rTransitionID == TRANS_AnyState_LadderTopOn) { return true; }
            if (rTransitionID == TRANS_EntryState_JumpToClimb) { return true; }
            if (rTransitionID == TRANS_AnyState_JumpToClimb) { return true; }
            if (rTransitionID == TRANS_LadderBottomOn_LadderUp) { return true; }
            if (rTransitionID == TRANS_LadderUp_LadderDown) { return true; }
            if (rTransitionID == TRANS_LadderUp_LadderPose) { return true; }
            if (rTransitionID == TRANS_LadderUp_ClimbToIdle) { return true; }
            if (rTransitionID == TRANS_LadderDown_LadderBottomOff) { return true; }
            if (rTransitionID == TRANS_LadderDown_LadderUp) { return true; }
            if (rTransitionID == TRANS_LadderDown_LadderPose) { return true; }
            if (rTransitionID == TRANS_LadderBottomOff_IdlePose) { return true; }
            if (rTransitionID == TRANS_LadderPose_LadderDown) { return true; }
            if (rTransitionID == TRANS_LadderPose_LadderUp) { return true; }
            if (rTransitionID == TRANS_ClimbToIdle_IdlePose) { return true; }
            if (rTransitionID == TRANS_LadderTopOn_LadderDown) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_LadderTopOn) { return true; }
            if (rTransitionID == TRANS_JumpToClimb_LadderPose) { return true; }
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
            TRANS_EntryState_LadderBottomOn = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbLadder-SM.LadderBottomOn");
            TRANS_AnyState_LadderBottomOn = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbLadder-SM.LadderBottomOn");
            TRANS_EntryState_IdleTurn180L = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbLadder-SM.IdleTurn180L");
            TRANS_AnyState_IdleTurn180L = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbLadder-SM.IdleTurn180L");
            TRANS_EntryState_LadderTopOn = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbLadder-SM.LadderTopOn");
            TRANS_AnyState_LadderTopOn = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbLadder-SM.LadderTopOn");
            TRANS_EntryState_JumpToClimb = mMotionController.AddAnimatorName("Entry -> Base Layer.ClimbLadder-SM.JumpToClimb");
            TRANS_AnyState_JumpToClimb = mMotionController.AddAnimatorName("AnyState -> Base Layer.ClimbLadder-SM.JumpToClimb");
            STATE_LadderBottomOn = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderBottomOn");
            TRANS_LadderBottomOn_LadderUp = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderBottomOn -> Base Layer.ClimbLadder-SM.LadderUp");
            STATE_LadderUp = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderUp");
            TRANS_LadderUp_LadderDown = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderUp -> Base Layer.ClimbLadder-SM.LadderDown");
            TRANS_LadderUp_LadderPose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderUp -> Base Layer.ClimbLadder-SM.LadderPose");
            TRANS_LadderUp_ClimbToIdle = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderUp -> Base Layer.ClimbLadder-SM.ClimbToIdle");
            STATE_LadderDown = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderDown");
            TRANS_LadderDown_LadderBottomOff = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderDown -> Base Layer.ClimbLadder-SM.LadderBottomOff");
            TRANS_LadderDown_LadderUp = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderDown -> Base Layer.ClimbLadder-SM.LadderUp");
            TRANS_LadderDown_LadderPose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderDown -> Base Layer.ClimbLadder-SM.LadderPose");
            STATE_LadderBottomOff = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderBottomOff");
            TRANS_LadderBottomOff_IdlePose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderBottomOff -> Base Layer.ClimbLadder-SM.IdlePose");
            STATE_LadderPose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderPose");
            TRANS_LadderPose_LadderDown = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderPose -> Base Layer.ClimbLadder-SM.LadderDown");
            TRANS_LadderPose_LadderUp = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderPose -> Base Layer.ClimbLadder-SM.LadderUp");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.IdlePose");
            STATE_ClimbToIdle = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.ClimbToIdle");
            TRANS_ClimbToIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.ClimbToIdle -> Base Layer.ClimbLadder-SM.IdlePose");
            STATE_LadderTopOn = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderTopOn");
            TRANS_LadderTopOn_LadderDown = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.LadderTopOn -> Base Layer.ClimbLadder-SM.LadderDown");
            STATE_IdleTurn180L = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.IdleTurn180L");
            TRANS_IdleTurn180L_LadderTopOn = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.IdleTurn180L -> Base Layer.ClimbLadder-SM.LadderTopOn");
            STATE_JumpToClimb = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.JumpToClimb");
            TRANS_JumpToClimb_LadderPose = mMotionController.AddAnimatorName("Base Layer.ClimbLadder-SM.JumpToClimb -> Base Layer.ClimbLadder-SM.LadderPose");
        }

#if UNITY_EDITOR

        private AnimationClip mLadderBottomOn = null;
        private AnimationClip mLadderUpLoop = null;
        private AnimationClip mLadderDownLoop = null;
        private AnimationClip mLadderBottomOff = null;
        private AnimationClip mLadderPose = null;
        private AnimationClip mIdlePose = null;
        private AnimationClip mPullUpToIdle = null;
        private AnimationClip mIdleTurn180L = null;
        private AnimationClip mJumpToClimb = null;

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

            UnityEditor.Animations.AnimatorState lLadderBottomOn = lMotionStateMachine.AddState("LadderBottomOn", new Vector3(384, -24, 0));
            lLadderBottomOn.motion = mLadderBottomOn;
            lLadderBottomOn.speed = 1f;

            UnityEditor.Animations.AnimatorState lLadderUp = lMotionStateMachine.AddState("LadderUp", new Vector3(756, -24, 0));
            lLadderUp.motion = mLadderUpLoop;
            lLadderUp.speed = 1f;

            UnityEditor.Animations.AnimatorState lLadderDown = lMotionStateMachine.AddState("LadderDown", new Vector3(756, 168, 0));
            lLadderDown.motion = mLadderDownLoop;
            lLadderDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lLadderBottomOff = lMotionStateMachine.AddState("LadderBottomOff", new Vector3(948, 276, 0));
            lLadderBottomOff.motion = mLadderBottomOff;
            lLadderBottomOff.speed = 1f;

            UnityEditor.Animations.AnimatorState lLadderPose = lMotionStateMachine.AddState("LadderPose", new Vector3(612, 84, 0));
            lLadderPose.motion = mLadderPose;
            lLadderPose.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(984, 84, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbToIdle = lMotionStateMachine.AddState("ClimbToIdle", new Vector3(948, -120, 0));
            lClimbToIdle.motion = mPullUpToIdle;
            lClimbToIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lLadderTopOn = lMotionStateMachine.AddState("LadderTopOn", new Vector3(384, 168, 0));
            lLadderTopOn.motion = mPullUpToIdle;
            lLadderTopOn.speed = -1f;

            UnityEditor.Animations.AnimatorState lIdleTurn180L = lMotionStateMachine.AddState("IdleTurn180L", new Vector3(384, 240, 0));
            lIdleTurn180L.motion = mIdleTurn180L;
            lIdleTurn180L.speed = 1f;

            UnityEditor.Animations.AnimatorState lJumpToClimb = lMotionStateMachine.AddState("JumpToClimb", new Vector3(384, 84, 0));
            lJumpToClimb.motion = mJumpToClimb;
            lJumpToClimb.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lLadderBottomOn);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1500f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn180L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.09999999f;
            lAnyStateTransition.offset = 0.2584679f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1506f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lLadderTopOn);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1505f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lJumpToClimb);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1504f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lLadderBottomOn.AddTransition(lLadderUp);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.9289518f;
            lStateTransition.duration = 0.03373986f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lLadderUp.AddTransition(lLadderDown);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6739131f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lLadderUp.AddTransition(lLadderPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lLadderUp.AddTransition(lClimbToIdle);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6875f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1520f, "L0MotionPhase");

            lStateTransition = lLadderDown.AddTransition(lLadderBottomOff);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8700946f;
            lStateTransition.duration = 0.1035532f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1510f, "L0MotionPhase");

            lStateTransition = lLadderDown.AddTransition(lLadderUp);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6739131f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lLadderDown.AddTransition(lLadderPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lLadderBottomOff.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5000001f;
            lStateTransition.duration = 0.1107834f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lLadderPose.AddTransition(lLadderDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lLadderPose.AddTransition(lLadderUp);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lClimbToIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8255814f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lLadderTopOn.AddTransition(lLadderDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8706896f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleTurn180L.AddTransition(lLadderTopOn);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7307953f;
            lStateTransition.duration = 0.2499998f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lJumpToClimb.AddTransition(lLadderPose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7f;
            lStateTransition.duration = 0.3f;
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
            mLadderBottomOn = CreateAnimationField("LadderBottomOn", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_Ladder.fbx/LadderBottomOn.anim", "LadderBottomOn", mLadderBottomOn);
            mLadderUpLoop = CreateAnimationField("LadderUp", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_Ladder.fbx/LadderUpLoop.anim", "LadderUpLoop", mLadderUpLoop);
            mLadderDownLoop = CreateAnimationField("LadderDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_Ladder.fbx/LadderDownLoop.anim", "LadderDownLoop", mLadderDownLoop);
            mLadderBottomOff = CreateAnimationField("LadderBottomOff", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_Ladder.fbx/LadderBottomOff.anim", "LadderBottomOff", mLadderBottomOff);
            mLadderPose = CreateAnimationField("LadderPose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_Ladder.fbx/LadderPose.anim", "LadderPose", mLadderPose);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);
            mPullUpToIdle = CreateAnimationField("ClimbToIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx/PullUpToIdle.anim", "PullUpToIdle", mPullUpToIdle);
            mIdleTurn180L = CreateAnimationField("IdleTurn180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180L.anim", "IdleTurn180L", mIdleTurn180L);
            mJumpToClimb = CreateAnimationField("JumpToClimb", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbCrouch.fbx/JumpToClimb.anim", "JumpToClimb", mJumpToClimb);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
