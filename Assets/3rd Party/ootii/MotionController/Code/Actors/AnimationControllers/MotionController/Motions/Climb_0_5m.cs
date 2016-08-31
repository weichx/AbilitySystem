using System.Collections.Generic;
using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Handles the basic motion for going up and down ladders
    /// </summary>
    [MotionName("Climb 0.5 meter")]
    [MotionDescription("Allows for getting ontop of an object that's roughly 0.5 meter high.")]
    public class Climb_0_5m : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 900;

        /// <summary>
        /// Min horizontal distance the actor can be from the ladder in order to climb
        /// </summary>
        public float _MinDistance = 0.2f;
        public float MinDistance
        {
            get { return _MinDistance; }
            set { _MinDistance = value; }
        }

        /// <summary>
        /// Max horizontal distance the actor can be from the ladder in order to climb
        /// </summary>
        public float _MaxDistance = 0.65f;
        public float MaxDistance
        {
            get { return _MaxDistance; }
            set { _MaxDistance = value; }
        }

        /// <summary>
        /// Min height of the object that can be climbed.
        /// </summary>
        public float _MinHeight = 0.5f;
        public float MinHeight
        {
            get { return _MinHeight; }
            set { _MinHeight = value; }
        }

        /// <summary>
        /// Max height of the object that can be climbed.
        /// </summary>
        public float _MaxHeight = 0.8f;
        public float MaxHeight
        {
            get { return _MaxHeight; }
            set { _MaxHeight = value; }
        }

        /// <summary>
        /// User layer id set for objects that are climbable.
        /// </summary>
        public int _ClimbableLayers = 1;
        public int ClimbableLayers
        {
            get { return _ClimbableLayers; }
            set { _ClimbableLayers = value; }
        }

        /// <summary>
        /// Reach offset value for the animation
        /// </summary>
        public float _ReachOffset1 = 0.02f;
        public float ReachOffset1
        {
            get { return _ReachOffset1; }
            set { _ReachOffset1 = value; }
        }

        public float _ReachOffset2 = -0.05f;
        public float ReachOffset2
        {
            get { return _ReachOffset2; }
            set { _ReachOffset2 = value; }
        }

        /// <summary>
        /// Tracks the object that is being climbed
        /// </summary>
        protected GameObject mClimbable = null;

        /// <summary>
        /// Rotation it takes to get to facing the climbable's normal
        /// </summary>
        protected float mFaceClimbableNormalAngle = 0f;

        /// <summary>
        /// Amount of rotation that is already used
        /// </summary>
        protected float mFaceClimbableNormalAngleUsed = 0f;

        /// <summary>
        /// Used to determine if we've triggered the desired state. This is
        /// meant to be generic.
        /// </summary>
        protected bool mIsExitTriggered = false;

        /// <summary>
        /// Keeps us from having to reallocate over and over
        /// </summary>
        protected RaycastHit mRaycastHitInfo = RaycastExt.EmptyHitInfo;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Climb_0_5m()
            : base()
        {
            _Priority = 30;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Climb_0_5m-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Climb_0_5m(MotionController rController)
            : base(rController)
        {
            _Priority = 30;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsGroundedExpected = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Climb_0_5m-SM"; }
#endif
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

            // Ensure we have input to test
            if (mMotionController.InputSource == null)
            {
                return false;
            }

            // Test if we're facing a wall and ready to go up it
            if (_ActionAlias.Length == 0 || mMotionController._InputSource.IsJustPressed(_ActionAlias))
            {
                if (TestForClimbUp())
                {
                    return true;
                }
            }

            // Return the final result
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
        {
            // Once we're at the top, we want to make sure there is no popping. So we'll force the
            // avatar to the right height
            if (mIsAnimatorActive && !IsInMotionState)
            {
                // Re-enable actor controller processing
                mActorController.IsGravityEnabled = true;
                mActorController.IsCollsionEnabled = true;
                mActorController.FixGroundPenetration = true;
                mActorController.SetGround(null);

                // Tell this motion to get out
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
            // Ensure we have good collision info
            if (mRaycastHitInfo.collider == null) { return false; }

            // Reset the triggger
            mIsExitTriggered = false;

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            Vector3 lClimbForward = Quaternion.AngleAxis(180, mActorController._Transform.up) * mRaycastHitInfo.normal;
            mFaceClimbableNormalAngle = mActorController._Transform.forward.HorizontalAngleTo(lClimbForward, mActorController._Transform.up);
            mFaceClimbableNormalAngleUsed = 0f;

            // Setup the reach data and clear any current values
            ClearReachData();

            MotionReachData lReachData = MotionReachData.Allocate();
            lReachData.StateID = STATE_IdleClimbMid;
            lReachData.StartTime = 0.2f;
            lReachData.EndTime = 0.6f;
            lReachData.Power = 3;
            lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.up * _ReachOffset1) + (mRaycastHitInfo.normal * _ReachOffset2);
            lReachData.ReachTargetGround = mActorController.State.Ground;
            mReachData.Add(lReachData);

            // Disable actor controller processing for a short time
            mActorController.IsGravityEnabled = false;
            mActorController.FixGroundPenetration = false;
            mActorController.SetGround(mClimbable.transform);

            // Start the animations
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);

            // Return
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            mClimbable = null;

            // Re-enable actor controller processing
            mActorController.IsGravityEnabled = true;
            mActorController.IsCollsionEnabled = true;
            mActorController.FixGroundPenetration = true;
            mActorController.SetGround(null);

            // Finish the deactivation process
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
            mVelocity = Vector3.zero;
            mMovement = Vector3.zero;
            mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;

            if (mClimbable == null) { return; }

            int lStateID = mMotionLayer._AnimatorStateID;
            float lStateTime = mMotionLayer._AnimatorStateNormalizedTime;

            // Reach data moves us the the specified position so we can line our animations up nicely.
            mMovement = GetReachMovement();

            // Once we start, we use this to blend into the climb
            if (lStateID == STATE_IdleClimbMid)
            {
                mRotation = GetReachRotation(0.3f, 0.6f, mFaceClimbableNormalAngle, ref mFaceClimbableNormalAngleUsed);

                if (lStateTime > 0.6f)
                {
                    mActorController.IsCollsionEnabled = true;
                }
                else if (lStateTime > 0f)
                {
                    mActorController.IsCollsionEnabled = false;
                }

                //if (lStateTime > 0.3f && lStateTime <= 0.6f)
                //{
                //    float lPercent = (lStateTime - 0.3f) / 0.3f;
                //    float lFrameYaw = (mFaceClimbableNormalAngle * lPercent) - mFaceClimbableNormalAngleUsed;

                //    mRotation = Quaternion.AngleAxis(lFrameYaw, Vector3.up);
                //    mFaceClimbableNormalAngleUsed = mFaceClimbableNormalAngle * lPercent;
                //}
            }
            // If we get off the ladder, end
            else if (lStateID == STATE_IdlePose)
            {
                Deactivate();
            }

            //Log.FileWrite(lStateTime + " " + StringHelper.ToString(mActorController._Transform.position - mStartPosition));
        }

        /// <summary>
        /// Shoot a ray to determine if we found a ladder
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestForClimbUp()
        {
            // If there is active input pulling us away from the object, stop
            if (Mathf.Abs(mMotionController._InputSource.InputFromAvatarAngle) > 100f)
            {
                return false;
            }

            // Test for an edge
            if (!RaycastExt.GetForwardEdge(mActorController._Transform, _MaxDistance, _MaxHeight, _ClimbableLayers, out mRaycastHitInfo))
            {
                return false;
            }

            // Ensure the edge is in range
            Vector3 lLocalHitPoint = mActorController._Transform.InverseTransformPoint(mRaycastHitInfo.point);
            if (lLocalHitPoint.y + mActorController.State.GroundSurfaceDistance < _MinHeight - 0.01f) { return false; }
            if (lLocalHitPoint.z < _MinDistance) { return false; }

            // If we got here, we're good
            return true;
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        // Used to hide/show the offset section
        private bool mEditorShowOffsets = false;

        /// <summary>
        /// Allow the constraint to render it's own GUI
        /// </summary>
        /// <returns>Reports if the object's value was changed</returns>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers a climb."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            float lNewMinDistance = EditorGUILayout.FloatField(new GUIContent("Min Distance", "Minimum distance inwhich the climb is valid."), MinDistance, GUILayout.MinWidth(30));
            if (lNewMinDistance != MinDistance)
            {
                lIsDirty = true;
                MinDistance = lNewMinDistance;
            }

            float lNewMaxDistance = EditorGUILayout.FloatField(new GUIContent("Max Distance", "Maximum distance at which the climb is valid."), MaxDistance, GUILayout.MinWidth(30));
            if (lNewMaxDistance != MaxDistance)
            {
                lIsDirty = true;
                MaxDistance = lNewMaxDistance;
            }

            float lNewMinHeight = EditorGUILayout.FloatField(new GUIContent("Min Height", "Minimum height inwhich the climb is valid."), MinHeight, GUILayout.MinWidth(30));
            if (lNewMinHeight != MinHeight)
            {
                lIsDirty = true;
                MinHeight = lNewMinHeight;
            }

            float lNewMaxHeight = EditorGUILayout.FloatField(new GUIContent("Max Height", "Maximum height at which the climb is valid."), MaxHeight, GUILayout.MinWidth(30));
            if (lNewMaxHeight != MaxHeight)
            {
                lIsDirty = true;
                MaxHeight = lNewMaxHeight;
            }

            int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("Climbing Layers", "Layers that identies objects that can be climbed."), ClimbableLayers);
            if (lNewClimbableLayers != ClimbableLayers)
            {
                lIsDirty = true;
                ClimbableLayers = lNewClimbableLayers;
            }

            EditorGUI.indentLevel++;
            mEditorShowOffsets = EditorGUILayout.Foldout(mEditorShowOffsets, new GUIContent("Reach Offsets"));
            if (mEditorShowOffsets)
            {
                float lNewReachOffset1 = EditorGUILayout.FloatField(new GUIContent("End actor up"), _ReachOffset1);
                if (lNewReachOffset1 != _ReachOffset1)
                {
                    lIsDirty = true;
                    _ReachOffset1 = lNewReachOffset1;
                }

                float lNewReachOffset2 = EditorGUILayout.FloatField(new GUIContent("End edge normal"), _ReachOffset2);
                if (lNewReachOffset2 != _ReachOffset2)
                {
                    lIsDirty = true;
                    _ReachOffset2 = lNewReachOffset2;
                }
            }
            EditorGUI.indentLevel--;

            return lIsDirty;
        }

