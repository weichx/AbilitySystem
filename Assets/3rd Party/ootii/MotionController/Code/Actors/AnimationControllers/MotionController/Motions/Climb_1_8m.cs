using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Handles the basic motion for getting the onto a 
    /// mid (0.75m) height object
    /// </summary>
    [MotionName("Climb 1.8 Meters")]
    [MotionDescription("Scaling climb that allows the actor to go up a 1.8m high wall.")]
    public class Climb_1_8m : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1250;

        /// <summary>
        /// Max horizontal distance the avatar can be from the object
        /// he is trying to climb onto.
        /// </summary>
        public float _MinDistance = 0.25f;
        public float MinDistance
        {
            get { return _MinDistance; }
            set { _MinDistance = value; }
        }

        /// <summary>
        /// Max horizontal distance the avatar can be from the object
        /// he is trying to climb onto.
        /// </summary>
        public float _MaxDistance = 0.625f;
        public float MaxDistance
        {
            get { return _MaxDistance; }
            set { _MaxDistance = value; }
        }

        /// <summary>
        /// Min height of the object that can be climbed.
        /// </summary>
        public float _MinHeight = 1.4f;
        public float MinHeight
        {
            get { return _MinHeight; }
            set { _MinHeight = value; }
        }

        /// <summary>
        /// Max height of the object that can be climbed.
        /// </summary>
        public float _MaxHeight = 2.4f;
        public float MaxHeight
        {
            get { return _MaxHeight; }
            set { _MaxHeight = value; }
        }

        /// <summary>
        /// The X distance from the grab position that the hands will
        /// be positions. If a value is set, we'll check to make sure there
        /// is something for them to grab or fail.
        /// </summary>
        public float _HandGrabOffset = 0.13f;
        public float HandGrabOffset
        {
            get { return _HandGrabOffset; }
            set { _HandGrabOffset = value; }
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
        public Vector3 _ReachOffset1 = new Vector3(0f, -1.6f, -0.1f);
        public Vector3 ReachOffset1
        {
            get { return _ReachOffset1; }
            set { _ReachOffset1 = value; }
        }

        public Vector3 _ReachOffset2 = new Vector3(0.0f, 0f, -0.5f);
        public Vector3 ReachOffset2
        {
            get { return _ReachOffset2; }
            set { _ReachOffset2 = value; }
        }

        public Vector3 _ReachOffset3 = new Vector3(-0.1f, -0.89f, -0.1f);
        public Vector3 ReachOffset3
        {
            get { return _ReachOffset3; }
            set { _ReachOffset3 = value; }
        }

        public Vector3 _ReachOffset4 = new Vector3(-0.15f, 0.01f, 0.2f);
        public Vector3 ReachOffset4
        {
            get { return _ReachOffset4; }
            set { _ReachOffset4 = value; }
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
        public Climb_1_8m()
            : base()
        {
            _Priority = 30;
            _ActionAlias = "Jump";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Climb_1_8m-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Climb_1_8m(MotionController rController)
            : base(rController)
        {
            _Priority = 30;
            _ActionAlias = "Jump";
            mIsStartable = true;
            //_IsGravityEnabled = false;
            //mIsGroundedExpected = false;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "Climb_1_8m-SM"; }
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
            // Once we're at the top, we want to make sure there is no popping. So we'll force the
            // avatar to the right height
            if ((mIsAnimatorActive && !IsInMotionState) ||
                mMotionLayer._AnimatorStateID == STATE_IdlePose)
            {
                // Re-enable actor controller processing
                mActorController.IsGravityEnabled = true;
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

            //_IsGravityEnabled = false;
            //mIsGroundedExpected = false;
            mStartPosition = mActorController._Transform.position;

            // Track the object we're trying to climb and store it
            mClimbable = mRaycastHitInfo.collider.gameObject;

            // Disable actor controller processing for a short time
            mActorController.IsGravityEnabled = false;
            mActorController.FixGroundPenetration = false;
            mActorController.SetGround(mClimbable.transform);

            Vector3 lClimbForward = Quaternion.AngleAxis(180, mActorController._Transform.up) * mRaycastHitInfo.normal;
            mFaceClimbableNormalAngle = mActorController._Transform.forward.HorizontalAngleTo(lClimbForward, mActorController._Transform.up);
            mFaceClimbableNormalAngleUsed = 0f;

            // Setup the reach data and clear any current values
            ClearReachData();

            // Set the animator in motion
            //Quaternion lWallHitRotation = Quaternion.LookRotation(-mRaycastHitInfo.normal, mActorController._Transform.up);
            Quaternion lWallHitRotation = mActorController._Transform.rotation;

            MotionReachData lReachData = MotionReachData.Allocate();
            lReachData.StateID = STATE_LegUpToIdle;
            lReachData.StartTime = 0.0f;
            lReachData.EndTime = 0.224f;
            lReachData.Power = 2;
            lReachData.ReachTarget = mRaycastHitInfo.point + (mActorController._Transform.rotation * _ReachOffset1) + (lWallHitRotation * _ReachOffset2);
            lReachData.ReachTargetGround = mActorController.State.Ground;
            mReachData.Add(lReachData);

            lReachData = MotionReachData.Allocate();
            lReachData.StateID = STATE_LegUpToIdle;
            lReachData.StartTime = 0.35f;
            lReachData.EndTime = 0.425f;
            lReachData.Power = 4;
            lReachData.ReachTarget = mRaycastHitInfo.point + (lWallHitRotation * _ReachOffset3);
            lReachData.ReachTargetGround = mActorController.State.Ground;
            mReachData.Add(lReachData);

            lReachData = MotionReachData.Allocate();
            lReachData.StateID = STATE_LegUpToIdle;
            lReachData.StartTime = 0.625f;
            lReachData.EndTime = 0.725f;
            lReachData.Power = 4;
            lReachData.ReachTarget = mRaycastHitInfo.point + (lWallHitRotation * _ReachOffset4);
            lReachData.ReachTargetGround = mActorController.State.Ground;
            mReachData.Add(lReachData);

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

            // Get any movement from the reach data
            mMovement = GetReachMovement();

            // Once we grab onto the ledge, we want to make sure we are facing the 'face of the wall'. So,
            // we may need to rotate
            if (lStateID == STATE_LegUpToIdle)
            {
                mRotation = GetReachRotation(0.4f, 0.55f, mFaceClimbableNormalAngle, ref mFaceClimbableNormalAngleUsed);

                if (lStateTime > 0.6f)
                {
                    mActorController.IsCollsionEnabled = true;
                }
                else if (lStateTime > 0.3f)
                {
                    mActorController.IsCollsionEnabled = false;
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
            // If there is active input pulling us away from the object, stop
            if (Mathf.Abs(mMotionController._InputSource.InputFromAvatarAngle) > 100f)
            {
                return false;
            }

            //Vector3 lRayStart = Vector3.zero;

            //float lTargetDistance = mMaxDistance;

            // Root position for the test
            //Transform lRoot = mActorController._Transform;

            // Determine the ray positions
            //float lEdgeTop = mMaxHeight;
            //float lEdgeBottom = mMinHeight;

            // Debug
            //Debug.DrawLine(lRoot.position + new Vector3(0f, lEdgeTop, 0f), lRoot.position + new Vector3(0f, lEdgeTop, 0f) + mMotionController.transform.forward * lTargetDistance, Color.red);
            //Debug.DrawLine(lRoot.position + new Vector3(0f, lEdgeBottom, 0f), lRoot.position + new Vector3(0f, lEdgeBottom, 0f) + mMotionController.transform.forward * lTargetDistance, Color.red);

            // Shoot forward and ensure below the edge is blocked
            //lRayStart = mActorController._Transform.position + (mActorController._Transform.up * lEdgeBottom);
            //if (!RaycastExt.SafeRaycast(lRayStart, mActorController._Transform.forward, lTargetDistance, mClimbableLayers, mActorController._Transform, out mRaycastHitInfo))
            //{
            //    return false;
            //}

            //// If it's too close, we're done
            //if (mRaycastHitInfo.distance < mMinDistance)
            //{
            //    return false;
            //}

            // Find the exact edge that is infront of us
            if (!RaycastExt.GetForwardEdge(mActorController._Transform, _MaxDistance, _MaxHeight, _ClimbableLayers, out mRaycastHitInfo))
            {
                return false;
            }

            // Ensure the edge is in range
            Vector3 lLocalHitPoint = mActorController._Transform.InverseTransformPoint(mRaycastHitInfo.point);
            if (lLocalHitPoint.y + mActorController.State.GroundSurfaceDistance < _MinHeight - 0.01f) { return false; }
            if (lLocalHitPoint.z < _MinDistance) { return false; }

            // If we have hand positions, ensure that they collide with something as well. Otherwise,
            // the hand will look like it's floating in the air.
            if (_HandGrabOffset > 0)
            {
                RaycastHit lHandHitInfo;

                // Check the right hand
                Vector3 lRightHandPosition = mRaycastHitInfo.point + (mRaycastHitInfo.normal * 1f) + (mActorController._Transform.rotation * new Vector3(_HandGrabOffset, 0f, 0f));
                if (!RaycastExt.SafeRaycast(lRightHandPosition, -mRaycastHitInfo.normal, out lHandHitInfo, 1.1f, _ClimbableLayers, mActorController._Transform))
                {
                    return false;
                }

                // Check the left hand
                Vector3 lLeftHandPosition = mRaycastHitInfo.point + (mRaycastHitInfo.normal * 1f) + (mActorController._Transform.rotation * new Vector3(-_HandGrabOffset, 0f, 0f));
                if (!RaycastExt.SafeRaycast(lLeftHandPosition, -mRaycastHitInfo.normal, out lHandHitInfo, 1.1f, _ClimbableLayers, mActorController._Transform))
                {
                    return false;
                }
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

            float lNewHandGrabOffset = EditorGUILayout.FloatField(new GUIContent("Hand Grab Offset", "Offset of the hands from the character's center."), HandGrabOffset, GUILayout.MinWidth(30));
            if (lNewHandGrabOffset != HandGrabOffset)
            {
                lIsDirty = true;
                HandGrabOffset = lNewHandGrabOffset;
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

                Vector3 lNewReachOffset3 = EditorGUILayout.Vector3Field(new GUIContent("Mid edge"), _ReachOffset3);
                if (lNewReachOffset3 != _ReachOffset3)
                {
                    lIsDirty = true;
                    _ReachOffset3 = lNewReachOffset3;
                }

                Vector3 lNewReachOffset4 = EditorGUILayout.Vector3Field(new GUIContent("End edge"), _ReachOffset4);
                if (lNewReachOffset4 != _ReachOffset4)
                {
                    lIsDirty = true;
                    _ReachOffset4 = lNewReachOffset4;
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
        public static int TRANS_EntryState_LegUpToIdle = -1;
        public static int TRANS_AnyState_LegUpToIdle = -1;
        public static int STATE_IdlePose = -1;
        public static int STATE_LegUpToIdle = -1;
        public static int TRANS_LegUpToIdle_IdlePose = -1;

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
                if (lStateID == STATE_LegUpToIdle) { return true; }
                if (lTransitionID == TRANS_EntryState_LegUpToIdle) { return true; }
                if (lTransitionID == TRANS_AnyState_LegUpToIdle) { return true; }
                if (lTransitionID == TRANS_LegUpToIdle_IdlePose) { return true; }
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
            if (rStateID == STATE_LegUpToIdle) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_LegUpToIdle) { return true; }
            if (rTransitionID == TRANS_EntryState_LegUpToIdle) { return true; }
            if (rTransitionID == TRANS_AnyState_LegUpToIdle) { return true; }
            if (rTransitionID == TRANS_LegUpToIdle_IdlePose) { return true; }
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
            TRANS_EntryState_LegUpToIdle = mMotionController.AddAnimatorName("Entry -> Base Layer.Climb_1_8m-SM.LegUpToIdle");
            TRANS_AnyState_LegUpToIdle = mMotionController.AddAnimatorName("AnyState -> Base Layer.Climb_1_8m-SM.LegUpToIdle");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.Climb_1_8m-SM.IdlePose");
            STATE_LegUpToIdle = mMotionController.AddAnimatorName("Base Layer.Climb_1_8m-SM.LegUpToIdle");
            TRANS_LegUpToIdle_IdlePose = mMotionController.AddAnimatorName("Base Layer.Climb_1_8m-SM.LegUpToIdle -> Base Layer.Climb_1_8m-SM.IdlePose");
        }

#if UNITY_EDITOR

        private AnimationClip mIdlePose = null;
        private AnimationClip mLegUpToIdle = null;

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

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(576, 12, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lLegUpToIdle = lMotionStateMachine.AddState("LegUpToIdle", new Vector3(288, 12, 0));
            lLegUpToIdle.motion = mLegUpToIdle;
            lLegUpToIdle.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lLegUpToIdle);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1250f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lLegUpToIdle.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8626919f;
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
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);
            mLegUpToIdle = CreateAnimationField("LegUpToIdle", "Assets/ootii/MotionController/Content/Animations/Humanoid/Climbing/unity_Idle_JumpUpMedium_2Hands_Idle.fbx/LegUpToIdle.anim", "LegUpToIdle", mLegUpToIdle);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
