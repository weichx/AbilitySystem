using UnityEngine;
using com.ootii.Helpers;
using com.ootii.Timing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// The sneak is a slow move that keeps the character facing forward.
    /// 
    /// This motion will force the camera into the third-person-fixed mode.
    /// </summary>
    [MotionName("Sneak (old)")]
    [MotionDescription("A forward motion that looks like the avatar is sneaking. The motion is slower than a walk and has the " +
                   "actor strafe instead of turn.")]
    public class Sneak : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 600;
        public const int PHASE_END = 610;

        /// <summary>
        /// Determines if the actor rotates based on the input
        /// </summary>
        public bool _RotateWithInput = true;
        public bool RotateWithInput
        {
            get { return _RotateWithInput; }

            set
            {
                _RotateWithInput = value;
                if (_RotateWithInput) { _RotateWithView = false; }
            }
        }

        /// <summary>
        /// Determines if the actor rotates to face the direction the
        /// camera is facing
        /// </summary>
        public bool _RotateWithView = false;
        public bool RotateWithView
        {
            get { return _RotateWithView; }

            set
            {
                _RotateWithView = value;
                if (_RotateWithView) { _RotateWithInput = false; }
            }
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
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Tracks if we've actually entered sneak. This helps create the toggle
        /// </summary>
        private bool mHasEnteredState = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Sneak()
            : base()
        {
            _Priority = 6;
            _ActionAlias = "ChangeStance";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Sneak-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Sneak(MotionController rController)
            : base(rController)
        {
            _Priority = 6;
            _ActionAlias = "ChangeStance";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Sneak-SM"; }
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
            if (!mIsStartable)
            {
                return false;
            }

            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            // If we're in the stealth mode, just activate (maybe we're coming out of a jump)
            if (mActorController.State.Stance == EnumControllerStance.STEALTH)
            {
                return true;
            }

            // Test for an input change
            if (mMotionController._InputSource != null && mMotionController._InputSource.IsEnabled)
            {
                if (mMotionController._InputSource.IsJustPressed(_ActionAlias))
                {
                    if (mActorController.State.Stance != EnumControllerStance.STEALTH)
                    {
                        mActorController.State.Stance = EnumControllerStance.STEALTH;
                        return true;
                    }
                    else
                    {
                        mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
                    }
                }
            }
            
            // If we get here, we should not be in the stance
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
            if (mHasEnteredState && !IsInSneakState) { return false; }
            if (mActorController.State.Stance != EnumControllerStance.STEALTH) { return false; }

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            mHasEnteredState = false;

            // Force the character's stance to change
            mActorController.State.Stance = EnumControllerStance.STEALTH;

            // Trigger the change in the animator
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, Sneak.PHASE_START, true);

            // Allow the base to finish
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            // If we're still flagged as in the sneak stance, move out
            if (mActorController.State.Stance == EnumControllerStance.STEALTH)
            {
                mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
            }

            // Deactivate
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
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            int lStateID = mMotionLayer._AnimatorStateID;

            // Determine if we should be moving out of the sneak
            if (lStateID == STATE_SneakIdle)
            {
                // Handle the input processing here for now
                if (mMotionController._InputSource != null && mMotionController._InputSource.IsEnabled)
                {
                    if (mMotionController.InputSource.IsJustPressed(_ActionAlias))
                    {
                        // Trigger the change in the animator
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, Sneak.PHASE_END, true);
                    }
                }
            }
            // Use the idle pose to smoothly transition to a normal idle
            else if (lStateID == STATE_IdlePose)
            {
                Deactivate();

                if (mActorController.State.Stance == EnumControllerStance.STEALTH)
                {
                    mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
                }
            }

            // Determine if the actor rotates as the input is used
            if (_RotateWithInput)
            {
                mRotation = Quaternion.identity;
                GetRotationVelocityWithInput(rDeltaTime, ref mRotation);
            }
            // Determine if the actor rotates as the view rotates
            else if (_RotateWithView)
            {
                mAngularVelocity = Vector3.zero;
                GetRotationVelocityWithView(rDeltaTime, ref mAngularVelocity);
            }

            // Trend data allows us to wait for the speed to peak or bottom-out before we send it to
            // the animator. This is important for pivots that need to be very precise.
            mUseTrendData = true;

            // Determine if we've actually reached a sneak state
            if (!mHasEnteredState && IsInSneakState)
            {
                mHasEnteredState = true;
            }
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
            float lSmoothedDeltaTime = TimeManager.SmoothedDeltaTime;

            // Determine the global direction the character should face
            float lAngle = NumberHelper.GetHorizontalAngle(mMotionController._Transform.forward, mMotionController._CameraTransform.forward);

            // We want to work our way to the goal smoothly
            if (lAngle > 0f)
            {
                // Rotate instantly
                if (_RotationSpeed == 0f)
                {
                    lRotationVelocity = lAngle / lSmoothedDeltaTime;
                }
                else
                {
                    // Use the MC's rotation velocity
                    if (_RotationSpeed < 0f)
                    {
                        lRotationVelocity = mMotionController._RotationSpeed;
                    }
                    // Rotate over time
                    else
                    {
                        lRotationVelocity = _RotationSpeed;
                    }

                    // If we're rotating too much, limit
                    if (lRotationVelocity * lSmoothedDeltaTime > lAngle)
                    {
                        lRotationVelocity = lAngle / lSmoothedDeltaTime;
                    }
                }
            }
            else if (lAngle < 0f)
            {
                // Rotate instantly
                if (_RotationSpeed == 0f)
                {
                    lRotationVelocity = lAngle / lSmoothedDeltaTime;
                }
                // Rotate over time
                else
                {
                    // Use the MC's rotation velocity
                    if (_RotationSpeed < 0f)
                    {
                        lRotationVelocity = -mMotionController._RotationSpeed;
                    }
                    // Rotate over time
                    else
                    {
                        lRotationVelocity = -_RotationSpeed;
                    }

                    // If we're rotating too much, limit
                    if (lRotationVelocity * lSmoothedDeltaTime < lAngle)
                    {
                        lRotationVelocity = lAngle / lSmoothedDeltaTime;
                    }
                }
            }

            rRotationalVelocity = mMotionController._Transform.up * lRotationVelocity;
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

            // Only process if we're currently viewing
            if (mMotionController._InputSource.IsViewingActivated)
            {
                float lYaw = mMotionController._InputSource.ViewX;
                rRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);
            }
        }

        /// <summary>
        /// Test to see if we're currently in the state
        /// </summary>
        public bool IsInSneakState
        {
            get
            {
                if (IsInMotionState)
                {
                    return true;
                }

                int lTransitionID = mMotionLayer._AnimatorTransitionID;
                if (lTransitionID == TRANS_AnyState_SneakForward ||
                    lTransitionID == TRANS_EntryState_SneakForward ||
                    lTransitionID == TRANS_AnyState_SneakIdle || 
                    lTransitionID == TRANS_EntryState_SneakIdle)
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
        /// Creates input settings in the Unity Input Manager
        /// </summary>
        public override void CreateInputManagerSettings()
        {
            if (!InputManagerHelper.IsDefined(_ActionAlias))
            {
                InputManagerEntry lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "t";
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

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers the motion."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            bool lNewRotateWithInput = EditorGUILayout.Toggle(new GUIContent("Rotate with Input", "Determines if the actor rotates based on the input."), _RotateWithInput);
            if (lNewRotateWithInput != _RotateWithInput)
            {
                lIsDirty = true;
                RotateWithInput = lNewRotateWithInput;
            }

            bool lNewRotateWithView = EditorGUILayout.Toggle(new GUIContent("Rotate with View", "Determines if the actor rotates to face the direction of the camera."), _RotateWithView);
            if (lNewRotateWithView != _RotateWithView)
            {
                lIsDirty = true;
                RotateWithView = lNewRotateWithView;
            }

            float lNewRotationVelocity = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second to rotate."), _RotationSpeed, GUILayout.MinWidth(30));
            if (lNewRotationVelocity != _RotationSpeed)
            {
                lIsDirty = true;
                RotationSpeed = lNewRotationVelocity;
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
        public static int STATE_SneakForward = -1;
        public static int STATE_SneakBackward = -1;
        public static int STATE_SneakIdle = -1;
        public static int STATE_SneakLeft = -1;
        public static int STATE_SneakRight = -1;
        public static int STATE_IdlePose = -1;
        public static int TRANS_AnyState_SneakIdle = -1;
        public static int TRANS_EntryState_SneakIdle = -1;
        public static int TRANS_AnyState_SneakForward = -1;
        public static int TRANS_EntryState_SneakForward = -1;
        public static int TRANS_SneakForward_SneakBackward = -1;
        public static int TRANS_SneakForward_SneakIdle = -1;
        public static int TRANS_SneakForward_SneakLeft = -1;
        public static int TRANS_SneakForward_SneakRight = -1;
        public static int TRANS_SneakBackward_SneakForward = -1;
        public static int TRANS_SneakBackward_SneakIdle = -1;
        public static int TRANS_SneakBackward_SneakLeft = -1;
        public static int TRANS_SneakBackward_SneakRight = -1;
        public static int TRANS_SneakIdle_SneakBackward = -1;
        public static int TRANS_SneakIdle_SneakForward = -1;
        public static int TRANS_SneakIdle_SneakLeft = -1;
        public static int TRANS_SneakIdle_SneakRight = -1;
        public static int TRANS_SneakIdle_IdlePose = -1;
        public static int TRANS_SneakLeft_SneakBackward = -1;
        public static int TRANS_SneakLeft_SneakForward = -1;
        public static int TRANS_SneakLeft_SneakRight = -1;
        public static int TRANS_SneakLeft_SneakIdle = -1;
        public static int TRANS_SneakRight_SneakForward = -1;
        public static int TRANS_SneakRight_SneakBackward = -1;
        public static int TRANS_SneakRight_SneakLeft = -1;
        public static int TRANS_SneakRight_SneakIdle = -1;

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

                if (lStateID == STATE_SneakForward) { return true; }
                if (lStateID == STATE_SneakBackward) { return true; }
                if (lStateID == STATE_SneakIdle) { return true; }
                if (lStateID == STATE_SneakLeft) { return true; }
                if (lStateID == STATE_SneakRight) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_AnyState_SneakIdle) { return true; }
                if (lTransitionID == TRANS_EntryState_SneakIdle) { return true; }
                if (lTransitionID == TRANS_AnyState_SneakForward) { return true; }
                if (lTransitionID == TRANS_EntryState_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakIdle) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakIdle) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakIdle_IdlePose) { return true; }
                if (lTransitionID == TRANS_SneakLeft_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakLeft_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakLeft_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakLeft_SneakIdle) { return true; }
                if (lTransitionID == TRANS_SneakRight_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakRight_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakRight_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakRight_SneakIdle) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_SneakForward) { return true; }
            if (rStateID == STATE_SneakBackward) { return true; }
            if (rStateID == STATE_SneakIdle) { return true; }
            if (rStateID == STATE_SneakLeft) { return true; }
            if (rStateID == STATE_SneakRight) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_SneakForward) { return true; }
            if (rStateID == STATE_SneakBackward) { return true; }
            if (rStateID == STATE_SneakIdle) { return true; }
            if (rStateID == STATE_SneakLeft) { return true; }
            if (rStateID == STATE_SneakRight) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rTransitionID == TRANS_AnyState_SneakIdle) { return true; }
            if (rTransitionID == TRANS_EntryState_SneakIdle) { return true; }
            if (rTransitionID == TRANS_AnyState_SneakForward) { return true; }
            if (rTransitionID == TRANS_EntryState_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakIdle) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakIdle) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakIdle_IdlePose) { return true; }
            if (rTransitionID == TRANS_SneakLeft_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakLeft_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakLeft_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakLeft_SneakIdle) { return true; }
            if (rTransitionID == TRANS_SneakRight_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakRight_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakRight_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakRight_SneakIdle) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            TRANS_AnyState_SneakIdle = mMotionController.AddAnimatorName("AnyState -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_EntryState_SneakIdle = mMotionController.AddAnimatorName("Entry -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_AnyState_SneakForward = mMotionController.AddAnimatorName("AnyState -> Base Layer.Sneak-SM.SneakForward");
            TRANS_EntryState_SneakForward = mMotionController.AddAnimatorName("Entry -> Base Layer.Sneak-SM.SneakForward");
            STATE_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakForward_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakForward_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_SneakForward_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakForward_SneakLeft = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakLeft");
            TRANS_SneakForward_SneakRight = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakRight");
            STATE_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakBackward_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakBackward -> Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakBackward_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakBackward -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_SneakBackward_SneakLeft = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakBackward -> Base Layer.Sneak-SM.SneakLeft");
            TRANS_SneakBackward_SneakRight = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakBackward -> Base Layer.Sneak-SM.SneakRight");
            STATE_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle");
            TRANS_SneakIdle_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakIdle_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakIdle_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakIdle_SneakLeft = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.SneakLeft");
            TRANS_SneakIdle_SneakRight = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.SneakRight");
            TRANS_SneakIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakIdle -> Base Layer.Sneak-SM.IdlePose");
            STATE_SneakLeft = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakLeft");
            TRANS_SneakLeft_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakLeft -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakLeft_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakLeft -> Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakLeft_SneakRight = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakLeft -> Base Layer.Sneak-SM.SneakRight");
            TRANS_SneakLeft_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakLeft -> Base Layer.Sneak-SM.SneakIdle");
            STATE_SneakRight = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakRight");
            TRANS_SneakRight_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakRight -> Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakRight_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakRight -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakRight_SneakLeft = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakRight -> Base Layer.Sneak-SM.SneakLeft");
            TRANS_SneakRight_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakRight -> Base Layer.Sneak-SM.SneakIdle");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip m15080 = null;
        private AnimationClip m15074 = null;
        private AnimationClip m15086 = null;
        private AnimationClip m15088 = null;
        private AnimationClip m15090 = null;
        private AnimationClip m14538 = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lSM_24432 = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
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

            UnityEditor.Animations.AnimatorStateMachine lSM_24506 = lRootSubStateMachine;
            if (lSM_24506 != null)
            {
                for (int i = lSM_24506.entryTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24506.RemoveEntryTransition(lSM_24506.entryTransitions[i]);
                }

                for (int i = lSM_24506.anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_24506.RemoveAnyStateTransition(lSM_24506.anyStateTransitions[i]);
                }

                for (int i = lSM_24506.states.Length - 1; i >= 0; i--)
                {
                    lSM_24506.RemoveState(lSM_24506.states[i].state);
                }

                for (int i = lSM_24506.stateMachines.Length - 1; i >= 0; i--)
                {
                    lSM_24506.RemoveStateMachine(lSM_24506.stateMachines[i].stateMachine);
                }
            }
            else
            {
                lSM_24506 = lSM_24432.AddStateMachine(_EditorAnimatorSMName, new Vector3(192, -480, 0));
            }

            UnityEditor.Animations.AnimatorState lS_24802 = lSM_24506.AddState("SneakForward", new Vector3(624, 264, 0));
            lS_24802.speed = 1.25f;
            lS_24802.motion = m15080;

            UnityEditor.Animations.AnimatorState lS_25696 = lSM_24506.AddState("SneakBackward", new Vector3(624, 684, 0));
            lS_25696.speed = 1.5f;
            lS_25696.motion = m15074;

            UnityEditor.Animations.AnimatorState lS_24800 = lSM_24506.AddState("SneakIdle", new Vector3(300, 336, 0));
            lS_24800.speed = 1f;
            lS_24800.motion = m15086;

            UnityEditor.Animations.AnimatorState lS_25698 = lSM_24506.AddState("SneakLeft", new Vector3(300, 504, 0));
            lS_25698.speed = 1.25f;
            lS_25698.motion = m15088;

            UnityEditor.Animations.AnimatorState lS_25700 = lSM_24506.AddState("SneakRight", new Vector3(960, 504, 0));
            lS_25700.speed = 1.5f;
            lS_25700.motion = m15090;

            UnityEditor.Animations.AnimatorState lS_25702 = lSM_24506.AddState("IdlePose", new Vector3(300, 180, 0));
            lS_25702.speed = 1f;
            lS_25702.motion = m14538;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_24656 = lRootStateMachine.AddAnyStateTransition(lS_24800);
            lT_24656.hasExitTime = false;
            lT_24656.hasFixedDuration = true;
            lT_24656.exitTime = 0.9f;
            lT_24656.duration = 0.1f;
            lT_24656.offset = 0f;
            lT_24656.mute = false;
            lT_24656.solo = false;
            lT_24656.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 600f, "L0MotionPhase");
            lT_24656.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_24658 = lRootStateMachine.AddAnyStateTransition(lS_24802);
            lT_24658.hasExitTime = false;
            lT_24658.hasFixedDuration = true;
            lT_24658.exitTime = 0.9f;
            lT_24658.duration = 0.1f;
            lT_24658.offset = 0f;
            lT_24658.mute = false;
            lT_24658.solo = false;
            lT_24658.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 600f, "L0MotionPhase");
            lT_24658.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_25704 = lS_24802.AddTransition(lS_25696);
            lT_25704.hasExitTime = false;
            lT_25704.hasFixedDuration = false;
            lT_25704.exitTime = 0.9f;
            lT_25704.duration = 0.1972967f;
            lT_25704.offset = 0f;
            lT_25704.mute = false;
            lT_25704.solo = false;
            lT_25704.canTransitionToSelf = true;
            lT_25704.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25704.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");
            lT_25704.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25706 = lS_24802.AddTransition(lS_24800);
            lT_25706.hasExitTime = true;
            lT_25706.hasFixedDuration = false;
            lT_25706.exitTime = 0.86f;
            lT_25706.duration = 0.2476415f;
            lT_25706.offset = 0f;
            lT_25706.mute = false;
            lT_25706.solo = false;
            lT_25706.canTransitionToSelf = true;
            lT_25706.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_25708 = lS_24802.AddTransition(lS_25696);
            lT_25708.hasExitTime = false;
            lT_25708.hasFixedDuration = false;
            lT_25708.exitTime = 0.9f;
            lT_25708.duration = 0.204735f;
            lT_25708.offset = 0f;
            lT_25708.mute = false;
            lT_25708.solo = false;
            lT_25708.canTransitionToSelf = true;
            lT_25708.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25708.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");
            lT_25708.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25710 = lS_24802.AddTransition(lS_25698);
            lT_25710.hasExitTime = false;
            lT_25710.hasFixedDuration = false;
            lT_25710.exitTime = 0.9f;
            lT_25710.duration = 0.2492742f;
            lT_25710.offset = 0f;
            lT_25710.mute = false;
            lT_25710.solo = false;
            lT_25710.canTransitionToSelf = true;
            lT_25710.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25710.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lT_25710.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -100f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25712 = lS_24802.AddTransition(lS_25700);
            lT_25712.hasExitTime = false;
            lT_25712.hasFixedDuration = false;
            lT_25712.exitTime = 0.9f;
            lT_25712.duration = 0.2532655f;
            lT_25712.offset = 0f;
            lT_25712.mute = false;
            lT_25712.solo = false;
            lT_25712.canTransitionToSelf = true;
            lT_25712.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25712.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lT_25712.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25714 = lS_25696.AddTransition(lS_24802);
            lT_25714.hasExitTime = false;
            lT_25714.hasFixedDuration = false;
            lT_25714.exitTime = 0.9f;
            lT_25714.duration = 0.1994301f;
            lT_25714.offset = 0f;
            lT_25714.mute = false;
            lT_25714.solo = false;
            lT_25714.canTransitionToSelf = true;
            lT_25714.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25714.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lT_25714.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");
            lT_25714.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25716 = lS_25696.AddTransition(lS_24800);
            lT_25716.hasExitTime = true;
            lT_25716.hasFixedDuration = false;
            lT_25716.exitTime = 0.91f;
            lT_25716.duration = 0.1958688f;
            lT_25716.offset = 0f;
            lT_25716.mute = false;
            lT_25716.solo = false;
            lT_25716.canTransitionToSelf = true;
            lT_25716.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_25718 = lS_25696.AddTransition(lS_25698);
            lT_25718.hasExitTime = false;
            lT_25718.hasFixedDuration = false;
            lT_25718.exitTime = 0.9f;
            lT_25718.duration = 0.1967947f;
            lT_25718.offset = 0f;
            lT_25718.mute = false;
            lT_25718.solo = false;
            lT_25718.canTransitionToSelf = true;
            lT_25718.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25718.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lT_25718.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25720 = lS_25696.AddTransition(lS_25700);
            lT_25720.hasExitTime = false;
            lT_25720.hasFixedDuration = false;
            lT_25720.exitTime = 0.9f;
            lT_25720.duration = 0.1977206f;
            lT_25720.offset = 0f;
            lT_25720.mute = false;
            lT_25720.solo = false;
            lT_25720.canTransitionToSelf = true;
            lT_25720.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25720.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lT_25720.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25722 = lS_24800.AddTransition(lS_25696);
            lT_25722.hasExitTime = false;
            lT_25722.hasFixedDuration = false;
            lT_25722.exitTime = 0.9f;
            lT_25722.duration = 0.4411758f;
            lT_25722.offset = 0f;
            lT_25722.mute = false;
            lT_25722.solo = false;
            lT_25722.canTransitionToSelf = true;
            lT_25722.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25722.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25724 = lS_24800.AddTransition(lS_24802);
            lT_25724.hasExitTime = false;
            lT_25724.hasFixedDuration = false;
            lT_25724.exitTime = 0.9f;
            lT_25724.duration = 0.4411758f;
            lT_25724.offset = 0f;
            lT_25724.mute = false;
            lT_25724.solo = false;
            lT_25724.canTransitionToSelf = true;
            lT_25724.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25724.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lT_25724.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25726 = lS_24800.AddTransition(lS_25696);
            lT_25726.hasExitTime = false;
            lT_25726.hasFixedDuration = false;
            lT_25726.exitTime = 0.9f;
            lT_25726.duration = 0.4411758f;
            lT_25726.offset = 0f;
            lT_25726.mute = false;
            lT_25726.solo = false;
            lT_25726.canTransitionToSelf = true;
            lT_25726.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25726.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25728 = lS_24800.AddTransition(lS_25698);
            lT_25728.hasExitTime = false;
            lT_25728.hasFixedDuration = false;
            lT_25728.exitTime = 0.9f;
            lT_25728.duration = 0.4411758f;
            lT_25728.offset = 0f;
            lT_25728.mute = false;
            lT_25728.solo = false;
            lT_25728.canTransitionToSelf = true;
            lT_25728.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25728.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lT_25728.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25730 = lS_24800.AddTransition(lS_25700);
            lT_25730.hasExitTime = false;
            lT_25730.hasFixedDuration = false;
            lT_25730.exitTime = 0.9f;
            lT_25730.duration = 0.4411758f;
            lT_25730.offset = 0f;
            lT_25730.mute = false;
            lT_25730.solo = false;
            lT_25730.canTransitionToSelf = true;
            lT_25730.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25730.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lT_25730.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25732 = lS_24800.AddTransition(lS_25702);
            lT_25732.hasExitTime = false;
            lT_25732.hasFixedDuration = true;
            lT_25732.exitTime = 0.5588242f;
            lT_25732.duration = 0.25f;
            lT_25732.offset = 0f;
            lT_25732.mute = false;
            lT_25732.solo = false;
            lT_25732.canTransitionToSelf = true;
            lT_25732.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 610f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lT_25734 = lS_25698.AddTransition(lS_25696);
            lT_25734.hasExitTime = false;
            lT_25734.hasFixedDuration = false;
            lT_25734.exitTime = 0.9f;
            lT_25734.duration = 0.2016607f;
            lT_25734.offset = 0f;
            lT_25734.mute = false;
            lT_25734.solo = false;
            lT_25734.canTransitionToSelf = true;
            lT_25734.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25734.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");
            lT_25734.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25736 = lS_25698.AddTransition(lS_24802);
            lT_25736.hasExitTime = false;
            lT_25736.hasFixedDuration = false;
            lT_25736.exitTime = 0.9f;
            lT_25736.duration = 0.2480766f;
            lT_25736.offset = 0f;
            lT_25736.mute = false;
            lT_25736.solo = false;
            lT_25736.canTransitionToSelf = true;
            lT_25736.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25736.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lT_25736.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25738 = lS_25698.AddTransition(lS_25700);
            lT_25738.hasExitTime = false;
            lT_25738.hasFixedDuration = false;
            lT_25738.exitTime = 0.9f;
            lT_25738.duration = 0.2045453f;
            lT_25738.offset = 0f;
            lT_25738.mute = false;
            lT_25738.solo = false;
            lT_25738.canTransitionToSelf = true;
            lT_25738.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25738.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lT_25738.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25740 = lS_25698.AddTransition(lS_24800);
            lT_25740.hasExitTime = true;
            lT_25740.hasFixedDuration = false;
            lT_25740.exitTime = 0.86f;
            lT_25740.duration = 0.2027097f;
            lT_25740.offset = 0f;
            lT_25740.mute = false;
            lT_25740.solo = false;
            lT_25740.canTransitionToSelf = true;
            lT_25740.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_25742 = lS_25700.AddTransition(lS_24802);
            lT_25742.hasExitTime = false;
            lT_25742.hasFixedDuration = false;
            lT_25742.exitTime = 0.9f;
            lT_25742.duration = 0.249285f;
            lT_25742.offset = 0f;
            lT_25742.mute = false;
            lT_25742.solo = false;
            lT_25742.canTransitionToSelf = true;
            lT_25742.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25742.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");
            lT_25742.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25744 = lS_25700.AddTransition(lS_25696);
            lT_25744.hasExitTime = false;
            lT_25744.hasFixedDuration = false;
            lT_25744.exitTime = 0.9f;
            lT_25744.duration = 0.1963095f;
            lT_25744.offset = 0f;
            lT_25744.mute = false;
            lT_25744.solo = false;
            lT_25744.canTransitionToSelf = true;
            lT_25744.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25744.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");
            lT_25744.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            UnityEditor.Animations.AnimatorStateTransition lT_25746 = lS_25700.AddTransition(lS_25698);
            lT_25746.hasExitTime = false;
            lT_25746.hasFixedDuration = false;
            lT_25746.exitTime = 0.9f;
            lT_25746.duration = 0.1965434f;
            lT_25746.offset = 0f;
            lT_25746.mute = false;
            lT_25746.solo = false;
            lT_25746.canTransitionToSelf = true;
            lT_25746.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_25746.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lT_25746.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_25748 = lS_25700.AddTransition(lS_24800);
            lT_25748.hasExitTime = true;
            lT_25748.hasFixedDuration = false;
            lT_25748.exitTime = 0.9f;
            lT_25748.duration = 0.2039498f;
            lT_25748.offset = 0f;
            lT_25748.mute = false;
            lT_25748.solo = false;
            lT_25748.canTransitionToSelf = true;
            lT_25748.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

        }

        /// <summary>
        /// Gathers the animations so we can use them when creating the sub-state machine.
        /// </summary>
        public override void FindAnimations()
        {
            m15080 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakForward.anim", "SneakForward");
            m15074 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakBackward.anim", "SneakBackward");
            m15086 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakIdle.anim", "SneakIdle");
            m15088 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakLeft.anim", "SneakLeft");
            m15090 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakRight.anim", "SneakRight");
            m14538 = FindAnimationClip("Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose");

            // Add the remaining functionality
            base.FindAnimations();
        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m15080 = CreateAnimationField("SneakForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakForward.anim", "SneakForward", m15080);
            m15074 = CreateAnimationField("SneakBackward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakBackward.anim", "SneakBackward", m15074);
            m15086 = CreateAnimationField("SneakIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakIdle.anim", "SneakIdle", m15086);
            m15088 = CreateAnimationField("SneakLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakLeft.anim", "SneakLeft", m15088);
            m15090 = CreateAnimationField("SneakRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakRight.anim", "SneakRight", m15090);
            m14538 = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", m14538);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
