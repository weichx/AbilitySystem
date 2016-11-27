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
    /// </summary>
    [MotionName("Walk Run Pivot")]
    [MotionDescription("Standard movement (walk/run) for an adventure game.")]
    public class WalkRunPivot_v2 : MotionControllerMotion, IWalkRunMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 27130;
        public const int PHASE_END_RUN = 27131;
        public const int PHASE_END_WALK = 27132;
        public const int PHASE_RESUME = 27133;

        public const int PHASE_START_IDLE_PIVOT = 27135;

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
        /// Speed (units per second) when walking
        /// </summary>
        public float _WalkSpeed = 0f;
        public virtual float WalkSpeed
        {
            get { return _WalkSpeed; }
            set { _WalkSpeed = value; }
        }

        /// <summary>
        /// Speed (units per second) when running
        /// </summary>
        public float _RunSpeed = 0f;
        public virtual float RunSpeed
        {
            get { return _RunSpeed; }
            set { _RunSpeed = value; }
        }

        /// <summary>
        /// Determines if we rotate to match the camera
        /// </summary>
        public bool _RotateWithCamera = true;
        public bool RotateWithCamera
        {
            get { return _RotateWithCamera; }
            set { _RotateWithCamera = value; }
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
        /// Degrees per second to rotate the actor in order to face the input direction
        /// </summary>
        public float _RotationSpeed = 180f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
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

            set
            {
                mStartInWalk = value;
                if (value) { mStartInMove = value; }
            }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        private bool mStartInRun = false;
        public bool StartInRun
        {
            get { return mStartInRun; }

            set
            {
                mStartInRun = value;
                if (value) { mStartInMove = value; }
            }
        }

        /// <summary>
        /// Determines if we'll use the start transitions when starting from idle
        /// </summary>
        public bool _UseStartTransitions = true;
        public bool UseStartTransitions
        {
            get { return _UseStartTransitions; }
            set { _UseStartTransitions = value; }
        }

        /// <summary>
        /// Determines if we'll use the start transitions when stopping movement
        /// </summary>
        public bool _UseStopTransitions = true;
        public bool UseStopTransitions
        {
            get { return _UseStopTransitions; }
            set { _UseStopTransitions = value; }
        }

        /// <summary>
        /// Determines if the character can pivot while idle
        /// </summary>
        public bool _UseTapToPivot = false;
        public bool UseTapToPivot
        {
            get { return _UseTapToPivot; }
            set { _UseTapToPivot = value; }
        }

        /// <summary>
        /// Determines how long we wait before testing for an idle pivot
        /// </summary>
        public float _TapToPivotDelay = 0.2f;
        public float TapToPivotDelay
        {
            get { return _TapToPivotDelay; }
            set { _TapToPivotDelay = value; }
        }

        /// <summary>
        /// Minimum angle before we use the pivot speed
        /// </summary>
        public float _MinPivotAngle = 20f;
        public float MinPivotAngle
        {
            get { return _MinPivotAngle; }
            set { _MinPivotAngle = value; }
        }

        /// <summary>
        /// Number of samples to use for smoothing
        /// </summary>
        public int _SmoothingSamples = 10;
        public int SmoothingSamples
        {
            get { return _SmoothingSamples; }

            set
            {
                _SmoothingSamples = value;

                mInputX.SampleCount = _SmoothingSamples;
                mInputY.SampleCount = _SmoothingSamples;
                mInputMagnitude.SampleCount = _SmoothingSamples;
            }
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
        /// Determine if we're pivoting from an idle
        /// </summary>
        protected bool mStartInPivot = false;

        /// <summary>
        /// Angle of the input from when the motion was activated
        /// </summary>
        protected Vector3 mSavedInputForward = Vector3.zero;

        /// <summary>
        /// Time that has elapsed since there was no input
        /// </summary>
        protected float mNoInputElapsed = 0f;

        /// <summary>
        /// Phase ID we're using to transition out
        /// </summary>
        protected int mExitPhaseID = 0;

        /// <summary>
        /// Frame level rotation test
        /// </summary>
        protected bool mRotateWithCamera = false;

        /// <summary>
        /// Determines if the actor rotation should be linked to the camera
        /// </summary>
        protected bool mLinkRotation = false;

        /// <summary>
        /// We use these classes to help smooth the input values so that
        /// movement doesn't drop from 1 to 0 immediately.
        /// </summary>
        protected FloatValue mInputX = new FloatValue(0f, 10);
        protected FloatValue mInputY = new FloatValue(0f, 10);
        protected FloatValue mInputMagnitude = new FloatValue(0f, 15);

        /// <summary>
        /// Last time we had input activity
        /// </summary>
        protected float mLastTapStartTime = 0f;
        protected float mLastTapInputFromAvatarAngle = 0f;
        protected Vector3 mLastTapInputForward = Vector3.zero;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkRunPivot_v2()
            : base()
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 5;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunPivot v2-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunPivot_v2(MotionController rController)
            : base(rController)
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 5;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunPivot v2-SM"; }
#endif
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Initialize the smoothing variables
            SmoothingSamples = _SmoothingSamples;
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable ||
                !mMotionController.IsGrounded ||
                mActorController.State.Stance != EnumControllerStance.TRAVERSAL)
            {
                mStartInPivot = false;
                mLastTapStartTime = 0f;
                return false;
            }

            bool lIsPivotable = (_UseTapToPivot && (mLastTapStartTime > 0f || Mathf.Abs(mMotionController.State.InputFromAvatarAngle) > _MinPivotAngle));

            bool lIsIdling = (_UseTapToPivot && mMotionLayer.ActiveMotion != null && mMotionLayer.ActiveMotion.Category == EnumMotionCategories.IDLE);

            // Determine if tapping is enabled
            if (_UseTapToPivot && lIsPivotable && lIsIdling)
            {
                // If there's input, it could be the start of a tap or true movement
                if (mMotionController.State.InputMagnitudeTrend.Value > 0.1f)
                {
                    // Start the timer
                    if (mLastTapStartTime == 0f)
                    {
                        mLastTapStartTime = Time.time;
                        mLastTapInputForward = mMotionController.State.InputForward;
                        mLastTapInputFromAvatarAngle = mMotionController.State.InputFromAvatarAngle;
                    }
                    // Timer has expired. So, we must really be moving
                    else if (mLastTapStartTime + _TapToPivotDelay <= Time.time)
                    {
                        mStartInPivot = false;
                        mLastTapStartTime = 0f;
                        return true;
                    }

                    // Keep waiting
                    return false;
                }
                // No input. So, at the end of a tap or there really is nothing
                else
                {
                    if (mLastTapStartTime > 0f)
                    {
                        mStartInPivot = true;
                        mLastTapStartTime = 0f;
                        return true;
                    }
                }
            }
            // If not, we do normal processing
            else
            {
                mStartInPivot = false;
                mLastTapStartTime = 0f;

                // If there's enough movement, start the motion
                if (mMotionController.State.InputMagnitudeTrend.Value > 0.49f)
                {
                    return true;
                }
            }

            // Don't activate
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            if (mIsActivatedFrame) { return true; }
            if (!mMotionController.IsGrounded) { return false; }

            // Our idle pose is a good exit
            if (mMotionLayer._AnimatorStateID == STATE_IdlePose)
            {
                return false;
            }

            // Our exit pose for the idle pivots
            if (mMotionLayer._AnimatorStateID == STATE_IdleTurnEndPose)
            {
                if (mMotionController.State.InputMagnitudeTrend.Value < 0.1f)
                {
                    return false;
                }
            }

            // One last check to make sure we're in this state
            if (mIsAnimatorActive && !IsInMotionState)
            {
                return false;
            }

            // If we no longer have input and we're in normal movement, we can stop
            if (mMotionController.State.InputMagnitudeTrend.Average < 0.1f)
            {
                if (mMotionLayer._AnimatorStateID == STATE_MoveTree && mMotionLayer._AnimatorTransitionID == 0)
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
            // Since we're dealing with a blend tree, keep the value until the transition completes            
            mMotionController.ForcedInput.x = mInputX.Average;
            mMotionController.ForcedInput.y = mInputY.Average;

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            mExitPhaseID = 0;
            mSavedInputForward = mMotionController.State.InputForward;

            if (mStartInPivot)
            {
                mMotionController.State.InputFromAvatarAngle = mLastTapInputFromAvatarAngle;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START_IDLE_PIVOT, 0, true);
            }
            else if (mStartInMove)
            {
                mStartInMove = false;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, 1, true);
            }
            else if (mMotionController._InputSource == null)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, (_UseStartTransitions ? 0 : 1), true);
            }
            else
            {
                // Grab the state info
                MotionState lState = mMotionController.State;

                // Convert the input to radial so we deal with keyboard and gamepad input the same.
                float lInputX = lState.InputX;
                float lInputY = lState.InputY;
                float lInputMagnitude = lState.InputMagnitudeTrend.Value;
                InputManagerHelper.ConvertToRadialInput(ref lInputX, ref lInputY, ref lInputMagnitude, (IsRunActive ? 1f : 0.5f));

                // Smooth the input
                if (lInputX != 0f || lInputY < 0f)
                {
                    mInputX.Clear(lInputX);
                    mInputY.Clear(lInputY);
                    mInputMagnitude.Clear(lInputMagnitude);
                }

                // Start the motion
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, (_UseStartTransitions ? 0 : 1), true);
            }

            // Register this motion with the camera
            if (_RotateWithCamera && mMotionController.CameraRig is BaseCameraRig)
            {
                ((BaseCameraRig)mMotionController.CameraRig).OnPostLateUpdate += OnCameraUpdated;
            }

            // Flag this motion as active
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
            mLastTapStartTime = 0f;
            mLastTapInputFromAvatarAngle = 0f;

            // Clear out the start
            mStartInPivot = false;
            mStartInRun = false;
            mStartInWalk = false;

            // Register this motion with the camera
            if (mMotionController.CameraRig is BaseCameraRig)
            {
                ((BaseCameraRig)mMotionController.CameraRig).OnPostLateUpdate -= OnCameraUpdated;
            }

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
            if ((mMotionLayer._AnimatorTransitionID == TRANS_AnyState_MoveTree) ||
                (mMotionLayer._AnimatorStateID == STATE_MoveTree && mMotionLayer._AnimatorTransitionID == 0))
            {
                rRotation = Quaternion.identity;

                // Override root motion if we're meant to
                float lMovementSpeed = (IsRunActive ? _RunSpeed : _WalkSpeed);
                if (lMovementSpeed > 0f)
                {
                    rMovement = rMovement.normalized * (lMovementSpeed * rDeltaTime);
                }

                rMovement.x = 0f;
                rMovement.y = 0f;
                if (rMovement.z < 0f) { rMovement.z = 0f; }
            }
            else
            {
                if (_UseTapToPivot && IsIdlePivoting())
                {
                    rMovement = Vector3.zero;
                }
                // If we're stopping, add some lag
                else if (IsStopping())
                {
                    rMovement = rMovement * 0.5f;
                }
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
            mRotation = Quaternion.identity;

            if (_UseTapToPivot && IsIdlePivoting())
            {
                UpdateIdlePivot(rDeltaTime, rUpdateIndex);
            }
            else
            {
                UpdateMovement(rDeltaTime, rUpdateIndex);
            }
        }

        /// <summary>
        /// Update processing for the idle pivot
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateIndex"></param>
        private void UpdateIdlePivot(float rDeltaTime, int rUpdateIndex)
        {
            int lStateID = mMotionLayer._AnimatorStateID;
            if (lStateID == STATE_IdleTurn180L ||
                lStateID == STATE_IdleTurn90L ||
                lStateID == STATE_IdleTurn20L ||
                lStateID == STATE_IdleTurn20R ||
                lStateID == STATE_IdleTurn90R ||
                lStateID == STATE_IdleTurn180R)
            {
                if (mMotionLayer._AnimatorTransitionID != 0 && mLastTapInputForward.sqrMagnitude > 0f)
                {
                    if (mMotionController._CameraTransform != null)
                    {
                        Vector3 lInputForward = mMotionController._CameraTransform.rotation * mLastTapInputForward;

                        float lAngle = Vector3Ext.HorizontalAngleTo(mMotionController._Transform.forward, lInputForward, mMotionController._Transform.up);
                        mRotation = Quaternion.Euler(0f, lAngle * mMotionLayer._AnimatorTransitionNormalizedTime, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// Update processing for moving
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateIndex"></param>
        private void UpdateMovement(float rDeltaTime, int rUpdateIndex)
        {
            bool lUpdateSamples = true;

            // Store the last valid input we had
            if (mMotionController.State.InputMagnitudeTrend.Value > 0.4f)
            {
                mExitPhaseID = 0;
                mNoInputElapsed = 0f;
                mSavedInputForward = mMotionController.State.InputForward;

                // If we were stopping, allow us to resume without leaving the motion
                if (IsStopping())
                {
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_RESUME, 0, true);
                }
            }
            else
            {
                mNoInputElapsed = mNoInputElapsed + rDeltaTime;

                if (_UseStopTransitions)
                {
                    lUpdateSamples = false;

                    // If we've passed the delay, we really are stopping
                    if (mNoInputElapsed > 0.2f)
                    {
                        // Determine how we'll stop
                        if (mExitPhaseID == 0)
                        {
                            mExitPhaseID = (mInputMagnitude.Average < 0.6f ? PHASE_END_WALK : PHASE_END_RUN);
                        }

                        // Ensure we actually stop that way
                        if (mExitPhaseID != 0 && mMotionLayer._AnimatorStateID == STATE_MoveTree && mMotionLayer._AnimatorTransitionID == 0)
                        {
                            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, mExitPhaseID, 0, true);
                        }
                    }
                }
            }

            // If we need to update the samples... 
            if (lUpdateSamples)
            {
                MotionState lState = mMotionController.State;

                // Convert the input to radial so we deal with keyboard and gamepad input the same.
                float lInputMax = (IsRunActive ? 1f : 0.5f);

                float lInputX = Mathf.Clamp(lState.InputX, -lInputMax, lInputMax);
                float lInputY = Mathf.Clamp(lState.InputY, -lInputMax, lInputMax);
                float lInputMagnitude = Mathf.Clamp(lState.InputMagnitudeTrend.Value, 0f, lInputMax);
                InputManagerHelper.ConvertToRadialInput(ref lInputX, ref lInputY, ref lInputMagnitude);

                // Smooth the input
                mInputX.Add(lInputX);
                mInputY.Add(lInputY);
                mInputMagnitude.Add(lInputMagnitude);
            }

            // Modify the input values to add some lag
            mMotionController.State.InputX = mInputX.Average;
            mMotionController.State.InputY = mInputY.Average;
            mMotionController.State.InputMagnitudeTrend.Replace(mInputMagnitude.Average);

            // We may want to rotate with the camera if we're facing forward
            mRotateWithCamera = false;
            if (_RotateWithCamera && mMotionController._CameraTransform != null)
            {
                float lToCameraAngle = Vector3Ext.HorizontalAngleTo(mMotionController._Transform.forward, mMotionController._CameraTransform.forward, mMotionController._Transform.up);
                mRotateWithCamera = (Mathf.Abs(lToCameraAngle) < _RotationSpeed * rDeltaTime);

                if (mRotateWithCamera && mMotionLayer._AnimatorStateID != STATE_MoveTree) { mRotateWithCamera = false; }
                if (mRotateWithCamera && mMotionLayer._AnimatorTransitionID != 0) { mRotateWithCamera = false; }
                if (mRotateWithCamera && (Mathf.Abs(mMotionController.State.InputX) > 0.05f || mMotionController.State.InputY <= 0f)) { mRotateWithCamera = false; }
                if (mRotateWithCamera && (_RotateActionAlias.Length > 0 && !mMotionController._InputSource.IsPressed(_RotateActionAlias))) { mRotateWithCamera = false; }
            }

            // If we're meant to rotate with the camera (and OnCameraUpdate isn't already attached), do it here
            if (_RotateWithCamera && !(mMotionController.CameraRig is BaseCameraRig))
            {
                OnCameraUpdated(rDeltaTime, rUpdateIndex, null);
            }

            // We only allow input rotation under certain circumstances
            if (mMotionLayer._AnimatorTransitionID == TRANS_EntryState_MoveTree ||
                (mMotionLayer._AnimatorStateID == STATE_MoveTree && mMotionLayer._AnimatorTransitionID == 0) ||

                (mMotionLayer._AnimatorStateID == STATE_IdleToWalk180L && mMotionLayer._AnimatorStateNormalizedTime > 0.7f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToWalk90L && mMotionLayer._AnimatorStateNormalizedTime > 0.6f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToWalk90R && mMotionLayer._AnimatorStateNormalizedTime > 0.6f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToWalk180R && mMotionLayer._AnimatorStateNormalizedTime > 0.7f) ||

                (mMotionLayer._AnimatorStateID == STATE_IdleToRun180L && mMotionLayer._AnimatorStateNormalizedTime > 0.6f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToRun90L && mMotionLayer._AnimatorStateNormalizedTime > 0.6f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToRun) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToRun90R && mMotionLayer._AnimatorStateNormalizedTime > 0.6f) ||
                (mMotionLayer._AnimatorStateID == STATE_IdleToRun180R && mMotionLayer._AnimatorStateNormalizedTime > 0.6f)
                )
            {
                // Since we're not rotating with the camera, rotate with input
                if (!mRotateWithCamera)
                {
                    if (mMotionController._CameraTransform != null && mMotionController.State.InputForward.sqrMagnitude == 0f)
                    {
                        RotateToInput(mMotionController._CameraTransform.rotation * mSavedInputForward, rDeltaTime, ref mRotation);
                    }
                    else
                    {
                        RotateToInput(mMotionController.State.InputFromAvatarAngle, rDeltaTime, ref mRotation);
                    }
                }
            }
        }

        /// <summary>
        /// Create a rotation velocity that rotates the character based on input
        /// </summary>
        /// <param name="rInputForward"></param>
        /// <param name="rDeltaTime"></param>
        private void RotateToInput(Vector3 rInputForward, float rDeltaTime, ref Quaternion rRotation)
        {
            float lAngle = Vector3Ext.HorizontalAngleTo(mMotionController._Transform.forward, rInputForward, mMotionController._Transform.up);
            if (lAngle != 0f)
            {
                if (_RotationSpeed > 0f && Mathf.Abs(lAngle) > _RotationSpeed * rDeltaTime)
                {
                    lAngle = Mathf.Sign(lAngle) * _RotationSpeed * rDeltaTime;
                }

                rRotation = Quaternion.Euler(0f, lAngle, 0f);
            }
        }

        /// <summary>
        /// Create a rotation velocity that rotates the character based on input
        /// </summary>
        /// <param name="rInputFromAvatarAngle"></param>
        /// <param name="rDeltaTime"></param>
        private void RotateToInput(float rInputFromAvatarAngle, float rDeltaTime, ref Quaternion rRotation)
        {
            if (rInputFromAvatarAngle != 0f)
            {
                if (_RotationSpeed > 0f && Mathf.Abs(rInputFromAvatarAngle) > _RotationSpeed * rDeltaTime)
                {
                    rInputFromAvatarAngle = Mathf.Sign(rInputFromAvatarAngle) * _RotationSpeed * rDeltaTime;
                }

                rRotation = Quaternion.Euler(0f, rInputFromAvatarAngle, 0f);
            }
        }

        /// <summary>
        /// When we want to rotate based on the camera direction, we need to tweak the actor
        /// rotation AFTER we process the camera. Otherwise, we can get small stutters during camera rotation. 
        /// 
        /// This is the only way to keep them totally in sync. It also means we can't run any of our AC processing
        /// as the AC already ran. So, we do minimal work here
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateCount"></param>
        /// <param name="rCamera"></param>
        private void OnCameraUpdated(float rDeltaTime, int rUpdateIndex, BaseCameraRig rCamera)
        {
            if (!mRotateWithCamera)
            {
                mLinkRotation = false;
                return;
            }

            float lToCameraAngle = Vector3Ext.HorizontalAngleTo(mMotionController._Transform.forward, mMotionController._CameraTransform.forward, mMotionController._Transform.up);
            if (!mLinkRotation && Mathf.Abs(lToCameraAngle) <= _RotationSpeed * rDeltaTime) { mLinkRotation = true; }

            if (!mLinkRotation)
            {
                float lRotationAngle = Mathf.Abs(lToCameraAngle);
                float lRotationSign = Mathf.Sign(lToCameraAngle);
                lToCameraAngle = lRotationSign * Mathf.Min(_RotationSpeed * rDeltaTime, lRotationAngle);
            }

            Quaternion lRotation = Quaternion.AngleAxis(lToCameraAngle, Vector3.up);
            mActorController.Yaw = mActorController.Yaw * lRotation;
            mActorController._Transform.rotation = mActorController.Tilt * mActorController.Yaw;
        }

        /// <summary>
        /// Tests if we're in one of the stopping states
        /// </summary>
        /// <returns></returns>
        private bool IsStopping()
        {
            if (!_UseStopTransitions) { return false; }

            int lStateID = mMotionLayer._AnimatorStateID;
            if (lStateID == STATE_RunToIdle_LDown) { return true; }
            if (lStateID == STATE_RunToIdle_RDown) { return true; }
            if (lStateID == STATE_WalkToIdle_LDown) { return true; }
            if (lStateID == STATE_WalkToIdle_RDown) { return true; }

            int lTransitionID = mMotionLayer._AnimatorTransitionID;
            if (lTransitionID == TRANS_MoveTree_RunToIdle_LDown) { return true; }
            if (lTransitionID == TRANS_MoveTree_RunToIdle_RDown) { return true; }
            if (lTransitionID == TRANS_MoveTree_WalkToIdle_LDown) { return true; }
            if (lTransitionID == TRANS_MoveTree_WalkToIdle_RDown) { return true; }

            return false;
        }

        /// <summary>
        /// Tests if we're in one of the pivoting states
        /// </summary>
        /// <returns></returns>
        private bool IsIdlePivoting()
        {
            if (!_UseTapToPivot) { return false; }

            int lStateID = mMotionLayer._AnimatorStateID;
            if (lStateID == STATE_IdleTurn180L) { return true; }
            if (lStateID == STATE_IdleTurn90L) { return true; }
            if (lStateID == STATE_IdleTurn20L) { return true; }
            if (lStateID == STATE_IdleTurn20R) { return true; }
            if (lStateID == STATE_IdleTurn90R) { return true; }
            if (lStateID == STATE_IdleTurn180R) { return true; }

            int lTransitionID = mMotionLayer._AnimatorTransitionID;
            if (lTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
            if (lTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
            if (lTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
            if (lTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
            if (lTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
            if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }

            return false;
        }

        #region Editor Functions

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

            if (EditorHelper.BoolField("Default to Run", "Determines if the default is to run or walk.", DefaultToRun, mMotionController))
            {
                lIsDirty = true;
                DefaultToRun = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.TextField("Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default).", ActionAlias, mMotionController))
            {
                lIsDirty = true;
                ActionAlias = EditorHelper.FieldStringValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.FloatField("Walk Speed", "Speed (units per second) to move when walking. Set to 0 to use root-motion.", WalkSpeed, mMotionController))
            {
                lIsDirty = true;
                WalkSpeed = EditorHelper.FieldFloatValue;
            }

            if (EditorHelper.FloatField("Run Speed", "Speed (units per second) to move when running. Set to 0 to use root-motion.", RunSpeed, mMotionController))
            {
                lIsDirty = true;
                RunSpeed = EditorHelper.FieldFloatValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.BoolField("Rotate With Camera", "Determines if we rotate to match the camera.", RotateWithCamera, mMotionController))
            {
                lIsDirty = true;
                RotateWithCamera = EditorHelper.FieldBoolValue;
            }

            if (RotateWithCamera)
            {
                if (EditorHelper.TextField("Rotate Action Alias", "Action alias determines if rotation is activated. This typically matches the input source's View Activator.", RotateActionAlias, mMotionController))
                {
                    lIsDirty = true;
                    RotateActionAlias = EditorHelper.FieldStringValue;
                }
            }

            if (EditorHelper.FloatField("Rotation Speed", "Degrees per second to rotate the actor ('0' means instant rotation).", RotationSpeed, mMotionController))
            {
                lIsDirty = true;
                RotationSpeed = EditorHelper.FieldFloatValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.BoolField("Use Start Transitions", "Determines if we'll use the start transitions when coming from idle", UseStartTransitions, mMotionController))
            {
                lIsDirty = true;
                UseStartTransitions = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.BoolField("Use Stop Transitions", "Determines if we'll use the stop transitions when stopping movement", UseStopTransitions, mMotionController))
            {
                lIsDirty = true;
                UseStopTransitions = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.BoolField("Use Tap to Pivot", "Determines if taping a direction while idle will pivot the character without moving them.", UseTapToPivot, mMotionController))
            {
                lIsDirty = true;
                UseTapToPivot = EditorHelper.FieldBoolValue;
            }

            if (UseTapToPivot)
            {
                EditorGUILayout.BeginHorizontal();

                if (EditorHelper.FloatField("Min Angle", "Sets the minimum angle between the input direction and character direction where we'll do a pivot.", MinPivotAngle, mMotionController))
                {
                    lIsDirty = true;
                    MinPivotAngle = EditorHelper.FieldFloatValue;
                }

                GUILayout.Space(10f);

                EditorGUILayout.LabelField(new GUIContent("Delay", "Delay in seconds before we test if we're NOT pivoting, but moving. In my tests, the average tap took 0.12 to 0.15 seconds."), GUILayout.Width(40f));
                if (EditorHelper.FloatField(TapToPivotDelay, "Delay", mMotionController, 40f))
                {
                    lIsDirty = true;
                    TapToPivotDelay = EditorHelper.FieldFloatValue;
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

            if (EditorHelper.IntField("Smoothing Samples", "The more samples the smoother movement is, but the less responsive.", SmoothingSamples, mMotionController))
            {
                lIsDirty = true;
                SmoothingSamples = EditorHelper.FieldIntValue;
            }

            return lIsDirty;
        }

#endif

        #endregion

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int STATE_MoveTree = -1;
        public static int STATE_IdleToWalk90L = -1;
        public static int STATE_IdleToWalk90R = -1;
        public static int STATE_IdleToWalk180R = -1;
        public static int STATE_IdleToWalk180L = -1;
        public static int STATE_IdlePose = -1;
        public static int STATE_IdleToRun90L = -1;
        public static int STATE_IdleToRun180L = -1;
        public static int STATE_IdleToRun90R = -1;
        public static int STATE_IdleToRun180R = -1;
        public static int STATE_IdleToRun = -1;
        public static int STATE_RunPivot180R_LDown = -1;
        public static int STATE_WalkPivot180L = -1;
        public static int STATE_RunToIdle_LDown = -1;
        public static int STATE_WalkToIdle_LDown = -1;
        public static int STATE_WalkToIdle_RDown = -1;
        public static int STATE_RunToIdle_RDown = -1;
        public static int STATE_IdleTurn20R = -1;
        public static int STATE_IdleTurn90R = -1;
        public static int STATE_IdleTurn180R = -1;
        public static int STATE_IdleTurn20L = -1;
        public static int STATE_IdleTurn90L = -1;
        public static int STATE_IdleTurn180L = -1;
        public static int STATE_IdleTurnEndPose = -1;
        public static int TRANS_AnyState_IdleToWalk90L = -1;
        public static int TRANS_EntryState_IdleToWalk90L = -1;
        public static int TRANS_AnyState_IdleToWalk90R = -1;
        public static int TRANS_EntryState_IdleToWalk90R = -1;
        public static int TRANS_AnyState_IdleToWalk180R = -1;
        public static int TRANS_EntryState_IdleToWalk180R = -1;
        public static int TRANS_AnyState_MoveTree = -1;
        public static int TRANS_EntryState_MoveTree = -1;
        public static int TRANS_AnyState_IdleToWalk180L = -1;
        public static int TRANS_EntryState_IdleToWalk180L = -1;
        public static int TRANS_AnyState_IdleToRun180L = -1;
        public static int TRANS_EntryState_IdleToRun180L = -1;
        public static int TRANS_AnyState_IdleToRun90L = -1;
        public static int TRANS_EntryState_IdleToRun90L = -1;
        public static int TRANS_AnyState_IdleToRun90R = -1;
        public static int TRANS_EntryState_IdleToRun90R = -1;
        public static int TRANS_AnyState_IdleToRun180R = -1;
        public static int TRANS_EntryState_IdleToRun180R = -1;
        public static int TRANS_AnyState_IdleToRun = -1;
        public static int TRANS_EntryState_IdleToRun = -1;
        public static int TRANS_AnyState_IdleTurn180L = -1;
        public static int TRANS_EntryState_IdleTurn180L = -1;
        public static int TRANS_AnyState_IdleTurn90L = -1;
        public static int TRANS_EntryState_IdleTurn90L = -1;
        public static int TRANS_AnyState_IdleTurn20L = -1;
        public static int TRANS_EntryState_IdleTurn20L = -1;
        public static int TRANS_AnyState_IdleTurn20R = -1;
        public static int TRANS_EntryState_IdleTurn20R = -1;
        public static int TRANS_AnyState_IdleTurn90R = -1;
        public static int TRANS_EntryState_IdleTurn90R = -1;
        public static int TRANS_AnyState_IdleTurn180R = -1;
        public static int TRANS_EntryState_IdleTurn180R = -1;
        public static int TRANS_MoveTree_RunPivot180R_LDown = -1;
        public static int TRANS_MoveTree_WalkPivot180L = -1;
        public static int TRANS_MoveTree_RunToIdle_LDown = -1;
        public static int TRANS_MoveTree_WalkToIdle_LDown = -1;
        public static int TRANS_MoveTree_RunToIdle_RDown = -1;
        public static int TRANS_MoveTree_WalkToIdle_RDown = -1;
        public static int TRANS_IdleToWalk90L_MoveTree = -1;
        public static int TRANS_IdleToWalk90L_IdlePose = -1;
        public static int TRANS_IdleToWalk90R_MoveTree = -1;
        public static int TRANS_IdleToWalk90R_IdlePose = -1;
        public static int TRANS_IdleToWalk180R_MoveTree = -1;
        public static int TRANS_IdleToWalk180R_IdlePose = -1;
        public static int TRANS_IdleToWalk180L_MoveTree = -1;
        public static int TRANS_IdleToWalk180L_IdlePose = -1;
        public static int TRANS_IdleToRun90L_MoveTree = -1;
        public static int TRANS_IdleToRun90L_IdlePose = -1;
        public static int TRANS_IdleToRun180L_MoveTree = -1;
        public static int TRANS_IdleToRun180L_IdlePose = -1;
        public static int TRANS_IdleToRun90R_MoveTree = -1;
        public static int TRANS_IdleToRun90R_IdlePose = -1;
        public static int TRANS_IdleToRun180R_MoveTree = -1;
        public static int TRANS_IdleToRun180R_IdlePose = -1;
        public static int TRANS_IdleToRun_MoveTree = -1;
        public static int TRANS_IdleToRun_IdlePose = -1;
        public static int TRANS_RunPivot180R_LDown_MoveTree = -1;
        public static int TRANS_WalkPivot180L_MoveTree = -1;
        public static int TRANS_RunToIdle_LDown_IdlePose = -1;
        public static int TRANS_RunToIdle_LDown_MoveTree = -1;
        public static int TRANS_WalkToIdle_LDown_MoveTree = -1;
        public static int TRANS_WalkToIdle_LDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_RDown_MoveTree = -1;
        public static int TRANS_WalkToIdle_RDown_IdlePose = -1;
        public static int TRANS_RunToIdle_RDown_MoveTree = -1;
        public static int TRANS_RunToIdle_RDown_IdlePose = -1;
        public static int TRANS_IdleTurn20R_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurn90R_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurn180R_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurn20L_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurn90L_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurn180L_IdleTurnEndPose = -1;
        public static int TRANS_IdleTurnEndPose_MoveTree = -1;

        /// <summary>
        /// Determines if we're using auto-generated code
        /// </summary>
        public override bool HasAutoGeneratedCode
        {
            get { return true; }
        }

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

                if (lStateID == STATE_MoveTree) { return true; }
                if (lStateID == STATE_IdleToWalk90L) { return true; }
                if (lStateID == STATE_IdleToWalk90R) { return true; }
                if (lStateID == STATE_IdleToWalk180R) { return true; }
                if (lStateID == STATE_IdleToWalk180L) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_IdleToRun90L) { return true; }
                if (lStateID == STATE_IdleToRun180L) { return true; }
                if (lStateID == STATE_IdleToRun90R) { return true; }
                if (lStateID == STATE_IdleToRun180R) { return true; }
                if (lStateID == STATE_IdleToRun) { return true; }
                if (lStateID == STATE_RunPivot180R_LDown) { return true; }
                if (lStateID == STATE_WalkPivot180L) { return true; }
                if (lStateID == STATE_RunToIdle_LDown) { return true; }
                if (lStateID == STATE_WalkToIdle_LDown) { return true; }
                if (lStateID == STATE_WalkToIdle_RDown) { return true; }
                if (lStateID == STATE_RunToIdle_RDown) { return true; }
                if (lStateID == STATE_IdleTurn20R) { return true; }
                if (lStateID == STATE_IdleTurn90R) { return true; }
                if (lStateID == STATE_IdleTurn180R) { return true; }
                if (lStateID == STATE_IdleTurn20L) { return true; }
                if (lStateID == STATE_IdleTurn90L) { return true; }
                if (lStateID == STATE_IdleTurn180L) { return true; }
                if (lStateID == STATE_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_AnyState_MoveTree) { return true; }
                if (lTransitionID == TRANS_EntryState_MoveTree) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk180L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk180L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun180L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun180L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun180R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_AnyState_MoveTree) { return true; }
                if (lTransitionID == TRANS_EntryState_MoveTree) { return true; }
                if (lTransitionID == TRANS_AnyState_MoveTree) { return true; }
                if (lTransitionID == TRANS_EntryState_MoveTree) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkPivot180L) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkPivot180L) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_RunToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_MoveTree_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdleToRun_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_LDown_MoveTree) { return true; }
                if (lTransitionID == TRANS_WalkPivot180L_MoveTree) { return true; }
                if (lTransitionID == TRANS_RunToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunToIdle_LDown_MoveTree) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_MoveTree) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_MoveTree) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunToIdle_RDown_MoveTree) { return true; }
                if (lTransitionID == TRANS_RunToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdleTurnEndPose) { return true; }
                if (lTransitionID == TRANS_IdleTurnEndPose_MoveTree) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_MoveTree) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkPivot180L) { return true; }
            if (rStateID == STATE_RunToIdle_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_RunToIdle_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleTurnEndPose) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_MoveTree) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkPivot180L) { return true; }
            if (rStateID == STATE_RunToIdle_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_RunToIdle_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_AnyState_MoveTree) { return true; }
            if (rTransitionID == TRANS_EntryState_MoveTree) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk180L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk180L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun180L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun180L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun180R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_AnyState_MoveTree) { return true; }
            if (rTransitionID == TRANS_EntryState_MoveTree) { return true; }
            if (rTransitionID == TRANS_AnyState_MoveTree) { return true; }
            if (rTransitionID == TRANS_EntryState_MoveTree) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkPivot180L) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkPivot180L) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_RunToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_MoveTree_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdleToRun_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_LDown_MoveTree) { return true; }
            if (rTransitionID == TRANS_WalkPivot180L_MoveTree) { return true; }
            if (rTransitionID == TRANS_RunToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunToIdle_LDown_MoveTree) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_MoveTree) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_MoveTree) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunToIdle_RDown_MoveTree) { return true; }
            if (rTransitionID == TRANS_RunToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdleTurnEndPose) { return true; }
            if (rTransitionID == TRANS_IdleTurnEndPose_MoveTree) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            TRANS_AnyState_IdleToWalk90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToWalk90L");
            TRANS_EntryState_IdleToWalk90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToWalk90L");
            TRANS_AnyState_IdleToWalk90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToWalk90R");
            TRANS_EntryState_IdleToWalk90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToWalk90R");
            TRANS_AnyState_IdleToWalk180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToWalk180R");
            TRANS_EntryState_IdleToWalk180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToWalk180R");
            TRANS_AnyState_MoveTree = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_EntryState_MoveTree = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_AnyState_IdleToWalk180L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToWalk180L");
            TRANS_EntryState_IdleToWalk180L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToWalk180L");
            TRANS_AnyState_IdleToRun180L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToRun180L");
            TRANS_EntryState_IdleToRun180L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToRun180L");
            TRANS_AnyState_IdleToRun90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToRun90L");
            TRANS_EntryState_IdleToRun90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToRun90L");
            TRANS_AnyState_IdleToRun90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToRun90R");
            TRANS_EntryState_IdleToRun90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToRun90R");
            TRANS_AnyState_IdleToRun180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToRun180R");
            TRANS_EntryState_IdleToRun180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToRun180R");
            TRANS_AnyState_IdleToRun = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleToRun");
            TRANS_EntryState_IdleToRun = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleToRun");
            TRANS_AnyState_MoveTree = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_EntryState_MoveTree = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_AnyState_MoveTree = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_EntryState_MoveTree = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_AnyState_IdleTurn180L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn180L");
            TRANS_EntryState_IdleTurn180L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn180L");
            TRANS_AnyState_IdleTurn90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn90L");
            TRANS_EntryState_IdleTurn90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn90L");
            TRANS_AnyState_IdleTurn20L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn20L");
            TRANS_EntryState_IdleTurn20L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn20L");
            TRANS_AnyState_IdleTurn20R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn20R");
            TRANS_EntryState_IdleTurn20R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn20R");
            TRANS_AnyState_IdleTurn90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn90R");
            TRANS_EntryState_IdleTurn90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn90R");
            TRANS_AnyState_IdleTurn180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunPivot v2-SM.IdleTurn180R");
            TRANS_EntryState_IdleTurn180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunPivot v2-SM.IdleTurn180R");
            STATE_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_MoveTree_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunPivot180R_LDown");
            TRANS_MoveTree_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunPivot180R_LDown");
            TRANS_MoveTree_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkPivot180L");
            TRANS_MoveTree_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkPivot180L");
            TRANS_MoveTree_RunToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunToIdle_LDown");
            TRANS_MoveTree_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkToIdle_LDown");
            TRANS_MoveTree_RunToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunToIdle_RDown");
            TRANS_MoveTree_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkToIdle_RDown");
            TRANS_MoveTree_RunToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunToIdle_RDown");
            TRANS_MoveTree_RunToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.RunToIdle_LDown");
            TRANS_MoveTree_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkToIdle_RDown");
            TRANS_MoveTree_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.Move Tree -> Base Layer.WalkRunPivot v2-SM.WalkToIdle_LDown");
            STATE_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90L");
            TRANS_IdleToWalk90L_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90L -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToWalk90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90L -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90R");
            TRANS_IdleToWalk90R_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90R -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToWalk90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk90R -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180R");
            TRANS_IdleToWalk180R_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180R -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToWalk180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180R -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180L");
            TRANS_IdleToWalk180L_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180L -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToWalk180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToWalk180L -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90L");
            TRANS_IdleToRun90L_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90L -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToRun90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90L -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180L");
            TRANS_IdleToRun180L_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180L -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToRun180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180L -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90R");
            TRANS_IdleToRun90R_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90R -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToRun90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun90R -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180R");
            TRANS_IdleToRun180R_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180R -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToRun180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun180R -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun");
            TRANS_IdleToRun_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_IdleToRun_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleToRun -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunPivot180R_LDown");
            TRANS_RunPivot180R_LDown_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunPivot180R_LDown -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            STATE_WalkPivot180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkPivot180L");
            TRANS_WalkPivot180L_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkPivot180L -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            STATE_RunToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_LDown");
            TRANS_RunToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_LDown -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            TRANS_RunToIdle_LDown_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_LDown -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            STATE_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_LDown");
            TRANS_WalkToIdle_LDown_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_LDown -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_WalkToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_LDown -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_RDown");
            TRANS_WalkToIdle_RDown_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_RDown -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_WalkToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.WalkToIdle_RDown -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_RunToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_RDown");
            TRANS_RunToIdle_RDown_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_RDown -> Base Layer.WalkRunPivot v2-SM.Move Tree");
            TRANS_RunToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.RunToIdle_RDown -> Base Layer.WalkRunPivot v2-SM.IdlePose");
            STATE_IdleTurn20R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn20R");
            TRANS_IdleTurn20R_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn20R -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurn90R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn90R");
            TRANS_IdleTurn90R_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn90R -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurn180R = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn180R");
            TRANS_IdleTurn180R_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn180R -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurn20L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn20L");
            TRANS_IdleTurn20L_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn20L -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurn90L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn90L");
            TRANS_IdleTurn90L_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn90L -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurn180L = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn180L");
            TRANS_IdleTurn180L_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurn180L -> Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            STATE_IdleTurnEndPose = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose");
            TRANS_IdleTurnEndPose_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunPivot v2-SM.IdleTurnEndPose -> Base Layer.WalkRunPivot v2-SM.Move Tree");
        }

