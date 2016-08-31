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
        public static int TRANS_EntryState_SneakIdle = -1;
        public static int TRANS_AnyState_SneakIdle = -1;
        public static int TRANS_EntryState_SneakForward = -1;
        public static int TRANS_AnyState_SneakForward = -1;
        public static int STATE_SneakForward = -1;
        public static int TRANS_SneakForward_SneakBackward = -1;
        public static int TRANS_SneakForward_SneakIdle = -1;
        public static int TRANS_SneakForward_SneakLeft = -1;
        public static int TRANS_SneakForward_SneakRight = -1;
        public static int STATE_SneakBackward = -1;
        public static int TRANS_SneakBackward_SneakForward = -1;
        public static int TRANS_SneakBackward_SneakIdle = -1;
        public static int TRANS_SneakBackward_SneakLeft = -1;
        public static int TRANS_SneakBackward_SneakRight = -1;
        public static int STATE_SneakIdle = -1;
        public static int TRANS_SneakIdle_SneakBackward = -1;
        public static int TRANS_SneakIdle_SneakForward = -1;
        public static int TRANS_SneakIdle_SneakLeft = -1;
        public static int TRANS_SneakIdle_SneakRight = -1;
        public static int TRANS_SneakIdle_IdlePose = -1;
        public static int STATE_SneakLeft = -1;
        public static int TRANS_SneakLeft_SneakBackward = -1;
        public static int TRANS_SneakLeft_SneakForward = -1;
        public static int TRANS_SneakLeft_SneakRight = -1;
        public static int TRANS_SneakLeft_SneakIdle = -1;
        public static int STATE_SneakRight = -1;
        public static int TRANS_SneakRight_SneakForward = -1;
        public static int TRANS_SneakRight_SneakBackward = -1;
        public static int TRANS_SneakRight_SneakLeft = -1;
        public static int TRANS_SneakRight_SneakIdle = -1;
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

                if (lStateID == STATE_SneakForward) { return true; }
                if (lStateID == STATE_SneakBackward) { return true; }
                if (lStateID == STATE_SneakIdle) { return true; }
                if (lStateID == STATE_SneakLeft) { return true; }
                if (lStateID == STATE_SneakRight) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_SneakIdle) { return true; }
                if (lTransitionID == TRANS_AnyState_SneakIdle) { return true; }
                if (lTransitionID == TRANS_EntryState_SneakForward) { return true; }
                if (lTransitionID == TRANS_AnyState_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakIdle) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakForward_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakForward) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakIdle) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakLeft) { return true; }
                if (lTransitionID == TRANS_SneakBackward_SneakRight) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
                if (lTransitionID == TRANS_SneakIdle_SneakForward) { return true; }
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
            if (rTransitionID == TRANS_EntryState_SneakIdle) { return true; }
            if (rTransitionID == TRANS_AnyState_SneakIdle) { return true; }
            if (rTransitionID == TRANS_EntryState_SneakForward) { return true; }
            if (rTransitionID == TRANS_AnyState_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakIdle) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakForward_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakForward) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakIdle) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakLeft) { return true; }
            if (rTransitionID == TRANS_SneakBackward_SneakRight) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakBackward) { return true; }
            if (rTransitionID == TRANS_SneakIdle_SneakForward) { return true; }
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
            /// <summary>
            /// These assignments go inside the 'LoadAnimatorData' function so that we can
            /// extract and assign the hash values for this run. These are typically used for debugging.
            /// </summary>
            TRANS_EntryState_SneakIdle = mMotionController.AddAnimatorName("Entry -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_AnyState_SneakIdle = mMotionController.AddAnimatorName("AnyState -> Base Layer.Sneak-SM.SneakIdle");
            TRANS_EntryState_SneakForward = mMotionController.AddAnimatorName("Entry -> Base Layer.Sneak-SM.SneakForward");
            TRANS_AnyState_SneakForward = mMotionController.AddAnimatorName("AnyState -> Base Layer.Sneak-SM.SneakForward");
            STATE_SneakForward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward");
            TRANS_SneakForward_SneakBackward = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakBackward");
            TRANS_SneakForward_SneakIdle = mMotionController.AddAnimatorName("Base Layer.Sneak-SM.SneakForward -> Base Layer.Sneak-SM.SneakIdle");
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

        private AnimationClip mSneakForward = null;
        private AnimationClip mSneakBackward = null;
        private AnimationClip mSneakIdle = null;
        private AnimationClip mSneakLeft = null;
        private AnimationClip mSneakRight = null;
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

            UnityEditor.Animations.AnimatorState lSneakForward = lMotionStateMachine.AddState("SneakForward", new Vector3(1008, -276, 0));
            lSneakForward.motion = mSneakForward;
            lSneakForward.speed = 1.25f;

            UnityEditor.Animations.AnimatorState lSneakBackward = lMotionStateMachine.AddState("SneakBackward", new Vector3(1008, 144, 0));
            lSneakBackward.motion = mSneakBackward;
            lSneakBackward.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lSneakIdle = lMotionStateMachine.AddState("SneakIdle", new Vector3(684, -204, 0));
            lSneakIdle.motion = mSneakIdle;
            lSneakIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lSneakLeft = lMotionStateMachine.AddState("SneakLeft", new Vector3(684, -36, 0));
            lSneakLeft.motion = mSneakLeft;
            lSneakLeft.speed = 1.25f;

            UnityEditor.Animations.AnimatorState lSneakRight = lMotionStateMachine.AddState("SneakRight", new Vector3(1344, -36, 0));
            lSneakRight.motion = mSneakRight;
            lSneakRight.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(432, -204, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lSneakIdle);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.8999999f;
            lAnyStateTransition.duration = 0.1440239f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 600f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lSneakForward);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.2f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 600f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lSneakForward.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1972967f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lSneakForward.AddTransition(lSneakIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.86f;
            lStateTransition.duration = 0.2476415f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lSneakForward.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.204735f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lSneakForward.AddTransition(lSneakLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2492742f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -100f, "InputAngleFromAvatar");

            lStateTransition = lSneakForward.AddTransition(lSneakRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2532655f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            lStateTransition = lSneakBackward.AddTransition(lSneakForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1994301f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lSneakBackward.AddTransition(lSneakIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.91f;
            lStateTransition.duration = 0.1958688f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lSneakBackward.AddTransition(lSneakLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1967947f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            lStateTransition = lSneakBackward.AddTransition(lSneakRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1977206f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4411758f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lSneakForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4411758f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4411758f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lSneakLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4411758f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lSneakRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.4411758f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            lStateTransition = lSneakIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5588242f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 610f, "L0MotionPhase");

            lStateTransition = lSneakLeft.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2016607f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lSneakLeft.AddTransition(lSneakForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2480766f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lSneakLeft.AddTransition(lSneakRight);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2045453f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 110f, "InputAngleFromAvatar");

            lStateTransition = lSneakLeft.AddTransition(lSneakIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.86f;
            lStateTransition.duration = 0.2027097f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lSneakRight.AddTransition(lSneakForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.249285f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputY");

            lStateTransition = lSneakRight.AddTransition(lSneakBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1963095f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -0.1f, "InputY");

            lStateTransition = lSneakRight.AddTransition(lSneakLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.1965434f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -110f, "InputAngleFromAvatar");

            lStateTransition = lSneakRight.AddTransition(lSneakIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2039498f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mSneakForward = CreateAnimationField("SneakForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakForward.anim", "SneakForward", mSneakForward);
            mSneakBackward = CreateAnimationField("SneakBackward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakBackward.anim", "SneakBackward", mSneakBackward);
            mSneakIdle = CreateAnimationField("SneakIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakIdle.anim", "SneakIdle", mSneakIdle);
            mSneakLeft = CreateAnimationField("SneakLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakLeft.anim", "SneakLeft", mSneakLeft);
            mSneakRight = CreateAnimationField("SneakRight", "Assets/ootii/MotionController/Content/Animations/Humanoid/Sneaking/Unity_Sneak.fbx/SneakRight.anim", "SneakRight", mSneakRight);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
