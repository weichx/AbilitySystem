using UnityEngine;
using com.ootii.Cameras;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// </summary>
    [MotionName("Walk Run Pivot")]
    [MotionDescription("Standard movement (walk/run) for an adventure game.")]
    public class WalkRunPivot : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 27100;
        public const int PHASE_START_SHORTCUT_WALK = 27114;
        public const int PHASE_START_SHORTCUT_RUN = 27115;

        public const int PHASE_STOP_RIGHT_DOWN = 27120;
        public const int PHASE_STOP_LEFT_DOWN = 27121;

        /// <summary>
        /// Determines if we run by default or walk
        /// </summary>
        public bool _DefaultToRun = false;
        public bool DefaultToRun
        {
            get { return _DefaultToRun; }
            set { _DefaultToRun = value; }
        }

        /// <summary>
        /// Degrees per second to rotate the actor in order to face the input direction
        /// </summary>
        public float _RotationSpeed = 180f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
        }

        /// <summary>
        /// Minimum angle before we use the pivot speed
        /// </summary>
        public float _MinPivotAngle = 40f;
        public float MinPivotAngle
        {
            get { return _MinPivotAngle; }
            set { _MinPivotAngle = value; }
        }

        /// <summary>
        /// Degrees per second to rotate the actor when pivoting to face a direction
        /// </summary>
        public float _PivotSpeed = 180f;
        public float PivotSpeed
        {
            get { return _PivotSpeed; }
            set { _PivotSpeed = value; }
        }

        /// <summary>
        /// Delay in seconds before we allow a stop. This is to support pivoting
        /// </summary>
        public float _StopDelay = 0.15f;
        public float StopDelay
        {
            get { return _StopDelay; }
            set { _StopDelay = value; }
        }

        /// <summary>
        /// Number of degrees we'll accelerate and decelerate by
        /// in order to reach the rotation target
        /// </summary>
        public bool _RemoveLateralMovement = true;
        public bool RemoveLateralMovement
        {
            get { return _RemoveLateralMovement; }
            set { _RemoveLateralMovement = value; }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        private bool mStartInWalk = false;
        public bool StartInWalk
        {
            get { return mStartInWalk; }
            set { mStartInWalk = value; }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        private bool mStartInRun = false;
        public bool StartInRun
        {
            get { return mStartInRun; }
            set { mStartInRun = value; }
        }

        /// <summary>
        /// Determines if the actor should be running based on input
        /// </summary>
        public bool IsRunActive
        {
            get
            {
                if (mMotionController._InputSource == null) { return _DefaultToRun; }
                return ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
            }
        }

        /// <summary>
        /// Track when we stop getting input
        /// </summary>
        private float mInputInactiveStartTime = 0f;

        /// <summary>
        /// Track the magnitude we have from the input
        /// </summary>
        private float mInputMagnitude = 0f;

        /// <summary>
        /// Track the angle we have from the input
        /// </summary>
        private float mInputFromAvatarAngleStart = 0f;

        /// <summary>
        /// Tracks the amount of rotation that was already used
        /// </summary>
        private float mInputFromAvatarAngleUsed = 0f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkRunPivot()
            : base()
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunPivot-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunPivot(MotionController rController)
            : base(rController)
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunPivot-SM"; }
#endif
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
            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            // If we're not actually moving, we can stop too
            MotionState lState = mMotionController.State;
            if (lState.InputMagnitudeTrend.Value < 0.03f)
            {
                return false;
            }

            // If we're not in the traversal state, this is easy
            //if (lState.Stance != EnumControllerStance.TRAVERSAL) 
            //{
            //    mInputActiveStartTime = 0f;
            //    return false; 
            //}

            //// If there is no activation delay, continue
            //if (ACTIVATE_DELAY > 0f)
            //{
            //    // Start the timer if it hasn't been
            //    if (mInputActiveStartTime == 0f)
            //    {
            //        mInputActiveStartTime = Time.time;
            //        return false;
            //    }
            //    // If we're delayed, see if we've waited long enough
            //    else if (Time.time - mInputActiveStartTime < ACTIVATE_DELAY)
            //    {
            //        return false;
            //    }
            //}

            // We're good to move
            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            // If we just entered this frame, stay
            if (mIsActivatedFrame)
            {
                return true;
            }

            // If we are no longer grounded, stop
            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            MotionState lState = mMotionController.State;
            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // If we're not in the normal traveral state, stop
            //if (lState.Stance != EnumControllerStance.TRAVERSAL) { return false; }

            // If we're in the idle state with no movement, stop
            if (mAge > 0.2f && lStateID == STATE_IdlePose)
            {
                if (lState.InputMagnitudeTrend.Value == 0f)
                {
                    return false;
                }
            }

            // If we're in the idle state machine, we can get out
            if (mAge > 0.2f && lStateID == Idle.STATE_IdlePose)
            {
                if (lTransitionID != TRANS_AnyState_IdlePose)
                {
                    return false;
                }
            }

            // One last check to make sure we're in this state
            if (mIsAnimatorActive && !IsMotionState(lStateID) && !mStartInRun && !mStartInWalk)
            {
                // This is a bit painful, but make sure we're not in a 
                // transition to this sub-state machine
                if (lTransitionID != TRANS_EntryState_IdlePose &&
                    lTransitionID != TRANS_EntryState_IdleToWalk &&
                    lTransitionID != TRANS_EntryState_IdleToRun &&
                    lTransitionID != TRANS_EntryState_IdleTurn20R &&
                    lTransitionID != TRANS_EntryState_IdleTurn90R &&
                    lTransitionID != TRANS_EntryState_IdleTurn180R &&
                    lTransitionID != TRANS_EntryState_IdleTurn20L &&
                    lTransitionID != TRANS_EntryState_IdleTurn90L &&
                    lTransitionID != TRANS_EntryState_IdleTurn180L &&
                    lTransitionID != TRANS_EntryState_RunFwdLoop &&
                    lTransitionID != TRANS_EntryState_WalkFwdLoop)
                {
                    return false;
                }
            }

            // Stay
            return true;
        }

        /// <summary>
        /// Raised when a motion is being interrupted by another motion
        /// </summary>
        /// <param name="rMotion">Motion doing the interruption</param>
        /// <returns>Boolean determining if it can be interrupted</returns>
        public override bool TestInterruption(MotionControllerMotion rMotion)
        {
            if (rMotion is Idle)
            {
                //int lStateID = mMotionController.State.AnimatorStates[mMotionLayer.AnimatorLayerIndex].StateInfo.fullPathHash;
                //if (lStateID != STATE_IdlePose)
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
            // Update the current state information so that the animator
            // believes we need to start moving at this time.
            //MotionState lState = mMotionController.State;

            //Log.FileWrite("WalkRunPivot.Activate() run:" + mStartInRun + " walk:" + mStartInWalk);

            if (mStartInRun)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START_SHORTCUT_RUN, true);
            }
            else if (mStartInWalk)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START_SHORTCUT_WALK, true);
            }
            else if (mMotionController._InputSource == null)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);
            }
            else
            {
                bool lIsRunActivated = ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
                if (lIsRunActivated)
                {
                    MotionState lState = mMotionController.State;
                    lState.InputMagnitudeTrend.Value = 1f;

                    mMotionController.State = lState;
                }

                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);
            }

            // Clear any internal values
            mInputInactiveStartTime = 0f;

            // Store the angle we're starting our movement at. We use this
            // as we pivot to the forward direction
            mInputFromAvatarAngleStart = mMotionController.State.InputFromAvatarAngle;
            mInputFromAvatarAngleUsed = 0f;

            // Flag this motion as active
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
            // Clear out the start
            mStartInRun = false;
            mStartInWalk = false;

            base.Deactivate();
        }

        /// <summary>
        /// Allows the motion to modify the velocity before it is applied. 
        /// 
        /// NOTE:
        /// Be careful when removing rotations
        /// as some transitions will want rotations even if the state they are transitioning from don't.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
            // Don't allow root motion if we're adjusting the forward direction
            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // If we're transitioning to the run forward, stop rotation
            if (lTransitionID == TRANS_EntryState_RunFwdLoop || lTransitionID == TRANS_EntryState_WalkFwdLoop)
            {
                rRotation = Quaternion.identity;

                if (_RemoveLateralMovement)
                {
                    rMovement.x = 0f;
                }
            }
            // No rotation whould occur when stopping
            else if (lStateID == STATE_WalkToIdle_RDown || lStateID == STATE_WalkToIdle_LDown)
            {
                rRotation = Quaternion.identity;
            }
            // No rotation should occur when adjusting to the forward position. Here, the animation
            // does have rotation, but we don't want it.
            else if (lStateID == STATE_IdleTurn20L || lStateID == STATE_IdleTurn20R)
            {
                rRotation = Quaternion.identity;
            }

            // Remove any side-to-side sway
            if (_RemoveLateralMovement && (lStateID == STATE_WalkFwdLoop || lStateID == STATE_IdleToWalk))
            {
                rMovement.x = 0f;
            }

            // Don't allow backwards movement when moving forward. Some animations have this
            //if (lStateID == STATE_WalkFwdLoop && rVelocityDelta.z < 0f)
            if (rMovement.z < 0f)
            {
                rMovement.z = 0f;
            }

            //Log.FileWrite("Adv_Forward.UpdateRootMotion Final VDelta:" + StringHelper.ToString(rVelocityDelta));
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            //Log.FileWrite("Adv_Forward.UpdateMotion UI:" + rUpdateIndex);

            // We only want to process on the first update iteration
            // of each frame. Some motions may be different, but this one is easy
            if (rUpdateIndex < 1) { return; }

            // Used to determine if we'll actually use the input values this frame
            bool lUseInput = true;

            // Determines if we need to update the state itself
            bool lUpdateAnimatorState = false;

            // Grab the state info
            MotionState lState = mMotionController.State;
            int lStateMotionPhase = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionPhase;

            AnimatorStateInfo lStateInfo = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo;
            int lStateID = lStateInfo.fullPathHash;

            // Just in case, clear the motion phase. We do this because we have instant transitions and we don't 
            // want to re-enter the states.
            if (lStateMotionPhase == PHASE_START)
            {
                if (lStateID == STATE_IdlePose ||
                    lStateID == STATE_IdleTurn20L ||
                    lStateID == STATE_IdleTurn90L ||
                    lStateID == STATE_IdleTurn180L ||
                    lStateID == STATE_IdleTurn20R ||
                    lStateID == STATE_IdleTurn90R ||
                    lStateID == STATE_IdleTurn180R)
                {
                    lUpdateAnimatorState = true;
                    lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionPhase = 0;
                }
            }

            // In order to swap from walking to running, we're going to modify the state value some.
            bool lIsRunActivated = _DefaultToRun;
            if (mMotionController._InputSource == null)
            {
                lIsRunActivated = (mMotionController.State.InputMagnitudeTrend.Value > 0.9f);
            }
            else
            {
                lIsRunActivated = ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
            }

            if (!lIsRunActivated)
            {
                lUpdateAnimatorState = true;
                lState.InputY = lState.InputY * 0.5f;

                if (lState.InputMagnitudeTrend.Value > 0.5f)
                {
                    lState.InputMagnitudeTrend.Replace(0.5f);
                }
            }

            // Update the animator state parameter as the "run" flag

            // TRT 4/5/2106: We want to ensure the transition occurs if the speed drops
            int lRunActivated = (lIsRunActivated && lState.InputMagnitudeTrend.Value > 0.9f ? 1 : 0);
            //if (lStateMotionParameter != lRunActivated)
            {
                lUpdateAnimatorState = true;
                lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = lRunActivated;
            }

            // We may not want to react to the 0 input too quickly. This way we can see
            // if the player is truley stoping or just pivoting...
            //
            // This first check is to see if we process the input immediately
            if (lState.InputMagnitudeTrend.Value >= 0.5f || _StopDelay == 0f || !(lStateID == STATE_WalkFwdLoop || lStateID == STATE_RunFwdLoop))
            {
                // No delay
                if (mInputInactiveStartTime == 0f)
                {
                    mInputMagnitude = lState.InputMagnitudeTrend.Value;
                }
                // Keep going with the delay until it expires or the input is raised
                else if (lState.InputMagnitudeTrend.Value < 0.6f && mInputInactiveStartTime + _StopDelay > Time.time)
                {
                    lUseInput = false;

                    lUpdateAnimatorState = true;
                    lState.InputMagnitudeTrend.Replace(mInputMagnitude);
                }
                // Clear the delay
                else
                {
                    mInputInactiveStartTime = 0f;
                }
            }
            // If not, we'll delay a bit before changing the magnitude to 0
            else
            {
                if (mInputInactiveStartTime == 0f)
                {
                    mInputInactiveStartTime = Time.time;
                }

                // We use this delay in order to enable the 180 pivot. Without it, the actor
                // comes to a stop and then pivots...which is awkward. However, with it,
                // when we do want to stop (and we're running left/right), the character will keep pivoting.
                if (mInputInactiveStartTime + _StopDelay > Time.time)
                {
                    lUseInput = false;

                    lUpdateAnimatorState = true;
                    lState.InputMagnitudeTrend.Replace(mInputMagnitude);
                }
            }

            // As long as we're not delaying the input, see if we need to pivot
            if (lUseInput)
            {
                // If we are processing input, we can clear the
                // last angular velocity and recalculate it (later)
                mAngularVelocity = Vector3.zero;

                // If we're starting to walk or run, allow the actor rotation
                if (lStateID == STATE_IdleToWalk || lStateID == STATE_IdleToRun)
                {
                    float lPercent = Mathf.Clamp01(lStateInfo.normalizedTime);
                    mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                }
                // Rotate the avatar if we're walking
                else if (lStateID == STATE_WalkFwdLoop)
                {
                    if (Mathf.Abs(lState.InputFromAvatarAngle) < 140f)
                    {
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime);
                    }

                    // We set the motion phase here because we want the transition to occur because we want to break the
                    // walk animation into two phases: left up and right up
                    if (lState.InputMagnitudeTrend.Value < 0.1f
                        && (lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == 0 ||
                            lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == PHASE_STOP_LEFT_DOWN ||
                            lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == PHASE_STOP_RIGHT_DOWN)
                        )
                    {
                        float lNormalizedTime = lStateInfo.normalizedTime % 1f;
                        if (lNormalizedTime > 0.25f && lNormalizedTime < 0.75f)
                        {
                            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_STOP_LEFT_DOWN, true);
                        }
                        else
                        {
                            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_STOP_RIGHT_DOWN, true);
                        }
                    }
                }
                // Rotate the avatar if we're running
                else if (lStateID == STATE_RunFwdLoop || lStateID == STATE_RunStop_RDown || lStateID == STATE_RunStop_LDown)
                {
                    if (Mathf.Abs(lState.InputFromAvatarAngle) < 140f)
                    {
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime);
                    }
                }
                // If we're coming out of a idle-pivot-to-walk, allow the actor rotation
                else if (lStateID == STATE_IdleToWalk90L || lStateID == STATE_IdleToWalk90R)
                {
                    if (lStateInfo.normalizedTime > 0.7f)
                    {
                        float lPercent = Mathf.Clamp01((lStateInfo.normalizedTime - 0.7f) / 0.3f);
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                    }
                }
                // If we're coming out of a walk-pivot-to-walk, allow the actor rotation.
                // The left pivot takes longer to pivot than the right. So, we need extra time
                else if (lStateID == STATE_WalkPivot180L)
                {
                    if (lStateInfo.normalizedTime > 0.95f)
                    {
                        float lPercent = Mathf.Clamp01((lStateInfo.normalizedTime - 0.95f) / 0.05f);
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                    }
                }
                // If we're coming out of a walk-pivot-to-walk, allow the actor rotation
                // The right pivot takes less to pivot than the left. So, we need less time
                else if (lStateID == STATE_WalkPivot180R)
                {
                    if (lStateInfo.normalizedTime > 0.7f)
                    {
                        float lPercent = Mathf.Clamp01((lStateInfo.normalizedTime - 0.7f) / 0.3f);
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                    }
                }
            }

            // Clear out our start flags
            if (lStateID == STATE_WalkFwdLoop || lStateID == STATE_RunFwdLoop)
            {
                mStartInRun = false;
                mStartInWalk = false;
            }

            // If we're in the idle pose and just turning a little to face in the input direction
            // cleanly, determine the rotation speed and use it to turn the actor.
            // Animations run from 0.55 to 1.0f
            //  0.55 [in]  = Transition Duration (s) + Transition Offset
            //  1.00 [out] = Exit Time + Transition Duration (%) 
            mRotation = Quaternion.identity;
            if (lStateID == STATE_IdleTurn20L || lStateID == STATE_IdleTurn20R)
            {
                float lPercent = Mathf.Clamp01((lStateInfo.normalizedTime - 0.55f) / 0.45f);
                float lTotalRotation = mInputFromAvatarAngleStart * lPercent;
                float lFrameRotation = lTotalRotation - mInputFromAvatarAngleUsed;

                mRotation = Quaternion.Euler(0f, lFrameRotation, 0f);

                mInputFromAvatarAngleUsed = lTotalRotation;
            }

            // If we need to update the animator state, do it once
            if (lUpdateAnimatorState)
            {
                mMotionController.State = lState;
            }

            //Log.FileScreenWrite("Adv_Forward.UpdateMotion Final MP:" + mMotionController.State.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase + " AngVel:" + mAngularVelocity.y.ToString("f3"), 6);
        }

        /// <summary>
        /// Retrieve the rotation speed we'll use to get the actor to face towards
        /// the direciton of the input
        /// </summary>
        /// <param name="rAngle"></param>
        /// <param name="rDeltaTime"></param>
        /// <returns></returns>
        private float GetRotationSpeed(float rAngle, float rDeltaTime)
        {
            int lPivotState = 0;
            float lAbsAngle = Mathf.Abs(rAngle);

            // Determine if we'll use the pivot speed
            if (_RotationSpeed == 0f && lAbsAngle > 10f)
            {
                lPivotState = 1;
            }
            else if (_MinPivotAngle != 0f && lAbsAngle >= _MinPivotAngle)
            {
                lPivotState = 1;
            }

            // Grab our final rotation speed, but make sure it doesn't exceed the target angle
            float lRotationSpeed = Mathf.Sign(rAngle) * (lPivotState == 0 ? _RotationSpeed : _PivotSpeed);
            if (lRotationSpeed == 0f || Mathf.Abs(lRotationSpeed * rDeltaTime) > lAbsAngle)
            {
                lRotationSpeed = rAngle / rDeltaTime;
            }

            // Return the result
            return lRotationSpeed;
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Creates input settings in the Unity Input Manager
        /// </summary>
        public override void CreateInputManagerSettings()
        {
            if (!InputManagerHelper.IsDefined(_ActionAlias))
            {
                InputManagerEntry lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "left shift";
                lEntry.Gravity = 1000;
                lEntry.Dead = 0.001f;
                lEntry.Sensitivity = 1000;
                lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
                lEntry.Axis = 0;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

                lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "";
                lEntry.Gravity = 1;
                lEntry.Dead = 0.3f;
                lEntry.Sensitivity = 1;
                lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
                lEntry.Axis = 5;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#else

                lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "";
                lEntry.Gravity = 1;
                lEntry.Dead = 0.3f;
                lEntry.Sensitivity = 1;
                lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
                lEntry.Axis = 9;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#endif
            }
        }
        
        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            bool lNewDefaultToRun = EditorGUILayout.Toggle(new GUIContent("Default to Run", "Determines if the default is to run or walk."), _DefaultToRun);
            if (lNewDefaultToRun != _DefaultToRun)
            {
                lIsDirty = true;
                DefaultToRun = lNewDefaultToRun;
            }

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default)."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            GUILayout.Space(5f);

            float lNewRotationSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second to rotate towards the camera forward (when not pivoting). A value of '0' means rotate instantly."), RotationSpeed);
            if (lNewRotationSpeed != RotationSpeed)
            {
                lIsDirty = true;
                RotationSpeed = lNewRotationSpeed;
            }

            float lNewMinPivotAngle = EditorGUILayout.FloatField(new GUIContent("Min Pivot Angle", "Degrees where we use the pivot speed for rotating."), MinPivotAngle);
            if (lNewMinPivotAngle != MinPivotAngle)
            {
                lIsDirty = true;
                MinPivotAngle = lNewMinPivotAngle;
            }

            float lNewPivotSpeed = EditorGUILayout.FloatField(new GUIContent("Pivot Speed", "Degrees per second to rotate when pivoting exceeds the min pivot angle."), PivotSpeed);
            if (lNewPivotSpeed != PivotSpeed)
            {
                lIsDirty = true;
                PivotSpeed = lNewPivotSpeed;
            }

            float lNewStopDelay = EditorGUILayout.FloatField(new GUIContent("Stop Delay", "Delay (in seconds) before we process a stop. This gives us time to test for a pivot."), StopDelay);
            if (lNewStopDelay != StopDelay)
            {
                lIsDirty = true;
                StopDelay = lNewStopDelay;
            }

            GUILayout.Space(5f);

            bool lNewRemoveLateralMovement = EditorGUILayout.Toggle(new GUIContent("Remove Lateral Movement", "Determines if we'll remove sideways movement to reduce swaying."), RemoveLateralMovement);
            if (lNewRemoveLateralMovement != RemoveLateralMovement)
            {
                lIsDirty = true;
                RemoveLateralMovement = lNewRemoveLateralMovement;
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
        public static int TRANS_EntryState_IdleTurn20R = -1;
        public static int TRANS_AnyState_IdleTurn20R = -1;
        public static int TRANS_EntryState_IdleTurn90R = -1;
        public static int TRANS_AnyState_IdleTurn90R = -1;
        public static int TRANS_EntryState_IdleTurn180R = -1;
        public static int TRANS_AnyState_IdleTurn180R = -1;
        public static int TRANS_EntryState_IdleTurn20L = -1;
        public static int TRANS_AnyState_IdleTurn20L = -1;
        public static int TRANS_EntryState_IdleTurn90L = -1;
        public static int TRANS_AnyState_IdleTurn90L = -1;
        public static int TRANS_EntryState_IdleTurn180L = -1;
        public static int TRANS_AnyState_IdleTurn180L = -1;
        public static int TRANS_EntryState_IdlePose = -1;
        public static int TRANS_AnyState_IdlePose = -1;
        public static int TRANS_EntryState_RunFwdLoop = -1;
        public static int TRANS_AnyState_RunFwdLoop = -1;
        public static int TRANS_EntryState_WalkFwdLoop = -1;
        public static int TRANS_AnyState_WalkFwdLoop = -1;
        public static int TRANS_EntryState_IdleToWalk = -1;
        public static int TRANS_AnyState_IdleToWalk = -1;
        public static int TRANS_EntryState_IdleToRun = -1;
        public static int TRANS_AnyState_IdleToRun = -1;
        public static int STATE_IdleToWalk = -1;
        public static int TRANS_IdleToWalk_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk_WalkToIdle = -1;
        public static int STATE_IdleToRun = -1;
        public static int TRANS_IdleToRun_RunFwdLoop = -1;
        public static int TRANS_IdleToRun_RunStop_LDown = -1;
        public static int STATE_IdleTurn90L = -1;
        public static int TRANS_IdleTurn90L_WalkFwdLoop = -1;
        public static int TRANS_IdleTurn90L_IdlePose = -1;
        public static int TRANS_IdleTurn90L_RunFwdLoop = -1;
        public static int STATE_IdleTurn180L = -1;
        public static int TRANS_IdleTurn180L_WalkFwdLoop = -1;
        public static int TRANS_IdleTurn180L_IdlePose = -1;
        public static int TRANS_IdleTurn180L_RunFwdLoop = -1;
        public static int STATE_IdleToWalk90L = -1;
        public static int TRANS_IdleToWalk90L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90L_IdlePose = -1;
        public static int STATE_IdleToWalk180L = -1;
        public static int TRANS_IdleToWalk180L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180L_IdlePose = -1;
        public static int STATE_IdleToRun90L = -1;
        public static int TRANS_IdleToRun90L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun90L_RunStop_LDown = -1;
        public static int STATE_IdleToRun180L = -1;
        public static int TRANS_IdleToRun180L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180L_RunStop_LDown = -1;
        public static int STATE_IdleTurn90R = -1;
        public static int TRANS_IdleTurn90R_WalkFwdLoop = -1;
        public static int TRANS_IdleTurn90R_IdlePose = -1;
        public static int TRANS_IdleTurn90R_RunFwdLoop = -1;
        public static int STATE_IdleTurn180R = -1;
        public static int TRANS_IdleTurn180R_IdlePose = -1;
        public static int TRANS_IdleTurn180R_WalkFwdLoop = -1;
        public static int TRANS_IdleTurn180R_RunFwdLoop = -1;
        public static int STATE_IdleToWalk90R = -1;
        public static int TRANS_IdleToWalk90R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90R_IdlePose = -1;
        public static int STATE_IdleToWalk180R = -1;
        public static int TRANS_IdleToWalk180R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180R_IdlePose = -1;
        public static int STATE_IdleToRun90R = -1;
        public static int TRANS_IdleToRun90R_RunStop_LDown = -1;
        public static int TRANS_IdleToRun90R_RunFwdLoop = -1;
        public static int STATE_IdleToRun180R = -1;
        public static int TRANS_IdleToRun180R_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180R_RunStop_LDown = -1;
        public static int STATE_IdlePose = -1;
        public static int TRANS_IdlePose_IdleToWalk180R = -1;
        public static int TRANS_IdlePose_IdleToWalk90R = -1;
        public static int TRANS_IdlePose_IdleToWalk180L = -1;
        public static int TRANS_IdlePose_IdleToWalk90L = -1;
        public static int TRANS_IdlePose_IdleToWalk = -1;
        public static int TRANS_IdlePose_IdleToRun = -1;
        public static int TRANS_IdlePose_IdleToRun90L = -1;
        public static int TRANS_IdlePose_IdleToRun180L = -1;
        public static int TRANS_IdlePose_IdleToRun90R = -1;
        public static int TRANS_IdlePose_IdleToRun180R = -1;
        public static int STATE_WalkFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_RunFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_RDown = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_LDown = -1;
        public static int TRANS_WalkFwdLoop_WalkPivot180L = -1;
        public static int TRANS_WalkFwdLoop_WalkPivot180R = -1;
        public static int STATE_RunFwdLoop = -1;
        public static int TRANS_RunFwdLoop_WalkFwdLoop = -1;
        public static int TRANS_RunFwdLoop_RunStop_RDown = -1;
        public static int TRANS_RunFwdLoop_RunStop_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_RDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_RDown = -1;
        public static int STATE_RunPivot180L_RDown = -1;
        public static int TRANS_RunPivot180L_RDown_RunFwdLoop = -1;
        public static int STATE_RunPivot180R_LDown = -1;
        public static int TRANS_RunPivot180R_LDown_RunFwdLoop = -1;
        public static int STATE_WalkToIdle_RDown = -1;
        public static int TRANS_WalkToIdle_RDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_RDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_RDown_WalkPivot180R = -1;
        public static int STATE_WalkToIdle_LDown = -1;
        public static int TRANS_WalkToIdle_LDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_LDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_LDown_WalkPivot180L = -1;
        public static int STATE_RunStop_RDown = -1;
        public static int TRANS_RunStop_RDown_IdlePose = -1;
        public static int TRANS_RunStop_RDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_RDown_RunPivot180R_LDown = -1;
        public static int STATE_RunStop_LDown = -1;
        public static int TRANS_RunStop_LDown_IdlePose = -1;
        public static int TRANS_RunStop_LDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_LDown_RunPivot180R_RDown = -1;
        public static int STATE_RunPivot180L_LDown = -1;
        public static int TRANS_RunPivot180L_LDown_RunFwdLoop = -1;
        public static int STATE_RunPivot180R_RDown = -1;
        public static int TRANS_RunPivot180R_RDown_RunFwdLoop = -1;
        public static int STATE_IdleTurn20R = -1;
        public static int TRANS_IdleTurn20R_IdlePose = -1;
        public static int STATE_IdleTurn20L = -1;
        public static int TRANS_IdleTurn20L_IdlePose = -1;
        public static int STATE_WalkToIdle = -1;
        public static int TRANS_WalkToIdle_IdlePose = -1;
        public static int STATE_WalkPivot180L = -1;
        public static int TRANS_WalkPivot180L_WalkFwdLoop = -1;
        public static int STATE_WalkPivot180R = -1;
        public static int TRANS_WalkPivot180R_WalkFwdLoop = -1;

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

                if (lStateID == STATE_IdleToWalk) { return true; }
                if (lStateID == STATE_IdleToRun) { return true; }
                if (lStateID == STATE_IdleTurn90L) { return true; }
                if (lStateID == STATE_IdleTurn180L) { return true; }
                if (lStateID == STATE_IdleToWalk90L) { return true; }
                if (lStateID == STATE_IdleToWalk180L) { return true; }
                if (lStateID == STATE_IdleToRun90L) { return true; }
                if (lStateID == STATE_IdleToRun180L) { return true; }
                if (lStateID == STATE_IdleTurn90R) { return true; }
                if (lStateID == STATE_IdleTurn180R) { return true; }
                if (lStateID == STATE_IdleToWalk90R) { return true; }
                if (lStateID == STATE_IdleToWalk180R) { return true; }
                if (lStateID == STATE_IdleToRun90R) { return true; }
                if (lStateID == STATE_IdleToRun180R) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_WalkFwdLoop) { return true; }
                if (lStateID == STATE_RunFwdLoop) { return true; }
                if (lStateID == STATE_RunPivot180L_RDown) { return true; }
                if (lStateID == STATE_RunPivot180R_LDown) { return true; }
                if (lStateID == STATE_WalkToIdle_RDown) { return true; }
                if (lStateID == STATE_WalkToIdle_LDown) { return true; }
                if (lStateID == STATE_RunStop_RDown) { return true; }
                if (lStateID == STATE_RunStop_LDown) { return true; }
                if (lStateID == STATE_RunPivot180L_LDown) { return true; }
                if (lStateID == STATE_RunPivot180R_RDown) { return true; }
                if (lStateID == STATE_IdleTurn20R) { return true; }
                if (lStateID == STATE_IdleTurn20L) { return true; }
                if (lStateID == STATE_WalkToIdle) { return true; }
                if (lStateID == STATE_WalkPivot180L) { return true; }
                if (lStateID == STATE_WalkPivot180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdlePose) { return true; }
                if (lTransitionID == TRANS_AnyState_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkToIdle) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk90R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk180L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk90L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun90L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun180L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun90R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun180R) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180R) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180L) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkPivot180L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkPivot180R_WalkFwdLoop) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_IdleToWalk) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_RunPivot180L_RDown) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_RunPivot180L_LDown) { return true; }
            if (rStateID == STATE_RunPivot180R_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_WalkToIdle) { return true; }
            if (rStateID == STATE_WalkPivot180L) { return true; }
            if (rStateID == STATE_WalkPivot180R) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdleToWalk) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_RunPivot180L_RDown) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_RunPivot180L_LDown) { return true; }
            if (rStateID == STATE_RunPivot180R_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_WalkToIdle) { return true; }
            if (rStateID == STATE_WalkPivot180L) { return true; }
            if (rStateID == STATE_WalkPivot180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdlePose) { return true; }
            if (rTransitionID == TRANS_AnyState_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkToIdle) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk90R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk180L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk90L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun90L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun180L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun90R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun180R) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180R) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180L) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkPivot180L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkPivot180R_WalkFwdLoop) { return true; }
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
            TRANS_EntryState_IdleTurn20R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn20R");
            TRANS_AnyState_IdleTurn20R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn20R");
            TRANS_EntryState_IdleTurn90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn90R");
            TRANS_AnyState_IdleTurn90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn90R");
            TRANS_EntryState_IdleTurn180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn180R");
            TRANS_AnyState_IdleTurn180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn180R");
            TRANS_EntryState_IdleTurn20L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn20L");
            TRANS_AnyState_IdleTurn20L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn20L");
            TRANS_EntryState_IdleTurn90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn90L");
            TRANS_AnyState_IdleTurn90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn90L");
            TRANS_EntryState_IdleTurn180L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleTurn180L");
            TRANS_AnyState_IdleTurn180L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleTurn180L");
            TRANS_EntryState_IdlePose = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_AnyState_IdlePose = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_EntryState_RunFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_AnyState_RunFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_EntryState_WalkFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_AnyState_WalkFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_EntryState_IdleToWalk = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleToWalk");
            TRANS_AnyState_IdleToWalk = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleToWalk");
            TRANS_EntryState_IdleToRun = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot-SM.IdleToRun");
            TRANS_AnyState_IdleToRun = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot-SM.IdleToRun");
            STATE_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk");
            TRANS_IdleToWalk_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleToWalk_WalkToIdle = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk -> Base Layer.WalkRunPivot-SM.WalkToIdle");
            STATE_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun");
            TRANS_IdleToRun_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_IdleToRun_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            STATE_IdleTurn90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90L");
            TRANS_IdleTurn90L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90L -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleTurn90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90L -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_IdleTurn90L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90L -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleTurn180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180L");
            TRANS_IdleTurn180L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180L -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleTurn180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180L -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_IdleTurn180L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180L -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90L");
            TRANS_IdleToWalk90L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90L -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleToWalk90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90L -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180L");
            TRANS_IdleToWalk180L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180L -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleToWalk180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180L -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90L");
            TRANS_IdleToRun90L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90L -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_IdleToRun90L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90L -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            STATE_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180L");
            TRANS_IdleToRun180L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180L -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_IdleToRun180L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180L -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            STATE_IdleTurn90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90R");
            TRANS_IdleTurn90R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90R -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleTurn90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90R -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_IdleTurn90R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn90R -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleTurn180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180R");
            TRANS_IdleTurn180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180R -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_IdleTurn180R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180R -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleTurn180R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn180R -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90R");
            TRANS_IdleToWalk90R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90R -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleToWalk90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk90R -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180R");
            TRANS_IdleToWalk180R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180R -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_IdleToWalk180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToWalk180R -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90R");
            TRANS_IdleToRun90R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90R -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            TRANS_IdleToRun90R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun90R -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180R");
            TRANS_IdleToRun180R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180R -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_IdleToRun180R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleToRun180R -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_IdlePose_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToWalk180R");
            TRANS_IdlePose_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToWalk90R");
            TRANS_IdlePose_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToWalk180L");
            TRANS_IdlePose_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToWalk90L");
            TRANS_IdlePose_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToWalk");
            TRANS_IdlePose_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToRun");
            TRANS_IdlePose_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToRun90L");
            TRANS_IdlePose_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToRun180L");
            TRANS_IdlePose_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToRun90R");
            TRANS_IdlePose_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdlePose -> Base Layer.WalkRunPivot-SM.IdleToRun180R");
            STATE_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop -> Base Layer.WalkRunPivot-SM.WalkToIdle_RDown");
            TRANS_WalkFwdLoop_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop -> Base Layer.WalkRunPivot-SM.WalkToIdle_LDown");
            TRANS_WalkFwdLoop_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop -> Base Layer.WalkRunPivot-SM.WalkPivot180L");
            TRANS_WalkFwdLoop_WalkPivot180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkFwdLoop -> Base Layer.WalkRunPivot-SM.WalkPivot180R");
            STATE_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_RunFwdLoop_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunStop_RDown");
            TRANS_RunFwdLoop_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunStop_LDown");
            TRANS_RunFwdLoop_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunPivot180L_RDown");
            TRANS_RunFwdLoop_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunPivot180R_LDown");
            TRANS_RunFwdLoop_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunPivot180L_LDown");
            TRANS_RunFwdLoop_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunFwdLoop -> Base Layer.WalkRunPivot-SM.RunPivot180R_RDown");
            STATE_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180L_RDown");
            TRANS_RunPivot180L_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180L_RDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180R_LDown");
            TRANS_RunPivot180R_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180R_LDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_RDown");
            TRANS_WalkToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_RDown -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_WalkToIdle_RDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_RDown -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_WalkToIdle_RDown_WalkPivot180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_RDown -> Base Layer.WalkRunPivot-SM.WalkPivot180R");
            STATE_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_LDown");
            TRANS_WalkToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_LDown -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_WalkToIdle_LDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_LDown -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            TRANS_WalkToIdle_LDown_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle_LDown -> Base Layer.WalkRunPivot-SM.WalkPivot180L");
            STATE_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_RDown");
            TRANS_RunStop_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_RDown -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_RunStop_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_RDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_RunStop_RDown_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_RDown -> Base Layer.WalkRunPivot-SM.RunPivot180R_LDown");
            STATE_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_LDown");
            TRANS_RunStop_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_LDown -> Base Layer.WalkRunPivot-SM.IdlePose");
            TRANS_RunStop_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_LDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            TRANS_RunStop_LDown_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunStop_LDown -> Base Layer.WalkRunPivot-SM.RunPivot180R_RDown");
            STATE_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180L_LDown");
            TRANS_RunPivot180L_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180L_LDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180R_RDown");
            TRANS_RunPivot180R_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.RunPivot180R_RDown -> Base Layer.WalkRunPivot-SM.RunFwdLoop");
            STATE_IdleTurn20R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn20R");
            TRANS_IdleTurn20R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn20R -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_IdleTurn20L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn20L");
            TRANS_IdleTurn20L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.IdleTurn20L -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_WalkToIdle = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle");
            TRANS_WalkToIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkToIdle -> Base Layer.WalkRunPivot-SM.IdlePose");
            STATE_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkPivot180L");
            TRANS_WalkPivot180L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkPivot180L -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
            STATE_WalkPivot180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkPivot180R");
            TRANS_WalkPivot180R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot-SM.WalkPivot180R -> Base Layer.WalkRunPivot-SM.WalkFwdLoop");
        }

