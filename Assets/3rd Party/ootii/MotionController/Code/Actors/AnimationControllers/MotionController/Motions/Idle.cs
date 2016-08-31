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
    /// Idle motion for when the character is just standing and waiting
    /// for input or some interaction.
    /// </summary>
    [MotionName("Idle")]
    [MotionDescription("Simple idle motion to be used as a default motion. It can also rotate the actor with the camera view.")]
    public class Idle : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 100;

        /// <summary>
        /// Determines if the mouse's horizontal movement is used to
        /// rotate teh character.
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
        /// Determines if the character always rotates to the camera view
        /// </summary>
        public bool _RotateWithView = false;
        public bool RotateWithView
        {
            get { return _RotateWithView; }
            set { _RotateWithView = value; }
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
        public Idle()
            : base()
        {
            _Priority = 0;
            _ActionAlias = "ActivateRotation";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Idle-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Idle(MotionController rController)
            : base(rController)
        {
            _Priority = 0;
            _ActionAlias = "ActivateRotation";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Idle-SM"; }
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
            // This is a catch all. If there are no motions found to match
            // the controller's state, we default to this motion.
            if (mMotionLayer.ActiveMotion == null)
            {
                // We used different timing based on the grounded flag
                if (mMotionController.IsGrounded)
                {
                    //if (mMotionLayer.ActiveMotionDuration > 0.1f)
                    {
                        return true;
                    }
                }
                else
                {
                    if (mMotionLayer.ActiveMotionDuration > 1.0f)
                    {
                        return true;
                    }
                }
            }

            // Handle the disqualifiers
            if (!mIsStartable) { return false; }
            if (!mMotionController.IsGrounded) { return false; }
            if (mMotionController.State.InputMagnitudeTrend.Average != 0f) { return false; }

            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            if (mIsAnimatorActive && !IsInMotionState)
            {
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
            // Reset the yaw info for smoothing
            mYaw = 0f;
            mYawTarget = 0f;
            mYawVelocity = 0f;

            // Determine if we need to rotate to the view. We do this test
            // when the activator first triggers
            if (_RotateWithViewInputX && mMotionController._InputSource != null && mMotionController._InputSource.IsPressed(_ActionAlias))
            {
                if (mMotionController._CameraTransform != null)
                {
                    float lAngle = NumberHelper.GetHorizontalAngle(mActorController._Transform.forward, mMotionController._CameraTransform.forward, mActorController._Transform.up);
                    mForceRotationToView = (Mathf.Abs(lAngle) > 1f);
                }
            }

            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, Idle.PHASE_START, true);
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
            // If we're still flagged as in the ranged stance, move out
            if (mActorController.State.Stance == EnumControllerStance.COMBAT_RANGED)
            {
                mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
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
            rMovement = Vector3.zero;
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
            mVelocity = Vector3.zero;
            mMovement = Vector3.zero;
            mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;

            // Update the stance if the camera mode is set
            if (mMotionController.CameraRig != null && mMotionController.CameraRig.Mode > 0)
            {
                mActorController.State.Stance = EnumControllerStance.COMBAT_RANGED;
            }
            else
            {
                mActorController.State.Stance = EnumControllerStance.TRAVERSAL;
            }

            // Determine if we need to rotate to the view. We do this test
            // when the activator first triggers
            if (_RotateWithViewInputX && mMotionController._InputSource != null && mMotionController._InputSource.IsJustPressed(_ActionAlias))
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
            else if ((_RotateWithViewInputX || (mMotionController.CameraRig != null && mMotionController.CameraRig.Mode > 0)) || _RotateWithMovementInputX)
            {
                GetRotationVelocityWithInput(rDeltaTime, ref mRotation);
            }
            // Determine if the actor rotates as the view rotates
            else if (_RotateWithView)
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

            if (lYawDelta == 0f && _RotateWithMovementInputX)
            {
                lYawDelta = mMotionController._InputSource.MovementX;
            }

            if (lYawDelta == 0f && (_RotateWithViewInputX || (mMotionController.CameraRig != null && mMotionController.CameraRig.Mode > 0)))
            {
                if (mMotionController._InputSource.IsPressed(_ActionAlias))
                //if (mMotionController._InputSource.IsViewingActivated)
                {
                    lYawDelta = mMotionController._InputSource.ViewX * mDegreesPer60FPSTick;
                }
            }

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
            //float lAngle = NumberHelper.GetHorizontalAngle(mMotionController._Transform.forward, mMotionController._CameraTransform.forward);
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

            //rRotationalVelocity = mActorController._Transform.up * lRotationVelocity;
            rRotationalVelocity = Vector3.up * lRotationVelocity;
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

            bool lNewRotateWithView = EditorGUILayout.Toggle(new GUIContent("Rotate with View", "Determines if the actor rotates to always face the camera's view."), RotateWithView);
            if (lNewRotateWithView != RotateWithView)
            {
                lIsDirty = true;
                RotateWithView = lNewRotateWithView;
            }

            float lNewRotationToViewSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation to View Speed", "Degrees per second to rotate towards the camera view."), RotationToViewSpeed, GUILayout.MinWidth(30));
            if (lNewRotationToViewSpeed != RotationToViewSpeed)
            {
                lIsDirty = true;
                RotationToViewSpeed = lNewRotationToViewSpeed;
            }

            GUILayout.Space(5f);

            bool lNewRotateWithViewInputX = EditorGUILayout.Toggle(new GUIContent("Rotate on ViewX", "Determines if the actor rotates based on the ViewX input."), RotateWithViewInputX);
            if (lNewRotateWithViewInputX != RotateWithViewInputX)
            {
                lIsDirty = true;
                RotateWithViewInputX = lNewRotateWithViewInputX;
            }

            bool lNewRotateWithMovementInputX = EditorGUILayout.Toggle(new GUIContent("Rotate on MoveX", "Determines if the actor rotates based on the MovementX input."), RotateWithMovementInputX);
            if (lNewRotateWithMovementInputX != RotateWithMovementInputX)
            {
                lIsDirty = true;
                RotateWithMovementInputX = lNewRotateWithMovementInputX;
            }

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Rotate Action Alias", "Action alias that is required to use the ViewX value for rotation."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            float lNewRotationSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second to rotate."), RotationSpeed, GUILayout.MinWidth(30));
            if (lNewRotationSpeed != RotationSpeed)
            {
                lIsDirty = true;
                RotationSpeed = lNewRotationSpeed;
            }

            float lNewRotationSmoothing = EditorGUILayout.FloatField(new GUIContent("Rotation Smoothing", ""), RotationSmoothing);
            if (lNewRotationSmoothing != RotationSmoothing)
            {
                lIsDirty = true;
                RotationSmoothing = lNewRotationSmoothing;
            }

            GUILayout.Space(5f);

            bool lNewForceViewOnInput = EditorGUILayout.Toggle(new GUIContent("Force View", "Determines we force the camera view to look in the direction of the actor when MovementX has a value."), ForceViewOnInput);
            if (lNewForceViewOnInput != ForceViewOnInput)
            {
                lIsDirty = true;
                ForceViewOnInput = lNewForceViewOnInput;
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
        public static int TRANS_EntryState_IdlePose = -1;
        public static int TRANS_AnyState_IdlePose = -1;
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

                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_IdlePose) { return true; }
                if (lTransitionID == TRANS_AnyState_IdlePose) { return true; }
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
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_IdlePose) { return true; }
            if (rTransitionID == TRANS_AnyState_IdlePose) { return true; }
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
            TRANS_EntryState_IdlePose = mMotionController.AddAnimatorName("Entry -> Base Layer.Idle-SM.IdlePose");
            TRANS_AnyState_IdlePose = mMotionController.AddAnimatorName("AnyState -> Base Layer.Idle-SM.IdlePose");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Idle-SM.IdlePose");
        }

#if UNITY_EDITOR

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

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(264, 72, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdlePose);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.8144876f;
            lAnyStateTransition.duration = 0.01185336f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 100f, "L0MotionPhase");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
