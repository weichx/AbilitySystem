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
    [MotionName("Walk Run Rotate")]
    [MotionDescription("WoW style movement. When Rotate Action Alias is held, character rotates to the camera's forward. When not held, camera rotates to the character's forward.\r\n\r\n" +
                       "Left/Right Action Alias = Strafe\r\n" +
                       "Horizontal movement keys = Rotate")]
    public class WalkRunRotate_v2 : MotionControllerMotion, IWalkRunMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1730;
        public const int PHASE_STOP = 1735;

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
        /// User layer id set for objects that are climbable.
        /// </summary>
        public string _RotateActionAlias = "ActivateRotation";
        public string RotateActionAlias
        {
            get { return _RotateActionAlias; }
            set { _RotateActionAlias = value; }
        }

        /// <summary>
        /// Determines if we rotate by ourselves
        /// </summary>
        public bool _RotateWithInput = true;
        public bool RotateWithInput
        {
            get { return _RotateWithInput; }
            set { _RotateWithInput = value; }
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
        /// Determines if we're forcing the rotation to match the camera
        /// </summary>
        protected bool mForceRotationToCamera = false;

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
        public WalkRunRotate_v2()
            : base()
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 5;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunRotate v2-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunRotate_v2(MotionController rController)
            : base(rController)
        {
            _Category = EnumMotionCategories.WALK;

            _Priority = 5;
            _ActionAlias = "Run";

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunRotate v2-SM"; }
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
            if ((mMotionController.State.InputY > -0.49f && mMotionController.State.InputY < 0.49f) &&
                !mMotionController._InputSource.IsPressed(_StrafeLeftActionAlias) &&
                !mMotionController._InputSource.IsPressed(_StrafeRightActionAlias))
            {
                if (!(mMotionController._InputSource.IsPressed(_RotateActionAlias) && 
                     (mMotionController.State.InputX < -0.49f || mMotionController.State.InputX > 0.49f)))
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
                if (mMotionController.State.InputX == 0f)
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
            mYaw = 0f;
            mYawTarget = 0f;
            mYawVelocity = 0f;
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

            // Determine if the view rotation is active
            mForceRotationToCamera = false;
            if (mMotionController._InputSource.IsPressed(_RotateActionAlias))
            {
                if (mMotionController._CameraTransform != null)
                {
                    mForceRotationToCamera = true;
                }
            }

            // Grab the state info
            float lInputMax = (IsRunActive ? 1f : 0.5f);
            MotionState lState = mMotionController.State;

            // Convert the input to radial so we deal with keyboard and gamepad input the same.
            float lInputX = (mMotionController._InputSource.IsPressed(_StrafeLeftActionAlias) ? -1f : 0f);
            lInputX = lInputX + (mMotionController._InputSource.IsPressed(_StrafeRightActionAlias) ? 1f : 0f); 
            if (mForceRotationToCamera && lInputX == 0f) { lInputX = lState.InputX; }
            
            float lInputMagnitude = Mathf.Sqrt((lInputX * lInputX) + (lState.InputY * lState.InputY));

            lInputX = Mathf.Clamp(lInputX, -lInputMax, lInputMax);
            float lInputY = Mathf.Clamp(lState.InputY, -lInputMax, lInputMax);
            lInputMagnitude = Mathf.Clamp(lInputMagnitude, 0f, lInputMax);
            InputManagerHelper.ConvertToRadialInput(ref lInputX, ref lInputY, ref lInputMagnitude);

            // Smooth the input
            mInputX.Add(lInputX);
            mInputY.Add(lInputY);
            mInputMagnitude.Add(lInputMagnitude);

            // Modify the input values to add some lag
            mMotionController.State.InputX = mInputX.Average;
            mMotionController.State.InputY = mInputY.Average;
            mMotionController.State.InputMagnitudeTrend.Replace(mInputMagnitude.Average);

            // If we're meant to rotate with the camera (and OnCameraUpdate isn't already attached), do it here
            if (_RotateWithCamera && !(mMotionController.CameraRig is BaseCameraRig))
            {
                OnCameraUpdated(rDeltaTime, rUpdateIndex, null);
            }

            // If we're not rotating with the camera, rotate with the input
            if (_RotateWithInput && !mForceRotationToCamera)
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

            lYawDelta = mMotionController._InputSource.MovementX * _RotationSpeed * rDeltaTime;

            if (lYawDelta == 0f && mMotionController._InputSource.IsViewingActivated)
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

            // Force the camera to our view if needed
            if (lYawDelta != 0f || mMotionController.State.InputMagnitudeTrend.Value > 0f)
            { 
                // If we have a camera, rotate it towards the character
                if (mMotionController.CameraRig is BaseCameraRig)
                {
                    ((BaseCameraRig)mMotionController.CameraRig).FrameForceToFollowAnchor = true;
                }
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
            if (!mForceRotationToCamera) { return; }
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

            if (EditorHelper.TextField("Left Action Alias", "Action alias for strafing to the left.", StrafeLeftActionAlias, mMotionController))
            {
                lIsDirty = true;
                StrafeLeftActionAlias = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.TextField("Right Action Alias", "Action alias for strafing to the right.", StrafeRightActionAlias, mMotionController))
            {
                lIsDirty = true;
                StrafeRightActionAlias = EditorHelper.FieldStringValue;
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

            if (EditorHelper.TextField("Rotate Action Alias", "Action alias determines if rotation is activated. This typically matches the input source's View Activator.", RotateActionAlias, mMotionController))
            {
                lIsDirty = true;
                RotateActionAlias = EditorHelper.FieldStringValue;
            }

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
            TRANS_AnyState_MoveTree = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunRotate v2-SM.Move Tree");
            TRANS_EntryState_MoveTree = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunRotate v2-SM.Move Tree");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate v2-SM.IdlePose");
            TRANS_IdlePose_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate v2-SM.IdlePose -> Base Layer.WalkRunRotate v2-SM.Move Tree");
            STATE_MoveTree = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate v2-SM.Move Tree");
            TRANS_MoveTree_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunRotate v2-SM.Move Tree -> Base Layer.WalkRunRotate v2-SM.IdlePose");
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

            UnityEditor.Animations.AnimatorStateMachine lSM_24604 = lRootSubStateMachine;
            if (lSM_24604 != null)
            {
                for (int i = lSM_24604.entryTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24604.RemoveEntryTransition(lSM_24604.entryTransitions[i]);
                }

                for (int i = lSM_24604.anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24604.RemoveAnyStateTransition(lSM_24604.anyStateTransitions[i]);
                }

                for (int i = lSM_24604.states.Length - 1; i >= 0; i--)
                {
                    lSM_24604.RemoveState(lSM_24604.states[i].state);
                }

                for (int i = lSM_24604.stateMachines.Length - 1; i >= 0; i--)
                {
                    lSM_24604.RemoveStateMachine(lSM_24604.stateMachines[i].stateMachine);
                }
            }
            else
            {
                lSM_24604 = lSM_24530.AddStateMachine(_EditorAnimatorSMName, new Vector3(192, -756, 0));
            }

            UnityEditor.Animations.AnimatorState lS_26176 = lSM_24604.AddState("IdlePose", new Vector3(600, 120, 0));
            lS_26176.speed = 1f;
            lS_26176.motion = m14552;

            UnityEditor.Animations.AnimatorState lS_24974 = lSM_24604.AddState("Move Tree", new Vector3(312, 120, 0));
            lS_24974.speed = 1f;

            UnityEditor.Animations.BlendTree lM_24102 = CreateBlendTree("Move Blend Tree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24102.blendType = UnityEditor.Animations.BlendTreeType.Simple1D;
            lM_24102.blendParameter = "InputMagnitude";
            lM_24102.blendParameterY = "InputX";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24102.useAutomaticThresholds = false;
#endif
            lM_24102.AddChild(m14552, 0f);

            UnityEditor.Animations.BlendTree lM_24140 = CreateBlendTree("WalkTree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24140.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
            lM_24140.blendParameter = "InputX";
            lM_24140.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24140.useAutomaticThresholds = true;
#endif
            lM_24140.AddChild(m20884, new Vector2(0f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_0_Children = lM_24140.children;
            lM_24140_0_Children[lM_24140_0_Children.Length - 1].mirror = false;
            lM_24140_0_Children[lM_24140_0_Children.Length - 1].timeScale = 1.1f;
            lM_24140.children = lM_24140_0_Children;

            lM_24140.AddChild(m20104, new Vector2(0.35f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_1_Children = lM_24140.children;
            lM_24140_1_Children[lM_24140_1_Children.Length - 1].mirror = false;
            lM_24140_1_Children[lM_24140_1_Children.Length - 1].timeScale = 1.2f;
            lM_24140.children = lM_24140_1_Children;

            lM_24140.AddChild(m20102, new Vector2(-0.35f, 0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_2_Children = lM_24140.children;
            lM_24140_2_Children[lM_24140_2_Children.Length - 1].mirror = false;
            lM_24140_2_Children[lM_24140_2_Children.Length - 1].timeScale = 1.2f;
            lM_24140.children = lM_24140_2_Children;

            lM_24140.AddChild(m20106, new Vector2(-0.35f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24140_3_Children = lM_24140.children;
            lM_24140_3_Children[lM_24140_3_Children.Length - 1].mirror = false;
            lM_24140_3_Children[lM_24140_3_Children.Length - 1].timeScale = 1.2f;
            lM_24140.children = lM_24140_3_Children;

            lM_24140.AddChild(m20108, new Vector2(0.35f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24140_4_Children = lM_24140.children;
            lM_24140_4_Children[lM_24140_4_Children.Length - 1].mirror = false;
            lM_24140_4_Children[lM_24140_4_Children.Length - 1].timeScale = 1.2f;
            lM_24140.children = lM_24140_4_Children;

            lM_24140.AddChild(m21194, new Vector2(-0.35f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_5_Children = lM_24140.children;
            lM_24140_5_Children[lM_24140_5_Children.Length - 1].mirror = false;
            lM_24140_5_Children[lM_24140_5_Children.Length - 1].timeScale = 1.1f;
            lM_24140.children = lM_24140_5_Children;

            lM_24140.AddChild(m21196, new Vector2(0.35f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_6_Children = lM_24140.children;
            lM_24140_6_Children[lM_24140_6_Children.Length - 1].mirror = false;
            lM_24140_6_Children[lM_24140_6_Children.Length - 1].timeScale = 1.1f;
            lM_24140.children = lM_24140_6_Children;

            lM_24140.AddChild(m24326, new Vector2(0f, -0.35f));
            UnityEditor.Animations.ChildMotion[] lM_24140_7_Children = lM_24140.children;
            lM_24140_7_Children[lM_24140_7_Children.Length - 1].mirror = false;
            lM_24140_7_Children[lM_24140_7_Children.Length - 1].timeScale = 1f;
            lM_24140.children = lM_24140_7_Children;

            lM_24102.AddChild(lM_24140, 0.5f);

            UnityEditor.Animations.BlendTree lM_24110 = CreateBlendTree("RunTree", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);
            lM_24110.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
            lM_24110.blendParameter = "InputX";
            lM_24110.blendParameterY = "InputY";
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            lM_24110.useAutomaticThresholds = true;
#endif
            lM_24110.AddChild(m14502, new Vector2(0f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_0_Children = lM_24110.children;
            lM_24110_0_Children[lM_24110_0_Children.Length - 1].mirror = false;
            lM_24110_0_Children[lM_24110_0_Children.Length - 1].timeScale = 1f;
            lM_24110.children = lM_24110_0_Children;

            lM_24110.AddChild(m22442, new Vector2(0.7f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_1_Children = lM_24110.children;
            lM_24110_1_Children[lM_24110_1_Children.Length - 1].mirror = false;
            lM_24110_1_Children[lM_24110_1_Children.Length - 1].timeScale = 1.1f;
            lM_24110.children = lM_24110_1_Children;

            lM_24110.AddChild(m22440, new Vector2(-0.7f, 0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_2_Children = lM_24110.children;
            lM_24110_2_Children[lM_24110_2_Children.Length - 1].mirror = false;
            lM_24110_2_Children[lM_24110_2_Children.Length - 1].timeScale = 1.1f;
            lM_24110.children = lM_24110_2_Children;

            lM_24110.AddChild(m22444, new Vector2(-0.7f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24110_3_Children = lM_24110.children;
            lM_24110_3_Children[lM_24110_3_Children.Length - 1].mirror = false;
            lM_24110_3_Children[lM_24110_3_Children.Length - 1].timeScale = 1f;
            lM_24110.children = lM_24110_3_Children;

            lM_24110.AddChild(m22446, new Vector2(0.7f, 0f));
            UnityEditor.Animations.ChildMotion[] lM_24110_4_Children = lM_24110.children;
            lM_24110_4_Children[lM_24110_4_Children.Length - 1].mirror = false;
            lM_24110_4_Children[lM_24110_4_Children.Length - 1].timeScale = 1f;
            lM_24110.children = lM_24110_4_Children;

            lM_24110.AddChild(m22436, new Vector2(-0.7f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_5_Children = lM_24110.children;
            lM_24110_5_Children[lM_24110_5_Children.Length - 1].mirror = false;
            lM_24110_5_Children[lM_24110_5_Children.Length - 1].timeScale = 1.1f;
            lM_24110.children = lM_24110_5_Children;

            lM_24110.AddChild(m22438, new Vector2(0.7f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_6_Children = lM_24110.children;
            lM_24110_6_Children[lM_24110_6_Children.Length - 1].mirror = false;
            lM_24110_6_Children[lM_24110_6_Children.Length - 1].timeScale = 1.1f;
            lM_24110.children = lM_24110_6_Children;

            lM_24110.AddChild(m18780, new Vector2(0f, -0.7f));
            UnityEditor.Animations.ChildMotion[] lM_24110_7_Children = lM_24110.children;
            lM_24110_7_Children[lM_24110_7_Children.Length - 1].mirror = false;
            lM_24110_7_Children[lM_24110_7_Children.Length - 1].timeScale = 1f;
            lM_24110.children = lM_24110_7_Children;

            lM_24102.AddChild(lM_24110, 1f);
            lS_24974.motion = lM_24102;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_24788 = lRootStateMachine.AddAnyStateTransition(lS_24974);
            lT_24788.hasExitTime = false;
            lT_24788.hasFixedDuration = true;
            lT_24788.exitTime = 0.9f;
            lT_24788.duration = 0.2f;
            lT_24788.offset = 0f;
            lT_24788.mute = false;
            lT_24788.solo = false;
            lT_24788.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1730f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_26178 = lS_26176.AddTransition(lS_24974);
            lT_26178.hasExitTime = false;
            lT_26178.hasFixedDuration = true;
            lT_26178.exitTime = 0f;
            lT_26178.duration = 0.25f;
            lT_26178.offset = 0f;
            lT_26178.mute = false;
            lT_26178.solo = false;
            lT_26178.canTransitionToSelf = true;
            lT_26178.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_26180 = lS_24974.AddTransition(lS_26176);
            lT_26180.hasExitTime = false;
            lT_26180.hasFixedDuration = true;
            lT_26180.exitTime = 1f;
            lT_26180.duration = 0.2f;
            lT_26180.offset = 0f;
            lT_26180.mute = false;
            lT_26180.solo = false;
            lT_26180.canTransitionToSelf = true;
            lT_26180.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1735f, "L0MotionPhase");
            lT_26180.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

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

