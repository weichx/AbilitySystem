using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// </summary>
    [MotionName("Walk Run Twitch")]
    [MotionDescription("Basic Super Mario style movement for arcade or 'twitch' games.")]
    public class WalkRunTwitch : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 27150;

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
        /// Speed to use when walking
        /// </summary>
        public float _WalkSpeed = 0f;
        public float WalkSpeed
        {
            get { return _WalkSpeed; }
            set { _WalkSpeed = value; }
        }

        /// <summary>
        /// Speed to use when running
        /// </summary>
        public float _RunSpeed = 0f;
        public float RunSpeed
        {
            get { return _RunSpeed; }
            set { _RunSpeed = value; }
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
        /// Default constructor
        /// </summary>
        public WalkRunTwitch()
            : base()
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mUseTrendData = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunTwitch-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRunTwitch(MotionController rController)
            : base(rController)
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mUseTrendData = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRunTwitch-SM"; }
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

            // If we're in the idle state with no movement, stop
            if (mMotionLayer._AnimatorStateID == STATE_IdlePose)
            {
                return false;
            }

            // One last check to make sure we're in this state
            if (mIsAnimatorActive && !IsInMotionState)
            {
                return false;
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
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, (IsRunActive ? 1 : 0), true);

            // Flag this motion as active
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
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
            // Remove any side-to-side sway
            rMovement.x = 0f;

            // Don't allow backwards movement when moving forward. Some animations have this
            if (rMovement.z < 0f)
            {
                rMovement.z = 0f;
            }

            // If we use any speed, we'll override root-motion
            if (_WalkSpeed > 0f || _RunSpeed > 0f)
            {
                rMovement.z = 0f;
                if ((mMotionLayer._AnimatorStateID == STATE_WalkFwdLoop || mMotionLayer._AnimatorStateID == STATE_RunFwdLoop) && mMotionLayer._AnimatorTransitionID != TRANS_WalkFwdLoop_IdlePose)
                {
                    float lSpeed = (_WalkSpeed > 0f ? _WalkSpeed : _RunSpeed);
                    lSpeed = (_RunSpeed == 0f || !IsRunActive ? _WalkSpeed : _RunSpeed);
                    rMovement.z = lSpeed * rDeltaTime;
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
            // Set the run flag
            mMotionController.SetAnimatorMotionParameter(mMotionLayer._AnimatorLayerIndex, IsRunActive ? 1 : 0);

            // Rotate the avatar if we're walking
            if (mMotionLayer._AnimatorStateID == STATE_WalkFwdLoop || mMotionLayer._AnimatorStateID == STATE_RunFwdLoop)
            {
                mRotation = Quaternion.AngleAxis(mMotionController.State.InputFromAvatarAngle, mMotionController._Transform.up);
            }
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR
        
        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            EditorGUILayout.HelpBox("To increase responsiveness, ensure to modify the Unity Input Manager entries for 'Horizontal' and 'Vertical' as desired.", MessageType.Info);

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

            float lNewWalkSpeed = EditorGUILayout.FloatField(new GUIContent("Walk Speed", "Speed (units per second) to walk. 0 means use root-motion if it exists."), WalkSpeed);
            if (lNewWalkSpeed != WalkSpeed)
            {
                lIsDirty = true;
                WalkSpeed = lNewWalkSpeed;
            }

            float lNewRunSpeed = EditorGUILayout.FloatField(new GUIContent("Run Speed", "Speed (units per second) to run. 0 means use root-motion if it exists."), RunSpeed);
            if (lNewRunSpeed != RunSpeed)
            {
                lIsDirty = true;
                RunSpeed = lNewRunSpeed;
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
        public static int TRANS_EntryState_WalkFwdLoop = -1;
        public static int TRANS_AnyState_WalkFwdLoop = -1;
        public static int TRANS_EntryState_RunFwdLoop = -1;
        public static int TRANS_AnyState_RunFwdLoop = -1;
        public static int STATE_WalkFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_IdlePose = -1;
        public static int TRANS_WalkFwdLoop_RunFwdLoop = -1;
        public static int STATE_IdlePose = -1;
        public static int TRANS_IdlePose_WalkFwdLoop = -1;
        public static int TRANS_IdlePose_RunFwdLoop = -1;
        public static int STATE_RunFwdLoop = -1;
        public static int TRANS_RunFwdLoop_IdlePose = -1;
        public static int TRANS_RunFwdLoop_WalkFwdLoop = -1;

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

                if (lStateID == STATE_WalkFwdLoop) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdlePose_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdlePose_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdlePose_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdlePose_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
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
            TRANS_EntryState_WalkFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunTwitch-SM.WalkFwdLoop");
            TRANS_AnyState_WalkFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunTwitch-SM.WalkFwdLoop");
            TRANS_EntryState_RunFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRunTwitch-SM.RunFwdLoop");
            TRANS_AnyState_RunFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRunTwitch-SM.RunFwdLoop");
            STATE_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.WalkFwdLoop");
            TRANS_WalkFwdLoop_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.WalkFwdLoop -> Base Layer.WalkRunTwitch-SM.IdlePose");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.WalkFwdLoop -> Base Layer.WalkRunTwitch-SM.RunFwdLoop");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.IdlePose");
            TRANS_IdlePose_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.IdlePose -> Base Layer.WalkRunTwitch-SM.WalkFwdLoop");
            TRANS_IdlePose_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.IdlePose -> Base Layer.WalkRunTwitch-SM.RunFwdLoop");
            STATE_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.RunFwdLoop");
            TRANS_RunFwdLoop_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.RunFwdLoop -> Base Layer.WalkRunTwitch-SM.IdlePose");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRunTwitch-SM.RunFwdLoop -> Base Layer.WalkRunTwitch-SM.WalkFwdLoop");
        }

#if UNITY_EDITOR

        private AnimationClip m185218 = null;
        private AnimationClip m185098 = null;
        private AnimationClip m185240 = null;

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

            UnityEditor.Animations.AnimatorState lWalkFwdLoop = lMotionStateMachine.AddState("WalkFwdLoop", new Vector3(300, 12, 0));
            lWalkFwdLoop.motion = m185218;
            lWalkFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(564, 48, 0));
            lIdlePose.motion = m185098;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunFwdLoop = lMotionStateMachine.AddState("RunFwdLoop", new Vector3(300, 84, 0));
            lRunFwdLoop.motion = m185240;
            lRunFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lWalkFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27150f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lRunFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 27150f, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lWalkFwdLoop.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.75f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.75f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");

            lStateTransition = lIdlePose.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

            lStateTransition = lIdlePose.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");

            lStateTransition = lRunFwdLoop.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6428572f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.6428572f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m185218 = CreateAnimationField("WalkFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD.fbx/WalkForward.anim", "WalkForward", m185218);
            m185098 = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", m185098);
            m185240 = CreateAnimationField("RunFwdLoop", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/RunForward.anim", "RunForward", m185240);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
