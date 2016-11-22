using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Cameras;
using com.ootii.Geometry;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Forward facing strafing walk/run animations.
    /// </summary>
    [MotionName("Walk Run Strafe")]
    [MotionDescription("Forward facing strafing walk/run animations.")]
    public class WalkRunStrafe_v2 : MotionControllerMotion, IWalkRunMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1130;
        public const int PHASE_STOP = 1135;

        /// <summary>
        /// Used to trigger the motion to activate when a button is
        /// held. This is useful for things like targeting or aiming.
        /// </summary>
        public string _ActivationAlias = "";
        public string ActivationAlias
        {
            get { return _ActivationAlias; }
            set { _ActivationAlias = value; }
        }

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
        /// Determines if we rotate by ourselves
        /// </summary>
        public bool _RotateWithInput = false;
        public bool RotateWithInput
        {
            get { return _RotateWithInput; }

            set
            {
                _RotateWithInput = value;
                if (_RotateWithInput) { _RotateWithCamera = false; }
            }
        }

        /// <summary>
        /// Determines if we rotate to match the camera
        /// </summary>
        public bool _RotateWithCamera = true;
        public bool RotateWithCamera
        {
            get { return _RotateWithCamera; }
            set
            {
                _RotateWithCamera = value;
                if (_RotateWithCamera) { _RotateWithInput = false; }
            }
        }

        /// <summary>
        /// Desired degrees of rotation per second
        /// </summary>
        public float _RotationSpeed = 180f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
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
        /// Determines if the actor rotation should be linked to the camera
        /// </summary>
        protected bool mLinkRotation = false;

        /// <summary>
        /// Fields to help smooth out the mouse rotation
        /// </summary>
        protected float mYaw = 0f;
        protected float mYawTarget = 0f;
        protected float mYawVelocity = 0f;

        /// <summary>
        /// We use these classes to help smooth the input values so that
        /// movement doesn't drop from 1 to 0 immediately.
        /// </summary>
        protected FloatValue mInputX = new FloatValue(0f, 10);
        protected FloatValue mInputY = new FloatValue(0f, 10);
        protected FloatValue mInputMagnitude = new FloatValue(0f, 15);

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkRunStrafe_v2()
            : base()
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 7;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunStrafe v2-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunStrafe_v2(MotionController rController)
            : base(rController)
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 7;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunStrafe v2-SM"; }
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
            if (!mIsStartable) { return false; }
            if (!mMotionController.IsGrounded) { return false; }
            if (mActorController.State.Stance != EnumControllerStance.TRAVERSAL) { return false; }

            // We need some minimum input before we can move
            if (mMotionController.State.InputMagnitudeTrend.Value < 0.49f)
            {
                return false;
            }

            if (_ActivationAlias.Length > 0)
            {
                if (mMotionController._InputSource == null || !mMotionController._InputSource.IsPressed(_ActivationAlias))
                {
                    return false;
                }
            }

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
            if (mIsActivatedFrame) { return true; }
            if (!mMotionController.IsGrounded) { return false; }
            if (mActorController.State.Stance != EnumControllerStance.TRAVERSAL) { return false; }

            // If we're down to no movement, we can exit
            if (mInputMagnitude.Average == 0f)
            {
                return false;
            }

            // If we're not activated, we can exit
            if (_ActivationAlias.Length > 0)
            {
                if (mMotionController._InputSource == null || !mMotionController._InputSource.IsPressed(_ActivationAlias))
                {
                    return false;
                }
            }

            // If we're in the idle state with no movement, stop
            if (mMotionLayer._AnimatorStateID == STATE_IdlePose)
            {
                return false;
            }

            // Ensure we're in the animation
            if (mIsAnimatorActive)
            {
                // One last check to make sure we're in this state
                if (!IsInMotionState)
                {
                    return false;
                }
            }

            // Stay in
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
            mLinkRotation = false;

            mInputX.Clear();
            mInputY.Clear();
            mInputMagnitude.Clear();

            // Determine how we'll start our animation
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);

            // Register this motion with the camera
            if (_RotateWithCamera && mMotionController.CameraRig is BaseCameraRig)
            {
                ((BaseCameraRig)mMotionController.CameraRig).OnPostLateUpdate += OnCameraUpdated;
            }

            // Finalize the activation
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
            // Register this motion with the camera
            if (mMotionController.CameraRig is BaseCameraRig)
            {
                ((BaseCameraRig)mMotionController.CameraRig).OnPostLateUpdate -= OnCameraUpdated;
            }

            // Continue with the deactivation
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
            rRotation = Quaternion.identity;

            // Override root motion if we're meant to
            float lMovementSpeed = (IsRunActive ? _RunSpeed : _WalkSpeed);
            if (lMovementSpeed > 0f)
            {
                rMovement.x = mMotionController.State.InputX;
                rMovement.y = 0f;
                rMovement.z = mMotionController.State.InputY;
                rMovement = rMovement.normalized * (lMovementSpeed * rDeltaTime);
            }
            // Remove any small movement that can be caused by the animation and blend tree
            else if (mMotionController.State.InputX == 0f && (rMovement.x > -0.01f && rMovement.x < 0.01f))
            {
                rMovement.x = 0f;
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

            // Grab the state info
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

            // Modify the input values to add some lag
            mMotionController.State.InputX = mInputX.Average;
            mMotionController.State.InputY = mInputY.Average;
            mMotionController.State.InputMagnitudeTrend.Replace(mInputMagnitude.Average);

            // If we're not dealing with an ootii camera rig, we need to rotate to the camera here
            if (_RotateWithCamera && !(mMotionController.CameraRig is BaseCameraRig))
            {
                OnCameraUpdated(rDeltaTime, rUpdateIndex, null);
            }

            if (!_RotateWithCamera && _RotateWithInput)
            {
                RotateUsingInput(rDeltaTime, ref mRotation);
            }
        }

        /// <summary>
        /// Create a rotation velocity that rotates the character based on input
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rAngularVelocity"></param>
        private void RotateUsingInput(float rDeltaTime, ref Quaternion rRotation)
        {
            // If we don't have an input source, stop
            if (mMotionController._InputSource == null) { return; }

            // Determine this frame's rotation
            float lYawDelta = 0f;

            if (mMotionController._InputSource.IsViewingActivated)
            {
                lYawDelta = mMotionController._InputSource.ViewX * _RotationSpeed * rDeltaTime;
            }

            mYawTarget = mYawTarget + lYawDelta;

            // Smooth the rotation
            lYawDelta = (_RotationSmoothing <= 0f ? mYawTarget : Mathf.SmoothDampAngle(mYaw, mYawTarget, ref mYawVelocity, _RotationSmoothing)) - mYaw;
            mYaw = mYaw + lYawDelta;

            // Use this frame's smoothed rotation
            if (lYawDelta != 0f)
            {
                rRotation = Quaternion.Euler(0f, lYawDelta, 0f);
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
            if (mMotionController._CameraTransform == null) { return; }

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

        #region Editor Functions

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

            if (EditorHelper.TextField("Activation Alias", "If set, the action alias that will activate this motion when pressed.", ActivationAlias, mMotionController))
            {
                lIsDirty = true;
                ActivationAlias = EditorHelper.FieldStringValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.BoolField("Default to Run", "Determines if the default is to run or walk.", DefaultToRun, mMotionController))
            {
                lIsDirty = true;
                DefaultToRun = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.TextField("Run Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default).", ActionAlias, mMotionController))
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

            if (EditorHelper.BoolField("Rotate With Input", "Determines if we rotate based on user input.", RotateWithInput, mMotionController))
            {
                lIsDirty = true;
                RotateWithInput = EditorHelper.FieldBoolValue;
            }

            if (EditorHelper.FloatField("Rotation Speed", "Degrees per second to rotate the actor.", RotationSpeed, mMotionController))
            {
                lIsDirty = true;
                RotationSpeed = EditorHelper.FieldFloatValue;
            }

            if (EditorHelper.FloatField("Rotation Smoothing", "Smoothing factor applied to rotation (0 disables).", RotationSmoothing, mMotionController))
            {
                lIsDirty = true;
                RotationSmoothing = EditorHelper.FieldFloatValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.IntField("Smoothing Samples", "Smoothing factor for input. The more samples the smoother, but the less responsive (0 disables).", SmoothingSamples, mMotionController))
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
        public static int STATE_IdlePose = -1;
        public static int STATE_MoveTree = -1;
        public static int TRANS_AnyState_MoveTree = -1;
        public static int TRANS_EntryState_MoveTree = -1;
        public static int TRANS_IdlePose_MoveTree = -1;
        public static int TRANS_MoveTree_IdlePose = -1;

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

                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_MoveTree) { return true; }
                if (lTransitionID == TRANS_AnyState_MoveTree) { return true; }
                if (lTransitionID == TRANS_EntryState_MoveTree) { return true; }
                if (lTransitionID == TRANS_IdlePose_MoveTree) { return true; }
                if (lTransitionID == TRANS_MoveTree_IdlePose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_MoveTree) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_MoveTree) { return true; }
            if (rTransitionID == TRANS_AnyState_MoveTree) { return true; }
            if (rTransitionID == TRANS_EntryState_MoveTree) { return true; }
            if (rTransitionID == TRANS_IdlePose_MoveTree) { return true; }
            if (rTransitionID == TRANS_MoveTree_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            TRANS_AnyState_MoveTree = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunStrafe v2-SM.Move Tree");
            TRANS_EntryState_MoveTree = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunStrafe v2-SM.Move Tree");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunStrafe v2-SM.IdlePose");
            TRANS_IdlePose_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunStrafe v2-SM.IdlePose -> Base Layer.WalkRunStrafe v2-SM.Move Tree");
            STATE_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunStrafe v2-SM.Move Tree");
            TRANS_MoveTree_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunStrafe v2-SM.Move Tree -> Base Layer.WalkRunStrafe v2-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip m14552 = null;
        private AnimationClip m20884 = null;
        private AnimationClip m20104 = null;
        private AnimationClip m20102 = null;
        private AnimationClip m20106 = null;
        private AnimationClip m20108 = null;
        private AnimationClip m21194 = null;
        private AnimationClip m21196 = null;
        private AnimationClip m24326 = null;
        private AnimationClip m14502 = null;
        private AnimationClip m22442 = null;
        private AnimationClip m22440 = null;
        private AnimationClip m22444 = null;
        private AnimationClip m22446 = null;
        private AnimationClip m22436 = null;
        private AnimationClip m22438 = null;
        private AnimationClip m18780 = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lSM_24530 = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
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

            UnityEditor.Animations.AnimatorStateMachine lSM_24570 = lRootSubStateMachine;
            if (lSM_24570 != null)
            {
                for (int i = lSM_24570.entryTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24570.RemoveEntryTransition(lSM_24570.entryTransitions[i]);
                }

                for (int i = lSM_24570.anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24570.RemoveAnyStateTransition(lSM_24570.anyStateTransitions[i]);
                }

                for (int i = lSM_24570.states.Length - 1; i >= 0; i--)
                {
                    lSM_24570.RemoveState(lSM_24570.states[i].state);
                }

                for (int i = lSM_24570.stateMachines.Length - 1; i >= 0; i--)
                {
                    lSM_24570.RemoveStateMachine(lSM_24570.stateMachines[i].stateMachine);
                }
            }
            else
            {
                lSM_24570 = lSM_24530.AddStateMachine(_EditorAnimatorSMName, new Vector3(408, -756, 0));
            }

            UnityEditor.Animations.AnimatorState lS_25730 = lSM_24570.AddState("IdlePose", new Vector3(600, 120, 0));
            lS_25730.speed = 1f;
            lS_25730.motion = m14552;

            UnityEditor.Animations.AnimatorState lS_24896 = lSM_24570.AddState("Move Tree", new Vector3(312, 120, 0));
            lS_24896.speed = 1f;

            UnityEditor.Animations.BlendTree lM_24106 = CreateBlendTree("Move Blend Tree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24106.blendType = UnityEditor.Animations.BlendTreeType.Simple1D;
            lM_24106.blendParameter = "InputMagnitude";
            lM_24106.blendParameterY = "InputX";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24106.useAutomaticThresholds = false;
#endif
            lM_24106.AddChild(m14552, 0f);

            UnityEditor.Animations.BlendTree lM_24146 = CreateBlendTree("WalkTree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24146.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
            lM_24146.blendParameter = "InputX";
            lM_24146.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24146.useAutomaticThresholds = true;
#endif
            lM_24146.AddChild(m20884, new Vector2(0f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_0_Children = lM_24146.children;
            lM_24146_0_Children[lM_24146_0_Children.Length - 1].mirror = false;
            lM_24146_0_Children[lM_24146_0_Children.Length - 1].timeScale = 1.1f;
            lM_24146.children = lM_24146_0_Children;

            lM_24146.AddChild(m20104, new Vector2(0.35f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_1_Children = lM_24146.children;
            lM_24146_1_Children[lM_24146_1_Children.Length - 1].mirror = false;
            lM_24146_1_Children[lM_24146_1_Children.Length - 1].timeScale = 1.2f;
            lM_24146.children = lM_24146_1_Children;

            lM_24146.AddChild(m20102, new Vector2(-0.35f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_2_Children = lM_24146.children;
            lM_24146_2_Children[lM_24146_2_Children.Length - 1].mirror = false;
            lM_24146_2_Children[lM_24146_2_Children.Length - 1].timeScale = 1.2f;
            lM_24146.children = lM_24146_2_Children;

            lM_24146.AddChild(m20106, new Vector2(-0.35f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24146_3_Children = lM_24146.children;
            lM_24146_3_Children[lM_24146_3_Children.Length - 1].mirror = false;
            lM_24146_3_Children[lM_24146_3_Children.Length - 1].timeScale = 1.2f;
            lM_24146.children = lM_24146_3_Children;

            lM_24146.AddChild(m20108, new Vector2(0.35f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24146_4_Children = lM_24146.children;
            lM_24146_4_Children[lM_24146_4_Children.Length - 1].mirror = false;
            lM_24146_4_Children[lM_24146_4_Children.Length - 1].timeScale = 1.2f;
            lM_24146.children = lM_24146_4_Children;

            lM_24146.AddChild(m21194, new Vector2(-0.35f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_5_Children = lM_24146.children;
            lM_24146_5_Children[lM_24146_5_Children.Length - 1].mirror = false;
            lM_24146_5_Children[lM_24146_5_Children.Length - 1].timeScale = 1.1f;
            lM_24146.children = lM_24146_5_Children;

            lM_24146.AddChild(m21196, new Vector2(0.35f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_6_Children = lM_24146.children;
            lM_24146_6_Children[lM_24146_6_Children.Length - 1].mirror = false;
            lM_24146_6_Children[lM_24146_6_Children.Length - 1].timeScale = 1.1f;
            lM_24146.children = lM_24146_6_Children;

            lM_24146.AddChild(m24326, new Vector2(0f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24146_7_Children = lM_24146.children;
            lM_24146_7_Children[lM_24146_7_Children.Length - 1].mirror = false;
            lM_24146_7_Children[lM_24146_7_Children.Length - 1].timeScale = 1f;
            lM_24146.children = lM_24146_7_Children;

            lM_24106.AddChild(lM_24146, 0.5f);

            UnityEditor.Animations.BlendTree lM_24116 = CreateBlendTree("RunTree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24116.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
            lM_24116.blendParameter = "InputX";
            lM_24116.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24116.useAutomaticThresholds = true;
#endif
            lM_24116.AddChild(m14502, new Vector2(0f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_0_Children = lM_24116.children;
            lM_24116_0_Children[lM_24116_0_Children.Length - 1].mirror = false;
            lM_24116_0_Children[lM_24116_0_Children.Length - 1].timeScale = 1f;
            lM_24116.children = lM_24116_0_Children;

            lM_24116.AddChild(m22442, new Vector2(0.7f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_1_Children = lM_24116.children;
            lM_24116_1_Children[lM_24116_1_Children.Length - 1].mirror = false;
            lM_24116_1_Children[lM_24116_1_Children.Length - 1].timeScale = 1.1f;
            lM_24116.children = lM_24116_1_Children;

            lM_24116.AddChild(m22440, new Vector2(-0.7f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_2_Children = lM_24116.children;
            lM_24116_2_Children[lM_24116_2_Children.Length - 1].mirror = false;
            lM_24116_2_Children[lM_24116_2_Children.Length - 1].timeScale = 1.1f;
            lM_24116.children = lM_24116_2_Children;

            lM_24116.AddChild(m22444, new Vector2(-0.7f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24116_3_Children = lM_24116.children;
            lM_24116_3_Children[lM_24116_3_Children.Length - 1].mirror = false;
            lM_24116_3_Children[lM_24116_3_Children.Length - 1].timeScale = 1f;
            lM_24116.children = lM_24116_3_Children;

            lM_24116.AddChild(m22446, new Vector2(0.7f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24116_4_Children = lM_24116.children;
            lM_24116_4_Children[lM_24116_4_Children.Length - 1].mirror = false;
            lM_24116_4_Children[lM_24116_4_Children.Length - 1].timeScale = 1f;
            lM_24116.children = lM_24116_4_Children;

            lM_24116.AddChild(m22436, new Vector2(-0.7f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_5_Children = lM_24116.children;
            lM_24116_5_Children[lM_24116_5_Children.Length - 1].mirror = false;
            lM_24116_5_Children[lM_24116_5_Children.Length - 1].timeScale = 1.1f;
            lM_24116.children = lM_24116_5_Children;

            lM_24116.AddChild(m22438, new Vector2(0.7f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_6_Children = lM_24116.children;
            lM_24116_6_Children[lM_24116_6_Children.Length - 1].mirror = false;
            lM_24116_6_Children[lM_24116_6_Children.Length - 1].timeScale = 1.1f;
            lM_24116.children = lM_24116_6_Children;

            lM_24116.AddChild(m18780, new Vector2(0f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24116_7_Children = lM_24116.children;
            lM_24116_7_Children[lM_24116_7_Children.Length - 1].mirror = false;
            lM_24116_7_Children[lM_24116_7_Children.Length - 1].timeScale = 1f;
            lM_24116.children = lM_24116_7_Children;

            lM_24106.AddChild(lM_24116, 1f);
            lS_24896.motion = lM_24106;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_24700 = lRootStateMachine.AddAnyStateTransition(lS_24896);
            lT_24700.hasExitTime = false;
            lT_24700.hasFixedDuration = true;
            lT_24700.exitTime = 0.9f;
            lT_24700.duration = 0.2f;
            lT_24700.offset = 0f;
            lT_24700.mute = false;
            lT_24700.solo = false;
            lT_24700.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1130f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_25732 = lS_25730.AddTransition(lS_24896);
            lT_25732.hasExitTime = false;
            lT_25732.hasFixedDuration = true;
            lT_25732.exitTime = 0f;
            lT_25732.duration = 0.25f;
            lT_25732.offset = 0f;
            lT_25732.mute = false;
            lT_25732.solo = false;
            lT_25732.canTransitionToSelf = true;
            lT_25732.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_25734 = lS_24896.AddTransition(lS_25730);
            lT_25734.hasExitTime = false;
            lT_25734.hasFixedDuration = true;
            lT_25734.exitTime = 1f;
            lT_25734.duration = 0.2f;
            lT_25734.offset = 0f;
            lT_25734.mute = false;
            lT_25734.solo = false;
            lT_25734.canTransitionToSelf = true;
            lT_25734.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1135f, "L0MotionPhase");
            lT_25734.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

        }

        /// <summary>
        /// Gathers the animations so we can use them when creating the sub-state machine.
        /// </summary>
        public override void FindAnimations()
        {
            m14552 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose");
            m20884 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD_v2.fbx/WalkForward.anim", "WalkForward");
            m20104 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkForwardRight.anim", "SWalkForwardRight");
            m20102 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkForwardLeft.anim", "SWalkForwardLeft");
            m20106 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkLeft.anim", "SWalkLeft");
            m20108 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkRight.anim", "SWalkRight");
            m21194 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2Strafe_AllAngles.fbx/WalkStrafeBackwardsLeft.anim", "WalkStrafeBackwardsLeft");
            m21196 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2Strafe_AllAngles.fbx/WalkStrafeBackwardsRight.anim", "WalkStrafeBackwardsRight");
            m24326 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_BWalk.fbx/WalkBackwards.anim", "WalkBackwards");
            m14502 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/RunForward.anim", "RunForward");
            m22442 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeForwardRight.anim", "RunStrafeForwardRight");
            m22440 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeForwardLeft.anim", "RunStrafeForwardLeft");
            m22444 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeLeft.anim", "RunStrafeLeft");
            m22446 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeRight.anim", "RunStrafeRight");
            m22436 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeBackwardLeft.anim", "RunStrafeBackwardLeft");
            m22438 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeBackwardRight.anim", "RunStrafeBackwardRight");
            m18780 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunBackward.fbx/RunBackwards.anim", "RunBackwards");

            // Add the remaining functionality
            base.FindAnimations();
        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m14552 = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", m14552);
            m20884 = CreateAnimationField("Move Tree.WalkForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD_v2.fbx/WalkForward.anim", "WalkForward", m20884);
            m20104 = CreateAnimationField("Move Tree.SWalkForwardRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkForwardRight.anim", "SWalkForwardRight", m20104);
            m20102 = CreateAnimationField("Move Tree.SWalkForwardLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkForwardLeft.anim", "SWalkForwardLeft", m20102);
            m20106 = CreateAnimationField("Move Tree.SWalkLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkLeft.anim", "SWalkLeft", m20106);
            m20108 = CreateAnimationField("Move Tree.SWalkRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_SWalk_v2.fbx/SWalkRight.anim", "SWalkRight", m20108);
            m21194 = CreateAnimationField("Move Tree.WalkStrafeBackwardsLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2Strafe_AllAngles.fbx/WalkStrafeBackwardsLeft.anim", "WalkStrafeBackwardsLeft", m21194);
            m21196 = CreateAnimationField("Move Tree.WalkStrafeBackwardsRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_Idle2Strafe_AllAngles.fbx/WalkStrafeBackwardsRight.anim", "WalkStrafeBackwardsRight", m21196);
            m24326 = CreateAnimationField("Move Tree.WalkBackwards", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_BWalk.fbx/WalkBackwards.anim", "WalkBackwards", m24326);
            m14502 = CreateAnimationField("Move Tree.RunForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunForward_v2.fbx/RunForward.anim", "RunForward", m14502);
            m22442 = CreateAnimationField("Move Tree.RunStrafeForwardRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeForwardRight.anim", "RunStrafeForwardRight", m22442);
            m22440 = CreateAnimationField("Move Tree.RunStrafeForwardLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeForwardLeft.anim", "RunStrafeForwardLeft", m22440);
            m22444 = CreateAnimationField("Move Tree.RunStrafeLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeLeft.anim", "RunStrafeLeft", m22444);
            m22446 = CreateAnimationField("Move Tree.RunStrafeRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeRight.anim", "RunStrafeRight", m22446);
            m22436 = CreateAnimationField("Move Tree.RunStrafeBackwardLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeBackwardLeft.anim", "RunStrafeBackwardLeft", m22436);
            m22438 = CreateAnimationField("Move Tree.RunStrafeBackwardRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunStrafe.fbx/RunStrafeBackwardRight.anim", "RunStrafeBackwardRight", m22438);
            m18780 = CreateAnimationField("Move Tree.RunBackwards", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/RunBackward.fbx/RunBackwards.anim", "RunBackwards", m18780);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}