#if UNITY_EDITOR

        private AnimationClip mIdleToWalk = null;
        private AnimationClip mIdleToRun = null;
        private AnimationClip mIdleTurn90L = null;
        private AnimationClip mIdleTurn180L = null;
        private AnimationClip mIdleToWalk90L = null;
        private AnimationClip mIdleToWalk180L = null;
        private AnimationClip mIdleToRun90L = null;
        private AnimationClip mIdleToRun180L = null;
        private AnimationClip mIdleTurn90R = null;
        private AnimationClip mIdleTurn180R = null;
        private AnimationClip mIdleToWalk90R = null;
        private AnimationClip mIdleToWalk180R = null;
        private AnimationClip mIdleToRun90R = null;
        private AnimationClip mIdleToRun180R = null;
        private AnimationClip mIdlePose = null;
        private AnimationClip mWalkForward = null;
        private AnimationClip mRunForward = null;
        private AnimationClip mRunPivot180L_RDown = null;
        private AnimationClip mRunPivot180R_LDown = null;
        private AnimationClip mWalkToIdle_RDown = null;
        private AnimationClip mWalkToIdle_LDown = null;
        private AnimationClip mRunToIdle_RDown = null;
        private AnimationClip mRunToIdle_LDown = null;
        private AnimationClip mIdleTurn20R = null;
        private AnimationClip mIdleTurn20L = null;
        private AnimationClip mWalkPivot180L = null;
        private AnimationClip mWalkPivot180R = null;

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

            UnityEditor.Animations.AnimatorState lIdleToWalk = lMotionStateMachine.AddState("IdleToWalk", new Vector3(-420, 216, 0));
            lIdleToWalk.motion = mIdleToWalk;
            lIdleToWalk.speed = 1.25f;

            UnityEditor.Animations.AnimatorState lIdleToRun = lMotionStateMachine.AddState("IdleToRun", new Vector3(348, 216, 0));
            lIdleToRun.motion = mIdleToRun;
            lIdleToRun.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn90L = lMotionStateMachine.AddState("IdleTurn90L", new Vector3(-204, 36, 0));
            lIdleTurn90L.motion = mIdleTurn90L;
            lIdleTurn90L.speed = 1.6f;

            UnityEditor.Animations.AnimatorState lIdleTurn180L = lMotionStateMachine.AddState("IdleTurn180L", new Vector3(-264, 108, 0));
            lIdleTurn180L.motion = mIdleTurn180L;
            lIdleTurn180L.speed = 1.4f;

            UnityEditor.Animations.AnimatorState lIdleToWalk90L = lMotionStateMachine.AddState("IdleToWalk90L", new Vector3(-420, 276, 0));
            lIdleToWalk90L.motion = mIdleToWalk90L;
            lIdleToWalk90L.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToWalk180L = lMotionStateMachine.AddState("IdleToWalk180L", new Vector3(-420, 336, 0));
            lIdleToWalk180L.motion = mIdleToWalk180L;
            lIdleToWalk180L.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToRun90L = lMotionStateMachine.AddState("IdleToRun90L", new Vector3(348, 276, 0));
            lIdleToRun90L.motion = mIdleToRun90L;
            lIdleToRun90L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun180L = lMotionStateMachine.AddState("IdleToRun180L", new Vector3(348, 336, 0));
            lIdleToRun180L.motion = mIdleToRun180L;
            lIdleToRun180L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn90R = lMotionStateMachine.AddState("IdleTurn90R", new Vector3(264, 36, 0));
            lIdleTurn90R.motion = mIdleTurn90R;
            lIdleTurn90R.speed = 1.6f;

            UnityEditor.Animations.AnimatorState lIdleTurn180R = lMotionStateMachine.AddState("IdleTurn180R", new Vector3(348, 120, 0));
            lIdleTurn180R.motion = mIdleTurn180R;
            lIdleTurn180R.speed = 1.4f;

            UnityEditor.Animations.AnimatorState lIdleToWalk90R = lMotionStateMachine.AddState("IdleToWalk90R", new Vector3(-420, 396, 0));
            lIdleToWalk90R.motion = mIdleToWalk90R;
            lIdleToWalk90R.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToWalk180R = lMotionStateMachine.AddState("IdleToWalk180R", new Vector3(-420, 456, 0));
            lIdleToWalk180R.motion = mIdleToWalk180R;
            lIdleToWalk180R.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToRun90R = lMotionStateMachine.AddState("IdleToRun90R", new Vector3(348, 396, 0));
            lIdleToRun90R.motion = mIdleToRun90R;
            lIdleToRun90R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun180R = lMotionStateMachine.AddState("IdleToRun180R", new Vector3(396, 480, 0));
            lIdleToRun180R.motion = mIdleToRun180R;
            lIdleToRun180R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(24, 276, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkFwdLoop = lMotionStateMachine.AddState("WalkFwdLoop", new Vector3(-108, 588, 0));
            lWalkFwdLoop.motion = mWalkForward;
            lWalkFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunFwdLoop = lMotionStateMachine.AddState("RunFwdLoop", new Vector3(228, 588, 0));
            lRunFwdLoop.motion = mRunForward;
            lRunFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180L_RDown = lMotionStateMachine.AddState("RunPivot180L_RDown", new Vector3(36, 732, 0));
            lRunPivot180L_RDown.motion = mRunPivot180L_RDown;
            lRunPivot180L_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180R_LDown = lMotionStateMachine.AddState("RunPivot180R_LDown", new Vector3(576, 792, 0));
            lRunPivot180R_LDown.motion = mRunPivot180R_LDown;
            lRunPivot180R_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_RDown = lMotionStateMachine.AddState("WalkToIdle_RDown", new Vector3(-528, 636, 0));
            lWalkToIdle_RDown.motion = mWalkToIdle_RDown;
            lWalkToIdle_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_LDown = lMotionStateMachine.AddState("WalkToIdle_LDown", new Vector3(-564, 588, 0));
            lWalkToIdle_LDown.motion = mWalkToIdle_LDown;
            lWalkToIdle_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_RDown = lMotionStateMachine.AddState("RunStop_RDown", new Vector3(624, 672, 0));
            lRunStop_RDown.motion = mRunToIdle_RDown;
            lRunStop_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_LDown = lMotionStateMachine.AddState("RunStop_LDown", new Vector3(588, 588, 0));
            lRunStop_LDown.motion = mRunToIdle_LDown;
            lRunStop_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180L_LDown = lMotionStateMachine.AddState("RunPivot180L_LDown", new Vector3(120, 792, 0));
            lRunPivot180L_LDown.motion = mRunPivot180R_LDown;
            lRunPivot180L_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180R_RDown = lMotionStateMachine.AddState("RunPivot180R_RDown", new Vector3(348, 792, 0));
            lRunPivot180R_RDown.motion = mRunPivot180L_RDown;
            lRunPivot180R_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn20R = lMotionStateMachine.AddState("IdleTurn20R", new Vector3(180, -48, 0));
            lIdleTurn20R.motion = mIdleTurn20R;
            lIdleTurn20R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn20L = lMotionStateMachine.AddState("IdleTurn20L", new Vector3(-120, -48, 0));
            lIdleTurn20L.motion = mIdleTurn20L;
            lIdleTurn20L.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle = lMotionStateMachine.AddState("WalkToIdle", new Vector3(-480, 120, 0));
            lWalkToIdle.motion = mWalkToIdle_LDown;
            lWalkToIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkPivot180L = lMotionStateMachine.AddState("WalkPivot180L", new Vector3(-348, 768, 0));
            lWalkPivot180L.motion = mWalkPivot180L;
            lWalkPivot180L.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkPivot180R = lMotionStateMachine.AddState("WalkPivot180R", new Vector3(-300, 816, 0));
            lWalkPivot180R.motion = mWalkPivot180R;
            lWalkPivot180R.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn20R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0.5f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn90R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0.2098995f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn180R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.09999996f;
            lAnyStateTransition.offset = 0.2566045f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn20L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 1.038159E-08f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0.5f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn90L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 3.24996E-08f;
            lAnyStateTransition.duration = 0.07699827f;
            lAnyStateTransition.offset = 0.122026f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn180L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 1.64697E-08f;
            lAnyStateTransition.duration = 0.09999998f;
            lAnyStateTransition.offset = 0.2566045f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdlePose);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = true;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lRunFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.1982508f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27115f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lWalkFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 1.068461E-07f;
            lAnyStateTransition.duration = 0.2f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27114f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToWalk);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05222222f;
            lAnyStateTransition.offset = 0.2523485f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToRun);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.5975192f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0.2245581f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lIdleToWalk.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6721453f;
            lStateTransition.duration = 0.04795915f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToWalk.AddTransition(lWalkToIdle);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6352268f;
            lStateTransition.duration = 0.1901305f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9683823f;
            lStateTransition.duration = 0.0316177f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.827f;
            lStateTransition.duration = 0.173888f;
            lStateTransition.offset = 0.6072354f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.04999999f;
            lStateTransition.duration = 0.1313883f;
            lStateTransition.offset = 0.3069344f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleTurn90L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5558147f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.4533213f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5634806f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5038666f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6944552f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.5630877f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.628346f;
            lStateTransition.duration = 0.2500001f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToWalk90L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6994989f;
            lStateTransition.duration = 0.06340849f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToWalk90L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.95f;
            lStateTransition.duration = 0.15f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7888889f;
            lStateTransition.duration = 0.04691353f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToWalk180L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7222222f;
            lStateTransition.duration = 0.2777778f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun90L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.949241f;
            lStateTransition.duration = 0.05075897f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun90L.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9012346f;
            lStateTransition.duration = 0.09876539f;
            lStateTransition.offset = 0.1933722f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.951137f;
            lStateTransition.duration = 0.048863f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180L.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8279572f;
            lStateTransition.duration = 0.09216576f;
            lStateTransition.offset = 0.5614038f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5524217f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5524217f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0.5217395f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6789519f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.0366848f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6789519f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0.5351971f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToWalk90R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9685065f;
            lStateTransition.duration = 0.102564f;
            lStateTransition.offset = 0.4421507f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToWalk90R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8026316f;
            lStateTransition.duration = 0.1973684f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9472222f;
            lStateTransition.duration = 0.05277774f;
            lStateTransition.offset = 0.4514289f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToWalk180R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8214286f;
            lStateTransition.duration = 0.1785714f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun90R.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.874892f;
            lStateTransition.duration = 0.125108f;
            lStateTransition.offset = 0.3181282f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun90R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9508197f;
            lStateTransition.duration = 0.0491803f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9530699f;
            lStateTransition.duration = 0.04693014f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180R.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 3.661065E-08f;
            lStateTransition.duration = 0.8589287f;
            lStateTransition.offset = 0.226401f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk90R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 2.895065f;
            lStateTransition.offset = 0.2517471f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk180L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.870536f;
            lStateTransition.offset = 0.2945986f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk90L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1.348813E-08f;
            lStateTransition.duration = 2.718084f;
            lStateTransition.offset = 0.09497488f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8421719f;
            lStateTransition.duration = 2f;
            lStateTransition.offset = 0.1284909f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.1030876f;
            lStateTransition.duration = 1.958004f;
            lStateTransition.offset = 0.2155401f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun90L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.8500001f;
            lStateTransition.offset = 0.1290736f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun180L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.9000003f;
            lStateTransition.offset = 0.2096772f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun90R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 2.300917E-08f;
            lStateTransition.duration = 0.901875f;
            lStateTransition.offset = 0.3279541f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.8819677f;
            lStateTransition.offset = 0.3309544f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4446053f;
            lStateTransition.duration = 0.1181976f;
            lStateTransition.offset = 0.9770986f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8021039f;
            lStateTransition.duration = 0.09642864f;
            lStateTransition.offset = 0.3232313f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27120f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3192836f;
            lStateTransition.duration = 0.09642851f;
            lStateTransition.offset = 0.3287362f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27121f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8185452f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.3509565f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4823796f;
            lStateTransition.duration = 0.1004542f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.04187637f;
            lStateTransition.duration = 0.1814208f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7450274f;
            lStateTransition.duration = 0.1604167f;
            lStateTransition.offset = 0.1113f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.1434378f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = true;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.06841651f;
            lStateTransition.duration = 0.06699998f;
            lStateTransition.offset = 0.4146295f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27120f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.07847699f;
            lStateTransition.duration = 0.07207506f;
            lStateTransition.offset = 0.5575527f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.07911026f;
            lStateTransition.duration = 0.2760905f;
            lStateTransition.offset = 0.6348336f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4535715f;
            lStateTransition.duration = 0.09285709f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7753307f;
            lStateTransition.duration = 0.2246693f;
            lStateTransition.offset = 0.277193f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180L_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.53f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180L_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.53f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8352495f;
            lStateTransition.duration = 0.3260869f;
            lStateTransition.offset = 0.2921261f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.08155745f;
            lStateTransition.duration = 0.2500001f;
            lStateTransition.offset = 0.3607909f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4214758f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunPivot180L_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.09999996f;
            lStateTransition.offset = 0.06979182f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunPivot180R_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9547194f;
            lStateTransition.duration = 0.04528057f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkToIdle_RDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8006474f;
            lStateTransition.duration = 0.3412638f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5057101f;
            lStateTransition.duration = 0.1875f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkPivot180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkPivot180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8f;
            lStateTransition.duration = 0.3f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.2970681f;
            lStateTransition.duration = 0.1630434f;
            lStateTransition.offset = 0.4911524f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkPivot180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkPivot180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8687333f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunStop_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4475232f;
            lStateTransition.duration = 0.1973684f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3613918f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3613918f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7665359f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunStop_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4860786f;
            lStateTransition.duration = 0.1217646f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3564995f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.08370973f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3564995f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.03969417f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunPivot180L_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.09999999f;
            lStateTransition.offset = 0.1781252f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunPivot180R_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.08214277f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleTurn20R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleTurn20L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkToIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8850076f;
            lStateTransition.duration = 0.1149924f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkPivot180L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8849771f;
            lStateTransition.duration = 0.1766388f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkPivot180R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8311951f;
            lStateTransition.duration = 0.1224689f;
            lStateTransition.offset = 0.2035105f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mIdleToWalk = CreateAnimationField("IdleToWalk", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkForward_NtrlFaceFwd.fbx/IdleToWalk.anim", "IdleToWalk", mIdleToWalk);
            mIdleToRun = CreateAnimationField("IdleToRun", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/IdleToRun.anim", "IdleToRun", mIdleToRun);
            mIdleTurn90L = CreateAnimationField("IdleTurn90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90L.anim", "IdleTurn90L", mIdleTurn90L);
            mIdleTurn180L = CreateAnimationField("IdleTurn180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180L.anim", "IdleTurn180L", mIdleTurn180L);
            mIdleToWalk90L = CreateAnimationField("IdleToWalk90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/IdleToWalk90L.anim", "IdleToWalk90L", mIdleToWalk90L);
            mIdleToWalk180L = CreateAnimationField("IdleToWalk180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/IdleToWalk180L.anim", "IdleToWalk180L", mIdleToWalk180L);
            mIdleToRun90L = CreateAnimationField("IdleToRun90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_AllAngles.fbx/IdleToRun90L.anim", "IdleToRun90L", mIdleToRun90L);
            mIdleToRun180L = CreateAnimationField("IdleToRun180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_AllAngles.fbx/IdleToRun180L.anim", "IdleToRun180L", mIdleToRun180L);
            mIdleTurn90R = CreateAnimationField("IdleTurn90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90R.anim", "IdleTurn90R", mIdleTurn90R);
            mIdleTurn180R = CreateAnimationField("IdleTurn180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180R.anim", "IdleTurn180R", mIdleTurn180R);
            mIdleToWalk90R = CreateAnimationField("IdleToWalk90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/IdleToWalk90R.anim", "IdleToWalk90R", mIdleToWalk90R);
            mIdleToWalk180R = CreateAnimationField("IdleToWalk180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/IdleToWalk180R.anim", "IdleToWalk180R", mIdleToWalk180R);
            mIdleToRun90R = CreateAnimationField("IdleToRun90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_AllAngles.fbx/IdleToRun90R.anim", "IdleToRun90R", mIdleToRun90R);
            mIdleToRun180R = CreateAnimationField("IdleToRun180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_AllAngles.fbx/IdleToRun180R.anim", "IdleToRun180R", mIdleToRun180R);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);
            mWalkForward = CreateAnimationField("WalkFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD.fbx/WalkForward.anim", "WalkForward", mWalkForward);
            mRunForward = CreateAnimationField("RunFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/RunForward.anim", "RunForward", mRunForward);
            mRunPivot180L_RDown = CreateAnimationField("RunPivot180L_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_1.fbx/RunPivot180L_RDown.anim", "RunPivot180L_RDown", mRunPivot180L_RDown);
            mRunPivot180R_LDown = CreateAnimationField("RunPivot180R_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_1.fbx/RunPivot180R_LDown.anim", "RunPivot180R_LDown", mRunPivot180R_LDown);
            mWalkToIdle_RDown = CreateAnimationField("WalkToIdle_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkToIdle_RDown.anim", "WalkToIdle_RDown", mWalkToIdle_RDown);
            mWalkToIdle_LDown = CreateAnimationField("WalkToIdle_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkToIdle_LDown.anim", "WalkToIdle_LDown", mWalkToIdle_LDown);
            mRunToIdle_RDown = CreateAnimationField("RunStop_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_HalfSteps2Idle_PasingLongStepTOIdle.fbx/RunToIdle_RDown.anim", "RunToIdle_RDown", mRunToIdle_RDown);
            mRunToIdle_LDown = CreateAnimationField("RunStop_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_2.fbx/RunToIdle_LDown.anim", "RunToIdle_LDown", mRunToIdle_LDown);
            mIdleTurn20R = CreateAnimationField("IdleTurn20R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn20R.anim", "IdleTurn20R", mIdleTurn20R);
            mIdleTurn20L = CreateAnimationField("IdleTurn20L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn20L.anim", "IdleTurn20L", mIdleTurn20L);
            mWalkPivot180L = CreateAnimationField("WalkPivot180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkPivot180L.anim", "WalkPivot180L", mWalkPivot180L);
            mWalkPivot180R = CreateAnimationField("WalkPivot180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkPivot180R.anim", "WalkPivot180R", mWalkPivot180R);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
