using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers {

    [MotionName("Walk Run")] public class WalkRunMotion : MotionControllerMotion {

        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 100;

        public float walkSpeed = 0f;
        public float runSpeed = 0f;

        public WalkRunMotion()
            : base() {
            _Priority = 5;
            _ActionAlias = "Run";
            mUseTrendData = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) {
                _EditorAnimatorSMName = "WalkRunMotion-SM";
            }
#endif
        }

        public WalkRunMotion(MotionController rController)
            : base(rController) {
            _Priority = 5;
            _ActionAlias = "Run";
            mUseTrendData = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) {
                _EditorAnimatorSMName = "WalkRunMotion-SM";
            }
#endif
        }



        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        public override bool TestActivate() {
            // If we're not startable, this is easy
            if (!mIsStartable) {
                return false;
            }

            // If we're not grounded, this is easy
            if (!mMotionController.IsGrounded) {
                return false;
            }

            // If we're not actually moving, we can stop too
            MotionState lState = mMotionController.State;
            if (lState.InputMagnitudeTrend.Value < 0.03f) {
                return false;
            }

            // We're good to move
            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate() {
            // If we just entered this frame, stay
            if (mIsActivatedFrame) {
                return true;
            }

            // If we are no longer grounded, stop
            if (!mMotionController.IsGrounded) {
                return false;
            }

            // One last check to make sure we're in this state
            if (mIsAnimatorActive && !IsInMotionState) {
                return false;
            }
            return true;
        }

        public override bool Activate(MotionControllerMotion rPrevMotion) {
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, 1, true);
            return base.Activate(rPrevMotion);
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
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation) {
            // Remove any side-to-side sway
            rMovement.x = 0f;
            // Don't allow backwards movement when moving forward. Some animations have this
            if (rMovement.z < 0f) {
                rMovement.z = 0f;
            }

            if (mMotionController.InputSource.IsPressed(KeyCode.LeftShift) && mMotionController.InputSource.IsPressed(KeyCode.W)) {
                rMovement.z = runSpeed * rDeltaTime;
            }
            else if (mMotionController.InputSource.IsPressed(KeyCode.W)) {
                rMovement.z = walkSpeed * rDeltaTime;
            }
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex) {
            // Set the run flag

            if (mMotionController.InputSource.IsPressed(KeyCode.LeftShift) && mMotionController.InputSource.IsPressed(KeyCode.W)) {
                mMotionController.SetAnimatorMotionParameter(mMotionLayer._AnimatorLayerIndex, 1);
            }
            else if (mMotionController.InputSource.IsPressed(KeyCode.W)) {
                mMotionController.SetAnimatorMotionParameter(mMotionLayer._AnimatorLayerIndex, 0);
            }

            //// Rotate the avatar if we're walking
            //if (mMotionLayer._AnimatorStateID == STATE_WalkFwdLoop || mMotionLayer._AnimatorStateID == STATE_RunFwdLoop) {
            //    mRotation = Quaternion.AngleAxis(mMotionController.State.InputFromAvatarAngle, mMotionController._Transform.up);
            //}
        }


