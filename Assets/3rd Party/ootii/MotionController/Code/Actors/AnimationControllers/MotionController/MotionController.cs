//#define OOTII_PROFILE
#define USE_MOTION_STATE_TIME

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Cameras;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Messages;
using com.ootii.Physics;
using com.ootii.Timing;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Delegate to support activation events
    /// </summary>
    public delegate void MotionActivationEvent(int rLayer, MotionControllerMotion rNewMotion, MotionControllerMotion rOldMotion);

    /// <summary>
    /// Delegate to support update events
    /// </summary>
    public delegate void MotionUpdateEvent(float rDeltaTime, int rUpdateCount, int rLayer, MotionControllerMotion rMotion);

    /// <summary>
    /// Delegate to support deactivation events
    /// </summary>
    public delegate void MotionEvent(int rLayer, MotionControllerMotion rMotion);

    /// <summary>
    /// The Motion Controller is built to manage character animations
    /// like run, jump, climb, fight, etc. We use layers which hold motions
    /// in order to manage the controller's state.
    /// 
    /// Mechanim's animator is still critical to the process.
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    [AddComponentMenu("ootii/Motion Controller")]
    public class MotionController : MonoBehaviour
    {
        /// <summary>
        /// Distance the avatar's feed are from the ground before
        /// it is considered on the ground.
        /// </summary>
        public const float GROUND_DISTANCE_TEST = 0.075f;

        /// <summary>
        /// Keeps us from creating string names each frame
        /// </summary>
        public static string[] MOTION_PHASE_NAMES = { "L0MotionPhase", "L1MotionPhase", "L2MotionPhase", "L3MotionPhase", "L4MotionPhase", "L5MotionPhase", "L6MotionPhase", "L7MotionPhase", "L8MotionPhase", "L9MotionPhase" };
        public static string[] MOTION_PARAMETER_NAMES = { "L0MotionParameter", "L1MotionParameter", "L2MotionParameter", "L3MotionParameter", "L4MotionParameter", "L5MotionParameter", "L6MotionParameter", "L7MotionParameter", "L8MotionParameter", "L9MotionParameter" };
        public static string[] MOTION_STATE_TIME = { "L0MotionStateTime", "L1MotionStateTime", "L2MotionStateTime", "L3MotionStateTime", "L4MotionStateTime", "L5MotionStateTime", "L6MotionStateTime", "L7MotionStateTime", "L8MotionStateTime", "L9MotionStateTime" };

        /// <summary>
        /// Tracks how long the update process is taking
        /// </summary>
        private static Utilities.Profiler mUpdateProfiler = new Utilities.Profiler("MotionController");
        public static Utilities.Profiler UpdateProfiler
        {
            get { return mUpdateProfiler; }
        }

        /// <summary>
        /// This transform. We cache this so we're not actually doing a Get everytime we access 'transform'
        /// </summary>
        [NonSerialized]
        public Transform _Transform = null;
        public Transform Transform
        {
            get { return _Transform; }
        }

        /// <summary>
        /// Actor controller that drives the actor
        /// </summary>
        [NonSerialized]
        public ActorController _ActorController = null;
        public ActorController ActorController
        {
            get { return _ActorController; }
            set { _ActorController = value; }
        }

        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }
            set { _InputSourceOwner = value; }
        }

        /// <summary>
        /// Defines the source of the input that we'll use to control
        /// the character movement, rotations, and animations.
        /// </summary>
        [NonSerialized]
        public IInputSource _InputSource = null;
        public IInputSource InputSource
        {
            get { return _InputSource; }
            set { _InputSource = value; }
        }

        /// <summary>
        /// Determines if we'll auto find the input source if one doesn't exist
        /// </summary>
        public bool _AutoFindInputSource = true;
        public bool AutoFindInputSource
        {
            get { return _AutoFindInputSource; }
            set { _AutoFindInputSource = value; }
        }

        /// <summary>
        /// Event for when a motion is activated. Allows external callers to tap into it.
        /// </summary>
        [NonSerialized]
        public MotionActivationEvent MotionActivated = null;

        /// <summary>
        /// Event for after the active motion has been updated.
        /// </summary>
        [NonSerialized]
        public MotionUpdateEvent MotionUpdated = null;

        /// <summary>
        /// Event for when a motion is deactivated. Allows external callers to tap into it.
        /// </summary>
        [NonSerialized]
        public MotionEvent MotionDeactivated = null;

        /// <summary>
        /// Determines if we're simulating input. This value will automatically be
        /// set when any target velocity or position is set.
        /// </summary>
        public bool _UseSimulatedInput = false;
        public bool UseSimulatedInput
        {
            get { return _UseSimulatedInput; }
            set { _UseSimulatedInput = value; }
        }

        /// <summary>
        /// Transform we used to understand the camera's position and orientation
        /// </summary>
        public Transform _CameraTransform;
        public Transform CameraTransform
        {
            get { return _CameraTransform; }
            set { _CameraTransform = value; }
        }

        /// <summary>
        /// Camera rig that may be in use. Typically, we extract this from the transform
        /// </summary>
        protected IBaseCameraRig mCameraRig = null;
        public IBaseCameraRig CameraRig
        {
            get { return mCameraRig; }

            set
            {
                mCameraRig = value;
                if (mCameraRig != null)
                {
                    _CameraTransform = mCameraRig.Transform;
                }
            }
        }

        /// <summary>
        /// Determines if we'll auto find the camera if one doesn't exist
        /// </summary>
        public bool _AutoFindCameraTransform = true;
        public bool AutoFindCameraTransform
        {
            get { return _AutoFindCameraTransform; }
            set { _AutoFindCameraTransform = value; }
        }

        /// <summary>
        /// Active motion on the primary (first) layer
        /// </summary>
        public MotionControllerMotion ActiveMotion
        {
            get
            {
                if (MotionLayers.Count > 0)
                {
                    return MotionLayers[0].ActiveMotion;
                }

                return null;
            }
        }

        /// <summary>
        /// Time smoothing is used to average delta-time over several frames
        /// in order to prevent jumps in movement due to time spikes.
        /// </summary>
        public bool _IsTimeSmoothingEnabled = false;
        public bool IsTimeSmoothingEnabled
        {
            get { return _IsTimeSmoothingEnabled; }
            set { _IsTimeSmoothingEnabled = value; }
        }

        /// <summary>
        /// Defines the max speed of the actor when in a full run
        /// </summary>
        public float _MaxSpeed = 7f;
        public float MaxSpeed
        {
            get { return _MaxSpeed; }
            set { _MaxSpeed = value; }
        }

        /// <summary>
        /// Determines how quickly the character is able to rotate
        /// </summary>
        public float _RotationSpeed = 360f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
        }

        /// <summary>
        /// When simulating input, this gives us a velocity 
        /// the controller uses to move with. The controller converts this information
        /// into psuedo input values to calculate movement.
        /// </summary>
        protected Vector3 mTargetVelocity = Vector3.zero;
        public Vector3 TargetVelocity
        {
            get { return mTargetVelocity; }
        }

        /// <summary>
        /// When simulating input, this gives us a target to move
        /// the controller to. The controller converts this information
        /// into psuedo input values to calculate movement.
        /// </summary>
        protected Vector3 mTargetPosition = Vector3.zero;
        public Vector3 TargetPosition
        {
            get { return mTargetPosition; }
        }

        /// <summary>
        /// Determines if a rotation is actually set. If not, the animation 
        /// will probably handle the rotations.
        /// </summary>
        protected bool mIsTargetMovementSet = false;
        public bool IsTargetMovementSet
        {
            get { return mIsTargetMovementSet; }
        }

        /// <summary>
        /// When simulating input, this gives us the speed at which we
        /// should move towards the PositionTarget. Think of this as how
        /// hard we push the gamepad stick forward.
        /// </summary>
        protected float mTargetNormalizedSpeed = 1f;
        public float TargetNormalizedSpeed
        {
            get { return mTargetNormalizedSpeed; }
        }

        /// <summary>
        /// When simulating input, this gives us a target to rotate
        /// the controller to. The controller converts this information
        /// into psuedo input values to calculate rotation.
        /// </summary>
        protected Quaternion mTargetRotation = Quaternion.identity;
        public Quaternion TargetRotation
        {
            get { return mTargetRotation; }
        }

        /// <summary>
        /// Determines if a rotation is actually set. If not, the animation 
        /// will probably handle the rotations.
        /// </summary>
        protected bool mIsTargetRotationSet = false;
        public bool IsTargetRotationSet
        {
            get { return mIsTargetRotationSet; }
        }

        /// <summary>
        /// Stance that the character is currently in
        /// </summary>
        public int Stance
        {
            get { return _ActorController.State.Stance; }
        }

        /// <summary>
        /// Reports back the grounded state of the actor
        /// </summary>
        public bool IsGrounded
        {
            get { return _ActorController.State.IsGrounded; }
        }

        /// <summary>
        /// Animator that the controller works with
        /// </summary>
        protected Animator mAnimator = null;
        public Animator Animator
        {
            get { return mAnimator; }

            set
            {
                mAnimator = value;

                // Build the list of available layers
                if (mAnimator != null)
                {
                    State.AnimatorStates = new AnimatorLayerState[mAnimator.layerCount];
                    PrevState.AnimatorStates = new AnimatorLayerState[mAnimator.layerCount];

                    // Initialize our objects with each of the animator layers
                    for (int i = 0; i < State.AnimatorStates.Length; i++)
                    {
                        State.AnimatorStates[i] = new AnimatorLayerState();
                        PrevState.AnimatorStates[i] = new AnimatorLayerState();
                    }
                }
            }
        }

        /// <summary>
        /// The current state of the controller including speed, direction, etc.
        /// </summary>
        public MotionState State = new MotionState();

        /// <summary>
        /// The previous state of the controller including speed, direction, etc.
        /// </summary>
        public MotionState PrevState = new MotionState();

        /// <summary>
        /// Contains a list of forces currently being applied to
        /// the controller.
        /// </summary>
        protected List<Force> mAppliedForces = new List<Force>();
        public List<Force> AppliedForces
        {
            get { return mAppliedForces; }
            set { mAppliedForces = value; }
        }

        /// <summary>
        /// List of motions the avatar is able to perform.
        /// </summary>
        public List<MotionControllerLayer> MotionLayers = new List<MotionControllerLayer>();

        /// <summary>
        /// Determines if the MC will show debug info at all
        /// </summary>
        public bool _ShowDebug = true;
        public bool ShowDebug
        {
            get { return _ShowDebug; }
            set { _ShowDebug = value; }
        }

        /// <summary>
        /// Determines if the MC will force motions to show debug even if they have them disabled
        /// </summary>
        public bool _ShowDebugForAllMotions = false;
        public bool ShowDebugForAllMotions
        {
            get { return _ShowDebugForAllMotions; }
            set { _ShowDebugForAllMotions = value; }
        }

        /// <summary>
        /// Forces the input value based on previous values. This helps with transitions from
        /// blend trees.
        /// </summary>
        [NonSerialized]
        public Vector2 ForcedInput = Vector2.zero;

        /// <summary>
        /// The current speed trend decreasing, static, increasing (-1, 0, or 1)
        /// </summary>
        private int mSpeedTrendDirection = EnumSpeedTrend.CONSTANT;

        /// <summary>
        /// Add a delay before we update the mecanim parameters. This way we can
        /// give a chance for things like speed to settle.
        /// </summary>
        private float mMecanimUpdateDelay = 0f;

        /// <summary>
        /// Acceleration that is being processed per frame. It takes into account the
        /// forces applied and drag.
        /// </summary>
        private Vector3 mAccumulatedAcceleration = Vector3.zero;
        public Vector3 AccumulatedAcceleration
        {
            get { return mAccumulatedAcceleration; }
        }

        /// <summary>
        /// Use this to store up velocity over time
        /// </summary>
        private Vector3 mAccumulatedVelocity = Vector3.zero;
        public Vector3 AccumulatedVelocity
        {
            get { return mAccumulatedVelocity; }
            set { mAccumulatedVelocity = value; }
        }

        /// <summary>
        /// Tracks the root motion so we can apply it later
        /// </summary>
        private Vector3 mRootMotionMovement = Vector3.zero;
        public Vector3 RootMotionMovement
        {
            get { return mRootMotionMovement; }
            set { mRootMotionMovement = value; }
        }

        /// <summary>
        /// Tracks the root motion rotation so we can apply it later
        /// </summary>
        private Quaternion mRootMotionRotation = Quaternion.identity;
        public Quaternion RootMotionRotation
        {
            get { return mRootMotionRotation; }
            set { mRootMotionRotation = value; }
        }

        /// <summary>
        /// Stores the animator state names by hash-id
        /// </summary>	
        [HideInInspector]
        public Dictionary<int, string> AnimatorStateNames = new Dictionary<int, string>();

        /// <summary>
        /// Stores the animator hash-ids by state name
        /// </summary>
        [HideInInspector]
        public Dictionary<string, int> AnimatorStateIDs = new Dictionary<string, int>();

        /// <summary>
        /// Serialized transforms that may be used by the motors. We have to store them
        /// here since Unity won't serialize them with and support polymorphism.
        /// </summary>
        public List<Transform> StoredTransforms = new List<Transform>();

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected void Awake()
        {
            _Transform = transform;

            // Grab a reference to the actor controller
            _ActorController = gameObject.GetComponent<ActorController>();
            if (this.enabled) { _ActorController.OnControllerPreLateUpdate += OnControllerLateUpdate; }

            // Initialize the camera if possible
            if (_AutoFindCameraTransform && _CameraTransform == null)
            {
                // We'll grab the main camera and store it's transform. This transform will hold
                // all the parent data as well as local data (which is typically empty)
                Camera lCamera = UnityEngine.Camera.main;
                if (lCamera == null) { lCamera = Component.FindObjectOfType<Camera>(); }
                if (lCamera != null)
                {
                    mCameraRig = ExtractCameraRig(lCamera.transform);
                    if (mCameraRig != null) { _CameraTransform = ((MonoBehaviour)mCameraRig).gameObject.transform; }

                    if (_CameraTransform == null) { _CameraTransform = lCamera.transform; }
                }
            }

            if (_CameraTransform != null)
            { 
                // If we find we have a camera rig with no anchor set, make this the anchor. This is
                // useful for script based initializers like ORK
                mCameraRig = ExtractCameraRig(_CameraTransform);
                if (mCameraRig != null && mCameraRig.Anchor == null) { mCameraRig.Anchor = _Transform; }
            }

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { _InputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // If the input source is still null, see if we can grab a local input source
            if (_AutoFindInputSource && _InputSource == null)
            {
                _InputSource = InterfaceHelper.GetComponent<IInputSource>(gameObject);
                if (_InputSource != null) { _InputSourceOwner = gameObject; }
            }

            // If that's still null, see if we can grab one from the scene. This may happen
            // if the MC was instanciated from a prefab which doesn't hold a reference to the input source
            if (_AutoFindInputSource && _InputSource == null)
            {
                IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
                for (int i = 0; i < lInputSources.Length; i++)
                {
                    GameObject lInputSourceOwner = ((MonoBehaviour)lInputSources[i]).gameObject;
                    if (lInputSourceOwner.activeSelf && lInputSources[i].IsEnabled)
                    {
                        _InputSource = lInputSources[i];
                        _InputSourceOwner = lInputSourceOwner;
                    }
                }
            }

            // Load the animator and grab all the state info
            mAnimator = GetComponent<Animator>();

            // Build the list of available layers
            if (mAnimator != null)
            {
                State.AnimatorStates = new AnimatorLayerState[mAnimator.layerCount];
                PrevState.AnimatorStates = new AnimatorLayerState[mAnimator.layerCount];

                // Initialize our objects with each of the animator layers
                for (int i = 0; i < State.AnimatorStates.Length; i++)
                {
                    State.AnimatorStates[i] = new AnimatorLayerState();
                    PrevState.AnimatorStates[i] = new AnimatorLayerState();
                }
            }

            // Ensure the layers and motions know about the controller
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                MotionLayers[i].MotionController = this;
                MotionLayers[i].Awake();
            }

            // Load the animator state and transition hash IDs
            LoadAnimatorData();
        }

        /// <summary>
        /// Called right before the first frame update
        /// </summary>
        protected void Start()
        {
#if USE_MESSAGE_DISPATCHER

            // Hook into the Message Dispatcher so we can listen for them
            MessageDispatcher.AddListener(this, "", OnMessageReceived, true);

#endif
        }

        /// <summary>
        /// Called when the component is enabled. This is also called after awake. So,
        /// we need to ensure we're not doubling up on the assignment.
        /// </summary>
        protected void OnEnable()
        {
            if (_ActorController != null)
            {
                if (_ActorController.OnControllerPreLateUpdate != null) { _ActorController.OnControllerPreLateUpdate -= OnControllerLateUpdate; }
                _ActorController.OnControllerPreLateUpdate += OnControllerLateUpdate;
            }
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            if (_ActorController != null && _ActorController.OnControllerPreLateUpdate != null)
            {
                _ActorController.OnControllerPreLateUpdate -= OnControllerLateUpdate; 
            }
        }

        /// <summary>
        /// Called once per frame to update objects. This happens after FixedUpdate().
        /// Reactions to calculations should be handled here.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public void OnControllerLateUpdate(ICharacterController rController, float rDeltaTime, int rUpdateIndex)
        {
            float lDeltaTime = (_IsTimeSmoothingEnabled ? TimeManager.SmoothedDeltaTime : rDeltaTime);

            //Log.FileWrite("MC.ControllerLateUpdate() ui:" + rUpdateIndex + " dt:" + lDeltaTime.ToString("f5"));

            // Start the timer for tracking performance
            mUpdateProfiler.Start();

            // Determines if we wait for a trend to stop before
            // passing information to the animator
            bool lUseTrendData = false;

            // 1. Shift the current state to previous and initialize the current
#if OOTII_PROFILE
            Utilities.Profiler.Start("01");
#endif
            MotionState.Shift(ref State, ref PrevState);
#if OOTII_PROFILE
            Utilities.Profiler.Stop("01");
#endif

            // 2. Update the animator state and transition information so it can by
            // used by the motions
#if OOTII_PROFILE
            Utilities.Profiler.Start("02");
#endif
            int lCount = 0;

            if (mAnimator != null)
            {
                lCount = State.AnimatorStates.Length;
                for (int i = 0; i < lCount; i++)
                {
                    MotionControllerMotion lActiveMotion = null;
                    if (i < MotionLayers.Count && MotionLayers[i] != null) { lActiveMotion = MotionLayers[i].ActiveMotion; }

                    State.AnimatorStates[i].StateInfo = mAnimator.GetCurrentAnimatorStateInfo(i);
                    State.AnimatorStates[i].TransitionInfo = mAnimator.GetAnimatorTransitionInfo(i);

                    // Check if it's time to clear the motion phase based on reaching a state or
                    // transition that is in the motion that is currently active
                    if (State.AnimatorStates[i].AutoClearMotionPhase && lActiveMotion != null)
                    {
                        if (lActiveMotion != null && lActiveMotion.HasAutoGeneratedCode)
                        {
                            if (lActiveMotion.IsMotionState(State.AnimatorStates[i].StateInfo.fullPathHash, State.AnimatorStates[i].TransitionInfo.fullPathHash))
                            {
                                lActiveMotion.IsAnimatorActive = true; 
                                State.AnimatorStates[i].MotionPhase = 0;
                                State.AnimatorStates[i].AutoClearMotionPhase = false;
                                State.AnimatorStates[i].AutoClearActiveTransitionID = 0;
                            }
                        }
                    }

                    // If we have auto-generated code, we'll use the "IsInMotion" to see if we can clear the phase
                    if (lActiveMotion != null && !lActiveMotion.HasAutoGeneratedCode)
                    {
                        // We need to see if we're ready to clear the motion phase
                        if (State.AnimatorStates[i].AutoClearMotionPhase && State.AnimatorStates[i].TransitionInfo.fullPathHash != 0)
                        {
                            // Update the transition info so the motion knows the animator is in synch
                            if (State.AnimatorStates[i].TransitionInfo.fullPathHash != State.AnimatorStates[i].AutoClearActiveTransitionID)
                            {
                                lActiveMotion.IsAnimatorActive = true;
                                State.AnimatorStates[i].AutoClearMotionPhaseReady = true;
                            }
                        }

                        // If the state has changed, raise the event and say we're ready to clear the phase
                        if (State.AnimatorStates[i].StateInfo.fullPathHash != PrevState.AnimatorStates[i].StateInfo.fullPathHash)
                        {
                            // Report the state change
                            OnAnimatorStateChange(i);

                            // Just as a backup, update the transition info so the motion knows the animator is in synch
                            if (lActiveMotion.IsMotionState(State.AnimatorStates[i].StateInfo.fullPathHash))
                            {
                                lActiveMotion.IsAnimatorActive = true;
                                State.AnimatorStates[i].AutoClearMotionPhaseReady = true;
                            }
                        }
                    }
                }
            }

#if OOTII_PROFILE
            Utilities.Profiler.Stop("02");
#endif

            // 4. Grab the direction and speed of the input from the keyboard, game controller, etc.
#if OOTII_PROFILE
            Utilities.Profiler.Start("04");
#endif

            if (_ActorController._UseTransformPosition)
            {
                ProcessMovementInput();
            }
            else if (_UseSimulatedInput || _InputSource == null || !_InputSource.IsEnabled)
            {
                ProcessSimulatedInput();
            }
            else
            {
                ProcessUserInput();
            }

#if OOTII_PROFILE
            Utilities.Profiler.Stop("04");
#endif

            // 5. Clean the existing root motion so we don't have motion we don't want
            // 6. Update each layer to determine the final velocity and rotation
#if OOTII_PROFILE
            Utilities.Profiler.Start("06");
#endif

            lCount = MotionLayers.Count;
            for (int i = 0; i < lCount; i++)
            {
                MotionLayers[i].UpdateRootMotion(lDeltaTime, rUpdateIndex, ref mRootMotionMovement, ref mRootMotionRotation);
                MotionLayers[i].UpdateMotions(lDeltaTime, rUpdateIndex);
            }

            if (MotionLayers.Count > 0)
            {
                if (MotionLayers[0].UseTrendData) { lUseTrendData = true; }
            }

#if OOTII_PROFILE
            Utilities.Profiler.Stop("06");
#endif

            // 7. Determine the trend so we can figure out acceleration
#if OOTII_PROFILE
            Utilities.Profiler.Start("07");
#endif
            DetermineTrendData();
#if OOTII_PROFILE
            Utilities.Profiler.Stop("07");
#endif

            // When we update the controller here, things are smooth and in synch
            // with the camera. If we put this code in the FixedUpdate() or OnAnimateMove()
            // the camera is out of synch with the camera (in LateUpdate()) and avatar stutters.
            //
            // We need the camera in LateUpdate() since this is where it's smoothest and 
            // preceeds for each draw call.

            // 8. Apply rotation
#if OOTII_PROFILE
            Utilities.Profiler.Start("08");
#endif


            // Finally, set the target rotation
            if (mIsTargetRotationSet)
            {
                _ActorController.SetRotation(mTargetRotation);
            }
            else
            {
                // Apply the angular velocity from each of the active motions
                Vector3 lMotionAngularVelocity = Vector3.zero;
                Quaternion lMotionRotation = Quaternion.identity;
                Quaternion lMotionTilt = Quaternion.identity;

                for (int i = 0; i < MotionLayers.Count; i++)
                {
                    lMotionAngularVelocity = lMotionAngularVelocity + MotionLayers[i].AngularVelocity;
                    lMotionRotation = lMotionRotation * MotionLayers[i].Rotation;
                    lMotionTilt = lMotionTilt * MotionLayers[i].Tilt;
                }

                // Rotate the avatar
                //_ActorController.Rotate(mRootMotionRotation * lMotionRotation * Quaternion.Euler(lMotionAngularVelocity * lDeltaTime));
                _ActorController.Rotate(mRootMotionRotation * lMotionRotation * Quaternion.Euler(lMotionAngularVelocity * lDeltaTime), lMotionTilt);
            }

#if OOTII_PROFILE
            Utilities.Profiler.Stop("08");
#endif

            // 9. Apply translation
#if OOTII_PROFILE
            Utilities.Profiler.Start("09");
#endif

            Vector3 lMotionVelocity = Vector3.zero;
            Vector3 lMotionMovement = Vector3.zero;
            int lMotionLayerCount = MotionLayers.Count;

            // Apply the velocity from each of the active motions
            if (lMotionLayerCount > 0)
            {
                for (int i = 0; i < lMotionLayerCount; i++)
                {
                    lMotionVelocity = lMotionVelocity + MotionLayers[i].Velocity;
                    lMotionMovement = lMotionMovement + MotionLayers[i].Movement;
                }
            }

            Vector3 lFinalMovement = (_Transform.rotation * mRootMotionMovement) + (lMotionVelocity * lDeltaTime) + lMotionMovement;

            // Use the new movement to move the actor.
            _ActorController.Move(lFinalMovement);

#if OOTII_PROFILE
            Utilities.Profiler.Stop("09");
#endif

            // 11. Send the current state data to the animator
#if OOTII_PROFILE
            Utilities.Profiler.Start("11");
#endif

            // We don't update the animator if we need to skip
            // a frame. This way we don't send a bad frame
            if (rUpdateIndex >= 1)
            {
                SetAnimatorProperties(ref State, lUseTrendData);
            }

#if OOTII_PROFILE
            Utilities.Profiler.Stop("11");
#endif

            // Stop the timer
            mUpdateProfiler.Stop();

#if OOTII_PROFILE

            //if (_Transform.name == "Jones")
            {
                Log.FileScreenWrite(String.Format("{0} MC.Update() Motion:{1} MotionDur:{2:f4} Phase:{3} Param:{4} State:{5}", name, (GetActiveMotion(0) != null ? GetActiveMotion(0).GetType().Name : "null"), (GetActiveMotion(0) != null ? GetActiveMotion(0).Age : 0), State.AnimatorStates[0].MotionPhase, State.AnimatorStates[0].MotionParameter, AnimatorHashToString(State.AnimatorStates[0].StateInfo, State.AnimatorStates[0].TransitionInfo)), 3);
                Log.FileScreenWrite(String.Format("{0} MC.Update() isGrv:{1} isGnd:{2} GSD:{3:f4} GSA:{4:f4} Grn:{5}", name, _ActorController.IsGravityEnabled, _ActorController.IsGrounded, (_ActorController.State.Ground == null ? "inf" : _ActorController.State.GroundSurfaceDistance.ToString("f3")), _ActorController.State.GroundSurfaceAngle.ToString("f3"), (_ActorController.State.Ground == null ? "null" : _ActorController.State.Ground.name)), 4);
                Log.FileScreenWrite(String.Format("IM:{0:f3} IFA:{1:f3} IFC:{2:f3} Input:{3:f3}, {4:f3} FInput:{5:f3}, {6:f3}", State.InputMagnitudeTrend.Value, State.InputFromAvatarAngle, State.InputFromCameraAngle, State.InputX, State.InputY, ForcedInput.x, ForcedInput.y), 5);

                if (_CameraTransform != null)
                {
                    float lCameraFromAvatar = NumberHelper.GetHorizontalAngle(_Transform.forward, _CameraTransform.forward);
                    Log.FileScreenWrite(String.Format("CFA:{0:f3}", lCameraFromAvatar), 6);
                }

                Log.FileWrite("");
            }

#endif

        }

        /// <summary>
        /// Determines if the specified motion is currently active
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        public bool IsMotionActive(int rLayerIndex, Type rType)
        {
            if (rLayerIndex >= MotionLayers.Count) { return false; }
            if (MotionLayers[rLayerIndex].ActiveMotion == null) { return false; }

            return (MotionLayers[rLayerIndex].ActiveMotion.GetType() == rType);
        }

        /// <summary>
        /// Determines if the specified motion is currently active
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        public bool IsMotionActive(int rLayerIndex, string rName)
        {
            if (rLayerIndex >= MotionLayers.Count) { return false; }
            if (MotionLayers[rLayerIndex].ActiveMotion == null) { return false; }

            return (MotionLayers[rLayerIndex].ActiveMotion.Name == rName);
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public T GetMotion<T>() where T : MotionControllerMotion
        {
            Type lType = typeof(T);
            MotionControllerMotion lMotion = default(T);

            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    if (MotionLayers[i].Motions[j].GetType() == lType)
                    {
                        lMotion = MotionLayers[i].Motions[j];
                        if (lMotion.IsEnabled) { return (T)lMotion; }
                    }
                }
            }

            return (T)lMotion;
        }
        
        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rLayerIndex">Layer to look through</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public T GetMotion<T>(int rLayerIndex) where T : MotionControllerMotion
        {
            if (rLayerIndex >= MotionLayers.Count) { return null; }

            Type lType = typeof(T);
            MotionControllerMotion lMotion = default(T);

            for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
            {
                if (MotionLayers[rLayerIndex].Motions[i].GetType() == lType)
                {
                    lMotion = MotionLayers[rLayerIndex].Motions[i];
                    if (lMotion.IsEnabled) { return (T)lMotion; }
                }
            }

            return (T)lMotion;
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rType">Type of controller motion to look for</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public MotionControllerMotion GetMotion(Type rType)
        {
            MotionControllerMotion lMotion = null;

            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    if (MotionLayers[i].Motions[j].GetType() == rType)
                    {
                        lMotion = MotionLayers[i].Motions[j];
                        if (lMotion.IsEnabled) { return lMotion; }
                    }
                }
            }

            return lMotion;
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rLayerIndex">Layer to look through</param>
        /// <param name="rType">Type of controller motion to look for</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public MotionControllerMotion GetMotion(int rLayerIndex, Type rType)
        {
            if (rLayerIndex >= MotionLayers.Count) { return null; }

            MotionControllerMotion lMotion = null;

            for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
            {
                if (MotionLayers[rLayerIndex].Motions[i].GetType() == rType)
                {
                    lMotion = MotionLayers[rLayerIndex].Motions[i];
                    if (lMotion.IsEnabled) { return lMotion; }
                }
            }

            return lMotion;
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rName">Name of controller motion to look for</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public MotionControllerMotion GetMotion(String rName)
        {
            if (rName.Length == 0) { return null; }

            MotionControllerMotion lMotion = null;

            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    if (MotionLayers[i].Motions[j].Name == rName)
                    {
                        lMotion = MotionLayers[i].Motions[j];
                        if (lMotion.IsEnabled) { return lMotion; }
                    }
                }
            }

            // Search by type name
            if (lMotion == null)
            {
                for (int i = 0; i < MotionLayers.Count; i++)
                {
                    for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                    {
                        if (MotionLayers[i].Motions[j].GetType().Name == rName)
                        {
                            lMotion = MotionLayers[i].Motions[j];
                            if (lMotion.IsEnabled) { return lMotion; }
                        }
                    }
                }
            }

            return lMotion;
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rName">Name of controller motion to look for</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public MotionControllerMotion GetMotion(int rLayerIndex, String rName)
        {
            MotionControllerMotion lMotion = null;

            for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
            {
                if (MotionLayers[rLayerIndex].Motions[i].Name == rName)
                {
                    lMotion = MotionLayers[rLayerIndex].Motions[i];
                    if (lMotion.IsEnabled) { return lMotion; }
                }
            }

            if (lMotion == null)
            {
                for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
                {
                    if (MotionLayers[rLayerIndex].Motions[i].GetType().Name == rName)
                    {
                        lMotion = MotionLayers[rLayerIndex].Motions[i];
                        if (lMotion.IsEnabled) { return lMotion; }
                    }
                }
            }

            return lMotion;
        }

        /// <summary>
        /// Return the first motion in a layer that matches the specific motion
        /// type.
        /// </summary>
        /// <param name="rLayerIndex">Layer to look through</param>
        /// <param name="rType">Type of controller motion to look for</param>
        /// <returns>Returns reference to the first motion matching the type or null if not found</returns>
        public T GetMotionInterface<T>(int rLayerIndex = 0) where T : class
        {
            if (rLayerIndex < 0 || rLayerIndex >= MotionLayers.Count) { return default(T); }
            if (MotionLayers[rLayerIndex].Motions.Count == 0) { return default(T); }

            Type lInterfaceType = typeof(T);

            T lInterface = default(T);
            for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
            {
                Type lType = MotionLayers[rLayerIndex].Motions[i].GetType();

#if !UNITY_EDITOR && (NETFX_CORE || WINDOWS_UWP || UNITY_WP8 || UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0)
                if (lInterfaceType.GetTypeInfo().IsAssignableFrom(lType.GetTypeInfo()))
#else
                if (lInterfaceType.IsAssignableFrom(lType))
#endif
                {
                    lInterface = MotionLayers[rLayerIndex].Motions[i] as T;
                    if (MotionLayers[rLayerIndex].Motions[i].IsEnabled)
                    {
                        return lInterface;
                    }
                }
            }

            return lInterface;
        }

        /// <summary>
        /// Return the first active motion in a layer.
        /// </summary>
        /// <param name="rLayerIndex">Layer to look through</param>
        /// <returns>Returns reference to the motion or null if not found</returns>
        public MotionControllerMotion GetActiveMotion(int rLayerIndex)
        {
            if (rLayerIndex >= MotionLayers.Count) { return null; }
            return MotionLayers[rLayerIndex].ActiveMotion;
        }

        /// <summary>
        /// Activate the specified motion (on the next frame).
        /// </summary>
        /// <param name="rMotion">Motion to activate</param>
        public void ActivateMotion(MotionControllerMotion rMotion, int rParameter = 0)
        {
            if (rMotion != null)
            {
                rMotion.MotionLayer.QueueMotion(rMotion, rParameter);
            }
        }

        /// <summary>
        /// Finds the first motion matching the motion type and then attempts
        /// to activate it (on the next frame).
        /// </summary>
        /// <param name="rMotionType">Type of motion to activate</param>
        /// <returns>Returns the motion to be activated or null if a matching motion isn't found</returns>
        public MotionControllerMotion ActivateMotion(Type rMotion, int rParameter = 0)
        {
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    MotionControllerMotion lMotion = MotionLayers[i].Motions[j];
                    if (lMotion.GetType() == rMotion)
                    {
                        MotionLayers[i].QueueMotion(lMotion, rParameter);
                        return lMotion;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the first motion matching the motion type and then attempts
        /// to activate it (on the next frame).
        /// </summary>
        /// <param name="rMotionType">Type of motion to activate</param>
        /// <returns>Returns the motion to be activated or null if a matching motion isn't found</returns>
        public MotionControllerMotion ActivateMotion(string rMotionName, int rParameter = 0)
        {
            // Search by string name
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    MotionControllerMotion lMotion = MotionLayers[i].Motions[j];
                    if (lMotion._Name == rMotionName)
                    {
                        MotionLayers[i].QueueMotion(lMotion, rParameter);
                        return lMotion;
                    }
                }
            }

            // Search by type name
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    MotionControllerMotion lMotion = MotionLayers[i].Motions[j];
                    if (lMotion.GetType().Name == rMotionName)
                    {
                        MotionLayers[i].QueueMotion(lMotion, rParameter);
                        return lMotion;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Activates the specified motion and waits for it to deactivate
        /// </summary>
        /// <param name="rLayerIndex">Layer that we're activating the motion on</param>
        /// <param name="rType">Type of motion to activate</param>
        /// <returns></returns>
        public IEnumerator WaitAfterActivateMotion(int rLayerIndex, Type rType)
        {
            if (rLayerIndex < MotionLayers.Count)
            {
                for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
                {
                    if (MotionLayers[rLayerIndex].Motions[i].GetType() == rType)
                    {
                        MotionControllerMotion lMotion = MotionLayers[rLayerIndex].Motions[i];
                        if (!lMotion.IsActive && !lMotion.QueueActivation)
                        {
                            // We can't interrupt a transition. So, let it finish
                            while (MotionLayers[rLayerIndex]._AnimatorTransitionID != 0)
                            {
                                yield return null;
                            }

                            // Now we can activate and wait
                            ActivateMotion(lMotion);
                            while (lMotion.IsActive || lMotion.QueueActivation)
                            {
                                yield return null;
                            }

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates the specified motion and waits for it to deactivate
        /// </summary>
        /// <param name="rLayerIndex">Layer that we're activating the motion on</param>
        /// <param name="rType">Type of motion to activate</param>
        /// <returns></returns>
        public IEnumerator WaitAfterActivateMotion(int rLayerIndex, string rName, int rParameter = 0)
        {
            if (rLayerIndex < MotionLayers.Count)
            {
                for (int i = 0; i < MotionLayers[rLayerIndex].Motions.Count; i++)
                {
                    if (MotionLayers[rLayerIndex].Motions[i].Name == rName)
                    {
                        MotionControllerMotion lMotion = MotionLayers[rLayerIndex].Motions[i];
                        if (!lMotion.IsActive && !lMotion.QueueActivation)
                        {
                            // We can't interrupt a transition. So, let it finish
                            while (MotionLayers[rLayerIndex]._AnimatorTransitionID != 0)
                            {
                                yield return null;
                            }

                            // Now we can activate and wait
                            lMotion.Parameter = rParameter;
                            ActivateMotion(lMotion);
                            while (lMotion.IsActive || lMotion.QueueActivation)
                            {
                                yield return null;
                            }

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Coroutine that waits for the "current" motion to be deactivated. This
        /// function considers a motion to be active it is actually active or queued
        /// for activation. It is possible that the "current" motion can change if
        /// motions are queued for activation.
        /// </summary>
        /// <param name="rLayerIndex">Layer that we're waiting for</param>
        /// <param name="rIncludeQueued">Determines if queued motions are considered active</param>
        /// <returns></returns>
        public IEnumerator WaitForCurrentMotion(int rLayerIndex, bool rIncludeQueued)
        {
            if (rLayerIndex < MotionLayers.Count)
            {
                MotionControllerMotion lMotion = MotionLayers[rLayerIndex].ActiveMotion;
                while (lMotion != null && (lMotion.IsActive || (rIncludeQueued && lMotion.QueueActivation)))
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Rotates the actor towards the target rotation.
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rNormalizedSpeed"></param>
        public void SetTargetRotation(Quaternion rRotation)
        {
            _UseSimulatedInput = true;

            mTargetRotation = rRotation;
            mIsTargetRotationSet = true;
        }

        /// <summary>
        /// Moves the actor based on the velocity passed. This will
        /// determine rotation as well as position as the actor typically
        /// attempts to face forward the velocity.
        /// </summary>
        /// <param name="rVelocity">Velocity to move the actor</param>
        public void SetTargetVelocity(Vector3 rVelocity, float rNormalizedSpeed = 0f)
        {
            _UseSimulatedInput = true;

            mTargetVelocity = rVelocity;
            mTargetNormalizedSpeed = rNormalizedSpeed;
            mIsTargetMovementSet = true;

            if (mTargetVelocity.sqrMagnitude > 0f)
            {
                mTargetPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Moves the actor towards the target position using the normalized
        /// speed. This normalized speed will be used to temper the standard
        /// root-motion velocity
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rNormalizedSpeed"></param>
        public void SetTargetPosition(Vector3 rPosition, float rNormalizedSpeed)
        {
            _UseSimulatedInput = true;

            mTargetPosition = rPosition;
            mTargetNormalizedSpeed = rNormalizedSpeed;
            mIsTargetMovementSet = true;

            if (mTargetPosition.sqrMagnitude > 0f) 
            { 
                mTargetVelocity = Vector3.zero; 
            }
        }

        /// <summary>
        /// Clears out the target movement values
        /// </summary>
        public void ClearTarget()
        {
            _UseSimulatedInput = false;

            mTargetVelocity = Vector3.zero;
            mTargetPosition = Vector3.zero;
            mIsTargetMovementSet = false;

            mTargetRotation = Quaternion.identity;
            mIsTargetRotationSet = false;

            mTargetNormalizedSpeed = 0f;
        }

        /// <summary>
        /// Clears just the target position
        /// </summary>
        public void ClearTargetPosition()
        {
            mTargetVelocity = Vector3.zero;
            mTargetPosition = Vector3.zero;
            mIsTargetMovementSet = false;

            mTargetNormalizedSpeed = 0f;
        }

        /// <summary>
        /// Clears the target rotation
        /// </summary>
        public void ClearTargetRotation()
        {
            mTargetRotation = Quaternion.identity;
            mIsTargetRotationSet = false;
        }

        /// <summary>
        /// This function is used to convert the game control stick value to
        /// speed and direction values for the player.
        /// </summary>
        private void ProcessUserInput()
        {
            //if (_CameraTransform == null) { return; }

            // Grab the movement, but create a bit of a dead zone
            float lHInput = _InputSource.MovementX;
            float lVInput = _InputSource.MovementY;
            float lMagnitude = Mathf.Sqrt((lHInput * lHInput) + (lVInput * lVInput));

            // Add the value to our averages so we track trends. 
            State.InputMagnitudeTrend.Value = lMagnitude;

            // Get out early if we can simply this
            if (lVInput == 0f && lHInput == 0f)
            {
                State.InputX = 0f;
                State.InputY = 0f;
                State.InputForward = Vector3.zero;
                State.InputFromAvatarAngle = 0f;
                State.InputFromCameraAngle = 0f;

                _InputSource.InputFromCameraAngle = float.NaN;
                _InputSource.InputFromAvatarAngle = float.NaN;

                return;
            }

            // Set the forward direction of the input
            State.InputX = lHInput;
            State.InputY = lVInput;
            State.InputForward = new Vector3(lHInput, 0f, lVInput);

            // Determine angles based on what components exist
            if (_CameraTransform == null)
            {
                // Without a camera, we hope there's an input source providing the info
                if (_InputSource == null)
                {
                    State.InputFromCameraAngle = 0f;
                    State.InputFromAvatarAngle = 0f;
                }
                else
                {
                    State.InputFromCameraAngle = _InputSource.InputFromCameraAngle;
                    State.InputFromAvatarAngle = _InputSource.InputFromAvatarAngle;
                }
            }
            else
            {
                // We do the inverse tilt so we calculate the rotation in "natural up" space vs. "actor up" space. 
                Quaternion lInvTilt = QuaternionExt.FromToRotation(_Transform.up, Vector3.up);

                // Forward direction of the actor in "natural up"
                Vector3 lControllerForward = lInvTilt * _Transform.forward;
                //lControllerForward.y = 0f;
                //lControllerForward.Normalize();

                // Camera forward in "natural up"
                Vector3 lCameraForward = lInvTilt * _CameraTransform.forward;

                // Create a quaternion that gets us from our world-forward to our camera direction.
                //Quaternion lToCamera = Quaternion.LookRotation(lCameraForward, lInvTilt * _Transform.up);

                // TRT 11/23/15: Removed redundancy (lInvTilt & _Transform.up) from above
                Quaternion lToCamera = Quaternion.LookRotation(lCameraForward, Vector3.up);

                // Convert to a horizontal forward
                //lCameraForward.y = 0f;
                //lCameraForward.Normalize();

                // Transform joystick from world space to camera space. Now the input is relative
                // to how the camera is facing.
                Vector3 lMoveDirection = lToCamera * State.InputForward;
                State.InputFromCameraAngle = NumberHelper.GetHorizontalAngle(lCameraForward, lMoveDirection);
                State.InputFromAvatarAngle = NumberHelper.GetHorizontalAngle(lControllerForward, lMoveDirection);

                // Keep this info in the camera as well. Note that this info isn't
                // reliable as objects looking for it's set it will have old data
                _InputSource.InputFromCameraAngle = (State.InputMagnitudeTrend.Value == 0f ? float.NaN : State.InputFromCameraAngle);
                _InputSource.InputFromAvatarAngle = (State.InputMagnitudeTrend.Value == 0f ? float.NaN : State.InputFromAvatarAngle);
            }
        }

        /// <summary>
        /// Simulate user input by looking at the movement that has occured. This is used
        /// when the actor controller is simply following the transform.
        /// </summary>
        private void ProcessMovementInput()
        {
            Vector3 lVelocity = _ActorController.State.Velocity;

            // Get out early if there's nothing to do
            if (lVelocity.sqrMagnitude == 0f && mTargetPosition.sqrMagnitude == 0f && mTargetRotation == Quaternion.identity)
            {
                State.InputX = 0;
                State.InputY = 0;
                State.InputForward = Vector3.zero;
                State.InputFromCameraAngle = 0f;
                State.InputFromAvatarAngle = 0f;
                State.InputMagnitudeTrend.Value = 0f;

                return;
            }

            float lRotation = 0f;
            Vector3 lMovement = Vector3.zero;

            // We could be moving based on velocity
            if (lVelocity.sqrMagnitude > 0f)
            {
                lMovement = lVelocity;

                if (mTargetNormalizedSpeed == 0f && _MaxSpeed > 0f)
                {
                    mTargetNormalizedSpeed = Mathf.Clamp01(lMovement.magnitude / _MaxSpeed);
                    if (mTargetNormalizedSpeed < 0.5f) { mTargetNormalizedSpeed = 0.5f; }
                }
            }
            // Or we could be moving based on a position
            else
            {
                NumberHelper.GetHorizontalDifference(_Transform.position, mTargetPosition, ref lMovement);
            }

            // Get the input relative to our forward. If we're forcing the forward, we'll use that
            // as the base value instead of our actual forward value
            if (mIsTargetRotationSet)
            {
                lRotation = NumberHelper.GetHorizontalAngle(mTargetRotation.Forward(), lMovement.normalized, _Transform.up);
            }
            else
            {
                lRotation = NumberHelper.GetHorizontalAngle(_Transform.forward, lMovement.normalized);
            }

            // Determine the simulated input
            float lHInput = 0f;
            float lVInput = 0f;

            // Simulate the input
            if (lMovement.magnitude < 0.001f)
            {
                lHInput = 0f;
                lVInput = 0f;
            }
            else
            {
                lHInput = 0f;
                lVInput = mTargetNormalizedSpeed;
            }

            // It's possible that our rotation will have us one way and the target is another. This
            // is how we can strafe.
            Quaternion lDeltaRotation = Quaternion.AngleAxis(lRotation, _Transform.up);
            Vector3 lInputForward = lDeltaRotation.Forward();
            lHInput = lInputForward.x * mTargetNormalizedSpeed;
            lVInput = lInputForward.z * mTargetNormalizedSpeed;

            // Set the forward direction of the input, making it relative to the forward direction of the actor
            State.InputForward = new Vector3(lHInput, 0f, lVInput);
            State.InputX = State.InputForward.x;
            State.InputY = State.InputForward.z;

            // Determine the relative speed
            State.InputMagnitudeTrend.Value = Mathf.Sqrt((lHInput * lHInput) + (lVInput * lVInput));

            // Direction of the avatar
            Vector3 lControllerForward = transform.forward;
            lControllerForward.y = 0f;
            lControllerForward.Normalize();

            // Direction of the camera
            if (_CameraTransform == null)
            {
                State.InputFromCameraAngle = lRotation;
            }
            else
            {
                Vector3 lCameraForward = _CameraTransform.forward;
                lCameraForward.y = 0f;
                lCameraForward.Normalize();

                // Create a quaternion that gets us from our world-forward to our camera direction.
                // FromToRotation creates a quaternion using the shortest method which can sometimes
                // flip the angle. LookRotation will attempt to keep the "up" direction "up".
                Quaternion rToCamera = Quaternion.LookRotation(lCameraForward);

                // Transform joystick from world space to camera space. Now the input is relative
                // to how the camera is facing.
                Vector3 lMoveDirection = rToCamera * State.InputForward;
                State.InputFromCameraAngle = NumberHelper.GetHorizontalAngle(lCameraForward, lMoveDirection);
            }

            State.InputFromAvatarAngle = lRotation;
        }

        /// <summary>
        /// Gen a target position and rotation, this function converts the data into
        /// input values that will drive the controller.
        /// </summary>
        private void ProcessSimulatedInput()
        {
            // Get out early if there's nothing to do
            if (mTargetVelocity.sqrMagnitude == 0f && mTargetPosition.sqrMagnitude == 0f && mTargetRotation == Quaternion.identity) 
            {
                State.InputX = 0;
                State.InputY = 0;
                State.InputForward = Vector3.zero;
                State.InputFromCameraAngle = 0f;
                State.InputFromAvatarAngle = 0f;
                State.InputMagnitudeTrend.Value = 0f;

                return; 
            }

            float lRotation = 0f;
            Vector3 lMovement = Vector3.zero;

            // Check if we're moving towards a target
            if (mIsTargetMovementSet)
            {
                // We could be moving based on velocity
                if (mTargetVelocity.sqrMagnitude > 0f)
                {
                    lMovement = mTargetVelocity;

                    if (mTargetNormalizedSpeed == 0f && _MaxSpeed > 0f)
                    {
                        mTargetNormalizedSpeed = Mathf.Clamp01(lMovement.magnitude / _MaxSpeed);
                    }
                }
                // Or we could be moving based on a position
                else
                {
                    NumberHelper.GetHorizontalDifference(_Transform.position, mTargetPosition, ref lMovement);
                }

                // Get the input relative to our forward. If we're forcing the forward, we'll use that
                // as the base value instead of our actual forward value
                if (mIsTargetRotationSet)
                {
                    lRotation = NumberHelper.GetHorizontalAngle(mTargetRotation.Forward(), lMovement.normalized, _Transform.up);
                }
                else
                {
                    lRotation = NumberHelper.GetHorizontalAngle(_Transform.forward, lMovement.normalized);
                }
            }

            // Determine the simulated input
            float lHInput = 0f;
            float lVInput = 0f;

            // Simulate the input
            if (lMovement.magnitude < 0.001f)
            {
                lHInput = 0f;
                lVInput = 0f;
            }
            else
            {
                lHInput = 0f;
                lVInput = mTargetNormalizedSpeed;
            }

            // It's possible that our rotation will have us one way and the target is another. This
            // is how we can strafe.
            if (mIsTargetMovementSet && mIsTargetRotationSet && mTargetPosition.sqrMagnitude > 0f)
            {
                Quaternion lDeltaRotation = Quaternion.AngleAxis(lRotation, _Transform.up);
                Vector3 lInputForward = lDeltaRotation.Forward();
                lHInput = lInputForward.x * mTargetNormalizedSpeed;
                lVInput = lInputForward.z * mTargetNormalizedSpeed;
            }

            // Set the forward direction of the input, making it relative to the forward direction of the actor
            State.InputForward = new Vector3(lHInput, 0f, lVInput);
            //State.InputForward = Quaternion.FromToRotation(transform.forward, lMovement.normalized) * State.InputForward;

            State.InputX = State.InputForward.x;
            State.InputY = State.InputForward.z;

            // Determine the relative speed
            State.InputMagnitudeTrend.Value = Mathf.Sqrt((lHInput * lHInput) + (lVInput * lVInput));

            // Direction of the avatar
            Vector3 lControllerForward = transform.forward;
            lControllerForward.y = 0f;
            lControllerForward.Normalize();

            // Direction of the camera
            if (_CameraTransform == null)
            {
                State.InputFromCameraAngle = lRotation;
            }
            else
            {
                Vector3 lCameraForward = _CameraTransform.forward;
                lCameraForward.y = 0f;
                lCameraForward.Normalize();

                // Create a quaternion that gets us from our world-forward to our camera direction.
                // FromToRotation creates a quaternion using the shortest method which can sometimes
                // flip the angle. LookRotation will attempt to keep the "up" direction "up".
                //Quaternion rToCamera = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(lCameraForward));
                Quaternion rToCamera = Quaternion.LookRotation(lCameraForward);

                // Transform joystick from world space to camera space. Now the input is relative
                // to how the camera is facing.
                Vector3 lMoveDirection = rToCamera * State.InputForward;
                State.InputFromCameraAngle = NumberHelper.GetHorizontalAngle(lCameraForward, lMoveDirection);
            }

            State.InputFromAvatarAngle = lRotation;
        }

        /// <summary>
        /// Returns the motion phase the animator is currently in. We can
        /// use this to test where we're have from a motion perspective
        /// </summary>
        /// <param name="rLayerIndex"></param>
        /// <returns></returns>
        public int GetAnimatorMotionPhase(int rLayerIndex)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return 0; }
            return State.AnimatorStates[rLayerIndex].MotionPhase;
        }

        /// <summary>
        /// Sets the motion phase that will be sent to the animator
        /// </summary>
        /// <param name="rLayer">Layer to apply the phase to</param>
        /// <param name="rPhase">Phase value to set</param>
        public void SetAnimatorMotionPhase(int rLayerIndex, int rPhase)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return; }
            State.AnimatorStates[rLayerIndex].MotionPhase = rPhase;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhase = false;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhaseReady = false;
            State.AnimatorStates[rLayerIndex].AutoClearActiveTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash;
        }

        /// <summary>
        /// Sets the motion phase that will be sent to the animator
        /// </summary>
        /// <param name="rLayer">Layer to apply the phase to</param>
        /// <param name="rPhase">Phase value to set</param>
        /// <param name="rParameter">Extra parameter to send to the animator</param>
        public void SetAnimatorMotionPhase(int rLayerIndex, int rPhase, int rParameter)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return; }
            State.AnimatorStates[rLayerIndex].MotionPhase = rPhase;
            State.AnimatorStates[rLayerIndex].MotionParameter = rParameter;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhase = false;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhaseReady = false;
            State.AnimatorStates[rLayerIndex].AutoClearActiveTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash;
        }

        /// <summary>
        /// Sets the motion phase that will be sent to the animator
        /// </summary>
        /// <param name="rLayer">Layer to apply the phase to</param>
        /// <param name="rPhase">Phase value to set</param>
        /// <param name="rAutoClear">Flag to have the system clear the phase once it changes</param>
        public void SetAnimatorMotionPhase(int rLayerIndex, int rPhase, bool rAutoClear)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return; }
            State.AnimatorStates[rLayerIndex].MotionPhase = rPhase;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhase = rAutoClear;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhaseReady = false;
            State.AnimatorStates[rLayerIndex].AutoClearActiveTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash;
        }

        /// <summary>
        /// Sets the motion phase that will be sent to the animator
        /// </summary>
        /// <param name="rLayer">Layer to apply the phase to</param>
        /// <param name="rPhase">Phase value to set</param>
        /// <param name="rParameter">Extra parameter to send to the animator</param>
        /// <param name="rAutoClear">Flag to have the system clear the phase once it changes</param>
        public void SetAnimatorMotionPhase(int rLayerIndex, int rPhase, int rParameter, bool rAutoClear)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return; }
            State.AnimatorStates[rLayerIndex].MotionPhase = rPhase;
            State.AnimatorStates[rLayerIndex].MotionParameter = rParameter;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhase = rAutoClear;
            State.AnimatorStates[rLayerIndex].AutoClearMotionPhaseReady = false;
            State.AnimatorStates[rLayerIndex].AutoClearActiveTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash;
        }

        /// <summary>
        /// Sets the motion parameter that will be sent to the animator
        /// </summary>
        /// <param name="rLayer">Layer to apply the phase to</param>
        /// <param name="rParameter">Parameter value to set</param>
        public void SetAnimatorMotionParameter(int rLayerIndex, int rParameter)
        {
            if (rLayerIndex >= State.AnimatorStates.Length) { return; }
            State.AnimatorStates[rLayerIndex].MotionParameter = rParameter;
        }

        /// <summary>
        /// Update the animator with data from the current state
        /// </summary>
        /// <param name="rState">MotionState containing the current data</param>
        private void SetAnimatorProperties(ref MotionState rState, bool rUseTrendData)
        {
            if (mAnimator == null) { return; }

#if OOTII_PROFILE
            //Log.FileWrite("Current: " + AnimatorHashToString(State.AnimatorStates[0].StateInfo, State.AnimatorStates[0].TransitionInfo));
            //Log.FileWrite("SetAnimatorProperties  UTD:" + rUseTrendData + " MUD:" + mMecanimUpdateDelay.ToString("f4"));
            //Log.FileWrite("SetAnimatorProperties  I.x:" + rState.InputX.ToString("f4") + " I.y:" + rState.InputY.ToString("f4") + " I.fwd:" + StringHelper.ToString(rState.InputForward));
            //Log.FileWrite("SetAnimatorProperties L0MP:" + rState.AnimatorStates[0].MotionPhase);
            //Log.FileWrite("SetAnimatorProperties L0Mp:" + rState.AnimatorStates[0].MotionParameter);
            //Log.FileWrite("SetAnimatorProperties   IM:" + rState.InputMagnitudeTrend.Value + (!rUseTrendData || mMecanimUpdateDelay <= 0f ? " used" : ""));
            //Log.FileWrite("SetAnimatorProperties  IMA:" + rState.InputMagnitudeTrend.Average);
            //Log.FileWrite("SetAnimatorProperties IFAA:" + rState.InputFromAvatarAngle);
            //Log.FileWrite("SetAnimatorProperties IFAC:" + rState.InputFromCameraAngle);
#endif

            // The the stance
            mAnimator.SetBool("IsGrounded", _ActorController.State.IsGrounded);
            mAnimator.SetInteger("Stance", _ActorController.State.Stance);

            // If we come from a blend tree, we need to force the input values so the
            // blend tree keeps it's current state. Do that until we're done transitioning in
            if (MotionLayers.Count > 0 && rState.AnimatorStates.Length > 0 && ForcedInput.sqrMagnitude > 0f)
            {
                mAnimator.SetFloat("InputX", ForcedInput.x);
                mAnimator.SetFloat("InputY", ForcedInput.y);
                mAnimator.SetFloat("InputMagnitude", Mathf.Sqrt((ForcedInput.x * ForcedInput.x) + (ForcedInput.y * ForcedInput.y)));

                // Once there is no transition left, clear the forced input
                if (rState.AnimatorStates[0].MotionPhase == 0 && MotionLayers[0]._AnimatorTransitionID == 0)
                {
                    ForcedInput = Vector2.zero;
                }
            }
            // Otherwise, use the true input
            else
            {
                // The raw input from the UI
                mAnimator.SetFloat("InputX", rState.InputX);
                mAnimator.SetFloat("InputY", rState.InputY);

                // The relative speed magnitude of the character (0 to 1)
                // Delay a bit before we update the speed if we're accelerating
                // or decelerating.
                mMecanimUpdateDelay -= Time.deltaTime;
                if (!rUseTrendData || mMecanimUpdateDelay <= 0f)
                {
                    mAnimator.SetFloat("InputMagnitude", rState.InputMagnitudeTrend.Value); //, 0.05f, Time.deltaTime);
                }
            }

            // The magnituded averaged out over the last 10 frames
            mAnimator.SetFloat("InputMagnitudeAvg", rState.InputMagnitudeTrend.Average);

            // Direction of the input relative to the avatar's forward (-180 to 180)
            mAnimator.SetFloat("InputAngleFromAvatar", rState.InputFromAvatarAngle); //, 0.15f, Time.deltaTime);

            // Direction of the input relative to the camera's forward (-180 to 180)
            mAnimator.SetFloat("InputAngleFromCamera", rState.InputFromCameraAngle); //, 0.15f, Time.deltaTime); //, 0.05f, Time.deltaTime);

            // Motion phase per layer. Layer index is identified as "L0", "L1", etc.
            for (int i = 0; i < rState.AnimatorStates.Length; i++)
            {
                AnimatorLayerState lState = rState.AnimatorStates[i];
                mAnimator.SetInteger(MOTION_PHASE_NAMES[i], lState.MotionPhase);
                mAnimator.SetInteger(MOTION_PARAMETER_NAMES[i], lState.MotionParameter);

#if USE_MOTION_STATE_TIME
                mAnimator.SetFloat(MOTION_STATE_TIME[i], lState.StateInfo.normalizedTime % 1f);
#endif

                // If we have auto-generated code, we'll use the "IsInMotion" to see if we can clear the phase
                if (i < MotionLayers.Count && MotionLayers[i].ActiveMotion != null && !MotionLayers[i].ActiveMotion.HasAutoGeneratedCode)
                {
                    // With Unity 5, we keep re-entering. So we need to clear out the motion
                    // phase as quickly as possible
                    if (rState.AnimatorStates[i].AutoClearMotionPhase &&
                        rState.AnimatorStates[i].AutoClearMotionPhaseReady &&
                        rState.AnimatorStates[i].MotionPhase != 0)
                    {
                        rState.AnimatorStates[i].MotionPhase = 0;
                        rState.AnimatorStates[i].AutoClearActiveTransitionID = 0;

                        //State.AnimatorStates[i].AutoClearMotionPhase = false;
                        //State.AnimatorStates[i].AutoClearMotionPhaseReady = false;
                    }
                }
            }
        }

        /// <summary>
        /// Called to apply root motion manually. The existance of this
        /// stops the application of any existing root motion since we're
        /// essencially overriding the function. 
        /// 
        /// This function is called after Update() and after the state machines 
        /// and the animations have been evaluated. However, before LateUpdate().
        /// </summary>
        private void OnAnimatorMove()
        {
            if (Time.deltaTime == 0f) { return; }

            //Log.FileWrite("OnAnimatorMove");

            // Clear any root motion values
            if (mAnimator == null)
            {
                mRootMotionMovement = Vector3.zero;
                mRootMotionRotation = Quaternion.identity;
            }
            // Store the root motion as relative to the forward direction.
            else
            {
                // Convert the movement to relative the current rotation
                mRootMotionMovement = Quaternion.Inverse(_Transform.rotation) * (mAnimator.deltaPosition);

                // We don't want delta time spikes to cause our character to move erratically. So, instead we
                // translate the movement using our smoothed delta time.

                // TRT 12/13/15: Protecting in order to avoid any sliding that occurs when the frame rate is super sporadic
                if (_IsTimeSmoothingEnabled)
                {
                    mRootMotionMovement = (mRootMotionMovement / Time.deltaTime) * TimeManager.SmoothedDeltaTime;
                }

                // Store the rotation as a velocity per second.
                mRootMotionRotation = mAnimator.deltaRotation;
            }
        }

        /// <summary>
        /// Callback for setting up animation IK(inverse kinematics).
        /// </summary>
        /// <param name="layerIndex"></param>
        private void OnAnimatorIK(int rLayerIndex)
        {
            // Find the Motion Layer tied to the Animator Layer
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                MotionControllerLayer lMotionLayer = MotionLayers[i];
                if (lMotionLayer.AnimatorLayerIndex == rLayerIndex)
                {
                    // If we have an active motion, call to it
                    MotionControllerMotion lActiveMotion = lMotionLayer.ActiveMotion;
                    if (lActiveMotion != null)
                    {
                        lActiveMotion.OnAnimatorIK(mAnimator, rLayerIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the animator's state has changed
        /// </summary>
        private void OnAnimatorStateChange(int rAnimatorLayer)
        {
            // Find the Motion Layers tied to the Animator Layer
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                if (MotionLayers[i].AnimatorLayerIndex == rAnimatorLayer)
                {
                    MotionLayers[i].OnAnimatorStateChange(rAnimatorLayer, PrevState.AnimatorStates[rAnimatorLayer].StateInfo.fullPathHash, State.AnimatorStates[rAnimatorLayer].StateInfo.fullPathHash);
                }
            }
        }

        /// <summary>
        /// Raised by the animation when an event occurs
        /// </summary>
        private void OnAnimationEvent(AnimationEvent rEvent)
        {
            int lStateHash = 0;
            if (rEvent != null && rEvent.isFiredByAnimator)
            {
                lStateHash = rEvent.animatorStateInfo.fullPathHash;
            }

            // Send the event to all the layers
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                if (lStateHash == 0 || MotionLayers[i]._AnimatorStateID == lStateHash)
                {
                    MotionLayers[i].OnAnimationEvent(rEvent);
                    if (lStateHash != 0) { break; }
                }
            }
        }

        /// <summary>
        /// Sends a message to all the motions that are enabled.
        /// </summary>
        /// <param name="rMessage">IMessage object containing information about the message</param>
        public void SendMessage(IMessage rMessage)
        {
            rMessage.IsSent = true;
            rMessage.IsHandled = false;
            OnMessageReceived(rMessage);
        }

        /// <summary>
        /// Processes an incoming message and sends it to the motions that are enabled.
        /// </summary>
        /// <param name="rMessage">IMessage object containing information about the message</param>
        private void OnMessageReceived(IMessage rMessage)
        {
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                MotionLayers[i].OnMessageReceived(rMessage);

                if (rMessage.IsHandled) { break; }
            }
        }

        /// <summary>
        /// Load the animator state and transition IDs
        /// </summary>
        private void LoadAnimatorData()
        {
            // Set the actual state names
            AddAnimatorName("Start");
            AddAnimatorName("Any State");

            // Allow the motion layers to set the names
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                MotionLayers[i].LoadAnimatorData();
            }
        }

        /// <summary>
        /// Initialize the id with the right has based on the name. Then store
        /// the data for easy recall.
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rID"></param>
        public int AddAnimatorName(string rName)
        {
            int lID = Animator.StringToHash(rName);
            if (!AnimatorStateNames.ContainsKey(lID)) { AnimatorStateNames.Add(lID, rName); }
            if (!AnimatorStateIDs.ContainsKey(rName)) { AnimatorStateIDs.Add(rName, lID); }

            return lID;
        }

        /// <summary>
        /// Tests if the current animator state matches the name passed in. If not
        /// found, it tests for a match with the transition
        /// </summary>
        /// <param name="rLayerIndex">Layer to test</param>
        /// <param name="rStateName">State name to test for</param>
        /// <returns></returns>
        public bool CompareAnimatorStateName(int rLayerIndex, string rStateName)
        {
            if (State.AnimatorStates[rLayerIndex].StateInfo.fullPathHash == AnimatorStateIDs[rStateName]) { return true; }
            if (State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash == AnimatorStateIDs[rStateName]) { return true; }

            return false;
        }

        /// <summary>
        /// Test if the current transition state matches the name passed in
        /// </summary>
        /// <param name="rLayerIndex">Layer to test</param>
        /// <param name="rTransitionName">Transition name to test for</param>
        /// <returns></returns>
        public bool CompareAnimatorTransitionName(int rLayerIndex, string rTransitionName)
        {
            return (State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash == AnimatorStateIDs[rTransitionName]);
        }
        
        /// <summary>
        /// Returns the friendly name of the state or transition that
        /// is currently being run by the first animator layer that is active. 
        /// </summary>
        /// <returns></returns>
        public string GetAnimatorStateName()
        {
            string lResult = "";

            for (int i = 0; i < Animator.layerCount; i++)
            {
                lResult = GetAnimatorStateName(i);
                if (lResult.Length > 0) { break; }
            }

            return lResult;
        }

        /// <summary>
        /// Returns the friendly name of the state or transition that
        /// is currently being run by the first animator layer that is active. 
        /// </summary>
        /// <returns></returns>
        public string GetAnimatorStateAndTransitionName()
        {
            int lStateID = State.AnimatorStates[0].StateInfo.fullPathHash;
            int lTransitionID = State.AnimatorStates[0].TransitionInfo.fullPathHash;
            return AnimatorHashToString(lStateID, lTransitionID);
        }

        /// <summary>
        /// Returns the friendly name of the state that
        /// is currently being run.
        /// </summary>
        /// <param name="rLayerIndex">Layer whose index we want the state for</param>
        /// <returns>Name of the state that the character is in</returns>
        public string GetAnimatorStateName(int rLayerIndex)
        {
            string lResult = "";
            int lStateID = State.AnimatorStates[rLayerIndex].StateInfo.fullPathHash;

            if (AnimatorStateNames.ContainsKey(lStateID))
            {
                lResult = AnimatorStateNames[lStateID];
            }

            return lResult;
        }

        /// <summary>
        /// Returns the friendly name of the state or transition that
        /// is currently being run.
        /// </summary>
        /// <param name="rLayerIndex">Layer whose index we want the state for</param>
        /// <returns>Name of the state or transition that the character is in</returns>
        public string GetAnimatorStateTransitionName(int rLayerIndex)
        {
            string lResult = "";

            int lStateID = State.AnimatorStates[rLayerIndex].StateInfo.fullPathHash;
            int lTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.fullPathHash;

            if (lTransitionID != 0 && AnimatorStateNames.ContainsKey(lTransitionID))
            {
                lResult = AnimatorStateNames[lTransitionID];
            }
            else
            {
                lTransitionID = State.AnimatorStates[rLayerIndex].TransitionInfo.nameHash;
                if (lTransitionID != 0 && AnimatorStateNames.ContainsKey(lTransitionID))
                {
                    lResult = AnimatorStateNames[lTransitionID];
                }
                else if (AnimatorStateNames.ContainsKey(lStateID))
                {
                    lResult = AnimatorStateNames[lStateID];
                }
            }

            return lResult;
        }

        /// <summary>
        /// Convert the animator hash ID to a readable string
        /// </summary>
        /// <param name="rStateID"></param>
        /// <param name="rTransitionID"></param>
        /// <returns></returns>
        public string AnimatorHashToString(int rStateID, int rTransitionID)
        {
            string lState = (AnimatorStateNames.ContainsKey(rStateID) ? AnimatorStateNames[rStateID] : rStateID.ToString());
            string lTransition = (AnimatorStateNames.ContainsKey(rTransitionID) ? AnimatorStateNames[rTransitionID] : rTransitionID.ToString());

            return String.Format("state:{0} trans:{1}", lState, lTransition);
        }

        /// <summary>
        /// Convert the animator hash ID to a readable string
        /// </summary>
        /// <param name="rStateID"></param>
        /// <returns></returns>
        public string StateHashToString(int rStateID)
        {
            string lState = (AnimatorStateNames.ContainsKey(rStateID) ? AnimatorStateNames[rStateID] : rStateID.ToString());
            return lState;
        }

        /// <summary>
        /// Convert the animator hash ID to a readable string
        /// </summary>
        /// <param name="rTransitionID"></param>
        /// <returns></returns>
        public string TransitionHashToString(int rTransitionID)
        {
            string lTransition = (AnimatorStateNames.ContainsKey(rTransitionID) ? AnimatorStateNames[rTransitionID] : rTransitionID.ToString());
            return lTransition;
        }

        /// <summary>
        /// Convert the animator hash ID to a readable string
        /// </summary>
        /// <param name="rStateID"></param>
        /// <param name="rTransitionID"></param>
        /// <returns></returns>
        public string AnimatorHashToString(AnimatorStateInfo rState, AnimatorTransitionInfo rTransition)
        {
            string lStateName = "0";
            string lTransitionName = "0";

            int lStateID = rState.fullPathHash;

            if (lStateID != 0)
            {
                lStateName = (AnimatorStateNames.ContainsKey(lStateID) ? AnimatorStateNames[lStateID] : lStateID.ToString());
            }
            else
            {
                lStateName = lStateID.ToString();
            }

            int lTransitionID = rTransition.fullPathHash;
            if (lTransitionID != 0)
            {
                if (AnimatorStateNames.ContainsKey(lTransitionID))
                {
                    lTransitionName = AnimatorStateNames[lTransitionID];
                }
                else
                {
                    lTransitionID = rTransition.nameHash;
                    if (AnimatorStateNames.ContainsKey(lTransitionID))
                    {
                        lTransitionName = AnimatorStateNames[lTransitionID];
                    }
                    else
                    {
                        lTransitionName = lTransitionID.ToString();
                    }
                }
            }

            return String.Format("state[{1}]:{0} trans[{3}]:{2}", lStateName, rState.normalizedTime.ToString("f3"), lTransitionName, (rTransition.normalizedTime - (int)rTransition.normalizedTime).ToString("f3"));
        }

        /// <summary>
        /// Find the first valid camera rig associated with the motion controller
        /// </summary>
        /// <param name="rCamera"></param>
        /// <returns></returns>
        public IBaseCameraRig ExtractCameraRig(Transform rCamera)
        {
            if (rCamera == null) { return null; }

            Transform lParent = rCamera;
            while (lParent != null)
            {
                IBaseCameraRig[] lRigs = InterfaceHelper.GetComponents<IBaseCameraRig>(lParent.gameObject);
                if (lRigs != null && lRigs.Length > 0)
                {
                    for (int i = 0; i < lRigs.Length; i++)
                    {
                        MonoBehaviour lComponent = (MonoBehaviour)lRigs[i];
                        if (lComponent.enabled && lComponent.gameObject.activeSelf)
                        {
                            return lRigs[i];
                        }
                    }
                }

                lParent = lParent.parent;
            }

            return null;
        }

        /// <summary>
        /// Check if the velocity has us trending so that we can
        /// determine if we'll update the animator immediately
        /// </summary>
        private void DetermineTrendData()
        {
            if (State.InputMagnitudeTrend.Value == PrevState.InputMagnitudeTrend.Value)
            {
                if (mSpeedTrendDirection != EnumSpeedTrend.CONSTANT)
                {
                    mSpeedTrendDirection = EnumSpeedTrend.CONSTANT;
                }
            }
            else if (State.InputMagnitudeTrend.Value < PrevState.InputMagnitudeTrend.Value)
            {
                if (mSpeedTrendDirection != EnumSpeedTrend.DECELERATE)
                {
                    mSpeedTrendDirection = EnumSpeedTrend.DECELERATE;
                    if (mMecanimUpdateDelay <= 0f) { mMecanimUpdateDelay = 0.2f; }
                }

                // Acceleration needs to stay consistant for mecanim
                //mNewState.Acceleration = mNewState.InputMagnitude - mSpeedTrendStart;
            }
            else if (State.InputMagnitudeTrend.Value > PrevState.InputMagnitudeTrend.Value)
            {
                if (mSpeedTrendDirection != EnumSpeedTrend.ACCELERATE)
                {
                    mSpeedTrendDirection = EnumSpeedTrend.ACCELERATE;
                    if (mMecanimUpdateDelay <= 0f) { mMecanimUpdateDelay = 0.2f; }
                }

                // Acceleration needs to stay consistant for mecanim
                //mNewState.Acceleration = mNewState.InputMagnitude - mSpeedTrendStart;
            }
        }

        /// <summary>
        /// Allow the controller to render debug info
        /// </summary>
        private void OnDrawGizmos()
        {
            // Find the Motion Layers tied to the Animator Layer
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                MotionLayers[i].OnDrawGizmos();
            }
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        // Values to help us manage the editor
        public int EditorTabIndex = 0;

        /// <summary>
        /// Allows us to re-open the last selected layer
        /// </summary>
        public int EditorLayerIndex = 0;

        /// <summary>
        /// Allows us to re-open the last selected motion
        /// </summary>
        public int EditorMotionIndex = 0;

        /// <summary>
        /// Allows us to re-open the last selected pack
        /// </summary>
        public int EditorPackIndex = 0;

        /// <summary>
        /// Reset to default values. Reset is called when the user hits the Reset button in the Inspector's 
        /// context menu or when adding the component the first time. This function is only called in editor mode.
        /// </summary>
        public void Reset()
        {
            Debug.Log("MC Reset()");
            for (int i = 0; i < MotionLayers.Count; i++)
            {
                for (int j = 0; j < MotionLayers[i].Motions.Count; j++)
                {
                    MotionLayers[i].Motions[j].Reset();
                }
            }
        }

        /// <summary>
        /// Get or create the specified motion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateMotion<T>(int rLayerIndex, string rName = "", bool rIsEnabled = true) where T : MotionControllerMotion
        {
            while (MotionLayers.Count <= rLayerIndex)
            {
                MotionLayers.Add(new MotionControllerLayer());
            }

            int lMotionIndex = -1;

            T lMotion = GetMotion(typeof(T)) as T;
            if (lMotion == null)
            {
                lMotion = Activator.CreateInstance(typeof(T)) as T;
                MotionLayers[rLayerIndex].AddMotion(lMotion);
                MotionLayers[rLayerIndex].MotionDefinitions.Add(lMotion.SerializeMotion());

                lMotionIndex = MotionLayers[rLayerIndex].Motions.Count - 1;

                if (EditorApplication.isPlaying)
                {
                    lMotion.Awake();
                    lMotion.Initialize();
                    lMotion.LoadAnimatorData();
                    lMotion.CreateInputManagerSettings();
                }
            }
            else
            {
                lMotionIndex = MotionLayers[rLayerIndex].Motions.IndexOf(lMotion);
            }

            if (lMotionIndex >= 0)
            {
                if (rName.Length > 0) { lMotion.Name = rName; }
                lMotion.IsEnabled = rIsEnabled;
                MotionLayers[rLayerIndex].MotionDefinitions[lMotionIndex] = lMotion.SerializeMotion();
            }

            return lMotion;
        }

        /// <summary>
        /// Serialize the motion and update it's defintion
        /// </summary>
        /// <param name="rMotion"></param>
        public void SerializeMotion(MotionControllerMotion rMotion)
        {
            int lIndex = rMotion.MotionLayer.Motions.IndexOf(rMotion);
            if (lIndex >= 0)
            {
                rMotion.MotionLayer.MotionDefinitions[lIndex] = rMotion.SerializeMotion();
            }
        }

#endif

    }
}