#endif

        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int TRANS_EntryState_IdleClimbMid = -1;
        public static int TRANS_AnyState_IdleClimbMid = -1;
        public static int STATE_IdleClimbMid = -1;
        public static int TRANS_IdleClimbMid_ClimbRecoverIdle = -1;
        public static int TRANS_IdleClimbMid_IdlePose = -1;
        public static int STATE_ClimbRecoverIdle = -1;
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

                if (lStateID == STATE_IdleClimbMid) { return true; }
                if (lStateID == STATE_ClimbRecoverIdle) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleClimbMid) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleClimbMid) { return true; }
                if (lTransitionID == TRANS_IdleClimbMid_ClimbRecoverIdle) { return true; }
                if (lTransitionID == TRANS_IdleClimbMid_IdlePose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_IdleClimbMid) { return true; }
            if (rStateID == STATE_ClimbRecoverIdle) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdleClimbMid) { return true; }
            if (rStateID == STATE_ClimbRecoverIdle) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleClimbMid) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleClimbMid) { return true; }
            if (rTransitionID == TRANS_IdleClimbMid_ClimbRecoverIdle) { return true; }
            if (rTransitionID == TRANS_IdleClimbMid_IdlePose) { return true; }
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
            TRANS_EntryState_IdleClimbMid = mMotionController.AddAnimatorName("Entry -> Base Layer.Climb_0_5m-SM.IdleClimbMid");
            TRANS_AnyState_IdleClimbMid = mMotionController.AddAnimatorName("AnyState -> Base Layer.Climb_0_5m-SM.IdleClimbMid");
            STATE_IdleClimbMid = mMotionController.AddAnimatorName("Base Layer.Climb_0_5m-SM.IdleClimbMid");
            TRANS_IdleClimbMid_ClimbRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Climb_0_5m-SM.IdleClimbMid -> Base Layer.Climb_0_5m-SM.ClimbRecoverIdle");
            TRANS_IdleClimbMid_IdlePose = mMotionController.AddAnimatorName("Base Layer.Climb_0_5m-SM.IdleClimbMid -> Base Layer.Climb_0_5m-SM.IdlePose");
            STATE_ClimbRecoverIdle = mMotionController.AddAnimatorName("Base Layer.Climb_0_5m-SM.ClimbRecoverIdle");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Climb_0_5m-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip mIdleClimbMid = null;
        private AnimationClip mClimbRecoverIdle = null;
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

            UnityEditor.Animations.AnimatorState lIdleClimbMid = lMotionStateMachine.AddState("IdleClimbMid", new Vector3(264, 12, 0));
            lIdleClimbMid.motion = mIdleClimbMid;
            lIdleClimbMid.speed = 1f;

            UnityEditor.Animations.AnimatorState lClimbRecoverIdle = lMotionStateMachine.AddState("ClimbRecoverIdle", new Vector3(504, -36, 0));
            lClimbRecoverIdle.motion = mClimbRecoverIdle;
            lClimbRecoverIdle.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(504, 36, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleClimbMid);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = false;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 900f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lIdleClimbMid.AddTransition(lClimbRecoverIdle);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1.00067f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = true;
            lStateTransition.solo = false;

            lStateTransition = lIdleClimbMid.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8125f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mIdleClimbMid = CreateAnimationField("IdleClimbMid", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbMid.fbx/IdleClimbMid.anim", "IdleClimbMid", mIdleClimbMid);
            mClimbRecoverIdle = CreateAnimationField("ClimbRecoverIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/ootii_ClimbMid.fbx/ClimbRecoverIdle.anim", "ClimbRecoverIdle", mClimbRecoverIdle);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
