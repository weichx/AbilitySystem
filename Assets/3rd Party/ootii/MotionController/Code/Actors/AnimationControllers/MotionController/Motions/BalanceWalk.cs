using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Timing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Idle motion for when the character is just standing and waiting
    /// for input or some interaction.
    /// </summary>
    [MotionName("Balance Walk")]
    [MotionDescription("Slow walk for balance beams and tight ropes.")]
    public class BalanceWalk : MotionControllerMotion
    {
        private const float EPSILON = 0.01f;

        /// <summary>
        /// Trigger values for th emotion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1400;
        public const int PHASE_FALL = 1405;
        public const int PHASE_END = 1410;

        /// <summary>
        /// Width that we're testing for.
        /// </summary>
        public float _MinBeamWidth = 0.05f;
        public float MinBeamWidth
        {
            get { return _MinBeamWidth; }
            set { _MinBeamWidth = value; }
        }

        /// <summary>
        /// Width that we're testing for.
        /// </summary>
        public float _MaxBeamWidth = 0.1f;
        public float MaxBeamWidth
        {
            get { return _MaxBeamWidth; }
            set { _MaxBeamWidth = value; }
        }

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
        /// Determines if the balance is activated using raycasts
        /// </summary>
        public bool _ActivateUsingRaycasts = true;
        public bool ActivateUsingRaycasts
        {
            get { return _ActivateUsingRaycasts; }
            set { _ActivateUsingRaycasts = value; }
        }

        /// <summary>
        /// Determines if the balance is activated using the layer tag of the object
        /// </summary>
        public bool _ActivateUsingLayers = false;
        public bool ActivateUsingLayers
        {
            get { return _ActivateUsingLayers; }
            set { _ActivateUsingLayers = value; }
        }

        /// <summary>
        /// Layer(s) that will automatically trigger the balance walk
        /// </summary>
        public int _BalanceLayers = 0;
        public int BalanceLayers
        {
            get { return _BalanceLayers; }
            set { _BalanceLayers = value; }
        }

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Checks if we're in the process of falling
        /// </summary>
        protected bool mIsFalling = false;

        /// <summary>
        /// Adds some push away from the beam
        /// </summary>
        protected Vector3 mPushDirection = Vector3.zero;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BalanceWalk()
            : base()
        {
            _Priority = 15;
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "BalanceWalk-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public BalanceWalk(MotionController rController)
            : base(rController)
        {
            _Priority = 15;
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "BalanceWalk-SM"; }
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
            if (!mIsStartable) { return false; }
            if (!mMotionController.IsGrounded) { return false; }

            if (!TestBeamWidth(_MinBeamWidth, _MaxBeamWidth)) { return false; }

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

            // Cancel if we're not in the motion
            if (mIsAnimatorActive && !IsInMotionState)
            {
                return false;
            }

            // If we are no longer falling, get out
            if (mMotionController.IsGrounded && mIsFalling)
            {
                return false;
            }

            // Deactivate if we're falling
            if (mMotionLayer._AnimatorStateID == STATE_BalanceFallLeft)
            {
                if (mMotionLayer._AnimatorStateNormalizedTime > 0.3f)
                {
                    return false;
                }
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
            mIsFalling = false;
            mPushDirection = Vector3.zero;

            mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);

            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
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
            // No automatic rotation in this motion
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
            mMovement = Vector3.zero;

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

            // If we're no longer grounded, we fell off
            if (!mActorController.State.IsGrounded)
            {
                // Move to our fall off animation if we're not there
                if (!mIsFalling || mMotionLayer._AnimatorStateID != STATE_BalanceFallLeft)
                {
                    mIsFalling = true;
                    mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_FALL, true);
                }

                // Add a small amount of push away from the beam
                RaycastHit lSphereHit;
                if (RaycastExt.SafeSphereCast(mActorController._Transform.position + (mActorController._Transform.up * mActorController._BaseRadius), -mActorController._Transform.up, mActorController._BaseRadius, out lSphereHit, mActorController._BaseRadius * 2f, -1, mActorController._Transform))
                {
                    Vector3 lHitDirection = (lSphereHit.point - mActorController._Transform.position).normalized;
                    if (lHitDirection != -mActorController._Transform.up)
                    {
                        mPushDirection = -(lHitDirection - Vector3.Project(lHitDirection, mActorController._Transform.up));
                    }
                }

                mMovement = mPushDirection * rDeltaTime;
            }

            // If we're not falling, we if we're done with the beam
            if (!mIsFalling)
            {
                bool lIsValidGroundWidth = TestBeamWidth(_MinBeamWidth, _MaxBeamWidth);

                if (mMotionLayer._AnimatorStateID == STATE_IdlePose)
                {
                    if (lIsValidGroundWidth)
                    {
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);
                    }
                    else if (mActorController.State.IsGrounded)
                    {
                        if (mMotionLayer._AnimatorStateNormalizedTime > 1f)
                        {
                            Deactivate();
                        }
                    }
                    else
                    { 
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_END, true);
                    }
                }
                else
                {
                    // If we are no longer on a balance beam, we can stop
                    if (!lIsValidGroundWidth)
                    {
                        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_END, true);
                    }
                }
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

            // Only process if we're currently viewing
            if (mMotionController._InputSource.IsViewingActivated)
            {
                float lYaw = mMotionController._InputSource.ViewX;
                rRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);
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
        /// Tests the ground under the actor to determine if we're dealing with a balance beam
        /// or object similiar.
        /// </summary>
        /// <param name="rWidth"></param>
        /// <returns></returns>
        private bool TestBeamWidth(float rMinWidth, float rMaxWidth)
        {
            if (_ActivateUsingLayers)
            {
                if (mActorController.State.Ground != null)
                {
                    GameObject lGameObject = mActorController.State.Ground.gameObject;

                    // Convert the object's layer to a bitfield for comparison
                    int lLayerMask = (1 << lGameObject.layer);
                    if ((_BalanceLayers & lLayerMask) > 0) { return true; }
                }
            }

            if (_ActivateUsingRaycasts)
            {
                RaycastHit lHitInfo1;
                RaycastHit lHitInfo2;

                Vector3 lRayBase = mActorController._Transform.position - (mActorController._Transform.up * (mActorController._SkinWidth * 2f));
                float lRayDistance = rMaxWidth * 3f;
                float lRayOffset = lRayDistance * 0.5f;

                Vector3 lRayDirection = mActorController._Transform.right;
                Vector3 lRayStart = lRayBase - (lRayDirection * lRayOffset);
                if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo1, lRayDistance, -1, mActorController._Transform))
                {
                    lRayDirection = -lRayDirection;
                    lRayStart = lRayBase - (lRayDirection * lRayOffset);
                    if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo2, lRayDistance, -1, mActorController._Transform))
                    {
                        if (lHitInfo1.collider == lHitInfo2.collider)
                        {
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo1.point, 0.02f, Color.red, 1f);
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo2.point, 0.02f, Color.blue, 1f);

                            float lDistance = Vector3.Distance(lHitInfo1.point, lHitInfo2.point);
                            if (lDistance > rMinWidth && lDistance - EPSILON < rMaxWidth)
                            {
                                float lAngle = Vector3.Angle(lHitInfo1.normal, lHitInfo2.normal);
                                if (lAngle > 170f) { return true; }
                            }
                        }
                    }
                }

                lRayDirection = mActorController._Transform.forward;
                lRayStart = lRayBase - (lRayDirection * lRayOffset);
                if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo1, lRayDistance, -1, mActorController._Transform))
                {
                    lRayDirection = -lRayDirection;
                    lRayStart = lRayBase - (lRayDirection * lRayOffset);
                    if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo2, lRayDistance, -1, mActorController._Transform))
                    {
                        if (lHitInfo1.collider == lHitInfo2.collider)
                        {
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo1.point, 0.02f, Color.red, 1f);
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo2.point, 0.02f, Color.blue, 1f);

                            float lDistance = Vector3.Distance(lHitInfo1.point, lHitInfo2.point);
                            if (lDistance > rMinWidth && lDistance - EPSILON < rMaxWidth)
                            {
                                float lAngle = Vector3.Angle(lHitInfo1.normal, lHitInfo2.normal);
                                if (lAngle > 170f) { return true; }
                            }
                        }
                    }
                }

                lRayDirection = (mActorController._Transform.forward + mActorController._Transform.right).normalized;
                lRayStart = lRayBase - (lRayDirection * lRayOffset);
                if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo1, lRayDistance, -1, mActorController._Transform))
                {
                    lRayDirection = -lRayDirection;
                    lRayStart = lRayBase - (lRayDirection * lRayOffset);
                    if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo2, lRayDistance, -1, mActorController._Transform))
                    {
                        if (lHitInfo1.collider == lHitInfo2.collider)
                        {
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo1.point, 0.02f, Color.red, 1f);
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo2.point, 0.02f, Color.blue, 1f);

                            float lDistance = Vector3.Distance(lHitInfo1.point, lHitInfo2.point);
                            if (lDistance > rMinWidth && lDistance - EPSILON < rMaxWidth)
                            {
                                float lAngle = Vector3.Angle(lHitInfo1.normal, lHitInfo2.normal);
                                if (lAngle > 170f) { return true; }
                            }
                        }
                    }
                }

                lRayDirection = (mActorController._Transform.forward - mActorController._Transform.right).normalized;
                lRayStart = lRayBase - (lRayDirection * lRayOffset);
                if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo1, lRayDistance, -1, mActorController._Transform))
                {
                    lRayDirection = -lRayDirection;
                    lRayStart = lRayBase - (lRayDirection * lRayOffset);
                    if (RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lHitInfo2, lRayDistance, -1, mActorController._Transform))
                    {
                        if (lHitInfo1.collider == lHitInfo2.collider)
                        {
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo1.point, 0.02f, Color.red, 1f);
                            //com.ootii.Utilities.Debug.DebugDraw.DrawSphereMesh(lHitInfo2.point, 0.02f, Color.blue, 1f);

                            float lDistance = Vector3.Distance(lHitInfo1.point, lHitInfo2.point);
                            if (lDistance > rMinWidth && lDistance - EPSILON < rMaxWidth)
                            {
                                float lAngle = Vector3.Angle(lHitInfo1.normal, lHitInfo2.normal);
                                if (lAngle > 170f) { return true; }
                            }
                        }
                    }
                }
            }

            return false;
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

            bool lNewActivateUsingRaycasts = EditorGUILayout.Toggle(new GUIContent("Activate Using Raycasts", "Determines if we test for balancing using raycasting."), ActivateUsingRaycasts);
            if (lNewActivateUsingRaycasts != _ActivateUsingRaycasts)
            {
                lIsDirty = true;
                ActivateUsingRaycasts = lNewActivateUsingRaycasts;
            }

            if (lNewActivateUsingRaycasts)
            {
                float lNewMinBeamWidth = EditorGUILayout.FloatField(new GUIContent("Min Beam Width", "Minimum width the ground can be in order to go into balancing."), MinBeamWidth, GUILayout.MinWidth(30));
                if (lNewMinBeamWidth != MinBeamWidth)
                {
                    lIsDirty = true;
                    MinBeamWidth = lNewMinBeamWidth;
                }

                float lNewMaxBeamWidth = EditorGUILayout.FloatField(new GUIContent("Max Beam Width", "Max width the ground can be in order to go into balancing."), MaxBeamWidth, GUILayout.MinWidth(30));
                if (lNewMaxBeamWidth != MaxBeamWidth)
                {
                    lIsDirty = true;
                    MaxBeamWidth = lNewMaxBeamWidth;
                }
            }

            bool lNewActivateUsingLayers = EditorGUILayout.Toggle(new GUIContent("Activate Using Layers", "Determines if we test for balancing using the object's layer property."), ActivateUsingLayers);
            if (lNewActivateUsingLayers != _ActivateUsingLayers)
            {
                lIsDirty = true;
                ActivateUsingLayers = lNewActivateUsingLayers;
            }

            if (lNewActivateUsingLayers)
            {
                // Balance layer
                int lNewBalanceLayers = EditorHelper.LayerMaskField(new GUIContent("Balance Layers", "Layers that we'll test balancing against"), _BalanceLayers);
                if (lNewBalanceLayers != _BalanceLayers)
                {
                    lIsDirty = true;
                    _BalanceLayers = lNewBalanceLayers;
                }
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
        public static int TRANS_EntryState_BalanceIdlePose = -1;
        public static int TRANS_AnyState_BalanceIdlePose = -1;
        public static int STATE_BalanceForward = -1;
        public static int TRANS_BalanceForward_BalanceBackward = -1;
        public static int TRANS_BalanceForward_BalanceIdlePose = -1;
        public static int TRANS_BalanceForward_IdlePose = -1;
        public static int TRANS_BalanceForward_BalanceFallLeft = -1;
        public static int STATE_BalanceBackward = -1;
        public static int TRANS_BalanceBackward_BalanceForward = -1;
        public static int TRANS_BalanceBackward_BalanceIdlePose = -1;
        public static int TRANS_BalanceBackward_IdlePose = -1;
        public static int TRANS_BalanceBackward_BalanceFallLeft = -1;
        public static int STATE_BalanceIdlePose = -1;
        public static int TRANS_BalanceIdlePose_BalanceForward = -1;
        public static int TRANS_BalanceIdlePose_BalanceBackward = -1;
        public static int TRANS_BalanceIdlePose_IdlePose = -1;
        public static int TRANS_BalanceIdlePose_BalanceFallLeft = -1;
        public static int STATE_IdlePose = -1;
        public static int TRANS_IdlePose_BalanceIdlePose = -1;
        public static int STATE_BalanceFallLeft = -1;

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

                if (lStateID == STATE_BalanceForward) { return true; }
                if (lStateID == STATE_BalanceBackward) { return true; }
                if (lStateID == STATE_BalanceIdlePose) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_BalanceFallLeft) { return true; }
                if (lTransitionID == TRANS_EntryState_BalanceIdlePose) { return true; }
                if (lTransitionID == TRANS_AnyState_BalanceIdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceForward_BalanceBackward) { return true; }
                if (lTransitionID == TRANS_BalanceForward_BalanceIdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceForward_IdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceForward_BalanceFallLeft) { return true; }
                if (lTransitionID == TRANS_BalanceBackward_BalanceForward) { return true; }
                if (lTransitionID == TRANS_BalanceBackward_BalanceIdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceBackward_IdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceBackward_BalanceFallLeft) { return true; }
                if (lTransitionID == TRANS_BalanceIdlePose_BalanceForward) { return true; }
                if (lTransitionID == TRANS_BalanceIdlePose_BalanceBackward) { return true; }
                if (lTransitionID == TRANS_BalanceIdlePose_IdlePose) { return true; }
                if (lTransitionID == TRANS_BalanceIdlePose_BalanceFallLeft) { return true; }
                if (lTransitionID == TRANS_IdlePose_BalanceIdlePose) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_BalanceForward) { return true; }
            if (rStateID == STATE_BalanceBackward) { return true; }
            if (rStateID == STATE_BalanceIdlePose) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_BalanceFallLeft) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_BalanceForward) { return true; }
            if (rStateID == STATE_BalanceBackward) { return true; }
            if (rStateID == STATE_BalanceIdlePose) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_BalanceFallLeft) { return true; }
            if (rTransitionID == TRANS_EntryState_BalanceIdlePose) { return true; }
            if (rTransitionID == TRANS_AnyState_BalanceIdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceForward_BalanceBackward) { return true; }
            if (rTransitionID == TRANS_BalanceForward_BalanceIdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceForward_IdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceForward_BalanceFallLeft) { return true; }
            if (rTransitionID == TRANS_BalanceBackward_BalanceForward) { return true; }
            if (rTransitionID == TRANS_BalanceBackward_BalanceIdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceBackward_IdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceBackward_BalanceFallLeft) { return true; }
            if (rTransitionID == TRANS_BalanceIdlePose_BalanceForward) { return true; }
            if (rTransitionID == TRANS_BalanceIdlePose_BalanceBackward) { return true; }
            if (rTransitionID == TRANS_BalanceIdlePose_IdlePose) { return true; }
            if (rTransitionID == TRANS_BalanceIdlePose_BalanceFallLeft) { return true; }
            if (rTransitionID == TRANS_IdlePose_BalanceIdlePose) { return true; }
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
            TRANS_EntryState_BalanceIdlePose = mMotionController.AddAnimatorName("Entry -> Base Layer.BalanceWalk-SM.BalanceIdlePose");
            TRANS_AnyState_BalanceIdlePose = mMotionController.AddAnimatorName("AnyState -> Base Layer.BalanceWalk-SM.BalanceIdlePose");
            STATE_BalanceForward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceForward");
            TRANS_BalanceForward_BalanceBackward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceForward -> Base Layer.BalanceWalk-SM.BalanceBackward");
            TRANS_BalanceForward_BalanceIdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceForward -> Base Layer.BalanceWalk-SM.BalanceIdlePose");
            TRANS_BalanceForward_IdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceForward -> Base Layer.BalanceWalk-SM.IdlePose");
            TRANS_BalanceForward_BalanceFallLeft = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceForward -> Base Layer.BalanceWalk-SM.BalanceFallLeft");
            STATE_BalanceBackward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceBackward");
            TRANS_BalanceBackward_BalanceForward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceBackward -> Base Layer.BalanceWalk-SM.BalanceForward");
            TRANS_BalanceBackward_BalanceIdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceBackward -> Base Layer.BalanceWalk-SM.BalanceIdlePose");
            TRANS_BalanceBackward_IdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceBackward -> Base Layer.BalanceWalk-SM.IdlePose");
            TRANS_BalanceBackward_BalanceFallLeft = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceBackward -> Base Layer.BalanceWalk-SM.BalanceFallLeft");
            STATE_BalanceIdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceIdlePose");
            TRANS_BalanceIdlePose_BalanceForward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceIdlePose -> Base Layer.BalanceWalk-SM.BalanceForward");
            TRANS_BalanceIdlePose_BalanceBackward = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceIdlePose -> Base Layer.BalanceWalk-SM.BalanceBackward");
            TRANS_BalanceIdlePose_IdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceIdlePose -> Base Layer.BalanceWalk-SM.IdlePose");
            TRANS_BalanceIdlePose_BalanceFallLeft = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceIdlePose -> Base Layer.BalanceWalk-SM.BalanceFallLeft");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.IdlePose");
            TRANS_IdlePose_BalanceIdlePose = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.IdlePose -> Base Layer.BalanceWalk-SM.BalanceIdlePose");
            STATE_BalanceFallLeft = mMotionController.AddAnimatorName("Base Layer.BalanceWalk-SM.BalanceFallLeft");
        }

