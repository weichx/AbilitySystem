using UnityEngine;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// This is a simple motion that shows a sliding animation when we're 
    /// sliding down a slope that exceeds our mininum slide condition.
    /// </summary>
    [MotionDescription("This motion is a simple pose used when the avatar starts to slide down a ramp.")]
    public class Slide : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 700;
        public const int PHASE_END = 705;

        /// <summary>
        /// Speed of the rotation that has the avatar rotating towards
        /// the slide. Set the value to 0 for no rotation.
        /// </summary>
        public float _RotationSpeed = 180f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Slide()
            : base()
        {
            _Priority = 6;
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Slide-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Slide(MotionController rController)
            : base(rController)
        {
            _Priority = 6;
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Slide-SM"; }
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
            if (!mMotionController.IsGrounded) { return false; }
            if (mMotionController.State.InputMagnitudeTrend.Value > 0f) { return false; }
            if (mActorController.State.GroundSurfaceAngle < mActorController.MinSlopeAngle) { return false; }

            // Get out
            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
        {
            if (mIsActivatedFrame) { return true; }

            if (!mMotionController.IsGrounded) { return false; }
            if (mMotionController.State.InputMagnitudeTrend.Value > 0f) { return false; }

            // Defalut to true
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);
            return base.Activate(rPrevMotion);
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
            rVelocityDelta = Vector3.zero;
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
            bool lRotate = true;

            // If we hit a lower slope than the slide allows, transition to the idle
            if (mActorController.State.GroundSurfaceAngle < mActorController.MinSlopeAngle)
            {
                lRotate = false;
                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_END, true);
            }

            // When we reach the final state, stop
            if (mMotionLayer._AnimatorStateID == STATE_IdlePose)
            {
                lRotate = false;
                Deactivate();
            }

            // If we're meant to, rotate towards the direction of the slide
            if (lRotate && _RotationSpeed > 0f)
            {
                // Grab the direction of force along the ground plane
                Vector3 lDirection = Vector3.down;
                Vector3 lGroundNormal = mActorController.State.GroundSurfaceNormal;
                Vector3.OrthoNormalize(ref lGroundNormal, ref lDirection);

                // Convert the direction to a horizontal forward
                Vector3 lUp = mActorController._Transform.up;
                lDirection = lDirection - lUp;
                lDirection.Normalize();

                float lAngle = NumberHelper.GetHorizontalAngle(mMotionController._Transform.forward, lDirection);
                if (Mathf.Abs(lAngle) > 0.01f)
                {
                    float lAngularSpeed = lAngle * _RotationSpeed * Time.deltaTime;
                    mAngularVelocity.y = lAngularSpeed;
                }
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

            float lNewRotationVelocity = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Determines how quickly the avatar rotates to face the downward slope. Set the value to 0 to not rotate."), _RotationSpeed, GUILayout.MinWidth(30));
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
        public static int TRANS_EntryState_Slide = -1;
        public static int TRANS_AnyState_Slide = -1;
        public static int STATE_Slide = -1;
        public static int TRANS_Slide_IdlePose = -1;
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

                if (lStateID == STATE_Slide) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_Slide) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
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
            TRANS_EntryState_Slide = mMotionController.AddAnimatorName("Entry -> Base Layer.Slide-SM.Slide");
            TRANS_AnyState_Slide = mMotionController.AddAnimatorName("AnyState -> Base Layer.Slide-SM.Slide");
            STATE_Slide = mMotionController.AddAnimatorName("Base Layer.Slide-SM.Slide");
            TRANS_Slide_IdlePose = mMotionController.AddAnimatorName("Base Layer.Slide-SM.Slide -> Base Layer.Slide-SM.IdlePose");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Slide-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip mSlide = null;
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

            UnityEditor.Animations.AnimatorState lSlide = lMotionStateMachine.AddState("Slide", new Vector3(300, 12, 0));
            lSlide.motion = mSlide;
            lSlide.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(540, 12, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lSlide);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.2984754f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 700f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lSlide.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.75f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 705f, "L0MotionPhase");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mSlide = CreateAnimationField("Slide", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/ootii_Slide.fbx/Slide.anim", "Slide", mSlide);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/Raw Mocap Data/Animations/Idle/Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