#if UNITY_EDITOR

        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI() {
            bool lIsDirty = false;
            
            float lNewWalkSpeed = EditorGUILayout.FloatField(new GUIContent("Walk Speed", "Speed (units per second) to walk."), walkSpeed);
            if (lNewWalkSpeed != walkSpeed) {
                lIsDirty = true;
                walkSpeed = lNewWalkSpeed;
            }

            float lNewRunSpeed = EditorGUILayout.FloatField(new GUIContent("Run Speed", "Speed (units per second) to run/"), runSpeed);
            if (lNewRunSpeed != runSpeed) {
                lIsDirty = true;
                runSpeed = lNewRunSpeed;
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
        public static int TRANS_EntryState_Standing_Walk_Forward = -1;
        public static int TRANS_AnyState_Standing_Walk_Forward = -1;
        public static int TRANS_EntryState_Standing_Run_Forward = -1;
        public static int TRANS_AnyState_Standing_Run_Forward = -1;
        public static int STATE_Standing_Walk_Forward = -1;
        public static int TRANS_Standing_Walk_Forward_Standing_Run_Forward = -1;
        public static int STATE_Standing_Run_Forward = -1;
        public static int TRANS_Standing_Run_Forward_Standing_Walk_Forward = -1;

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsInMotionState {
            get {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lStateID == STATE_Standing_Walk_Forward) { return true; }
                if (lStateID == STATE_Standing_Run_Forward) { return true; }
                if (lTransitionID == TRANS_EntryState_Standing_Walk_Forward) { return true; }
                if (lTransitionID == TRANS_AnyState_Standing_Walk_Forward) { return true; }
                if (lTransitionID == TRANS_EntryState_Standing_Run_Forward) { return true; }
                if (lTransitionID == TRANS_AnyState_Standing_Run_Forward) { return true; }
                if (lTransitionID == TRANS_Standing_Walk_Forward_Standing_Run_Forward) { return true; }
                if (lTransitionID == TRANS_Standing_Run_Forward_Standing_Walk_Forward) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID) {
            if (rStateID == STATE_Standing_Walk_Forward) { return true; }
            if (rStateID == STATE_Standing_Run_Forward) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID) {
            if (rStateID == STATE_Standing_Walk_Forward) { return true; }
            if (rStateID == STATE_Standing_Run_Forward) { return true; }
            if (rTransitionID == TRANS_EntryState_Standing_Walk_Forward) { return true; }
            if (rTransitionID == TRANS_AnyState_Standing_Walk_Forward) { return true; }
            if (rTransitionID == TRANS_EntryState_Standing_Run_Forward) { return true; }
            if (rTransitionID == TRANS_AnyState_Standing_Run_Forward) { return true; }
            if (rTransitionID == TRANS_Standing_Walk_Forward_Standing_Run_Forward) { return true; }
            if (rTransitionID == TRANS_Standing_Run_Forward_Standing_Walk_Forward) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData() {
            /// <summary>
            /// These assignments go inside the 'LoadAnimatorData' function so that we can
            /// extract and assign the hash values for this run. These are typically used for debugging.
            /// </summary>
            TRANS_EntryState_Standing_Walk_Forward = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRun.Standing_Walk_Forward");
            TRANS_AnyState_Standing_Walk_Forward = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRun.Standing_Walk_Forward");
            TRANS_EntryState_Standing_Run_Forward = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRun.Standing_Run_Forward");
            TRANS_AnyState_Standing_Run_Forward = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRun.Standing_Run_Forward");
            STATE_Standing_Walk_Forward = mMotionController.AddAnimatorName("Base Layer.WalkRun.Standing_Walk_Forward");
            TRANS_Standing_Walk_Forward_Standing_Run_Forward = mMotionController.AddAnimatorName("Base Layer.WalkRun.Standing_Walk_Forward -> Base Layer.WalkRun.Standing_Run_Forward");
            STATE_Standing_Run_Forward = mMotionController.AddAnimatorName("Base Layer.WalkRun.Standing_Run_Forward");
            TRANS_Standing_Run_Forward_Standing_Walk_Forward = mMotionController.AddAnimatorName("Base Layer.WalkRun.Standing_Run_Forward -> Base Layer.WalkRun.Standing_Walk_Forward");
        }

#if UNITY_EDITOR

        private AnimationClip m182328 = null;
        private AnimationClip m182412 = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine() {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;

            // If we find the sm with our name, remove it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++) {
                // Look for a sm with the matching name
                if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName) {
                    // Allow the user to stop before we remove the sm
                    if (!UnityEditor.EditorUtility.DisplayDialog("Motion Controller", _EditorAnimatorSMName + " already exists. Delete and recreate it?", "Yes", "No")) {
                        return;
                    }

                    // Remove the sm
                    lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);
                }
            }

            UnityEditor.Animations.AnimatorStateMachine lMotionStateMachine = lRootStateMachine.AddStateMachine(_EditorAnimatorSMName);

            // Attach the behaviour if needed
            if (_EditorAttachBehaviour) {
                MotionControllerBehaviour lBehaviour = lMotionStateMachine.AddStateMachineBehaviour(typeof(MotionControllerBehaviour)) as MotionControllerBehaviour;
                lBehaviour._MotionKey = (_Key.Length > 0 ? _Key : this.GetType().FullName);
            }

            UnityEditor.Animations.AnimatorState lStanding_Walk_Forward = lMotionStateMachine.AddState("Standing_Walk_Forward", new Vector3(360, -120, 0));
            lStanding_Walk_Forward.motion = m182328;
            lStanding_Walk_Forward.speed = 1f;

            UnityEditor.Animations.AnimatorState lStanding_Run_Forward = lMotionStateMachine.AddState("Standing_Run_Forward", new Vector3(360, 144, 0));
            lStanding_Run_Forward.motion = m182412;
            lStanding_Run_Forward.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lStanding_Walk_Forward);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lStanding_Run_Forward);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 100f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L1MotionParameter");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lStanding_Walk_Forward.AddTransition(lStanding_Run_Forward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3077585f;
            lStateTransition.duration = 0.7845403f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");

            lStateTransition = lStanding_Run_Forward.AddTransition(lStanding_Walk_Forward);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6590909f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI() {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m182328 = CreateAnimationField("Standing_Walk_Forward", "Assets/Mixamo/Magic Pack/Animations/Standing_Walk_Forward.anim", "Standing_Walk_Forward", m182328);
            m182412 = CreateAnimationField("Standing_Run_Forward", "Assets/Mixamo/Magic Pack/Animations/Standing_Run_Forward.anim", "Standing_Run_Forward", m182412);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion

    }
}