#if UNITY_EDITOR

        private AnimationClip m13260 = null;
        private AnimationClip m17382 = null;
        private AnimationClip m13224 = null;
        private AnimationClip m19272 = null;
        private AnimationClip m19274 = null;
        private AnimationClip m19278 = null;
        private AnimationClip m19276 = null;
        private AnimationClip m16604 = null;
        private AnimationClip m16608 = null;
        private AnimationClip m16606 = null;
        private AnimationClip m16610 = null;
        private AnimationClip m13222 = null;
        private AnimationClip m16520 = null;
        private AnimationClip m19282 = null;
        private AnimationClip m16028 = null;
        private AnimationClip m19286 = null;
        private AnimationClip m19288 = null;
        private AnimationClip m17970 = null;
        private AnimationClip m13270 = null;
        private AnimationClip m13274 = null;
        private AnimationClip m13268 = null;
        private AnimationClip m13272 = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lSM_19610 = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lRootSubStateMachine = null;

            // If we find the sm with our name, remove it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                // Look for a sm with the matching name
                if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)
                {
                    lRootSubStateMachine = lRootStateMachine.stateMachines[i].stateMachine;

                    // Allow the user to stop before we remove the sm
                    if (!UnityEditor.EditorUtility.DisplayDialog("Motion Controller", _EditorAnimatorSMName + " already exists. Delete and recreate it?", "Yes", "No"))
                    {
                        return;
                    }

                    // Remove the sm
                    //lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);
                    break;
                }
            }

            UnityEditor.Animations.AnimatorStateMachine lSM_19664 = lRootSubStateMachine;
            if (lSM_19664 != null)
            {
                for (int i = lSM_19664.entryTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_19664.RemoveEntryTransition(lSM_19664.entryTransitions[i]);
                }

                for (int i = lSM_19664.anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_19664.RemoveAnyStateTransition(lSM_19664.anyStateTransitions[i]);
                }

                for (int i = lSM_19664.states.Length - 1; i >= 0; i--)
                {
                    lSM_19664.RemoveState(lSM_19664.states[i].state);
                }

                for (int i = lSM_19664.stateMachines.Length - 1; i >= 0; i--)
                {
                    lSM_19664.RemoveStateMachine(lSM_19664.stateMachines[i].stateMachine);
                }
            }
            else
            {
                lSM_19664 = lSM_19610.AddStateMachine(_EditorAnimatorSMName, new Vector3(624, -756, 0));
            }

            UnityEditor.Animations.AnimatorState lS_19960 = lSM_19664.AddState("Move Tree", new Vector3(240, 372, 0));
            lS_19960.speed = 1f;

            UnityEditor.Animations.BlendTree lM_14372 = CreateBlendTree("Move Blend Tree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_14372.blendType = UnityEditor.Animations.BlendTreeType.Simple1D;
            lM_14372.blendParameter = "InputMagnitude";
            lM_14372.blendParameterY = "InputX";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_14372.useAutomaticThresholds = false;
#endif
            lM_14372.AddChild(m13260, 0f);
            lM_14372.AddChild(m17382, 0.5f);
            lM_14372.AddChild(m13224, 1f);
            lS_19960.motion = lM_14372;

            UnityEditor.Animations.AnimatorState lS_19954 = lSM_19664.AddState("IdleToWalk90L", new Vector3(-180, 204, 0));
            lS_19954.speed = 1.3f;
            lS_19954.motion = m19272;

            UnityEditor.Animations.AnimatorState lS_19956 = lSM_19664.AddState("IdleToWalk90R", new Vector3(-180, 264, 0));
            lS_19956.speed = 1.3f;
            lS_19956.motion = m19274;

            UnityEditor.Animations.AnimatorState lS_19958 = lSM_19664.AddState("IdleToWalk180R", new Vector3(-180, 324, 0));
            lS_19958.speed = 1.3f;
            lS_19958.motion = m19278;

            UnityEditor.Animations.AnimatorState lS_19962 = lSM_19664.AddState("IdleToWalk180L", new Vector3(-180, 144, 0));
            lS_19962.speed = 1.3f;
            lS_19962.motion = m19276;

            UnityEditor.Animations.AnimatorState lS_20836 = lSM_19664.AddState("IdlePose", new Vector3(132, 216, 0));
            lS_20836.speed = 1f;
            lS_20836.motion = m13260;

            UnityEditor.Animations.AnimatorState lS_19966 = lSM_19664.AddState("IdleToRun90L", new Vector3(-168, 492, 0));
            lS_19966.speed = 1.5f;
            lS_19966.motion = m16604;

            UnityEditor.Animations.AnimatorState lS_19964 = lSM_19664.AddState("IdleToRun180L", new Vector3(-168, 432, 0));
            lS_19964.speed = 1.3f;
            lS_19964.motion = m16608;

            UnityEditor.Animations.AnimatorState lS_19968 = lSM_19664.AddState("IdleToRun90R", new Vector3(-168, 612, 0));
            lS_19968.speed = 1.5f;
            lS_19968.motion = m16606;

            UnityEditor.Animations.AnimatorState lS_19970 = lSM_19664.AddState("IdleToRun180R", new Vector3(-168, 672, 0));
            lS_19970.speed = 1.3f;
            lS_19970.motion = m16610;

            UnityEditor.Animations.AnimatorState lS_19972 = lSM_19664.AddState("IdleToRun", new Vector3(-168, 552, 0));
            lS_19972.speed = 2f;
            lS_19972.motion = m13222;

            UnityEditor.Animations.AnimatorState lS_20838 = lSM_19664.AddState("RunPivot180R_LDown", new Vector3(144, 564, 0));
            lS_20838.speed = 1.2f;
            lS_20838.motion = m16520;

            UnityEditor.Animations.AnimatorState lS_20840 = lSM_19664.AddState("WalkPivot180L", new Vector3(360, 564, 0));
            lS_20840.speed = 1.5f;
            lS_20840.motion = m19282;

            UnityEditor.Animations.AnimatorState lS_20842 = lSM_19664.AddState("RunToIdle_LDown", new Vector3(576, 336, 0));
            lS_20842.speed = 1f;
            lS_20842.motion = m16028;

            UnityEditor.Animations.AnimatorState lS_20844 = lSM_19664.AddState("WalkToIdle_LDown", new Vector3(576, 492, 0));
            lS_20844.speed = 1f;
            lS_20844.motion = m19286;

            UnityEditor.Animations.AnimatorState lS_20846 = lSM_19664.AddState("WalkToIdle_RDown", new Vector3(576, 420, 0));
            lS_20846.speed = 1f;
            lS_20846.motion = m19288;

            UnityEditor.Animations.AnimatorState lS_20848 = lSM_19664.AddState("RunToIdle_RDown", new Vector3(576, 264, 0));
            lS_20848.speed = 1f;
            lS_20848.motion = m17970;

            UnityEditor.Animations.AnimatorState lS_N50738 = lSM_19664.AddState("IdleTurn20R", new Vector3(-720, 408, 0));
            lS_N50738.speed = 1f;
            lS_N50738.motion = m13270;

            UnityEditor.Animations.AnimatorState lS_N50740 = lSM_19664.AddState("IdleTurn90R", new Vector3(-720, 468, 0));
            lS_N50740.speed = 1.6f;
            lS_N50740.motion = m13270;

            UnityEditor.Animations.AnimatorState lS_N50742 = lSM_19664.AddState("IdleTurn180R", new Vector3(-720, 528, 0));
            lS_N50742.speed = 1.4f;
            lS_N50742.motion = m13274;

            UnityEditor.Animations.AnimatorState lS_N50744 = lSM_19664.AddState("IdleTurn20L", new Vector3(-720, 348, 0));
            lS_N50744.speed = 1f;
            lS_N50744.motion = m13268;

            UnityEditor.Animations.AnimatorState lS_N50746 = lSM_19664.AddState("IdleTurn90L", new Vector3(-720, 288, 0));
            lS_N50746.speed = 1.6f;
            lS_N50746.motion = m13268;

            UnityEditor.Animations.AnimatorState lS_N50748 = lSM_19664.AddState("IdleTurn180L", new Vector3(-720, 228, 0));
            lS_N50748.speed = 1.4f;
            lS_N50748.motion = m13272;

            UnityEditor.Animations.AnimatorState lS_N56374 = lSM_19664.AddState("IdleTurnEndPose", new Vector3(-984, 372, 0));
            lS_N56374.speed = 1f;
            lS_N56374.motion = m13260;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19788 = lRootStateMachine.AddAnyStateTransition(lS_19954);
            lT_19788.hasExitTime = false;
            lT_19788.hasFixedDuration = true;
            lT_19788.exitTime = 0.9f;
            lT_19788.duration = 0.1f;
            lT_19788.offset = 0f;
            lT_19788.mute = false;
            lT_19788.solo = false;
            lT_19788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -20f, "InputAngleFromAvatar");
            lT_19788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");
            lT_19788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19790 = lRootStateMachine.AddAnyStateTransition(lS_19956);
            lT_19790.hasExitTime = false;
            lT_19790.hasFixedDuration = true;
            lT_19790.exitTime = 0.9f;
            lT_19790.duration = 0.1f;
            lT_19790.offset = 0f;
            lT_19790.mute = false;
            lT_19790.solo = false;
            lT_19790.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19790.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19790.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 20f, "InputAngleFromAvatar");
            lT_19790.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");
            lT_19790.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19792 = lRootStateMachine.AddAnyStateTransition(lS_19958);
            lT_19792.hasExitTime = false;
            lT_19792.hasFixedDuration = true;
            lT_19792.exitTime = 0.9f;
            lT_19792.duration = 0.1f;
            lT_19792.offset = 0f;
            lT_19792.mute = false;
            lT_19792.solo = false;
            lT_19792.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19792.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19792.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");
            lT_19792.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19794 = lRootStateMachine.AddAnyStateTransition(lS_19960);
            lT_19794.hasExitTime = false;
            lT_19794.hasFixedDuration = true;
            lT_19794.exitTime = 0.9f;
            lT_19794.duration = 0.1f;
            lT_19794.offset = 0f;
            lT_19794.mute = false;
            lT_19794.solo = false;
            lT_19794.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19794.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -20f, "InputAngleFromAvatar");
            lT_19794.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 20f, "InputAngleFromAvatar");
            lT_19794.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19796 = lRootStateMachine.AddAnyStateTransition(lS_19962);
            lT_19796.hasExitTime = false;
            lT_19796.hasFixedDuration = true;
            lT_19796.exitTime = 0.9f;
            lT_19796.duration = 0.1f;
            lT_19796.offset = 0f;
            lT_19796.mute = false;
            lT_19796.solo = false;
            lT_19796.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19796.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19796.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");
            lT_19796.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19798 = lRootStateMachine.AddAnyStateTransition(lS_19964);
            lT_19798.hasExitTime = false;
            lT_19798.hasFixedDuration = true;
            lT_19798.exitTime = 0.9f;
            lT_19798.duration = 0.1f;
            lT_19798.offset = 0f;
            lT_19798.mute = false;
            lT_19798.solo = false;
            lT_19798.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19798.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19798.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");
            lT_19798.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19800 = lRootStateMachine.AddAnyStateTransition(lS_19966);
            lT_19800.hasExitTime = false;
            lT_19800.hasFixedDuration = true;
            lT_19800.exitTime = 0.9f;
            lT_19800.duration = 0.1f;
            lT_19800.offset = 0f;
            lT_19800.mute = false;
            lT_19800.solo = false;
            lT_19800.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19800.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19800.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -20f, "InputAngleFromAvatar");
            lT_19800.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");
            lT_19800.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19802 = lRootStateMachine.AddAnyStateTransition(lS_19968);
            lT_19802.hasExitTime = false;
            lT_19802.hasFixedDuration = true;
            lT_19802.exitTime = 0.9f;
            lT_19802.duration = 0.1f;
            lT_19802.offset = 0f;
            lT_19802.mute = false;
            lT_19802.solo = false;
            lT_19802.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19802.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19802.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 20f, "InputAngleFromAvatar");
            lT_19802.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");
            lT_19802.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19804 = lRootStateMachine.AddAnyStateTransition(lS_19970);
            lT_19804.hasExitTime = false;
            lT_19804.hasFixedDuration = true;
            lT_19804.exitTime = 0.9f;
            lT_19804.duration = 0.1f;
            lT_19804.offset = 0f;
            lT_19804.mute = false;
            lT_19804.solo = false;
            lT_19804.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19804.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19804.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");
            lT_19804.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19806 = lRootStateMachine.AddAnyStateTransition(lS_19972);
            lT_19806.hasExitTime = false;
            lT_19806.hasFixedDuration = true;
            lT_19806.exitTime = 0.9f;
            lT_19806.duration = 0.1f;
            lT_19806.offset = 0f;
            lT_19806.mute = false;
            lT_19806.solo = false;
            lT_19806.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19806.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_19806.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -20f, "InputAngleFromAvatar");
            lT_19806.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 20f, "InputAngleFromAvatar");
            lT_19806.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19808 = lRootStateMachine.AddAnyStateTransition(lS_19960);
            lT_19808.hasExitTime = false;
            lT_19808.hasFixedDuration = true;
            lT_19808.exitTime = 0.9f;
            lT_19808.duration = 0.1f;
            lT_19808.offset = 0.5f;
            lT_19808.mute = false;
            lT_19808.solo = false;
            lT_19808.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19808.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 2f, "L0MotionParameter");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_19810 = lRootStateMachine.AddAnyStateTransition(lS_19960);
            lT_19810.hasExitTime = false;
            lT_19810.hasFixedDuration = true;
            lT_19810.exitTime = 0.9f;
            lT_19810.duration = 0.1f;
            lT_19810.offset = 0f;
            lT_19810.mute = false;
            lT_19810.solo = false;
            lT_19810.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27130f, "L0MotionPhase");
            lT_19810.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N564826 = lRootStateMachine.AddAnyStateTransition(lS_N50748);
            lT_N564826.hasExitTime = false;
            lT_N564826.hasFixedDuration = true;
            lT_N564826.exitTime = 0.9f;
            lT_N564826.duration = 0.05f;
            lT_N564826.offset = 0.2228713f;
            lT_N564826.mute = false;
            lT_N564826.solo = false;
            lT_N564826.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N564826.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -135f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N565214 = lRootStateMachine.AddAnyStateTransition(lS_N50746);
            lT_N565214.hasExitTime = false;
            lT_N565214.hasFixedDuration = true;
            lT_N565214.exitTime = 0.9f;
            lT_N565214.duration = 0.05f;
            lT_N565214.offset = 0.1442637f;
            lT_N565214.mute = false;
            lT_N565214.solo = false;
            lT_N565214.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N565214.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -45f, "InputAngleFromAvatar");
            lT_N565214.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -135f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N565602 = lRootStateMachine.AddAnyStateTransition(lS_N50744);
            lT_N565602.hasExitTime = false;
            lT_N565602.hasFixedDuration = true;
            lT_N565602.exitTime = 0.9f;
            lT_N565602.duration = 0.05f;
            lT_N565602.offset = 0.1442637f;
            lT_N565602.mute = false;
            lT_N565602.solo = false;
            lT_N565602.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N565602.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0f, "InputAngleFromAvatar");
            lT_N565602.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -45f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N565990 = lRootStateMachine.AddAnyStateTransition(lS_N50738);
            lT_N565990.hasExitTime = false;
            lT_N565990.hasFixedDuration = true;
            lT_N565990.exitTime = 0.9f;
            lT_N565990.duration = 0.05f;
            lT_N565990.offset = 0.2277291f;
            lT_N565990.mute = false;
            lT_N565990.solo = false;
            lT_N565990.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N565990.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0f, "InputAngleFromAvatar");
            lT_N565990.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 45f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N566378 = lRootStateMachine.AddAnyStateTransition(lS_N50740);
            lT_N566378.hasExitTime = false;
            lT_N566378.hasFixedDuration = true;
            lT_N566378.exitTime = 0.8999999f;
            lT_N566378.duration = 0.05000001f;
            lT_N566378.offset = 0.2277291f;
            lT_N566378.mute = false;
            lT_N566378.solo = false;
            lT_N566378.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N566378.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 45f, "InputAngleFromAvatar");
            lT_N566378.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 135f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N566766 = lRootStateMachine.AddAnyStateTransition(lS_N50742);
            lT_N566766.hasExitTime = false;
            lT_N566766.hasFixedDuration = true;
            lT_N566766.exitTime = 0.9f;
            lT_N566766.duration = 0.05f;
            lT_N566766.offset = 0.2689505f;
            lT_N566766.mute = false;
            lT_N566766.solo = false;
            lT_N566766.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27135f, "L0MotionPhase");
            lT_N566766.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 135f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_20850 = lS_19960.AddTransition(lS_20838);
            lT_20850.hasExitTime = false;
            lT_20850.hasFixedDuration = true;
            lT_20850.exitTime = 0.5481927f;
            lT_20850.duration = 0.1f;
            lT_20850.offset = 0f;
            lT_20850.mute = false;
            lT_20850.solo = false;
            lT_20850.canTransitionToSelf = true;
            lT_20850.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");
            lT_20850.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20852 = lS_19960.AddTransition(lS_20838);
            lT_20852.hasExitTime = false;
            lT_20852.hasFixedDuration = true;
            lT_20852.exitTime = 0.5481927f;
            lT_20852.duration = 0.1f;
            lT_20852.offset = 0f;
            lT_20852.mute = false;
            lT_20852.solo = false;
            lT_20852.canTransitionToSelf = true;
            lT_20852.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");
            lT_20852.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20854 = lS_19960.AddTransition(lS_20840);
            lT_20854.hasExitTime = false;
            lT_20854.hasFixedDuration = true;
            lT_20854.exitTime = 0.5481927f;
            lT_20854.duration = 0.1f;
            lT_20854.offset = 0f;
            lT_20854.mute = false;
            lT_20854.solo = false;
            lT_20854.canTransitionToSelf = true;
            lT_20854.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");
            lT_20854.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.2f, "InputMagnitude");
            lT_20854.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20856 = lS_19960.AddTransition(lS_20840);
            lT_20856.hasExitTime = false;
            lT_20856.hasFixedDuration = true;
            lT_20856.exitTime = 0.5481927f;
            lT_20856.duration = 0.1f;
            lT_20856.offset = 0f;
            lT_20856.mute = false;
            lT_20856.solo = false;
            lT_20856.canTransitionToSelf = true;
            lT_20856.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");
            lT_20856.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.2f, "InputMagnitude");
            lT_20856.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20858 = lS_19960.AddTransition(lS_20842);
            lT_20858.hasExitTime = true;
            lT_20858.hasFixedDuration = true;
            lT_20858.exitTime = 0.5f;
            lT_20858.duration = 0.2f;
            lT_20858.offset = 0.3595567f;
            lT_20858.mute = false;
            lT_20858.solo = false;
            lT_20858.canTransitionToSelf = true;
            lT_20858.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27131f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20860 = lS_19960.AddTransition(lS_20844);
            lT_20860.hasExitTime = true;
            lT_20860.hasFixedDuration = true;
            lT_20860.exitTime = 0.5f;
            lT_20860.duration = 0.2f;
            lT_20860.offset = 0.5352634f;
            lT_20860.mute = false;
            lT_20860.solo = false;
            lT_20860.canTransitionToSelf = true;
            lT_20860.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27132f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20862 = lS_19960.AddTransition(lS_20848);
            lT_20862.hasExitTime = true;
            lT_20862.hasFixedDuration = true;
            lT_20862.exitTime = 1f;
            lT_20862.duration = 0.2f;
            lT_20862.offset = 0f;
            lT_20862.mute = false;
            lT_20862.solo = false;
            lT_20862.canTransitionToSelf = true;
            lT_20862.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27131f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20864 = lS_19960.AddTransition(lS_20846);
            lT_20864.hasExitTime = true;
            lT_20864.hasFixedDuration = true;
            lT_20864.exitTime = 1f;
            lT_20864.duration = 0.2f;
            lT_20864.offset = 0.4974566f;
            lT_20864.mute = false;
            lT_20864.solo = false;
            lT_20864.canTransitionToSelf = true;
            lT_20864.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27132f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20866 = lS_19960.AddTransition(lS_20848);
            lT_20866.hasExitTime = true;
            lT_20866.hasFixedDuration = true;
            lT_20866.exitTime = 0.25f;
            lT_20866.duration = 0.2f;
            lT_20866.offset = 0.1060333f;
            lT_20866.mute = false;
            lT_20866.solo = false;
            lT_20866.canTransitionToSelf = true;
            lT_20866.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27131f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20868 = lS_19960.AddTransition(lS_20842);
            lT_20868.hasExitTime = true;
            lT_20868.hasFixedDuration = true;
            lT_20868.exitTime = 0.75f;
            lT_20868.duration = 0.2f;
            lT_20868.offset = 0.4174516f;
            lT_20868.mute = false;
            lT_20868.solo = false;
            lT_20868.canTransitionToSelf = true;
            lT_20868.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27131f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20870 = lS_19960.AddTransition(lS_20846);
            lT_20870.hasExitTime = true;
            lT_20870.hasFixedDuration = true;
            lT_20870.exitTime = 0.75f;
            lT_20870.duration = 0.2f;
            lT_20870.offset = 0.256667f;
            lT_20870.mute = false;
            lT_20870.solo = false;
            lT_20870.canTransitionToSelf = true;
            lT_20870.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27132f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20872 = lS_19960.AddTransition(lS_20844);
            lT_20872.hasExitTime = true;
            lT_20872.hasFixedDuration = true;
            lT_20872.exitTime = 0.25f;
            lT_20872.duration = 0.2f;
            lT_20872.offset = 0.2689477f;
            lT_20872.mute = false;
            lT_20872.solo = false;
            lT_20872.canTransitionToSelf = true;
            lT_20872.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27132f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20874 = lS_19954.AddTransition(lS_19960);
            lT_20874.hasExitTime = true;
            lT_20874.hasFixedDuration = true;
            lT_20874.exitTime = 0.75f;
            lT_20874.duration = 0.15f;
            lT_20874.offset = 0.0963606f;
            lT_20874.mute = false;
            lT_20874.solo = false;
            lT_20874.canTransitionToSelf = true;
            lT_20874.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20878 = lS_19954.AddTransition(lS_20836);
            lT_20878.hasExitTime = true;
            lT_20878.hasFixedDuration = true;
            lT_20878.exitTime = 0.8404255f;
            lT_20878.duration = 0.25f;
            lT_20878.offset = 0f;
            lT_20878.mute = false;
            lT_20878.solo = false;
            lT_20878.canTransitionToSelf = true;
            lT_20878.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20880 = lS_19956.AddTransition(lS_19960);
            lT_20880.hasExitTime = true;
            lT_20880.hasFixedDuration = true;
            lT_20880.exitTime = 0.75f;
            lT_20880.duration = 0.15f;
            lT_20880.offset = 0.6026077f;
            lT_20880.mute = false;
            lT_20880.solo = false;
            lT_20880.canTransitionToSelf = true;
            lT_20880.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20884 = lS_19956.AddTransition(lS_20836);
            lT_20884.hasExitTime = true;
            lT_20884.hasFixedDuration = true;
            lT_20884.exitTime = 0.7916668f;
            lT_20884.duration = 0.25f;
            lT_20884.offset = 0f;
            lT_20884.mute = false;
            lT_20884.solo = false;
            lT_20884.canTransitionToSelf = true;
            lT_20884.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20886 = lS_19958.AddTransition(lS_19960);
            lT_20886.hasExitTime = true;
            lT_20886.hasFixedDuration = true;
            lT_20886.exitTime = 0.8846154f;
            lT_20886.duration = 0.25f;
            lT_20886.offset = 0.8864383f;
            lT_20886.mute = false;
            lT_20886.solo = false;
            lT_20886.canTransitionToSelf = true;
            lT_20886.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20890 = lS_19958.AddTransition(lS_20836);
            lT_20890.hasExitTime = true;
            lT_20890.hasFixedDuration = true;
            lT_20890.exitTime = 0.8584907f;
            lT_20890.duration = 0.25f;
            lT_20890.offset = 0f;
            lT_20890.mute = false;
            lT_20890.solo = false;
            lT_20890.canTransitionToSelf = true;
            lT_20890.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20892 = lS_19962.AddTransition(lS_19960);
            lT_20892.hasExitTime = true;
            lT_20892.hasFixedDuration = true;
            lT_20892.exitTime = 0.9074074f;
            lT_20892.duration = 0.25f;
            lT_20892.offset = 0.3468954f;
            lT_20892.mute = false;
            lT_20892.solo = false;
            lT_20892.canTransitionToSelf = true;
            lT_20892.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20896 = lS_19962.AddTransition(lS_20836);
            lT_20896.hasExitTime = true;
            lT_20896.hasFixedDuration = true;
            lT_20896.exitTime = 0.8584907f;
            lT_20896.duration = 0.25f;
            lT_20896.offset = 0f;
            lT_20896.mute = false;
            lT_20896.solo = false;
            lT_20896.canTransitionToSelf = true;
            lT_20896.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20906 = lS_19966.AddTransition(lS_19960);
            lT_20906.hasExitTime = true;
            lT_20906.hasFixedDuration = true;
            lT_20906.exitTime = 0.7222224f;
            lT_20906.duration = 0.25f;
            lT_20906.offset = 0f;
            lT_20906.mute = false;
            lT_20906.solo = false;
            lT_20906.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20910 = lS_19966.AddTransition(lS_20836);
            lT_20910.hasExitTime = true;
            lT_20910.hasFixedDuration = true;
            lT_20910.exitTime = 0.7794119f;
            lT_20910.duration = 0.25f;
            lT_20910.offset = 0f;
            lT_20910.mute = false;
            lT_20910.solo = false;
            lT_20910.canTransitionToSelf = true;
            lT_20910.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20912 = lS_19964.AddTransition(lS_19960);
            lT_20912.hasExitTime = true;
            lT_20912.hasFixedDuration = true;
            lT_20912.exitTime = 0.7580653f;
            lT_20912.duration = 0.25f;
            lT_20912.offset = 0f;
            lT_20912.mute = false;
            lT_20912.solo = false;
            lT_20912.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20916 = lS_19964.AddTransition(lS_20836);
            lT_20916.hasExitTime = true;
            lT_20916.hasFixedDuration = true;
            lT_20916.exitTime = 0.8125004f;
            lT_20916.duration = 0.25f;
            lT_20916.offset = 0f;
            lT_20916.mute = false;
            lT_20916.solo = false;
            lT_20916.canTransitionToSelf = true;
            lT_20916.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20918 = lS_19968.AddTransition(lS_19960);
            lT_20918.hasExitTime = true;
            lT_20918.hasFixedDuration = true;
            lT_20918.exitTime = 0.7580646f;
            lT_20918.duration = 0.25f;
            lT_20918.offset = 0.5379788f;
            lT_20918.mute = false;
            lT_20918.solo = false;
            lT_20918.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20922 = lS_19968.AddTransition(lS_20836);
            lT_20922.hasExitTime = true;
            lT_20922.hasFixedDuration = true;
            lT_20922.exitTime = 0.7794119f;
            lT_20922.duration = 0.25f;
            lT_20922.offset = 0f;
            lT_20922.mute = false;
            lT_20922.solo = false;
            lT_20922.canTransitionToSelf = true;
            lT_20922.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20924 = lS_19970.AddTransition(lS_19960);
            lT_20924.hasExitTime = true;
            lT_20924.hasFixedDuration = true;
            lT_20924.exitTime = 0.8255816f;
            lT_20924.duration = 0.25f;
            lT_20924.offset = 0.5181294f;
            lT_20924.mute = false;
            lT_20924.solo = false;
            lT_20924.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20928 = lS_19970.AddTransition(lS_20836);
            lT_20928.hasExitTime = true;
            lT_20928.hasFixedDuration = true;
            lT_20928.exitTime = 0.8125004f;
            lT_20928.duration = 0.25f;
            lT_20928.offset = 0f;
            lT_20928.mute = false;
            lT_20928.solo = false;
            lT_20928.canTransitionToSelf = true;
            lT_20928.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20932 = lS_19972.AddTransition(lS_19960);
            lT_20932.hasExitTime = true;
            lT_20932.hasFixedDuration = true;
            lT_20932.exitTime = 0.6182807f;
            lT_20932.duration = 0.25f;
            lT_20932.offset = 0.02634108f;
            lT_20932.mute = false;
            lT_20932.solo = false;
            lT_20932.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20934 = lS_19972.AddTransition(lS_20836);
            lT_20934.hasExitTime = true;
            lT_20934.hasFixedDuration = true;
            lT_20934.exitTime = 0.6250002f;
            lT_20934.duration = 0.25f;
            lT_20934.offset = 0f;
            lT_20934.mute = false;
            lT_20934.solo = false;
            lT_20934.canTransitionToSelf = true;
            lT_20934.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.4f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_20938 = lS_20838.AddTransition(lS_19960);
            lT_20938.hasExitTime = true;
            lT_20938.hasFixedDuration = true;
            lT_20938.exitTime = 0.8469388f;
            lT_20938.duration = 0.25f;
            lT_20938.offset = 0f;
            lT_20938.mute = false;
            lT_20938.solo = false;
            lT_20938.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20940 = lS_20840.AddTransition(lS_19960);
            lT_20940.hasExitTime = true;
            lT_20940.hasFixedDuration = true;
            lT_20940.exitTime = 0.8636364f;
            lT_20940.duration = 0.25f;
            lT_20940.offset = 0.8593867f;
            lT_20940.mute = false;
            lT_20940.solo = false;
            lT_20940.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20944 = lS_20842.AddTransition(lS_20836);
            lT_20944.hasExitTime = true;
            lT_20944.hasFixedDuration = true;
            lT_20944.exitTime = 0.7f;
            lT_20944.duration = 0.2f;
            lT_20944.offset = 0f;
            lT_20944.mute = false;
            lT_20944.solo = false;
            lT_20944.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20946 = lS_20842.AddTransition(lS_19960);
            lT_20946.hasExitTime = false;
            lT_20946.hasFixedDuration = true;
            lT_20946.exitTime = 0.8684211f;
            lT_20946.duration = 0.25f;
            lT_20946.offset = 0f;
            lT_20946.mute = false;
            lT_20946.solo = false;
            lT_20946.canTransitionToSelf = true;
            lT_20946.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27133f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20948 = lS_20844.AddTransition(lS_19960);
            lT_20948.hasExitTime = false;
            lT_20948.hasFixedDuration = true;
            lT_20948.exitTime = 0.75f;
            lT_20948.duration = 0.25f;
            lT_20948.offset = 0f;
            lT_20948.mute = false;
            lT_20948.solo = false;
            lT_20948.canTransitionToSelf = true;
            lT_20948.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27133f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20950 = lS_20844.AddTransition(lS_20836);
            lT_20950.hasExitTime = true;
            lT_20950.hasFixedDuration = true;
            lT_20950.exitTime = 0.8f;
            lT_20950.duration = 0.2f;
            lT_20950.offset = 0f;
            lT_20950.mute = false;
            lT_20950.solo = false;
            lT_20950.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20952 = lS_20846.AddTransition(lS_19960);
            lT_20952.hasExitTime = false;
            lT_20952.hasFixedDuration = true;
            lT_20952.exitTime = 0.75f;
            lT_20952.duration = 0.25f;
            lT_20952.offset = 0f;
            lT_20952.mute = false;
            lT_20952.solo = false;
            lT_20952.canTransitionToSelf = true;
            lT_20952.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27133f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20954 = lS_20846.AddTransition(lS_20836);
            lT_20954.hasExitTime = true;
            lT_20954.hasFixedDuration = true;
            lT_20954.exitTime = 0.8f;
            lT_20954.duration = 0.2f;
            lT_20954.offset = 0f;
            lT_20954.mute = false;
            lT_20954.solo = false;
            lT_20954.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_20956 = lS_20848.AddTransition(lS_19960);
            lT_20956.hasExitTime = false;
            lT_20956.hasFixedDuration = true;
            lT_20956.exitTime = 0.8170732f;
            lT_20956.duration = 0.25f;
            lT_20956.offset = 0f;
            lT_20956.mute = false;
            lT_20956.solo = false;
            lT_20956.canTransitionToSelf = true;
            lT_20956.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27133f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_20958 = lS_20848.AddTransition(lS_20836);
            lT_20958.hasExitTime = true;
            lT_20958.hasFixedDuration = true;
            lT_20958.exitTime = 0.5021765f;
            lT_20958.duration = 0.1999999f;
            lT_20958.offset = 0.04457206f;
            lT_20958.mute = false;
            lT_20958.solo = false;
            lT_20958.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N58220 = lS_N50738.AddTransition(lS_N56374);
            lT_N58220.hasExitTime = true;
            lT_N58220.hasFixedDuration = true;
            lT_N58220.exitTime = 0.3138752f;
            lT_N58220.duration = 0.15f;
            lT_N58220.offset = 0f;
            lT_N58220.mute = false;
            lT_N58220.solo = false;
            lT_N58220.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N58526 = lS_N50740.AddTransition(lS_N56374);
            lT_N58526.hasExitTime = true;
            lT_N58526.hasFixedDuration = true;
            lT_N58526.exitTime = 0.5643811f;
            lT_N58526.duration = 0.15f;
            lT_N58526.offset = 0f;
            lT_N58526.mute = false;
            lT_N58526.solo = false;
            lT_N58526.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N58832 = lS_N50742.AddTransition(lS_N56374);
            lT_N58832.hasExitTime = true;
            lT_N58832.hasFixedDuration = true;
            lT_N58832.exitTime = 0.7016318f;
            lT_N58832.duration = 0.15f;
            lT_N58832.offset = 0f;
            lT_N58832.mute = false;
            lT_N58832.solo = false;
            lT_N58832.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N57916 = lS_N50744.AddTransition(lS_N56374);
            lT_N57916.hasExitTime = true;
            lT_N57916.hasFixedDuration = true;
            lT_N57916.exitTime = 0.2468245f;
            lT_N57916.duration = 0.15f;
            lT_N57916.offset = 0f;
            lT_N57916.mute = false;
            lT_N57916.solo = false;
            lT_N57916.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N57610 = lS_N50746.AddTransition(lS_N56374);
            lT_N57610.hasExitTime = true;
            lT_N57610.hasFixedDuration = true;
            lT_N57610.exitTime = 0.5180793f;
            lT_N57610.duration = 0.15f;
            lT_N57610.offset = 0f;
            lT_N57610.mute = false;
            lT_N57610.solo = false;
            lT_N57610.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N57274 = lS_N50748.AddTransition(lS_N56374);
            lT_N57274.hasExitTime = true;
            lT_N57274.hasFixedDuration = true;
            lT_N57274.exitTime = 0.6774405f;
            lT_N57274.duration = 0.15f;
            lT_N57274.offset = 0f;
            lT_N57274.mute = false;
            lT_N57274.solo = false;
            lT_N57274.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N964602 = lS_N56374.AddTransition(lS_19960);
            lT_N964602.hasExitTime = false;
            lT_N964602.hasFixedDuration = true;
            lT_N964602.exitTime = 0f;
            lT_N964602.duration = 0.1f;
            lT_N964602.offset = 0f;
            lT_N964602.mute = false;
            lT_N964602.solo = false;
            lT_N964602.canTransitionToSelf = true;
            lT_N964602.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");

        }

        /// <summary>
        /// Gathers the animations so we can use them when creating the sub-state machine.
        /// </summary>
        public override void FindAnimations()
        {
            m13260 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose");
            m17382 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD_v2.fbx/WalkForward.anim", "WalkForward");
            m13224 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/RunForward.anim", "RunForward");
            m19272 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk90L.anim", "IdleToWalk90L");
            m19274 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk90R.anim", "IdleToWalk90R");
            m19278 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk180R.anim", "IdleToWalk180R");
            m19276 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk180L.anim", "IdleToWalk180L");
            m16604 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun90L.anim", "IdleToRun90L");
            m16608 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun180L.anim", "IdleToRun180L");
            m16606 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun90R.anim", "IdleToRun90R");
            m16610 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun180R.anim", "IdleToRun180R");
            m13222 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/IdleToRun.anim", "IdleToRun");
            m16520 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_1.fbx/RunPivot180R_LDown.anim", "RunPivot180R_LDown");
            m19282 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkPivot180L.anim", "WalkPivot180L");
            m16028 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_2.fbx/RunToIdle_LDown.anim", "RunToIdle_LDown");
            m19286 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkToIdle_LDown.anim", "WalkToIdle_LDown");
            m19288 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkToIdle_RDown.anim", "WalkToIdle_RDown");
            m17970 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_HalfSteps2Idle_PasingLongStepTOIdle.fbx/RunToIdle_RDown.anim", "RunToIdle_RDown");
            m13270 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90R.anim", "IdleTurn90R");
            m13274 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180R.anim", "IdleTurn180R");
            m13268 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90L.anim", "IdleTurn90L");
            m13272 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180L.anim", "IdleTurn180L");

            // Add the remaining functionality
            base.FindAnimations();
        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m13260 = CreateAnimationField("Move Tree.IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", m13260);
            m17382 = CreateAnimationField("Move Tree.WalkForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD_v2.fbx/WalkForward.anim", "WalkForward", m17382);
            m13224 = CreateAnimationField("Move Tree.RunForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/RunForward.anim", "RunForward", m13224);
            m19272 = CreateAnimationField("IdleToWalk90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk90L.anim", "IdleToWalk90L", m19272);
            m19274 = CreateAnimationField("IdleToWalk90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk90R.anim", "IdleToWalk90R", m19274);
            m19278 = CreateAnimationField("IdleToWalk180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk180R.anim", "IdleToWalk180R", m19278);
            m19276 = CreateAnimationField("IdleToWalk180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/IdleToWalk180L.anim", "IdleToWalk180L", m19276);
            m16604 = CreateAnimationField("IdleToRun90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun90L.anim", "IdleToRun90L", m16604);
            m16608 = CreateAnimationField("IdleToRun180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun180L.anim", "IdleToRun180L", m16608);
            m16606 = CreateAnimationField("IdleToRun90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun90R.anim", "IdleToRun90R", m16606);
            m16610 = CreateAnimationField("IdleToRun180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_Idle2Run_v2.fbx/IdleToRun180R.anim", "IdleToRun180R", m16610);
            m13222 = CreateAnimationField("IdleToRun", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/IdleToRun.anim", "IdleToRun", m13222);
            m16520 = CreateAnimationField("RunPivot180R_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_1.fbx/RunPivot180R_LDown.anim", "RunPivot180R_LDown", m16520);
            m19282 = CreateAnimationField("WalkPivot180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkPivot180L.anim", "WalkPivot180L", m19282);
            m16028 = CreateAnimationField("RunToIdle_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_PlantNTurn180_Run_R_2.fbx/RunToIdle_LDown.anim", "RunToIdle_LDown", m16028);
            m19286 = CreateAnimationField("WalkToIdle_LDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkToIdle_LDown.anim", "WalkToIdle_LDown", m19286);
            m19288 = CreateAnimationField("WalkToIdle_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2walk_v2.fbx/WalkToIdle_RDown.anim", "WalkToIdle_RDown", m19288);
            m17970 = CreateAnimationField("RunToIdle_RDown", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_HalfSteps2Idle_PasingLongStepTOIdle.fbx/RunToIdle_RDown.anim", "RunToIdle_RDown", m17970);
            m13270 = CreateAnimationField("IdleTurn20R.IdleTurn90R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90R.anim", "IdleTurn90R", m13270);
            m13274 = CreateAnimationField("IdleTurn180R", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180R.anim", "IdleTurn180R", m13274);
            m13268 = CreateAnimationField("IdleTurn20L.IdleTurn90L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn90L.anim", "IdleTurn90L", m13268);
            m13272 = CreateAnimationField("IdleTurn180L", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdleTurn180L.anim", "IdleTurn180L", m13272);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
