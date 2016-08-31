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
    /// Handles the basic motion for getting the onto a 
    /// mid (0.75m) height object
    /// </summary>
    [MotionName("Vault 1 Meter")]
    [MotionDescription("Walking/running value that allows us to go over a 1m high wall that is 0.2m or shallower in depth.")]
    public class Vault_1m : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1300;
        public const int PHASE_START_RUN = 1305;

        /// <summary>
        /// Max horizontal distance the avatar can be from the object
        /// he is trying to climb onto.
        /// </summary>
        public float _MinDistance = 0.2f;
        public float MinDistance
        {
            get { return _MinDistance; }
            set { _MinDistance = value; }
        }

        /// <summary>
        /// Max horizontal distance the avatar can be from the object
        /// he is trying to climb onto.
        /// </summary>
        public float _MaxDistance = 1.5f;
        public float MaxDistance
        {
            get { return _MaxDistance; }
            set { _MaxDistance = value; }
        }

        /// <summary>
        /// Min height of the object that can be climbed.
        /// </summary>
        public float _MinHeight = 0.9f;
        public float MinHeight
        {
            get { return _MinHeight; }
            set { _MinHeight = value; }
        }

        /// <summary>
        /// Max height of the object that can be climbed.
        /// </summary>
        public float _MaxHeight = 1f;
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
        public Vector3 _ReachOffset1 = new Vector3(0f, -0.9f, -0.6f);
        public Vector3 ReachOffset1
        {
            get { return _ReachOffset1; }
            set { _ReachOffset1 = value; }
        }

        public Vector3 _ReachOffset2 = new Vector3(0.2f, 0f, 0.2f);
        public Vector3 ReachOffset2
        {
            get { return _ReachOffset2; }
            set { _ReachOffset2 = value; }
        }

        /// <summary>
        /// Tracks the object that is being climbed
        /// </summary>
        protected GameObject mClimbable = null;

        /// <summary>
        /// Determines if we should start with a run
        /// </summary>
        protected bool mDefaultToRun = false;

        /// <summary>
        /// Connect to the move motion if we can
        /// </summary>
        protected WalkRunPivot mWalkRunPivot = null;
        protected WalkRunStrafe mWalkRunStrafe = null;
        protected WalkRunRotate mWalkRunRotate = null;

        /// <summary>
        /// Determines if we've already triggered the exit animation
        /// </summary>
        protected bool mIsExitTriggered = false;

        /// <summary>
        /// Starting position of the climb
        /// </summary>
        protected Vector3 mStartPosition = Vector3.zero;

        /// <summary>
        /// Keeps us from having to reallocate over and over
        /// </summary>
        protected RaycastHit mRaycastHitInfo = RaycastExt.EmptyHitInfo;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Vault_1m()
            : base()
        {
            _Priority = 40;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Vault_1m-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Vault_1m(MotionController rController)
            : base(rController)
        {
            _Priority = 40;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsGroundedExpected = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Vault_1m-SM"; }
#endif
        }

        /// <summary>
        /// Initialize is called after all the motions have been initialized. This allow us time to
        /// create references before the motions start working
        /// </summary>
        public override void Initialize()
        {
            if (mMotionController != null)
            {
                if (mWalkRunPivot == null) { mWalkRunPivot = mMotionController.GetMotion<WalkRunPivot>(); }
                if (mWalkRunStrafe == null) { mWalkRunStrafe = mMotionController.GetMotion<WalkRunStrafe>(); }
                if (mWalkRunRotate == null) { mWalkRunRotate = mMotionController.GetMotion<WalkRunRotate>(); }
            }
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

            bool lEdgeGrabbed = false;
            if (_ActionAlias.Length == 0 || mMotionController.InputSource.IsJustPressed(_ActionAlias))
            {
                lEdgeGrabbed = TestForClimbUp();
            }

            // Return the final result
            return lEdgeGrabbed;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
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
            // Ensure we have good collision info
            if (mRaycastHitInfo.collider == null) { return false; }

            mIsExitTriggered = false;
            mStartPosition = mActorController._Transform.position;

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            // Disable actor controller processing for a short time
            mActorController.IsGravityEnabled = false;
            mActorController.FixGroundPenetration = false;
            mActorController.SetGround(mClimbable.transform);

            // Set the animator in motion
            if (rPrevMotion is WalkRunPivot)
            {
                mDefaultToRun = ((WalkRunPivot)rPrevMotion).IsRunActive;
            }
            else if (rPrevMotion is WalkRunStrafe)
            {
                mDefaultToRun = ((WalkRunStrafe)rPrevMotion).IsRunActive;
            }
            else if (rPrevMotion is WalkRunRotate)
            {
                mDefaultToRun = ((WalkRunRotate)rPrevMotion).IsRunActive;
            }

            // Setup the reach data and clear any current values
            ClearReachData();

            if (mDefaultToRun)
            {
                //Quaternion lWallHitRotation = Quaternion.LookRotation(-mRaycastHitInfo.normal, mActorController._Transform.up);
                Quaternion lWallHitRotation = mActorController._Transform.rotation;

                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_WalkVault_1m;
                lReachData.StartTime = 0.0f;
                lReachData.EndTime = 0.25f;
                lReachData.Power = 3;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.rotation * _ReachOffset1) + (lWallHitRotation * _ReachOffset2);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);
            }
            else
            {
                //Quaternion lWallHitRotation = Quaternion.LookRotation(-mRaycastHitInfo.normal, mActorController._Transform.up);
                Quaternion lWallHitRotation = mActorController._Transform.rotation;

                MotionReachData lReachData = MotionReachData.Allocate();
                lReachData.StateID = STATE_WalkVault_1m;
                lReachData.StartTime = 0.0f;
                lReachData.EndTime = 0.25f;
                lReachData.Power = 3;
                lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.rotation * _ReachOffset1) + (lWallHitRotation * _ReachOffset2);
                lReachData.ReachTargetGround = mActorController.State.Ground;
                mReachData.Add(lReachData);

                mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);
            }

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
            //rRotationDelta = Quaternion.identity;
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

            // Get any movement from the reach data
            mMovement = GetReachMovement();

            if (lStateID == STATE_WalkVault_1m)
            {
                if (lStateTime > 0.65f)
                {
                    mActorController.IsGravityEnabled = true;
                    mActorController.FixGroundPenetration = true;
                }
                else if (lStateTime > 0.3f)
                {
                    mActorController.IsCollsionEnabled = false;
                }
            }
            // This is the first state in the jump where we hit the ground
            else if (lStateID == STATE_WalkForward || lStateID == STATE_RunForward)
            {
                mActorController.IsGravityEnabled = true;
                mActorController.IsCollsionEnabled = true;
                mActorController.FixGroundPenetration = true;
                mActorController.SetGround(null);

                if (!mIsExitTriggered)
                {
                    mIsExitTriggered = true;

                    if (mWalkRunPivot != null && mWalkRunPivot.IsEnabled)
                    {
                        mWalkRunPivot.StartInRun = mWalkRunPivot.IsRunActive;
                        mWalkRunPivot.StartInWalk = !mWalkRunPivot.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunPivot);
                    }
                    else if (mWalkRunStrafe != null && mWalkRunStrafe.IsEnabled)
                    {
                        mWalkRunStrafe.StartInRun = mWalkRunStrafe.IsRunActive;
                        mWalkRunStrafe.StartInWalk = !mWalkRunStrafe.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunStrafe);
                    }
                    else if (mWalkRunRotate != null && mWalkRunRotate.IsEnabled)
                    {
                        mWalkRunRotate.StartInRun = mWalkRunRotate.IsRunActive;
                        mWalkRunRotate.StartInWalk = !mWalkRunRotate.StartInRun;
                        mMotionController.ActivateMotion(mWalkRunRotate);
                    }
                }
            }

            //Log.FileWrite(lStateTime + " " + StringHelper.ToString(mActorController._Transform.position - mStartPosition));
        }

        /// <summary>
        /// Shoot rays to determine if a horizontal edge exists that
        /// we may be able to grab onto. It needs to be within the range
        /// of the avatar's feelers.
        /// </summary>
        /// <returns>Boolean that says if we've found an acceptable edge</returns>
        public virtual bool TestForClimbUp()
        {
            float lTargetDistance = _MaxDistance;

            // Root position for the test
            Transform lTransform = mActorController._Transform;

            // Determine the ray positions
            //float lEdgeTop = _MaxHeight;
            //float lEdgeBottom = _MinHeight;

            // Debug
            //Debug.DrawLine(lRoot.position + new Vector3(0f, lEdgeTop, 0f), lRoot.position + new Vector3(0f, lEdgeTop, 0f) + mMotionController.transform.forward * lTargetDistance, Color.red);
            //Debug.DrawLine(lRoot.position + new Vector3(0f, lEdgeBottom, 0f), lRoot.position + new Vector3(0f, lEdgeBottom, 0f) + mMotionController.transform.forward * lTargetDistance, Color.red);

            // Shoot forward and ensure below the edge is blocked
            Vector3 lRayStart = lTransform.position + (lTransform.up * _MinHeight);
            Vector3 lRayDirection = lTransform.forward;
            float lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, lTransform))
            {
                return false;
            }

            float lHitDepth = mRaycastHitInfo.distance;

            // If it's too close, we're done
            if (lHitDepth < _MinDistance)
            {
                return false;
            }

            // Shoot forward and ensure above the edge is open
            lRayStart = lTransform.position + (lTransform.up * _MaxHeight);

            if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out mRaycastHitInfo, lRayDistance, _ClimbableLayers, lTransform))
            {
                return false;
            }

            // Now that we know there is an edge, determine it's exact position.
            // First, we sink into the collision point a tad. Then, we use our 
            // collision point and start above it (where the top ray failed). Finally,
            // we shoot a ray down
            lRayStart = lRayStart + (lTransform.forward * (lHitDepth + 0.01f));
            lRayDirection = -lTransform.up;
            lRayDistance = _MaxHeight;

            if (!RaycastExt.SafeRaycast(lRayStart, -mMotionController.transform.up, out mRaycastHitInfo, _MaxHeight - _MinHeight + 0.01f, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            Vector3 lLocalHitPoint = lTransform.InverseTransformPoint(mRaycastHitInfo.point);

            // Finally we shoot one last ray forward. We do this because we want the collision
            // data to be about the wall facing the avatar, not the wall facing the
            // last ray (which was shot down).
            lRayStart = lTransform.position + (lTransform.up * (lLocalHitPoint.y - 0.01f));
            lRayDirection = lTransform.forward;
            lRayDistance = _MaxDistance;

            if (!RaycastExt.SafeRaycast(lRayStart, mMotionController.transform.forward, out mRaycastHitInfo, lTargetDistance, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // Finally, test for the depth. This needs to be a fence or shallow wall.
            RaycastHit lDepthHitInfo;

            lRayStart = mRaycastHitInfo.point + (mActorController.transform.forward * 0.3f) + (mActorController._Transform.up * 0.2f);
            if (RaycastExt.SafeRaycast(lRayStart, -mMotionController.transform.up, out lDepthHitInfo, _MinHeight, _ClimbableLayers, mActorController._Transform))
            {
                return false;
            }

            // If we got here, we found an edge
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

            // Balance layer
            int lNewClimbableLayers = EditorHelper.LayerMaskField(new GUIContent("Climb Layers", "Layers that identies objects that can be climbed."), ClimbableLayers);
            if (lNewClimbableLayers != ClimbableLayers)
            {
                lIsDirty = true;
                ClimbableLayers = lNewClimbableLayers;
            }

            EditorGUI.indentLevel++;
            mEditorShowOffsets = EditorGUILayout.Foldout(mEditorShowOffsets, new GUIContent("Reach Offsets"));
            if (mEditorShowOffsets)
            {
                Vector3 lNewReachOffset1 = EditorGUILayout.Vector3Field(new GUIContent("Start actor"), _ReachOffset1);
                if (lNewReachOffset1 != _ReachOffset1)
                {
                    lIsDirty = true;
                    _ReachOffset1 = lNewReachOffset1;
                }

                Vector3 lNewReachOffset2 = EditorGUILayout.Vector3Field(new GUIContent("Start edge"), _ReachOffset2);
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
        public static int TRANS_EntryState_WalkVault_1m = -1;
        public static int TRANS_AnyState_WalkVault_1m = -1;
        public static int TRANS_EntryState_RunVault_1m = -1;
        public static int TRANS_AnyState_RunVault_1m = -1;
        public static int STATE_WalkVault_1m = -1;
        public static int TRANS_WalkVault_1m_WalkForward = -1;
        public static int STATE_WalkForward = -1;
        public static int STATE_RunVault_1m = -1;
        public static int TRANS_RunVault_1m_RunForward = -1;
        public static int STATE_RunForward = -1;

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

                if (lStateID == STATE_WalkVault_1m) { return true; }
                if (lStateID == STATE_WalkForward) { return true; }
                if (lStateID == STATE_RunVault_1m) { return true; }
                if (lStateID == STATE_RunForward) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkVault_1m) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkVault_1m) { return true; }
                if (lTransitionID == TRANS_EntryState_RunVault_1m) { return true; }
                if (lTransitionID == TRANS_AnyState_RunVault_1m) { return true; }
                if (lTransitionID == TRANS_WalkVault_1m_WalkForward) { return true; }
                if (lTransitionID == TRANS_RunVault_1m_RunForward) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_WalkVault_1m) { return true; }
            if (rStateID == STATE_WalkForward) { return true; }
            if (rStateID == STATE_RunVault_1m) { return true; }
            if (rStateID == STATE_RunForward) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_WalkVault_1m) { return true; }
            if (rStateID == STATE_WalkForward) { return true; }
            if (rStateID == STATE_RunVault_1m) { return true; }
            if (rStateID == STATE_RunForward) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkVault_1m) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkVault_1m) { return true; }
            if (rTransitionID == TRANS_EntryState_RunVault_1m) { return true; }
            if (rTransitionID == TRANS_AnyState_RunVault_1m) { return true; }
            if (rTransitionID == TRANS_WalkVault_1m_WalkForward) { return true; }
            if (rTransitionID == TRANS_RunVault_1m_RunForward) { return true; }
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
            TRANS_EntryState_WalkVault_1m = mMotionController.AddAnimatorName("Entry -> Base Layer.Vault_1m-SM.WalkVault_1m");
            TRANS_AnyState_WalkVault_1m = mMotionController.AddAnimatorName("AnyState -> Base Layer.Vault_1m-SM.WalkVault_1m");
            TRANS_EntryState_RunVault_1m = mMotionController.AddAnimatorName("Entry -> Base Layer.Vault_1m-SM.RunVault_1m");
            TRANS_AnyState_RunVault_1m = mMotionController.AddAnimatorName("AnyState -> Base Layer.Vault_1m-SM.RunVault_1m");
            STATE_WalkVault_1m = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.WalkVault_1m");
            TRANS_WalkVault_1m_WalkForward = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.WalkVault_1m -> Base Layer.Vault_1m-SM.WalkForward");
            STATE_WalkForward = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.WalkForward");
            STATE_RunVault_1m = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.RunVault_1m");
            TRANS_RunVault_1m_RunForward = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.RunVault_1m -> Base Layer.Vault_1m-SM.RunForward");
            STATE_RunForward = mMotionController.AddAnimatorName("Base Layer.Vault_1m-SM.RunForward");
        }

