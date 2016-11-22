using UnityEngine;
using com.ootii.Cameras;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Idle motion for when the character is just standing and waiting
    /// for input or some interaction.
    /// </summary>
    [MotionName("Walk Run Rotate (old)")]
    [MotionDescription("Simple walking motion that keeps the character facing forward, but rotates with the 'a' and 'd' keys.")]
    public class WalkRunRotate : MotionControllerMotion, IWalkRunMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1700;
        public const int PHASE_START_RUN = 1710;
        public const int PHASE_START_SHORTCUT_WALK = 1714;
        public const int PHASE_START_SHORTCUT_RUN = 1715;

        public const int PHASE_STOP_RIGHT_DOWN = 1720;
        public const int PHASE_STOP_LEFT_DOWN = 1721;

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
        /// User layer id set for objects that are climbable.
        /// </summary>
        public string _RotateActionAlias = "ActivateRotation";
        public string RotateActionAlias
        {
            get { return _RotateActionAlias; }
            set { _RotateActionAlias = value; }
        }

        /// <summary>
        /// Alias use to activate strafing
        /// </summary>
        public string _StrafeLeftActionAlias = "StrafeLeft";
        public string StrafeLeftActionAlias
        {
            get { return _StrafeLeftActionAlias; }
            set { _StrafeLeftActionAlias = value; }
        }

        /// <summary>
        /// Alias use to activate strafing
        /// </summary>
        public string _StrafeRightActionAlias = "StrafeRight";
        public string StrafeRightActionAlias
        {
            get { return _StrafeRightActionAlias; }
            set { _StrafeRightActionAlias = value; }
        }

        /// <summary>
        /// Determines if the mouse's horizontal movement is used to
        /// rotate the character.
        /// </summary>
        public bool _RotateWithViewInputX = true;
        public bool RotateWithViewInputX
        {
            get { return _RotateWithViewInputX; }
            set { _RotateWithViewInputX = value; }
        }

        /// <summary>
        /// Determines if the "horizontal" movement is used to
        /// rotate the character.
        /// </summary>
        public bool _RotateWithMovementInputX = false;
        public bool RotateWithMovementInputX
        {
            get { return _RotateWithMovementInputX; }
            set { _RotateWithMovementInputX = value; }
        }

        /// <summary>
        /// Determines if we'll attempt to force the camera to view 
        /// or forward direction... even if it's an orbit camera
        /// </summary>
        public bool _ForceViewOnInput = false;
        public bool ForceViewOnInput
        {
            get { return _ForceViewOnInput; }
            set { _ForceViewOnInput = value; }
        }

        /// <summary>
        /// Desired degrees of rotation per second
        /// </summary>
        public float _RotationSpeed = 120f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }

            set
            {
                _RotationSpeed = value;
                mDegreesPer60FPSTick = _RotationSpeed / 60f;
            }
        }

        /// <summary>
        /// Used to apply some smoothing to the mouse movement
        /// </summary>
        public float _RotationSmoothing = 0.1f;
        public virtual float RotationSmoothing
        {
            get { return _RotationSmoothing; }
            set { _RotationSmoothing = value; }
        }

        /// <summary>
        /// Desired degrees of rotation per second
        /// </summary>
        public float _RotationToViewSpeed = 360f;
        public float RotationToViewSpeed
        {
            get { return _RotationToViewSpeed; }
            set { _RotationToViewSpeed = value; }
        }

        /// <summary>
        /// Determines if we remove the side-to-side swaying or lateral movement during the walk cycle
        /// </summary>
        public bool _RemoveLateralMovement = true;
        public bool RemoveLateralMovement
        {
            get { return _RemoveLateralMovement; }
            set { _RemoveLateralMovement = value; }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in the loop
        /// </summary>
        private bool mStartInMove = false;
        public bool StartInMove
        {
            get { return mStartInMove; }
            set { mStartInMove = value; }
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
        /// Determines if we'll force our rotation to match the camera view
        /// </summary>
        protected bool mForceRotationToView = false;

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Fields to help smooth out the mouse rotation
        /// </summary>
        protected float mYaw = 0f;
        protected float mYawTarget = 0f;
        protected float mYawVelocity = 0f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkRunRotate()
            : base()
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunRotate-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunRotate(MotionController rController)
            : base(rController)
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunRotate-SM"; }
#endif
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Default the speed we'll use to rotate
            mDegreesPer60FPSTick = _RotationSpeed / 60f;
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable) { return false; }
            if (!mMotionController.IsGrounded) { return false; }

            if (mMotionController._InputSource == null) { return false; }

            if (mMotionController._InputSource.MovementY == 0f &&
                !mMotionController._InputSource.IsPressed(_StrafeLeftActionAlias) &&
                !mMotionController._InputSource.IsPressed(_StrafeRightActionAlias))
            {
                return false;
            }

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

            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // If we're not in the normal traveral state, stop
            //if (lState.Stance != EnumControllerStance.TRAVERSAL) { return false; }

            // If we're in the idle state with no movement, stop
            if (lStateID == STATE_IdlePose && lTransitionID == 0)
            {
                if (mMotionController._InputSource.MovementY == 0f &&
                    !mMotionController._InputSource.IsPressed(_StrafeLeftActionAlias) &&
                    !mMotionController._InputSource.IsPressed(_StrafeRightActionAlias))
                {
                    return false;
                }
            }

            // If we're in the idle state machine, we can get out
            if (mAge > 0.2f && lStateID == Idle.STATE_IdlePose)
            {
                if (lTransitionID != TRANS_AnyState_IdlePose)
                {
                    //return false;
                }
            }

            // One last check to make sure we're in this state

            // TRT 4/15/2016 - Having the other conditions below isn't really needed
            if (!IsInMotionState)
            {
                return false;
            }

            //if (mIsAnimatorActive && !IsInMotionState && !mStartInRun && !mStartInWalk)
            //{
                //// If the animator actually registered our activation, continue
                //if (lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].AutoClearMotionPhaseReady)
                //{
                //    // This is a bit painful, but make sure we're not in a 
                //    // transition to this sub-state machine
                //    if (lTransitionID != TRANS_EntryState_IdlePose)
                //    {
                //return false;
                //    }
                //}
            //}

            // Stay
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Reset the yaw info for smoothing
            mYaw = 0f;
            mYawTarget = 0f;
            mYawVelocity = 0f;

            // Determine if we need to rotate to the view. We do this test
            // when the activator first triggers
            if (mMotionController._InputSource.IsPressed(_RotateActionAlias))
            {
                if (mMotionController._CameraTransform != null)
                {
                    float lAngle = NumberHelper.GetHorizontalAngle(mActorController._Transform.forward, mMotionController._CameraTransform.forward, mActorController._Transform.up);
                    mForceRotationToView = (Mathf.Abs(lAngle) > 1f);
                }
            }

            // Determine our start state
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
                // Determine if we're walking or running
                // In order to swap from walking to running, we're going to modify the state value some.
                bool lIsRunActivated = ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
                if (!lIsRunActivated)
                {
                    MotionState lState = mMotionController.State;

                    lState.InputY = lState.InputY * 0.5f;
                    if (lState.InputMagnitudeTrend.Value > 0.5f) { lState.InputMagnitudeTrend.Replace(0.5f); }

                    mMotionController.State = lState;
                }
                else
                {
                    MotionState lState = mMotionController.State;
                    float lSign = (lState.InputFromAvatarAngle < -90f || lState.InputFromAvatarAngle > 90f ? -1 : 1);

                    lState.InputMagnitudeTrend.Value = lSign * 1f;
                    lState.InputY = lSign * 1f;

                    mMotionController.State = lState;
                }

                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);
            }

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
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
            // Remove any side-to-side sway
            if (_RemoveLateralMovement)
            {
                AnimatorStateInfo lStateInfo = mMotionController.State.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo;
                int lStateID = lStateInfo.fullPathHash;

                if (lStateID == STATE_WalkFwdLoop ||
                    lStateID == STATE_WalkToIdle ||
                    lStateID == STATE_WalkToIdle_LDown ||
                    lStateID == STATE_WalkToIdle_RDown ||
                    lStateID == STATE_RunFwdLoop ||
                    lStateID == STATE_WalkBackward)
                {
                    rMovement.x = 0f;
                }
            }

            // No automatic rotation in this motion
            rRotation = Quaternion.identity;
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;

            // We only want to process on the first update iteration
            // of each frame. Some motions may be different, but this one is easy
            if (rUpdateIndex < 1) { return; }

            // Determines if we need to update the state itself
            bool lUpdateAnimatorState = false;

            // Grab the state info
            MotionState lState = mMotionController.State;
            int lStateMotionPhase = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionPhase;
            int lStateMotionParameter = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter;

            AnimatorStateInfo lStateInfo = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo;
            int lStateID = lStateInfo.fullPathHash;

            // Just in case, clear the motion phase. We do this because we have instant transitions and we don't 
            // want to re-enter the states.
            if (lStateMotionPhase == PHASE_START ||
                lStateMotionPhase == PHASE_START_RUN)
            {
                if (lStateID == STATE_IdlePose ||
                    lStateID == STATE_IdleToRun)
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
            int lRunActivated = (lIsRunActivated ? 1 : 0);

            if (mMotionController._InputSource.IsPressed(_StrafeLeftActionAlias))
            {
                lStateMotionParameter = 2;

                lUpdateAnimatorState = true;
                lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = lStateMotionParameter;
            }
            else if (mMotionController._InputSource.IsPressed(_StrafeRightActionAlias))
            {
                lStateMotionParameter = 3;

                lUpdateAnimatorState = true;
                lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = lStateMotionParameter;
            }
            else //if (lStateMotionParameter != lRunActivated)
            {
                lStateMotionParameter = lRunActivated;

                lUpdateAnimatorState = true;
                lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = lStateMotionParameter;
            }
            //else
            //{
            //    lUpdateAnimatorState = true;
            //    lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = 0;
            //}

            // If we're starting to walk or run, allow the actor rotation
            if (lStateID == STATE_WalkFwdLoop)
            {
                mStartInWalk = false;

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
            else if (lStateID == STATE_RunFwdLoop)
            {
                mStartInRun = false;
            }

            // If we need to update the animator state, do it once
            if (lUpdateAnimatorState)
            {
                mMotionController.State = lState;
            }

            // Determine if we need to rotate to the view. We do this test
            // when the activator first triggers
            //if (UnityEngine.Input.GetMouseButtonDown(1))
            if (mMotionController._InputSource.IsPressed(_RotateActionAlias))
            {
                if (mMotionController._CameraTransform != null)
                {
                    float lAngle = NumberHelper.GetHorizontalAngle(mActorController._Transform.forward, mMotionController._CameraTransform.forward, mActorController._Transform.up);
                    mForceRotationToView = (Mathf.Abs(lAngle) > 1f);
                }
            }

            // If we're trying to get to the original rotation, keep going
            if (mForceRotationToView)
            {
                GetRotationVelocityWithView(rDeltaTime, ref mAngularVelocity);
            }
            // Determine if the actor rotates as the input is used
            else if (_RotateWithViewInputX || _RotateWithMovementInputX)
            {
                GetRotationVelocityWithInput(rDeltaTime, ref mRotation);
            }
            // Determine if the actor rotates as the view rotates
            else
            {
                GetRotationVelocityWithView(rDeltaTime, ref mAngularVelocity);
            }
        }

        /// <summary>
        /// Create a rotation velocity that rotates the character based on input
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rAngularVelocity"></param>
        private void GetRotationVelocityWithInput(float rDeltaTime, ref Quaternion rRotation)
        {
            // If we don't have an input source, stop
            if (mMotionController._InputSource == null) { return; }

            // Determine this frame's rotation
            float lYawDelta = 0f;
            if (lYawDelta == 0f && _RotateWithMovementInputX) { lYawDelta = mMotionController._InputSource.MovementX; }
            //if (lYawDelta == 0f && _RotateWithViewInputX && UnityEngine.Input.GetMouseButton(1)) { lYawDelta = mMotionController._InputSource.ViewX; }
            if (lYawDelta == 0f && _RotateWithViewInputX && mMotionController._InputSource.IsPressed(_RotateActionAlias)) { lYawDelta = mMotionController._InputSource.ViewX; }

            // TRT 4/6/2016: Added due to some unwanted rotation
            // If there is no rotation and our rotation key isn't active, clear out our settings
            if (lYawDelta == 0f && !mMotionController._InputSource.IsPressed(_RotateActionAlias))
            {
                lYawDelta = 0f;
                mYawVelocity = 0f;
                mYawTarget = mYaw;
            }

            // Apply any delta to our target
            mYawTarget = mYawTarget + lYawDelta;

            // Smooth the rotation
            lYawDelta = (_RotationSmoothing <= 0f ? mYawTarget : Mathf.SmoothDampAngle(mYaw, mYawTarget, ref mYawVelocity, _RotationSmoothing)) - mYaw;
            mYaw = mYaw + lYawDelta;

            // Use this frame's smoothed rotation
            if (lYawDelta != 0f)
            {
                rRotation = Quaternion.Euler(0f, lYawDelta, 0f);

                // If we have a camera, rotate it towards the character
                if (_ForceViewOnInput && mMotionController.CameraRig is BaseCameraRig)
                {
                    ((BaseCameraRig)mMotionController.CameraRig).FrameForceToFollowAnchor = true;
                }
            }

            //float lYaw = 0f;
            //if (lYaw == 0f && _RotateWithMovementInputX) { lYaw = mMotionController._InputSource.MovementX; }
            //if (lYaw == 0f && _RotateWithViewInputX && UnityEngine.Input.GetMouseButton(1)) { lYaw = mMotionController._InputSource.ViewX; }

            //if (lYaw != 0f)
            //{
            //    rRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);

            //    // If we have a camera, rotate it towards the character
            //    if (_ForceViewOnInput && mCameraRig != null)
            //    {
            //        mCameraRig.ForceToFollowAnchor = true;
            //    }
            //}
        }

        /// <summary>
        /// Create a rotation velocity that rotates the character to match the camera's current view.
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rAngularVelocity"></param>
        private void GetRotationVelocityWithView(float rDeltaTime, ref Vector3 rRotationalVelocity)
        {
            if (mMotionController._CameraTransform == null) { return; }

            float lRotationVelocity = 0f;

            // Determine the global direction the character should face
            float lAngle = NumberHelper.GetHorizontalAngle(mActorController._Transform.forward, mMotionController._CameraTransform.forward, mActorController._Transform.up);

            // Test if we need to force a rotation speed for a smooth rotation
            float lRotationSpeed = _RotationToViewSpeed;

            // We want to work our way to the goal smoothly
            if (lAngle > 0f)
            {
                // Rotate instantly
                if (lRotationSpeed == 0f)
                {
                    lRotationVelocity = lAngle / rDeltaTime;
                }
                else
                {
                    // Use the motion's rotation velocity
                    lRotationVelocity = lRotationSpeed;

                    // If we're rotating too much, limit
                    if (lRotationVelocity * rDeltaTime > lAngle)
                    {
                        lRotationVelocity = lAngle / rDeltaTime;
                    }
                }
            }
            else if (lAngle < 0f)
            {
                // Rotate instantly
                if (lRotationSpeed == 0f)
                {
                    lRotationVelocity = lAngle / rDeltaTime;
                }
                // Rotate over time
                else
                {
                    // Use the motion's rotation velocity
                    lRotationVelocity = -lRotationSpeed;

                    // If we're rotating too much, limit
                    if (lRotationVelocity * rDeltaTime < lAngle)
                    {
                        lRotationVelocity = lAngle / rDeltaTime;
                    }
                }
            }
            else
            {
                mForceRotationToView = false;
            }

            rRotationalVelocity = Vector3.up * lRotationVelocity;
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

            if (!InputManagerHelper.IsDefined(_StrafeLeftActionAlias))
            {
                InputManagerEntry lEntry = new InputManagerEntry();
                lEntry.Name = _StrafeLeftActionAlias;
                lEntry.PositiveButton = "q";
                lEntry.Gravity = 1000;
                lEntry.Dead = 0.001f;
                lEntry.Sensitivity = 1000;
                lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
                lEntry.Axis = 0;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);
            }

            if (!InputManagerHelper.IsDefined(_StrafeRightActionAlias))
            {
                InputManagerEntry lEntry = new InputManagerEntry();
                lEntry.Name = _StrafeRightActionAlias;
                lEntry.PositiveButton = "e";
                lEntry.Gravity = 1000;
                lEntry.Dead = 0.001f;
                lEntry.Sensitivity = 1000;
                lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
                lEntry.Axis = 0;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);
            }
        }

        /// <summary>
        /// Allow the constraint to render it's own GUI
        /// </summary>
        /// <returns>Reports if the object's value was changed</returns>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            bool lNewDefaultToRun = EditorGUILayout.Toggle(new GUIContent("Default to Run", "Determines if the default is to run or walk."), DefaultToRun);
            if (lNewDefaultToRun != DefaultToRun)
            {
                lIsDirty = true;
                DefaultToRun = lNewDefaultToRun;
            }

            string lNewRunActionAlias = EditorGUILayout.TextField(new GUIContent("Run Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default)."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewRunActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewRunActionAlias;
            }

            string lNewStrafeLeftActionAlias = EditorGUILayout.TextField(new GUIContent("Left Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default)."), StrafeLeftActionAlias, GUILayout.MinWidth(30));
            if (lNewStrafeLeftActionAlias != StrafeLeftActionAlias)
            {
                lIsDirty = true;
                StrafeLeftActionAlias = lNewStrafeLeftActionAlias;
            }

            string lNewStrafeRightActionAlias = EditorGUILayout.TextField(new GUIContent("Right Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default)."), StrafeRightActionAlias, GUILayout.MinWidth(30));
            if (lNewStrafeRightActionAlias != StrafeRightActionAlias)
            {
                lIsDirty = true;
                StrafeRightActionAlias = lNewStrafeRightActionAlias;
            }

            GUILayout.Space(5f);

            bool lNewRotateWithViewInputX = EditorGUILayout.Toggle(new GUIContent("Rotate on ViewX", "Determines if the actor rotates based on the ViewX input."), RotateWithViewInputX);
            if (lNewRotateWithViewInputX != RotateWithViewInputX)
            {
                lIsDirty = true;
                RotateWithViewInputX = lNewRotateWithViewInputX;
            }

            string lNewRotateActionAlias = EditorGUILayout.TextField(new GUIContent("Rotate Action Alias", "Action alias that is required to use the ViewX value for rotation."), RotateActionAlias, GUILayout.MinWidth(30));
            if (lNewRotateActionAlias != RotateActionAlias)
            {
                lIsDirty = true;
                RotateActionAlias = lNewRotateActionAlias;
            }

            float lNewRotationToViewSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation to Rig Speed", "Degrees per second to rotate towards the camera view."), RotationToViewSpeed, GUILayout.MinWidth(30));
            if (lNewRotationToViewSpeed != RotationToViewSpeed)
            {
                lIsDirty = true;
                RotationToViewSpeed = lNewRotationToViewSpeed;
            }

            GUILayout.Space(5f);

            bool lNewRotateWithMovementInputX = EditorGUILayout.Toggle(new GUIContent("Rotate on MoveX", "Determines if the actor rotates based on the input."), RotateWithMovementInputX);
            if (lNewRotateWithMovementInputX != RotateWithMovementInputX)
            {
                lIsDirty = true;
                RotateWithMovementInputX = lNewRotateWithMovementInputX;
            }

            float lNewRotationVelocity = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second to rotate."), RotationSpeed, GUILayout.MinWidth(30));
            if (lNewRotationVelocity != RotationSpeed)
            {
                lIsDirty = true;
                RotationSpeed = lNewRotationVelocity;
            }

            float lNewRotationSmoothing = EditorGUILayout.FloatField(new GUIContent("Rotation Smoothing", ""), RotationSmoothing);
            if (lNewRotationSmoothing != RotationSmoothing)
            {
                lIsDirty = true;
                RotationSmoothing = lNewRotationSmoothing;
            }

            bool lNewForceViewOnInput = EditorGUILayout.Toggle(new GUIContent("Force View", "Determines we force the camera view to look in the direction of the actor when MovementX has a value."), ForceViewOnInput);
            if (lNewForceViewOnInput != ForceViewOnInput)
            {
                lIsDirty = true;
                ForceViewOnInput = lNewForceViewOnInput;
            }

            GUILayout.Space(5f);

            bool lNewRemoveLateralMovement = EditorGUILayout.Toggle(new GUIContent("Remove Lateral Movement", "When no side-to-side input is found, remove any side-to-side root motion."), _RemoveLateralMovement);
            if (lNewRemoveLateralMovement != _RemoveLateralMovement)
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
        public static int TRANS_EntryState_RunFwdLoop = -1;
        public static int TRANS_AnyState_RunFwdLoop = -1;
        public static int TRANS_EntryState_WalkFwdLoop = -1;
        public static int TRANS_AnyState_WalkFwdLoop = -1;
        public static int TRANS_EntryState_IdlePose = -1;
        public static int TRANS_AnyState_IdlePose = -1;
        public static int TRANS_EntryState_IdleToRun = -1;
        public static int TRANS_AnyState_IdleToRun = -1;
        public static int STATE_IdleToWalk = -1;
        public static int TRANS_IdleToWalk_WalkToIdle = -1;
        public static int TRANS_IdleToWalk_WalkFwdLoop = -1;
        public static int STATE_IdleToRun = -1;
        public static int TRANS_IdleToRun_RunFwdLoop = -1;
        public static int TRANS_IdleToRun_RunStop_LDown = -1;
        public static int TRANS_IdleToRun_WalkFwdLoop = -1;
        public static int STATE_RunFwdLoop = -1;
        public static int TRANS_RunFwdLoop_RunStop_RDown = -1;
        public static int TRANS_RunFwdLoop_RunStop_LDown = -1;
        public static int TRANS_RunFwdLoop_WalkFwdLoop = -1;
        public static int STATE_WalkToIdle_RDown = -1;
        public static int TRANS_WalkToIdle_RDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_RDown_WalkFwdLoop = -1;
        public static int STATE_WalkToIdle_LDown = -1;
        public static int TRANS_WalkToIdle_LDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_LDown_WalkFwdLoop = -1;
        public static int STATE_RunStop_RDown = -1;
        public static int TRANS_RunStop_RDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_RDown_IdlePose = -1;
        public static int STATE_RunStop_LDown = -1;
        public static int TRANS_RunStop_LDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_LDown_IdlePose = -1;
        public static int STATE_WalkToIdle = -1;
        public static int TRANS_WalkToIdle_IdlePose = -1;
        public static int STATE_WalkFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_WalkBackward = -1;
        public static int TRANS_WalkFwdLoop_WalkLeft = -1;
        public static int TRANS_WalkFwdLoop_WalkRight = -1;
        public static int TRANS_WalkFwdLoop_RunFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_LDown = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_RDown = -1;
        public static int STATE_WalkBackward = -1;
        public static int TRANS_WalkBackward_WalkFwdLoop = -1;
        public static int TRANS_WalkBackward_IdlePose = -1;
        public static int TRANS_WalkBackward_WalkLeft = -1;
        public static int TRANS_WalkBackward_WalkRight = -1;
        public static int STATE_IdlePose = -1;
        public static int TRANS_IdlePose_WalkBackward = -1;
        public static int TRANS_IdlePose_WalkLeft = -1;
        public static int TRANS_IdlePose_WalkRight = -1;
        public static int TRANS_IdlePose_IdleToWalk = -1;
        public static int TRANS_IdlePose_IdleToRun = -1;
        public static int STATE_WalkLeft = -1;
        public static int TRANS_WalkLeft_WalkBackward = -1;
        public static int TRANS_WalkLeft_WalkFwdLoop = -1;
        public static int TRANS_WalkLeft_WalkRight = -1;
        public static int TRANS_WalkLeft_IdlePose = -1;
        public static int STATE_WalkRight = -1;
        public static int TRANS_WalkRight_WalkFwdLoop = -1;
        public static int TRANS_WalkRight_WalkBackward = -1;
        public static int TRANS_WalkRight_WalkLeft = -1;
        public static int TRANS_WalkRight_IdlePose = -1;

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
                if (lStateID == STATE_RunFwdLoop) { return true; }
                if (lStateID == STATE_WalkToIdle_RDown) { return true; }
                if (lStateID == STATE_WalkToIdle_LDown) { return true; }
                if (lStateID == STATE_RunStop_RDown) { return true; }
                if (lStateID == STATE_RunStop_LDown) { return true; }
                if (lStateID == STATE_WalkToIdle) { return true; }
                if (lStateID == STATE_WalkFwdLoop) { return true; }
                if (lStateID == STATE_WalkBackward) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_WalkLeft) { return true; }
                if (lStateID == STATE_WalkRight) { return true; }
                if (lTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_IdlePose) { return true; }
                if (lTransitionID == TRANS_AnyState_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkToIdle) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkBackward) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkLeft) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkRight) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_WalkBackward_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkBackward_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkBackward_WalkLeft) { return true; }
                if (lTransitionID == TRANS_WalkBackward_WalkRight) { return true; }
                if (lTransitionID == TRANS_IdlePose_WalkBackward) { return true; }
                if (lTransitionID == TRANS_IdlePose_WalkLeft) { return true; }
                if (lTransitionID == TRANS_IdlePose_WalkRight) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
                if (lTransitionID == TRANS_WalkLeft_WalkBackward) { return true; }
                if (lTransitionID == TRANS_WalkLeft_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkLeft_WalkRight) { return true; }
                if (lTransitionID == TRANS_WalkLeft_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkRight_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkRight_WalkBackward) { return true; }
                if (lTransitionID == TRANS_WalkRight_WalkLeft) { return true; }
                if (lTransitionID == TRANS_WalkRight_IdlePose) { return true; }
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
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_WalkBackward) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkLeft) { return true; }
            if (rStateID == STATE_WalkRight) { return true; }
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
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_WalkBackward) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkLeft) { return true; }
            if (rStateID == STATE_WalkRight) { return true; }
            if (rTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_IdlePose) { return true; }
            if (rTransitionID == TRANS_AnyState_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkToIdle) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkBackward) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkLeft) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkRight) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_WalkBackward_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkBackward_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkBackward_WalkLeft) { return true; }
            if (rTransitionID == TRANS_WalkBackward_WalkRight) { return true; }
            if (rTransitionID == TRANS_IdlePose_WalkBackward) { return true; }
            if (rTransitionID == TRANS_IdlePose_WalkLeft) { return true; }
            if (rTransitionID == TRANS_IdlePose_WalkRight) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
            if (rTransitionID == TRANS_WalkLeft_WalkBackward) { return true; }
            if (rTransitionID == TRANS_WalkLeft_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkLeft_WalkRight) { return true; }
            if (rTransitionID == TRANS_WalkLeft_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkRight_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkRight_WalkBackward) { return true; }
            if (rTransitionID == TRANS_WalkRight_WalkLeft) { return true; }
            if (rTransitionID == TRANS_WalkRight_IdlePose) { return true; }
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
            TRANS_EntryState_RunFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_AnyState_RunFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_EntryState_WalkFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_AnyState_WalkFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_EntryState_IdlePose = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_AnyState_IdlePose = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_EntryState_IdleToRun = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunRotate-SM.IdleToRun");
            TRANS_AnyState_IdleToRun = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunRotate-SM.IdleToRun");
            STATE_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToWalk");
            TRANS_IdleToWalk_WalkToIdle = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToWalk -> Base Layer.WalkRunRotate-SM.WalkToIdle");
            TRANS_IdleToWalk_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToWalk -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            STATE_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToRun");
            TRANS_IdleToRun_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToRun -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_IdleToRun_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToRun -> Base Layer.WalkRunRotate-SM.RunStop_LDown");
            TRANS_IdleToRun_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdleToRun -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            STATE_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_RunFwdLoop_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunFwdLoop -> Base Layer.WalkRunRotate-SM.RunStop_RDown");
            TRANS_RunFwdLoop_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunFwdLoop -> Base Layer.WalkRunRotate-SM.RunStop_LDown");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunFwdLoop -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            STATE_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_RDown");
            TRANS_WalkToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_RDown -> Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_WalkToIdle_RDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_RDown -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            STATE_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_LDown");
            TRANS_WalkToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_LDown -> Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_WalkToIdle_LDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle_LDown -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            STATE_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_RDown");
            TRANS_RunStop_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_RDown -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_RunStop_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_RDown -> Base Layer.WalkRunRotate-SM.IdlePose");
            STATE_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_LDown");
            TRANS_RunStop_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_LDown -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_RunStop_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.RunStop_LDown -> Base Layer.WalkRunRotate-SM.IdlePose");
            STATE_WalkToIdle = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle");
            TRANS_WalkToIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkToIdle -> Base Layer.WalkRunRotate-SM.IdlePose");
            STATE_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_WalkFwdLoop_WalkBackward = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.WalkBackward");
            TRANS_WalkFwdLoop_WalkLeft = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.WalkLeft");
            TRANS_WalkFwdLoop_WalkRight = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.WalkRight");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.WalkToIdle_LDown");
            TRANS_WalkFwdLoop_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkFwdLoop -> Base Layer.WalkRunRotate-SM.WalkToIdle_RDown");
            STATE_WalkBackward = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkBackward");
            TRANS_WalkBackward_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkBackward -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_WalkBackward_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkBackward -> Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_WalkBackward_WalkLeft = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkBackward -> Base Layer.WalkRunRotate-SM.WalkLeft");
            TRANS_WalkBackward_WalkRight = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkBackward -> Base Layer.WalkRunRotate-SM.WalkRight");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose");
            TRANS_IdlePose_WalkBackward = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose -> Base Layer.WalkRunRotate-SM.WalkBackward");
            TRANS_IdlePose_WalkLeft = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose -> Base Layer.WalkRunRotate-SM.WalkLeft");
            TRANS_IdlePose_WalkRight = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose -> Base Layer.WalkRunRotate-SM.WalkRight");
            TRANS_IdlePose_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose -> Base Layer.WalkRunRotate-SM.IdleToWalk");
            TRANS_IdlePose_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.IdlePose -> Base Layer.WalkRunRotate-SM.IdleToRun");
            STATE_WalkLeft = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkLeft");
            TRANS_WalkLeft_WalkBackward = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkLeft -> Base Layer.WalkRunRotate-SM.WalkBackward");
            TRANS_WalkLeft_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkLeft -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_WalkLeft_WalkRight = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkLeft -> Base Layer.WalkRunRotate-SM.WalkRight");
            TRANS_WalkLeft_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkLeft -> Base Layer.WalkRunRotate-SM.IdlePose");
            STATE_WalkRight = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkRight");
            TRANS_WalkRight_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkRight -> Base Layer.WalkRunRotate-SM.WalkFwdLoop");
            TRANS_WalkRight_WalkBackward = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkRight -> Base Layer.WalkRunRotate-SM.WalkBackward");
            TRANS_WalkRight_WalkLeft = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkRight -> Base Layer.WalkRunRotate-SM.WalkLeft");
            TRANS_WalkRight_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate-SM.WalkRight -> Base Layer.WalkRunRotate-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip m8446 = null;
        private AnimationClip m6646 = null;
        private AnimationClip m6650 = null;
        private AnimationClip m7042 = null;
        private AnimationClip m7040 = null;
        private AnimationClip m12958 = null;
        private AnimationClip m10830 = null;
        private AnimationClip m13452 = null;
        private AnimationClip m14724 = null;
        private AnimationClip m7568 = null;
        private AnimationClip m9102 = null;
        private AnimationClip m9104 = null;

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

            UnityEditor.Animations.AnimatorState lIdleToWalk = lMotionStateMachine.AddState("IdleToWalk", new Vector3(-432, 504, 0));
            lIdleToWalk.motion = m8446;
            lIdleToWalk.speed = 1.25f;

            UnityEditor.Animations.AnimatorState lIdleToRun = lMotionStateMachine.AddState("IdleToRun", new Vector3(300, 276, 0));
            lIdleToRun.motion = m6646;
            lIdleToRun.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunFwdLoop = lMotionStateMachine.AddState("RunFwdLoop", new Vector3(228, 588, 0));
            lRunFwdLoop.motion = m6650;
            lRunFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_RDown = lMotionStateMachine.AddState("WalkToIdle_RDown", new Vector3(-12, 408, 0));
            lWalkToIdle_RDown.motion = m7042;
            lWalkToIdle_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_LDown = lMotionStateMachine.AddState("WalkToIdle_LDown", new Vector3(-84, 336, 0));
            lWalkToIdle_LDown.motion = m7040;
            lWalkToIdle_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_RDown = lMotionStateMachine.AddState("RunStop_RDown", new Vector3(372, 444, 0));
            lRunStop_RDown.motion = m12958;
            lRunStop_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_LDown = lMotionStateMachine.AddState("RunStop_LDown", new Vector3(372, 384, 0));
            lRunStop_LDown.motion = m10830;
            lRunStop_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle = lMotionStateMachine.AddState("WalkToIdle", new Vector3(-384, 564, 0));
            lWalkToIdle.motion = m7040;
            lWalkToIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkFwdLoop = lMotionStateMachine.AddState("WalkFwdLoop", new Vector3(-72, 588, 0));
            lWalkFwdLoop.motion = m13452;
            lWalkFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkBackward = lMotionStateMachine.AddState("WalkBackward", new Vector3(-420, 96, 0));
            lWalkBackward.motion = m14724;
            lWalkBackward.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(-24, 156, 0));
            lIdlePose.motion = m7568;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkLeft = lMotionStateMachine.AddState("WalkLeft", new Vector3(-528, 216, 0));
            lWalkLeft.motion = m9102;
            lWalkLeft.speed = 1.2f;

            UnityEditor.Animations.AnimatorState lWalkRight = lMotionStateMachine.AddState("WalkRight", new Vector3(-528, 336, 0));
            lWalkRight.motion = m9104;
            lWalkRight.speed = 1.2f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lRunFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9000002f;
            lAnyStateTransition.duration = 0.2f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1715f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lWalkFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.3f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1714f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdlePose);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.00690246f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1700f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputY");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToRun);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1700f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lIdleToWalk.AddTransition(lWalkToIdle);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6352268f;
            lStateTransition.duration = 0.1901305f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lIdleToWalk.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6721453f;
            lStateTransition.duration = 0.04795915f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleToRun.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9683823f;
            lStateTransition.duration = 0.0316177f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputY");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8278595f;
            lStateTransition.duration = 0.1738888f;
            lStateTransition.offset = 0.6072354f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lIdleToRun.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4591338f;
            lStateTransition.duration = 0.2499999f;
            lStateTransition.offset = 0.7079729f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputY");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.05f;
            lStateTransition.duration = 0.1313883f;
            lStateTransition.offset = 0.3069344f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4535715f;
            lStateTransition.duration = 0.09285709f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.3f, "InputY");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8589286f;
            lStateTransition.duration = 0.1410714f;
            lStateTransition.offset = 0.2815929f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.3f, "InputY");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7766021f;
            lStateTransition.duration = 0.3260868f;
            lStateTransition.offset = 0.2640708f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitudeAvg");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.2527953f;
            lStateTransition.duration = 0.3543528f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitudeAvg");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.081f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0.36f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.3f, "InputY");

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
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

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
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lRunStop_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4475232f;
            lStateTransition.duration = 0.1973684f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputY");

            lStateTransition = lRunStop_RDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8687333f;
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
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputY");

            lStateTransition = lRunStop_LDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7665359f;
            lStateTransition.duration = 0.1f;
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

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1972967f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2492742f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2532655f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9358001f;
            lStateTransition.duration = 0.07f;
            lStateTransition.offset = 0.4224093f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitudeAvg");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3192836f;
            lStateTransition.duration = 0.09642851f;
            lStateTransition.offset = 0.3287362f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1721f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8021039f;
            lStateTransition.duration = 0.09642864f;
            lStateTransition.offset = 0.3232313f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1720f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.1647059f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitudeAvg");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.09933357f;
            lStateTransition.duration = 0.1144445f;
            lStateTransition.offset = 0.6099998f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1720f, "L0MotionPhase");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitudeAvg");

            lStateTransition = lWalkBackward.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1994301f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lWalkBackward.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3841325f;
            lStateTransition.duration = 0.1958688f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkBackward.AddTransition(lWalkLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1967947f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2f, "L0MotionParameter");

            lStateTransition = lWalkBackward.AddTransition(lWalkRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1977206f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3f, "L0MotionParameter");

            lStateTransition = lWalkBackward.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7222223f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lIdlePose.AddTransition(lWalkBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 3f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lIdlePose.AddTransition(lWalkLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 3f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2f, "L0MotionParameter");

            lStateTransition = lIdlePose.AddTransition(lWalkRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 3f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3f, "L0MotionParameter");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 2f;
            lStateTransition.offset = 0.1284909f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputY");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 1.958004f;
            lStateTransition.offset = 0.2155401f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputY");

            lStateTransition = lWalkLeft.AddTransition(lWalkBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2016607f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lWalkLeft.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2480766f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lWalkLeft.AddTransition(lWalkRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2045453f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 3f, "L0MotionParameter");

            lStateTransition = lWalkLeft.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4316539f;
            lStateTransition.duration = 0.2027097f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkLeft.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7727274f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkRight.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.249285f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lWalkRight.AddTransition(lWalkBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1963095f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lWalkRight.AddTransition(lWalkLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1965434f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2f, "L0MotionParameter");

            lStateTransition = lWalkRight.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4093298f;
            lStateTransition.duration = 0.2039498f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

            lStateTransition = lWalkRight.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7727274f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -0.1f, "InputY");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputY");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m8446 = CreateAnimationField("IdleToWalk", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkForward_NtrlFaceFwd.fbx/IdleToWalk.anim", "IdleToWalk", m8446);
            m6646 = CreateAnimationField("IdleToRun", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/IdleToRun.anim", "IdleToRun", m6646);
            m6650 = CreateAnimationField("RunFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/RunForward.anim", "RunForward", m6650);
            m7042 = CreateAnimationField("WalkToIdle_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkToIdle_RDown.anim", "WalkToIdle_RDown", m7042);
            m7040 = CreateAnimationField("WalkToIdle_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_AllAngles.fbx/WalkToIdle_LDown.anim", "WalkToIdle_LDown", m7040);
            m12958 = CreateAnimationField("RunStop_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_HalfSteps2Idle_PasingLongStepTOIdle.fbx/RunToIdle_RDown.anim", "RunToIdle_RDown", m12958);
            m10830 = CreateAnimationField("RunStop_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_2.fbx/RunToIdle_LDown.anim", "RunToIdle_LDown", m10830);
            m13452 = CreateAnimationField("WalkFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD.fbx/WalkForward.anim", "WalkForward", m13452);
            m14724 = CreateAnimationField("WalkBackward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_BWalk.fbx/WalkBackwards.anim", "WalkBackwards", m14724);
            m7568 = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", m7568);
            m9102 = CreateAnimationField("WalkLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk.fbx/SWalkLeft.anim", "SWalkLeft", m9102);
            m9104 = CreateAnimationField("WalkRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk.fbx/SWalkRight.anim", "SWalkRight", m9104);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