#if UNITY_EDITOR

        private AnimationClip mBalanceForward = null;
        private AnimationClip mBalanceBackward = null;
        private AnimationClip mBalanceIdlePose = null;
        private AnimationClip mIdlePose = null;
        private AnimationClip mBalanceFallLeft = null;

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

            UnityEditor.Animations.AnimatorState lBalanceForward = lMotionStateMachine.AddState("BalanceForward", new Vector3(204, 180, 0));
            lBalanceForward.motion = mBalanceForward;
            lBalanceForward.speed = 0.6f;

            UnityEditor.Animations.AnimatorState lBalanceBackward = lMotionStateMachine.AddState("BalanceBackward", new Vector3(480, 180, 0));
            lBalanceBackward.motion = mBalanceBackward;
            lBalanceBackward.speed = 0.3f;

            UnityEditor.Animations.AnimatorState lBalanceIdlePose = lMotionStateMachine.AddState("BalanceIdlePose", new Vector3(336, 72, 0));
            lBalanceIdlePose.motion = mBalanceIdlePose;
            lBalanceIdlePose.speed = 0.1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(192, 372, 0));
            lIdlePose.motion = mIdlePose;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lBalanceFallLeft = lMotionStateMachine.AddState("BalanceFallLeft", new Vector3(492, 372, 0));
            lBalanceFallLeft.motion = mBalanceFallLeft;
            lBalanceFallLeft.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lBalanceIdlePose);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.2f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1400f, "L0MotionPhase");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lBalanceForward.AddTransition(lBalanceBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7500005f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            lStateTransition = lBalanceForward.AddTransition(lBalanceBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7500005f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            lStateTransition = lBalanceForward.AddTransition(lBalanceIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7500005f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lBalanceForward.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7500005f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1410f, "L0MotionPhase");

            lStateTransition = lBalanceForward.AddTransition(lBalanceFallLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7272731f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1405f, "L0MotionPhase");

            lStateTransition = lBalanceBackward.AddTransition(lBalanceForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7000002f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            lStateTransition = lBalanceBackward.AddTransition(lBalanceIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7000002f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lBalanceBackward.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.7000002f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1410f, "L0MotionPhase");

            lStateTransition = lBalanceBackward.AddTransition(lBalanceFallLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3333367f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1405f, "L0MotionPhase");

            lStateTransition = lBalanceIdlePose.AddTransition(lBalanceForward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 7.428772E-10f;
            lStateTransition.duration = 0.09999999f;
            lStateTransition.offset = 0.5934225f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -80f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 80f, "InputAngleFromAvatar");

            lStateTransition = lBalanceIdlePose.AddTransition(lBalanceBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 8.051802E-10f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0.192521f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -110f, "InputAngleFromAvatar");

            lStateTransition = lBalanceIdlePose.AddTransition(lBalanceBackward);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 8.051802E-10f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0.1283474f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 110f, "InputAngleFromAvatar");

            lStateTransition = lBalanceIdlePose.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1410f, "L0MotionPhase");

            lStateTransition = lBalanceIdlePose.AddTransition(lBalanceFallLeft);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3333333f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1405f, "L0MotionPhase");

            lStateTransition = lIdlePose.AddTransition(lBalanceIdlePose);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0.25f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1400f, "L0MotionPhase");

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mBalanceForward = CreateAnimationField("BalanceForward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Navigating/cmu_Balance_89_06.fbx/BalanceForward.anim", "BalanceForward", mBalanceForward);
            mBalanceBackward = CreateAnimationField("BalanceBackward", "Assets/ootii/MotionController/Content/Animations/Humanoid/Navigating/cmu_Balance_89_06.fbx/BalanceBackward.anim", "BalanceBackward", mBalanceBackward);
            mBalanceIdlePose = CreateAnimationField("BalanceIdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Navigating/cmu_Balance_89_06.fbx/BalanceIdlePose.anim", "BalanceIdlePose", mBalanceIdlePose);
            mIdlePose = CreateAnimationField("IdlePose", "Assets/ootii/MotionController/Content/Animations/Humanoid/Idling/unity_Idle_IdleToIdlesR.fbx/IdlePose.anim", "IdlePose", mIdlePose);
            mBalanceFallLeft = CreateAnimationField("BalanceFallLeft", "Assets/ootii/MotionController/Content/Animations/Humanoid/Navigating/cmu_Balance_89_06.fbx/BalanceFallLeft.anim", "BalanceFallLeft", mBalanceFallLeft);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}