#if UNITY_EDITOR

        private AnimationClip mWalkVault_1m = null;
        private AnimationClip mWalkForward = null;
        private AnimationClip mRunVault_1m = null;
        private AnimationClip mRunForward = null;

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

            UnityEditor.Animations.AnimatorState lWalkVault_1m = lMotionStateMachine.AddState("WalkVault_1m", new Vector3(348, 12, 0));
            lWalkVault_1m.motion = mWalkVault_1m;
            lWalkVault_1m.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkForward = lMotionStateMachine.AddState("WalkForward", new Vector3(600, 12, 0));
            lWalkForward.motion = mWalkForward;
            lWalkForward.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunVault_1m = lMotionStateMachine.AddState("RunVault_1m", new Vector3(348, 96, 0));
            lRunVault_1m.motion = mRunVault_1m;
            lRunVault_1m.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunForward = lMotionStateMachine.AddState("RunForward", new Vector3(600, 96, 0));
            lRunForward.motion = mRunForward;
            lRunForward.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lWalkVault_1m);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1300f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lRunVault_1m);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1305f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lWalkVault_1m.AddTransition(lWalkForward);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7967739f;
            lStateTransition.duration = 0.103878f;
            lStateTransition.offset = 0.0009236346f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunVault_1m.AddTransition(lRunForward);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8584905f;
            lStateTransition.duration = 0.2499999f;
            lStateTransition.offset = 0.4060542f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mWalkVault_1m = CreateAnimationField("WalkVault_1m", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/unity_WalkJump_ToLeft_R_2.fbx/WalkVault_1m.anim", "WalkVault_1m", mWalkVault_1m);
            mWalkForward = CreateAnimationField("WalkForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Walking/unity_WalkFWD.fbx/WalkForward.anim", "WalkForward", mWalkForward);
            mRunVault_1m = CreateAnimationField("RunVault_1m", "Assets/ootii/MotionController/Content/Animations/Humanoid/Jumping/unity_RunJump_ToLeft_4.fbx/RunVault_1m.anim", "RunVault_1m", mRunVault_1m);
            mRunForward = CreateAnimationField("RunForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Running/unity_JogForward_NtrlFaceFwd.fbx/RunForward.anim", "RunForward", mRunForward);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
