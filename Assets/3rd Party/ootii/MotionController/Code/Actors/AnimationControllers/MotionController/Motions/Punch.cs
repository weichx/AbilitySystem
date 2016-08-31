using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using com.ootii.Base;
using com.ootii.Cameras;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Utilities.Debug;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// This is a simple punch used to test using different
    /// motions at the same time with MotionLayers.
    /// </summary>
    [MotionName("Punch")]
    [MotionDescription("A simple motion used to test motions on different layers. When put on a seperate layer, " +
                   "this motion will cause the avatar to punch with his left hand.")]
    public class Punch : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 500;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Punch()
            : base()
        {
            _Priority = 10;
            _ActionAlias = "Fire1";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Punch-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Punch(MotionController rController)
            : base(rController)
        {
            _Priority = 10;
            _ActionAlias = "Fire1";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Punch-SM"; }
#endif
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            // Handle the input processing here for now
            if (mMotionController._InputSource != null && mMotionController._InputSource.IsJustPressed(_ActionAlias))
            {
                return true;
            }         

            // Get out
            return false;
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

            // Cancel if we're not in the motion
            if (mIsAnimatorActive && !IsInMotionState)
            {
                return false;
            }

            // Once we've exceeded the motion time, get out
            if (mMotionLayer._AnimatorStateNormalizedTime > 0.8f)
            {
                return false;
            }

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
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, Punch.PHASE_START, true);
            return base.Activate(rPrevMotion);
        }

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int TRANS_EntryState_Punch = -1;
        public static int TRANS_AnyState_Punch = -1;
        public static int STATE_Punch = -1;

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

                if (lStateID == STATE_Punch) { return true; }
                if (lTransitionID == TRANS_EntryState_Punch) { return true; }
                if (lTransitionID == TRANS_AnyState_Punch) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_Punch) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_Punch) { return true; }
            if (rTransitionID == TRANS_EntryState_Punch) { return true; }
            if (rTransitionID == TRANS_AnyState_Punch) { return true; }
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
            TRANS_EntryState_Punch = mMotionController.AddAnimatorName("Entry -> Fighting.Punch-SM.Punch");
            TRANS_AnyState_Punch = mMotionController.AddAnimatorName("AnyState -> Fighting.Punch-SM.Punch");
            STATE_Punch = mMotionController.AddAnimatorName("Fighting.Punch-SM.Punch");
        }

#if UNITY_EDITOR

        private AnimationClip mPunch = null;

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

            UnityEditor.Animations.AnimatorState lPunch = lMotionStateMachine.AddState("Punch", new Vector3(276, 12, 0));
            lPunch.motion = mPunch;
            lPunch.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lPunch);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 500f, "L1MotionPhase");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mPunch = CreateAnimationField("Punch", "Assets/ootii/MotionController/Content/Animations/Humanoid/Fighting/ootii_Punch.fbx/Punch.anim", "Punch", mPunch);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
