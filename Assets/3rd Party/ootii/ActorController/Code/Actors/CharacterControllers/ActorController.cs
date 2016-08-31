//#define OOTII_PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Physics;
using com.ootii.Utilities;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors
{
    [Serializable]
    [AddComponentMenu("ootii/Actor Controller")]
    public class ActorController : BaseSystemController, ICharacterController
    {
        /// <summary>
        /// Provides a value for "numerical error"
        /// </summary>
        public const float EPSILON = 0.0001f;
        public const float EPSILON_SQR = 0.00000001f;

        /// <summary>
        /// Fixed math we don't want to recalculate over and over
        /// </summary>
        public const float ONE_OVER_COS45 = 1.41421356237f;

        /// <summary>
        /// Extra spacing between the collision objects
        /// </summary>
        public const float COLLISION_BUFFER = 0.001f;

        /// <summary>
        /// Maximum number of segments we'll process when predicting movement.
        /// </summary>
        public const float MAX_SEGMENTS = 20;

        /// <summary>
        /// Max angle we can be grounded on when using the sphere cast
        /// </summary>
        public const float MAX_GROUNDING_ANGLE = 70f;

        /// <summary>
        /// Enabled/disables the AC, but keep the camera processing
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }

            set
            {
                _IsEnabled = value;

                if (_IsEnabled)
                {
                    // Initialize state info
                    RaycastHit lGroundHitInfo;
                    ProcessGrounding(_Transform.position, Vector3.zero, _Transform.up, _Transform.up, _BaseRadius, out lGroundHitInfo);

                    mState.PrevGround = mState.Ground;
                    mState.Position = _Transform.position;
                    mState.Rotation = _Transform.rotation;

                    mPrevState.Ground = mState.Ground;
                    mPrevState.Position = _Transform.position;
                    mPrevState.Rotation = _Transform.rotation;
                }
            }
        }

        /// <summary>
        /// Determines if the base system controller actually runs the "update" cycle
        /// in Unity's Update() or LateUpdate() functions. It's up to the derived class
        /// to respect this flag.
        /// </summary>
        public bool _ProcessInLateUpdate = true;
        public bool ProcessInLateUpdate
        {
            get { return _ProcessInLateUpdate; }
            set { _ProcessInLateUpdate = value; }
        }

        /// <summary>
        /// Defines the number of states that we'll keep active
        /// </summary>
        public int _StateCount = ActorState.STATE_COUNT;
        public int StateCount
        {
            get { return _StateCount; }

            set
            {
                if (value <= 0) { return; }
                if (value == _StateCount) { return; }

                _StateCount = value;
                if (mStateIndex >= _StateCount) { mStateIndex = _StateCount - 1; }

                if (mStates != null)
                {
                    Array.Resize<ActorState>(ref mStates, _StateCount);
                }
            }
        }

        /// <summary>
        /// Determines if we'll "borrow" physics based force information in the
        /// Update() functions that skip FixedUpdate();
        /// </summary>
        public bool _ExtrapolatePhysics = false;
        public bool ExtrapolatePhysics
        {
            get { return _ExtrapolatePhysics; }
            set { _ExtrapolatePhysics = value; }
        }

        /// <summary>
        /// Determines if we'll use gravity
        /// </summary>
        public bool _IsGravityEnabled = true;
        public bool IsGravityEnabled
        {
            get { return _IsGravityEnabled; }
            set { _IsGravityEnabled = value; }
        }

        /// <summary>
        /// Determines if the gravity is relative to the actor
        /// </summary>
        public bool _IsGravityRelative = false;
        public bool IsGravityRelative
        {
            get { return _IsGravityRelative; }
            set { _IsGravityRelative = value; }
        }

        /// <summary>
        /// If we're using gravity, determines the actual value. A magnitude
        /// of 0 means we use Unity's gravity value.
        /// </summary>
        public Vector3 _Gravity = new Vector3(0f, 0f, 0f);
        public Vector3 Gravity
        {
            get { return _Gravity; }
            set { _Gravity = value; }
        }

        /// <summary>
        /// Determines if we apply gravity in fixed update or late update
        /// </summary>
        public bool _ApplyGravityInFixedUpdate = true;
        public bool ApplyGravityInFixedUpdate
        {
            get { return _ApplyGravityInFixedUpdate; }
            set { _ApplyGravityInFixedUpdate = value; }
        }

        /// <summary>
        /// Skin width used to help force grounding
        /// </summary>
        public float _SkinWidth = 0.01f;
        public float SkinWidth
        {
            get { return _SkinWidth; }
            set { _SkinWidth = value; }
        }

        /// <summary>
        /// Mass of the actor
        /// </summary>
        public float _Mass = 2f;
        public float Mass
        {
            get { return _Mass; }
            set { _Mass = value; }
        }

        /// <summary>
        /// Height of the actor calculated using the body shapes (when the current value is 0).
        /// </summary>
        protected float mHeight = 0f;
        public float Height
        {
            get
            {
                if (mHeight <= 0f)
                {
                    for (int i = 0; i < BodyShapes.Count; i++)
                    {
                        float lHeight = 0f;

                        BodyShape lBodyShape = BodyShapes[i];

                        Transform lTransform = (lBodyShape._Transform != null ? lBodyShape._Transform : lBodyShape._Parent);
                        Vector3 lTop = lTransform.position + (lTransform.rotation * lBodyShape._Offset) + (lBodyShape._Parent.up * lBodyShape.Radius);
                        lHeight = Vector3.Distance(lTop, _Transform.position);

                        if (BodyShapes[i] is BodyCapsule)
                        {
                            BodyCapsule lCapsuleShape = BodyShapes[i] as BodyCapsule;

                            Transform lEndTransform = (lCapsuleShape._EndTransform != null ? lCapsuleShape._EndTransform : lCapsuleShape._Parent);
                            Vector3 lEndTop = lEndTransform.position + (lEndTransform.rotation * lCapsuleShape._EndOffset) + (lCapsuleShape._Parent.up * lCapsuleShape.Radius);
                            lHeight = Mathf.Max(Vector3.Distance(lEndTop, _Transform.position), lHeight);
                        }

                        mHeight = Mathf.Max(lHeight, mHeight);
                    }
                }

                return mHeight;
            }

            set { mHeight = value; }
        }

        /// <summary>
        /// Distance from the actor's origin that the grounding test will start
        /// </summary>
        public float _GroundingStartOffset = 1f;
        public float GroundingStartOffset
        {
            get { return _GroundingStartOffset; }
            set { _GroundingStartOffset = value; }
        }

        /// <summary>
        /// Distance from the actor's origin that the grounding test will end
        /// </summary>
        public float _GroundingDistance = 3f;
        public float GroundingDistance
        {
            get { return _GroundingDistance; }
            set { _GroundingDistance = value; }
        }

        /// <summary>
        /// Determines if we actually use grounding layers
        /// </summary>
        public bool _IsGroundingLayersEnabled = false;
        public bool IsGroundingLayersEnabled
        {
            get { return _IsGroundingLayersEnabled; }
            set { _IsGroundingLayersEnabled = value; }
        }

        /// <summary>
        /// Layer we'll use to collide (ground) against. The default
        /// value is the 'Default' layer (Layer 1)
        /// </summary>
        public int _GroundingLayers = 1;
        public int GroundingLayers
        {
            get { return _GroundingLayers; }
            set { _GroundingLayers = value; }
        }

        /// <summary>
        /// Radius of the "feet" used to test for ground collisions in the event
        /// that the single ray cast fails.
        /// </summary>
        public float _BaseRadius = 0.1f;
        public float BaseRadius
        {
            get { return _BaseRadius; }
            set { _BaseRadius = value; }
        }

        /// <summary>
        /// Determines if we'll automatically push the actor out of the ground when there is ground penetration
        /// </summary>
        public bool _FixGroundPenetration = true;
        public bool FixGroundPenetration
        {
            get { return _FixGroundPenetration; }
            set { _FixGroundPenetration = value; }
        }

        /// <summary>
        /// Determines if the actor is supposed to be grounded. If so, we'll 
        /// force the actor to the ground (if they are within a minimal range).
        /// </summary>
        public bool _ForceGrounding = true;
        public bool ForceGrounding
        {
            get { return _ForceGrounding; }
            set { _ForceGrounding = value; }
        }

        /// <summary>
        /// When forcing to the ground, the distance that we'll use as the max to clamp.
        /// </summary>
        public float _ForceGroundingDistance = 0.05f;
        public float ForceGroundingDistance
        {
            get { return _ForceGroundingDistance; }
            set { _ForceGroundingDistance = value; }
        }

        /// <summary>
        /// Determines if we'll process collisions
        /// </summary>
        public bool _IsCollisionEnabled = true;
        public bool IsCollsionEnabled
        {
            get { return _IsCollisionEnabled; }
            set { _IsCollisionEnabled = value; }
        }

        /// <summary>
        /// Determines if we'll test the current ground for collisions.
        /// </summary>
        public bool _StopOnRotationCollision = false;
        public bool StopOnRotationCollision
        {
            get { return _StopOnRotationCollision; }
            set { _StopOnRotationCollision = value; }
        }

        /// <summary>
        /// Allows moving objects to push the actor even if he's not moving
        /// </summary>
        public bool _AllowPushback = false;
        public bool AllowPushback
        {
            get { return _AllowPushback; }
            set { _AllowPushback = value; }
        }

        /// <summary>
        /// Layer we'll use to collide against. The default
        /// value is the 'Default' layer (Layer 1)
        /// </summary>
        public int _CollisionLayers = 1;
        public int CollisionLayers
        {
            get { return _CollisionLayers; }
            set { _CollisionLayers = value; }
        }

        /// <summary>
        /// Radius of the actor. This is used when determining the overlap of other objects
        /// </summary>
        public float _OverlapRadius = 0.9f;
        public float OverlapRadius
        {
            get { return _OverlapRadius; }
            set { _OverlapRadius = value; }
        }

        /// <summary>
        /// Center point where the overlap for collisions are tested
        /// </summary>
        public Vector3 _OverlapCenter = new Vector3(0f, 0.9f, 0f);
        public Vector3 OverlapCenter
        {
            get { return _OverlapCenter; }
            set { _OverlapCenter = value; }
        }

        /// <summary>
        /// Determines if we allow characters to slide
        /// </summary>
        public bool _IsSlidingEnabled = false;
        public bool IsSlidingEnabled
        {
            get { return _IsSlidingEnabled; }
            set { _IsSlidingEnabled = value; }
        }

        /// <summary>
        /// Slope at which the character starts sliding down. They
        /// can still go up it, but they are slowed down based on gravity.
        /// </summary>
        public float _MinSlopeAngle = 20f;
        public float MinSlopeAngle
        {
            get { return _MinSlopeAngle; }
            set { _MinSlopeAngle = value; }
        }

        /// <summary>
        /// When we're on a slope larger than the MinSlopeAngle, the percentage
        /// of gravity that is applied to our downward slide.
        /// </summary>
        public float _MinSlopeGravityCoefficient = 1f;
        public float MinSlopeGravityCoefficient
        {
            get { return _MinSlopeGravityCoefficient; }
            set { _MinSlopeGravityCoefficient = value; }
        }

        /// <summary>
        /// Max slope angle the character can go up
        /// </summary>
        public float _MaxSlopeAngle = 40f;
        public float MaxSlopeAngle
        {
            get { return _MaxSlopeAngle; }
            set { _MaxSlopeAngle = value; }
        }

        /// <summary>
        /// For slope testing, movement is broken up into small
        /// chunks and collisions are tested. This is the max size
        /// of those movement steps.
        /// </summary>
        public float _SlopeMovementStep = 0.01f;
        public float SlopeMovementStep
        {
            get { return _SlopeMovementStep; }
            set { _SlopeMovementStep = value; }
        }

        /// <summary>
        /// Determines if our actor orients to match the angle of the ground.
        /// </summary>
        public bool _OrientToGround = false;
        public bool OrientToGround
        {
            get { return _OrientToGround; }

            set
            {
                _OrientToGround = value;

                if (_OrientToGround)
                {
                    mOrientToGroundNormal = _Transform.up;
                    _Transform.rotation.DecomposeSwingTwist(Vector3.up, ref mTilt, ref mYaw);
                }
                else
                {
                    // If we're not on the natural ground, allow us to fall 
                    // and collide as we should
                    mState.IsTilting = true;

                    // If we're already on the natural ground, make sure we align
                    // ourselves correctly so there is no "bump".
                    if (_Transform.up == Vector3.up)
                    {
                        mYaw = _Transform.rotation;
                        mTilt = Quaternion.identity;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if we keep the last orientation while the avatar is in the air (ie jumping)
        /// </summary>
        public bool _KeepOrientationInAir = false;
        public bool KeepOrientationInAir
        {
            get { return _KeepOrientationInAir; }
            set { _KeepOrientationInAir = value; }
        }

        /// <summary>
        /// Maximum distance to stay oriented as the actor falls. 
        /// </summary>
        public float _OrientToGroundDistance = 2f;
        public float OrientToGroundDistance
        {
            get { return _OrientToGroundDistance; }
            set { _OrientToGroundDistance = value; }
        }

        /// <summary>
        /// Max time it will take the actor to rotate 180 degrees
        /// </summary>
        public float _OrientToGroundSpeed = 1f;
        public float OrientToGroundSpeed
        {
            get { return _OrientToGroundSpeed; }
            set { _OrientToGroundSpeed = value; }
        }

        /// <summary>
        /// Minimum angle needed before we start doing a slow rotation to the orientation.
        /// Otherwise, it's instant.
        /// </summary>
        public float _MinOrientToGroundAngleForSpeed = 5f;
        public float MinOrientToGroundAngleForSpeed
        {
            get { return _MinOrientToGroundAngleForSpeed; }
            set { _MinOrientToGroundAngleForSpeed = value; }
        }

        /// <summary>
        /// Max height the character can just walk up
        /// </summary>
        public float _MaxStepHeight = 0.3f;
        public float MaxStepHeight
        {
            get { return _MaxStepHeight; }
            set { _MaxStepHeight = value; }
        }

        /// <summary>
        /// Speed at which we smoothly move up steps
        /// </summary>
        public float _StepUpSpeed = 1.5f;
        public float StepUpSpeed
        {
            get { return _StepUpSpeed; }
            set { _StepUpSpeed = value; }
        }

        /// <summary>
        /// Maximum angle that we'll allow smooth stepping up.
        /// </summary>
        public float _MaxStepUpAngle = 10f;
        public float MaxStepUpAngle
        {
            get { return _MaxStepUpAngle; }
            set { _MaxStepUpAngle = value; }
        }

        /// <summary>
        /// Speed at which we smoothly move down steps
        /// </summary>
        public float _StepDownSpeed = 1.5f;
        public float StepDownSpeed
        {
            get { return _StepDownSpeed; }
            set { _StepDownSpeed = value; }
        }

        /// <summary>
        /// Determines if we prevent movement on the x axis
        /// </summary>
        public bool _FreezePositionX = false;
        public bool FreezePositionX
        {
            get { return _FreezePositionX; }
            set { _FreezePositionX = value; }
        }

        /// <summary>
        /// Determines if we prevent movement on the y axis
        /// </summary>
        public bool _FreezePositionY = false;
        public bool FreezePositionY
        {
            get { return _FreezePositionY; }
            set { _FreezePositionY = value; }
        }

        /// <summary>
        /// Determines if we prevent movement on the z axis
        /// </summary>
        public bool _FreezePositionZ = false;
        public bool FreezePositionZ
        {
            get { return _FreezePositionZ; }
            set { _FreezePositionZ = value; }
        }

        /// <summary>
        /// Determines if we prevent rotation on the x axis
        /// </summary>
        public bool _FreezeRotationX = false;
        public bool FreezeRotationX
        {
            get { return _FreezeRotationX; }
            set { _FreezeRotationX = value; }
        }

        /// <summary>
        /// Determines if we prevent rotation on the y axis
        /// </summary>
        public bool _FreezeRotationY = false;
        public bool FreezeRotationY
        {
            get { return _FreezeRotationY; }
            set { _FreezeRotationY = value; }
        }

        /// <summary>
        /// Determines if we prevent rotation on the z axis
        /// </summary>
        public bool _FreezeRotationZ = false;
        public bool FreezeRotationZ
        {
            get { return _FreezeRotationZ; }
            set { _FreezeRotationZ = value; }
        }

        /// <summary>
        /// By enabling this flag, most processing is disabled and the
        /// actor controller applies movement and rotations directly. Use this
        /// when an animation or other process will totally control movement.
        /// </summary>
        protected bool mOverrideProcessing = false;
        public bool OverrideProcessing
        {
            get { return mOverrideProcessing; }
            set { mOverrideProcessing = value; }
        }

        /// <summary>
        /// List of shapes that define the rough shape of the actor
        /// for collision detection. Since Unity doesn't serialize well,
        /// we don't use inheritance here.
        /// </summary>
        [NonSerialized]
        public List<BodyShape> BodyShapes = new List<BodyShape>();

        /// <summary>
        /// Determines if we're actually on the ground
        /// </summary>
        public bool IsGrounded
        {
            get { return mState.IsGrounded; }
        }

        /// <summary>
        /// Length of time the actor has been grounded
        /// </summary>
        protected float mGroundedDuration = 0f;
        public float GroundedDuration
        {
            get { return mGroundedDuration; }
        }

        /// <summary>
        /// Length of time the actor has been falling
        /// </summary>
        protected float mFallDuration = 0f;
        public float FallDuration
        {
            get { return mFallDuration; }
        }

        /// <summary>
        /// Previous position
        /// </summary>
        public Quaternion Rotation
        {
            get { return _Transform.rotation; }
        }

        /// <summary>
        /// Current yaw rotation of the character without any ground tilt
        /// </summary>
        protected Quaternion mYaw = Quaternion.identity;
        public Quaternion Yaw
        {
            get { return mYaw; }
            set { mYaw = value; }
        }

        /// <summary>
        /// Current tilt (pitch/roll) rotation of the character without any yaw
        /// </summary>
        protected Quaternion mTilt = Quaternion.identity;
        public Quaternion Tilt
        {
            get { return mTilt; }
            set { mTilt = value; }
        }

        /// <summary>
        /// Previous position
        /// </summary>
        public Vector3 Position
        {
            get { return _Transform.position; }
        }

        /// <summary>
        /// Current velocity
        /// </summary>
        public Vector3 Velocity
        {
            get { return mState.Velocity; }
        }

        /// <summary>
        /// Determines if we're previously grounded
        /// </summary>
        public bool PrevIsGrounded
        {
            get { return mPrevState.IsGrounded; }
        }

        /// <summary>
        /// Previous position
        /// </summary>
        public Vector3 PrevPosition
        {
            get { return mPrevState.Position; }
        }

        /// <summary>
        /// Previous velocity
        /// </summary>
        public Vector3 PrevVelocity
        {
            get { return mPrevState.Velocity; }
        }

        /// <summary>
        /// The current state of the controller including speed, direction, etc.
        /// </summary>
        protected ActorState mState = new ActorState();
        public ActorState State
        {
            get { return mState; }
            set { mState = value; }
        }

        /// <summary>
        /// The previous state of the controller including speed, direction, etc.
        /// </summary>
        protected ActorState mPrevState = new ActorState();
        public ActorState PrevState
        {
            get { return mPrevState; }
            set { mPrevState = value; }
        }

        /// <summary>
        /// Use this to store up gravitational and force velocity over time
        /// </summary>
        private Vector3 mAccumulatedVelocity = Vector3.zero;
        public Vector3 AccumulatedVelocity
        {
            get { return mAccumulatedVelocity; }
            set { mAccumulatedVelocity = value; }
        }

        /// <summary>
        /// Used to store the gravitational and force position change over time
        /// </summary>
        private Vector3 mAccumulatedMovement = Vector3.zero;

        /// <summary>
        /// Used to store the non-gravitational force that is applied. This isn't
        /// used for calculations, but for conditional logic.
        /// </summary>
        private Vector3 mAccumulatedForceVelocity = Vector3.zero;

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
        /// Allows for external processing prior to the actor controller doing it's 
        /// work this frame.
        /// </summary>
        protected ControllerLateUpdateDelegate mOnControllerPreLateUpdate = null;
        public ControllerLateUpdateDelegate OnControllerPreLateUpdate
        {
            get { return mOnControllerPreLateUpdate; }
            set { mOnControllerPreLateUpdate = value; }
        }

        /// <summary>
        /// Allows for external processing after the actor controller doing it's 
        /// work this frame.
        /// </summary>
        protected ControllerLateUpdateDelegate mOnControllerPostLateUpdate = null;
        public ControllerLateUpdateDelegate OnControllerPostLateUpdate
        {
            get { return mOnControllerPostLateUpdate; }
            set { mOnControllerPostLateUpdate = value; }
        }

        /// <summary>
        /// Callback that allows the caller to change the final position/rotation
        /// before it's set on the actual transform.
        /// </summary>
        protected ControllerMoveDelegate mOnPreControllerMove = null;
        public ControllerMoveDelegate OnPreControllerMove
        {
            get { return mOnPreControllerMove; }
            set { mOnPreControllerMove = value; }
        }

        /// <summary>
        /// Current state index that we are using
        /// </summary>
        protected int mStateIndex = 0;
        public int StateIndex
        {
            get { return mStateIndex; }
        }

        /// <summary>
        /// Tracks the states so we can reapply or use as needed
        /// </summary>
        protected ActorState[] mStates = null;
        public ActorState[] States
        {
            get { return mStates; }
        }

        /// <summary>
        /// Number of Update() functions that occured where there was no FixedUpdate()
        /// </summary>
        protected float mFixedUpdates = 0f;
        public float FixedUpdates
        {
            get { return mFixedUpdates; }
        }

        /// <summary>
        /// Keeps us from re-calculating the world-up over and over
        /// </summary>
        protected Vector3 mWorldUp = Vector3.up;

        /// <summary>
        /// Speed to orient to the desired direction
        /// </summary>
        protected float mOrientToSpeed = 0f;

        /// <summary>
        /// Normal we'll use to orient the actor to the ground.
        /// </summary>
        protected Vector3 mOrientToGroundNormal = Vector3.up;

        /// <summary>
        /// Target ground normal we're attempting to reach
        /// </summary>
        protected Vector3 mOrientToGroundNormalTarget = Vector3.up;

        /// <summary>
        /// Ground that we want to attach the actor to
        /// </summary>
        protected Transform mTargetGround = null;

        /// <summary>
        /// Sets the ground normal that we'll try to orient the actor to
        /// </summary>
        protected Vector3 mTargetGroundNormal = Vector3.zero;

        /// <summary>
        /// Forces an absolute rotation during the next frame update
        /// </summary>
        protected Quaternion mTargetRotation = new Quaternion(float.MaxValue, 0f, 0f, 0f);

        /// <summary>
        /// Determines the amount of rotation that should occurin the frame
        /// </summary>
        protected Quaternion mTargetRotate = Quaternion.identity;

        /// <summary>
        /// Determines the amount of rotation that should occurin the frame
        /// </summary>
        protected Quaternion mTargetTilt = Quaternion.identity;

        /// <summary>
        /// Determines the speed at which we'll keep rotating
        /// </summary>
        protected Vector3 mTargetRotationVelocity = Vector3.zero;

        /// <summary>
        /// Forces an absolute position during the next frame update
        /// </summary>
        protected Vector3 mTargetPosition = new Vector3(float.MaxValue, 0f, 0f);

        /// <summary>
        /// Determines the amount of movement that should occur in the frame
        /// </summary>
        protected Vector3 mTargetMove = Vector3.zero;

        /// <summary>
        /// Determines the speed at which we'll keep moving
        /// </summary>
        protected Vector3 mTargetVelocity = Vector3.zero;

        /// <summary>
        /// Movement borrowed from the next fixed update while extrapolationg
        /// </summary>
        protected Vector3 mBorrowedFixedMovement = Vector3.zero;

        /// <summary>
        /// Current step speed
        /// </summary>
        protected float mCurrentStepSpeed = 0f;

        /// <summary>
        /// Colliders that impact the actor
        /// </summary>
        protected List<BodyShapeHit> mBodyShapeHits = new List<BodyShapeHit>();

        /// <summary>
        /// Since Unity can't serialize polymorphic lists correctly (even with ScriptableObjects),
        /// we need to do this work around where w serialize things using the definitions.
        /// </summary>
        [SerializeField]
        protected List<string> mBodyShapeDefinitions = new List<string>();

        /// <summary>
        /// List that determines what colliders will be ignored
        /// </summary>
        [NonSerialized]
        protected List<Collider> mIgnoreCollisions = null;
        protected List<Transform> mIgnoreTransforms = null;

        /// <summary>
        /// Used primarily for debuggin
        /// </summary>
        public int IgnoreCollisionCount
        {
            get
            {
                if (mIgnoreCollisions == null) { return 0; }
                return mIgnoreCollisions.Count;
            }
        }

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Used for initialization before Update() occurs
        /// </summary>
        void Start()
        {
            mStates = new ActorState[_StateCount];
            for (int i = 0; i < mStates.Length; i++) { mStates[i] = new ActorState(); }

            mPrevState = mStates[mStates.Length - 2];
            mState = mStates[mStates.Length - 1];

            // Cache the transform
            _Transform = transform;

            // Initialize the orientation
            mOrientToGroundNormal = _Transform.up;
            _Transform.rotation.DecomposeSwingTwist(Vector3.up, ref mTilt, ref mYaw);

            // Create the body shapes from the definitions
            DeserializeBodyShapes();

            // Create the unity colliders as needed
            for (int i = 0; i < BodyShapes.Count; i++)
            {
                if (BodyShapes[i]._UseUnityColliders)
                {
                    BodyShapes[i].CreateUnityColliders();
                }
            }

            // Initialize state info
            RaycastHit lGroundHitInfo;
            ProcessGrounding(_Transform.position, Vector3.zero, _Transform.up, _Transform.up, _BaseRadius, out lGroundHitInfo);

            mState.PrevGround = mState.Ground;
            mState.Position = _Transform.position;
            mState.Rotation = _Transform.rotation;

            mPrevState.Ground = mState.Ground;
            mPrevState.Position = _Transform.position;
            mPrevState.Rotation = _Transform.rotation;
        }

        /// <summary>
        /// Applies an instant force. As an impulse, the force of a full second
        /// is automatically applied. The resulting impulse is Force / delta-time.
        /// The impulse is immediately removed after being applied.
        /// </summary>
        /// <param name="rForce">Force including direction and magnitude</param>
        public void AddImpulse(Vector3 rForce)
        {
            // Only add forces during the first update cycle. This way, we
            // don't double forces due to slower frame rates
            if (mUpdateIndex != 1) { return; }

            // Create the force array
            if (mAppliedForces == null) { mAppliedForces = new List<Force>(); }

            // Add the resulting force
            Force lForce = Force.Allocate();
            lForce.Type = ForceMode.Impulse;
            lForce.Value = rForce;
            lForce.StartTime = Time.time;
            lForce.Duration = 0f;

            mAppliedForces.Add(lForce);
        }

        /// <summary>
        /// Applies a force to the avatar over time. 
        /// 
        /// DO NOT USE AddForce AS YOUR PRIMARY METHOD OF MOVING YOUR CHARACTER.
        /// IT IS NOT A RIGID-BODY. USE Move() or RelativeMove().
        /// </summary>
        /// <param name="rForce">Force including direction and magnitude. This is applied each tick for the duration.</param>
        /// <param name="rDuration">Number of seconds to apply the force for (0f is infinite)</param>
        public void AddForce(Vector3 rForce, float rDuration)
        {
            // Only add forces during the first update cycle. This way, we
            // don't double forces due to slower frame rates
            if (mUpdateIndex != 1) { return; }

            // Create the force array
            if (mAppliedForces == null) { mAppliedForces = new List<Force>(); }

            // Allocate the force
            Force lForce = Force.Allocate();
            lForce.Type = ForceMode.Force;
            lForce.Value = rForce;
            lForce.StartTime = Time.time;
            lForce.Duration = rDuration;

            mAppliedForces.Add(lForce);
        }

        /// <summary>
        /// Provides a reliable update time for us to evaluate physics based properties
        /// </summary>
        protected void FixedUpdate()
        {
            //Log.FileWrite("FixedUpdate()");

            // Report that we're not skipping this time
            mFixedUpdates = 0;

            // Since we're grounded, clear out all accumulated velocity
            if (mState.IsGrounded) { mAccumulatedForceVelocity = Vector3.zero; }

            // Gather any forces that have been applied
            Vector3 lForceVelocity = ProcessForces(Time.fixedDeltaTime);
            mAccumulatedForceVelocity = mAccumulatedForceVelocity + lForceVelocity;

            // Setup the world up. It can change based on settings
            mWorldUp = Vector3.up;
            if (_IsGravityRelative)
            {
                // If we're grounded, our "world up" is an average of the last
                // set of ground normals. This ensures we avoid one-off bumps (ie edges)
                if (mState.IsGrounded)
                {
                    mWorldUp = Vector3.zero;

                    int lStateCount = Mathf.Min(_StateCount, 20);
                    for (int i = 0; i < lStateCount; i++)
                    {
                        int lStateIndex = (mStateIndex + i < _StateCount ? mStateIndex + i : mStateIndex + i - _StateCount);
                        if (mStates[lStateIndex] != null) { mWorldUp = mWorldUp + mStates[lStateIndex].GroundSurfaceDirectNormal; }
                    }

                    mWorldUp = mWorldUp.normalized;
                }
                // If we're not grounded, we may head towards the natural world up
                else
                {
                    mWorldUp = Vector3.up;

                    if (mTargetGroundNormal.sqrMagnitude > 0f)
                    {
                        mWorldUp = mTargetGroundNormal;
                    }
                    else if (_KeepOrientationInAir || (mAccumulatedForceVelocity.sqrMagnitude == 0f && mState.GroundSurfaceDistance < _OrientToGroundDistance))
                    {
                        mWorldUp = mState.GroundSurfaceDirectNormal;
                    }
                }
            }

            // Determine the gravity to apply
            Vector3 lWorldGravity = Vector3.zero;

            if (_IsGravityEnabled && _ApplyGravityInFixedUpdate)
            {
                // Setup the world up. It can change based on settings
                lWorldGravity = (_Gravity.sqrMagnitude == 0f ? UnityEngine.Physics.gravity : _Gravity);
                if (_IsGravityRelative) { lWorldGravity = -mWorldUp * lWorldGravity.magnitude; }

                // Accumulate the gravity over time
                mAccumulatedVelocity = mAccumulatedVelocity + (lWorldGravity * Time.fixedDeltaTime);
            }

            // Apply the forces to the currect velociy (which includes gravity)
            mAccumulatedVelocity = mAccumulatedVelocity + lForceVelocity;

            if (mAccumulatedVelocity.sqrMagnitude > 0f && _ApplyGravityInFixedUpdate)
            {
                Vector3 lFixedMovement = mAccumulatedVelocity * Time.fixedDeltaTime;

                // If we borrowed movement due to a skipped fixed update, reduce the current value
                if (lFixedMovement.sqrMagnitude < mBorrowedFixedMovement.sqrMagnitude)
                {
                    lFixedMovement = Vector3.zero;
                }
                else
                {
                    lFixedMovement = lFixedMovement - mBorrowedFixedMovement;
                }

                // Accumulate the movement
                mAccumulatedMovement = mAccumulatedMovement + lFixedMovement;
            }

            // Clear out any borrowed movement
            mBorrowedFixedMovement = Vector3.zero;
        }

        /// <summary>
        /// Called every frame to perform processing. We only use
        /// this function if it's not called by another component.
        /// </summary>
        protected override void LateUpdate()
        {
            // The BaseSystemController.LateUpdate will actually drive our
            // ControllerLateUpdate that we'll use to perform processing
            base.LateUpdate();

            // Allow hooks to process after we do our update this frame
            if (mOnControllerPostLateUpdate != null)
            {
                // Do as many updates as we need to in order to simulate
                // the desired frame rates
                if (mUpdateCount > 0)
                {
                    for (int i = 1; i <= mUpdateCount; i++)
                    {
                        mOnControllerPostLateUpdate(this, _DeltaTime, i);
                    }
                }
                // In this case, there shouldn't be an update. This typically
                // happens when the true FPS is much faster than our desired FPS
                else
                {
                    mOnControllerPostLateUpdate(this, _DeltaTime, 0);
                }
            }
        }

        /// <summary>
        /// Update logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void ControllerUpdate(float rDeltaTime, int rUpdateIndex)
        {
            if (!_ProcessInLateUpdate)
            {
                InternalUpdate(rDeltaTime, rUpdateIndex);
            }
        }

        /// <summary>
        /// LateUpdate logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void ControllerLateUpdate(float rDeltaTime, int rUpdateIndex)
        {
            if (_ProcessInLateUpdate)
            {
                InternalUpdate(rDeltaTime, rUpdateIndex);
            }
        }

        /// <summary>
        /// Update logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        private void InternalUpdate(float rDeltaTime, int rUpdateIndex)
        { 
            //Log.FileWrite("AC.CLU() dt:" + rDeltaTime.ToString("f6") + " ui:" + rUpdateIndex + " udt:" + Time.deltaTime.ToString("f6"));

            // If it's not time for an update, wait for the next frame
            if (rUpdateIndex == 0) { return; }

#if OOTII_PROFILE
            Utilities.Profiler.Start("CLU0");
#endif

            Vector3 lActorUp = _Transform.up;
            Vector3 lActorPosition = _Transform.position;

            // ----------------------------------------------------------------------
            // PROCESS CONTROLLERS
            // ----------------------------------------------------------------------

            // Allow hooks to process before we handle the movement this frame
            if (mOnControllerPreLateUpdate != null)
            {
                mOnControllerPreLateUpdate(this, rDeltaTime, rUpdateIndex);
            }

            // If we are not enabled, stop
            if (!_IsEnabled) { return; }

            // ----------------------------------------------------------------------
            // SHIFT STATES
            // ----------------------------------------------------------------------

#if OOTII_PROFILE
            Utilities.Profiler.Start("CLU1");
#endif

            // Move the current state into the previous and clear the current
            mPrevState = mState;

            mStateIndex = ActorState.Shift(ref mStates, mStateIndex);
            mState = mStates[mStateIndex];

#if OOTII_PROFILE
            Utilities.Profiler.Stop("CLU1");
#endif

            // ----------------------------------------------------------------------
            // UPDATE BODY SHAPES
            // ----------------------------------------------------------------------
            for (int i = 0; i < BodyShapes.Count; i++)
            {
                BodyShapes[i].LateUpdate();
            }

            // ----------------------------------------------------------------------
            // ROTATION
            // ----------------------------------------------------------------------

            // Force the rotation if we need to
            if (mTargetRotation.x != float.MaxValue)
            {
                // Force the rotation
                mYaw = mTargetRotation;

                mState.Rotation = mYaw;
                _Transform.rotation = mState.Rotation;

                // Clear out any values for the next frame
                mTargetRotation.x = float.MaxValue;
            }
            else
            {
                if (!_FreezeRotationY)
                {
                    // Determine the actual movement that will occur based on velocity
                    Quaternion lTargetRotate = Quaternion.Euler(mTargetRotationVelocity * rDeltaTime);
                    mYaw = mYaw * lTargetRotate;

                    // Add any explicit rotation that may have been set
                    mYaw = mYaw * mTargetRotate;
                }

                if (!_FreezeRotationX)
                {
                    mTilt = mTilt * mTargetTilt;
                }

                mTargetRotate = Quaternion.identity;
                mTargetTilt = Quaternion.identity;
            }

            // ----------------------------------------------------------------------
            // POSITION
            // ----------------------------------------------------------------------

            // Force the position if we need to
            if (mTargetPosition.x != float.MaxValue)
            {
                // Track if the user initiated the movement
                mState.IsMoveRequested = true;

                // Zero out any velocity that exists velocity
                mState.Velocity = Vector3.zero;

                // Force the position
                mState.Position = mTargetPosition;
                _Transform.position = mTargetPosition;

                // Do a simple check to see if we're still grounded
                RaycastHit lGroundHitInfo;
                ProcessGrounding(_Transform.position, mState.MovementPlatformAdjust, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);

                // Clear out any target values for the next frame
                mTargetPosition.x = float.MaxValue;
            }
            // Move to the next position
            else
            {
#if OOTII_PROFILE
                Utilities.Profiler.Start("CLU2");
#endif

                // Holds the resulting changes
                Vector3 lFinalMovement = Vector3.zero;
                Quaternion lFinalRotation = Quaternion.identity;

                // If we are currently grounded, we won't be applying any gravity. This isn't our real
                // test, but a way for us to help manage our movement predictions
                RaycastHit lGroundHitInfo;
                bool lIsGrounded = false;

                // ----------------------------------------------------------------------
                // Gather movement from the target set by the user
                // ----------------------------------------------------------------------

                // Determine the actual movement that will occur based on velocity
                mState.Movement = mTargetVelocity * rDeltaTime;

                // Add explicit movement that may have been set
                mState.Movement = mState.Movement + mTargetMove;
                mTargetMove = Vector3.zero;

                // Track if the user initiated the movement
                mState.IsMoveRequested = (mState.Movement.sqrMagnitude > ActorController.EPSILON_SQR);

                // If we want to control movement directly, we only have a limited amount of processing.
                // In this case, we'll handle platforming, but ignore everything else
                if (mOverrideProcessing)
                {
                    // Set the final rotation
					// TRT 2/25/2106
                    //lFinalRotation = (_OrientToGround ? mTilt * mYaw : mYaw);
                    lFinalRotation = mTilt * mYaw;

                    // Determine the final movement based on input and platform movement
                    lFinalMovement = mState.Movement;
                }
                // If we're not overrideding the processing, we do everything.
                else
                {
                    // ----------------------------------------------------------------------
                    // Collect physics based forces from FixedUpdate
                    // ----------------------------------------------------------------------

                    // Since gravity and things like jump are physics based, we want them to run in fixed update. This gives us a
                    // nice consistant velocity and make jumps and falling consistant. However, FixedUpdate and Update aren't in 
                    // synch. So, if we don't adjust, we can get falling that feels like it stutters. If we find that a fixed update
                    // is skipped, we'll "borrow" some velocity from the next frame to try and smooth it out a bit.
                    if (_ApplyGravityInFixedUpdate)
                    {
                        if (_ExtrapolatePhysics && mFixedUpdates > 0f)
                        {
                            mBorrowedFixedMovement = mAccumulatedVelocity * (rDeltaTime / (mFixedUpdates + 1f));
                            mAccumulatedMovement = mBorrowedFixedMovement;
                        }

                        // Determine our final movement based on the forces 
                        mState.MovementForceAdjust = mAccumulatedMovement;

                        // Clear the movement so that we only apply the value during fixed updates.
                        mFixedUpdates++;
                        mAccumulatedMovement = Vector3.zero;
                    }
                    else
                    {
                        if (_IsGravityEnabled)
                        {
                            // Setup the world up. It can change based on settings
                            Vector3 lWorldGravity = (_Gravity.sqrMagnitude == 0f ? UnityEngine.Physics.gravity : _Gravity);
                            if (_IsGravityRelative) { lWorldGravity = -mWorldUp * lWorldGravity.magnitude; }

                            // Accumulate the gravity over time
                            mAccumulatedVelocity = mAccumulatedVelocity + (lWorldGravity * rDeltaTime);

                            // Determine our final movement based on the forces 
                            mState.MovementForceAdjust = mAccumulatedVelocity * rDeltaTime;
                        }
                    }

                    // ----------------------------------------------------------------------
                    // Since we were on a platform last frame, we need to assume we're still on 
                    // it and use it's movement for the grounding. Otherwise, we're testing 
                    // ground (the platform) from an invalid position (without the platform).
                    // ----------------------------------------------------------------------
                    ProcessPlatforming(lActorPosition, lActorUp, Vector3.zero, mPrevState);

                    // ----------------------------------------------------------------------
                    // Quick test for grounding
                    // ----------------------------------------------------------------------
                    float lMaxAngle = (_MaxSlopeAngle > 0f ? _MaxSlopeAngle - 0.5f : MAX_GROUNDING_ANGLE);

                    // Do a simple check on our ground. If we're grounded, we'll remove any downward velocity.
                    lIsGrounded = ProcessGrounding(_Transform.position, mState.MovementPlatformAdjust, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);
                    if (lIsGrounded)
                    {
                        mFallDuration = 0f;
                        mGroundedDuration = mGroundedDuration + rDeltaTime;

                        // Track our vertical movement due to the forces
                        Vector3 lVerticalMovementForceAdjust = Vector3.Project(mState.MovementForceAdjust, mWorldUp);
                        if (Vector3.Dot(lVerticalMovementForceAdjust.normalized, mWorldUp) < 0f)
                        {
                            Vector3 lLateralMovementForceAdjust = mState.MovementForceAdjust - lVerticalMovementForceAdjust;

                            // We'll actually temper how much we remove based on the direct-ness of the ground collision
                            if (mState.GroundSurfaceAngle < lMaxAngle)
                            {
                                lVerticalMovementForceAdjust = lVerticalMovementForceAdjust - Vector3.Project(lVerticalMovementForceAdjust, mState.GroundSurfaceDirection);
                            }

                            // If the user is causing movement, cancel out the vertical drop. This
                            // reduces the stutter that can occur as we go up corners.
                            if (mState.IsMoveRequested && mState.GroundSurfaceAngle < MAX_GROUNDING_ANGLE)
                            {
                                lVerticalMovementForceAdjust = Vector3.zero;
                            }
                            // If we're grounded and the angle is 90f, we must be 
                            else if (!mState.IsGroundSurfaceDirect && mState.GroundSurfaceAngle > 90f - EPSILON)
                            {
                                lVerticalMovementForceAdjust = Vector3.zero;
                            }
                            // If our direct distance is low enough, we'll cancel any downward movement. This
                            // way we won't slide down slopes here.
                            else if (!mState.IsGroundSurfaceDirect && mState.GroundSurfaceAngle >= 0f && mState.GroundSurfaceAngle < lMaxAngle)
                            {
                                lVerticalMovementForceAdjust = Vector3.zero;
                            }
                            // If we're on an edge, we want to increase our speed so we slip off
                            else if (!mState.IsGroundSurfaceDirect && mState.GroundSurfaceAngle > _MinSlopeAngle)
                            {
                                // DO NOT do this. Otherwise, we can get bad stuttering while over gaps.
                                // We will let it fall naturally.

                                //float lGravity = (_Gravity.sqrMagnitude == 0f ? UnityEngine.Physics.gravity.magnitude : _Gravity.magnitude) * rDeltaTime;
                                //lVerticalMovementForceAdjust = lVerticalMovementForceAdjust.normalized * Mathf.Max(lGravity, lVerticalMovementForceAdjust.magnitude);
                            }
                            // If we're oriented to the ground and the angle is small enough, make sure we don't slide
                            else if (_OrientToGround && mState.IsGrounded && Mathf.Abs(mState.GroundSurfaceAngle) < _MinSlopeAngle)
                            {
                                lVerticalMovementForceAdjust = Vector3.zero;
                            }

                            // Reassign the movement
                            mState.MovementForceAdjust = lLateralMovementForceAdjust + lVerticalMovementForceAdjust;
                        }
                    }
                    else
                    {
                        // If we're stepping down, we consider ourselves grounded
                        if (mPrevState.IsSteppingDown && mState.GroundSurfaceDistance < _MaxStepHeight && mState.GroundSurfaceDistance <= mPrevState.GroundSurfaceDistance)
                        {
                            lIsGrounded = true;
                            mState.IsGrounded = true;
                        }
                        else
                        {
                            mGroundedDuration = 0f;
                            mFallDuration = mFallDuration + rDeltaTime;
                        }
                    }

                    // We use this movement variable to compound all the pieces
                    Vector3 lMovement = mState.Movement + mState.MovementForceAdjust + mState.MovementPlatformAdjust;

                    // ----------------------------------------------------------------------
                    // Process slopes for sliding or redirecting movement
                    // ----------------------------------------------------------------------

                    // If we're currently on a slope that is too steep, don't let us try to move up it.
                    // It doesn't really even matter if we're grounded or just close to it
                    if (mState.GroundSurfaceDistance < _SkinWidth + EPSILON && mState.GroundSurfaceAngle > lMaxAngle)
                    {
                        // This check just makes sure we're dealing with slope we are directly over
                        if (mState.GroundSurfaceNormal == mState.GroundSurfaceDirectNormal)
                        {
                            // Check if we're moving into the slope
                            Vector3 lLateralSurfaceNormal = (mState.GroundSurfaceNormal - Vector3.Project(mState.GroundSurfaceNormal, lActorUp)).normalized;
                            if (Vector3.Dot(lMovement.normalized, lLateralSurfaceNormal) < 0f)
                            {
                                // If we have a velocity pushing up, stop it
                                Vector3 lVerticalMovementProj = Vector3.Project(lMovement, lActorUp);
                                if (Vector3.Dot(lVerticalMovementProj.normalized, lActorUp) > 0f)
                                {
                                    mAccumulatedVelocity = Vector3.zero;
                                }
                                // If we have a velocity pushing down, allow it
                                else
                                {
                                    lVerticalMovementProj = Vector3.zero;
                                }

                                // Remove the vertical movement  (if needed) and deflect the lateral movement.
                                lMovement = lMovement - lVerticalMovementProj - Vector3.Project(lMovement, lLateralSurfaceNormal);
                            }
                        }
                    }

                    // If we're not moving (or we are on a max angle slope), we may slide down. This
                    // Does NOT stop our movement based on a max slope. It just causes a slide if we're on a steep slope
                    if (_IsSlidingEnabled && (_MinSlopeAngle > 0f || _MaxSlopeAngle > 0f))
                    {
                        if (lIsGrounded)
                        {
                            if (!mState.IsMoveRequested || mState.GroundSurfaceAngle > lMaxAngle)
                            {
                                // Setup the world gravity. It can change based on settings
                                Vector3 lWorldGravity = (_Gravity.sqrMagnitude == 0f ? UnityEngine.Physics.gravity : _Gravity);
                                if (_IsGravityRelative) { lWorldGravity = -mWorldUp * lWorldGravity.magnitude; }

                                // Apply slope movement based on the gravity
                                ProcessSlope(lMovement, mState, lActorUp, lWorldGravity, rDeltaTime);
                                lMovement = lMovement + mState.MovementSlideAdjust;
                            }
                        }
                        else
                        {
                            // I've disabled this because it adds some stutter
                            if (mState.GroundSurfaceDistance < _BaseRadius && mPrevState.GroundSurfaceAngle > lMaxAngle)
                            {
                                // Setup the world gravity. It can change based on settings
                                Vector3 lWorldGravity = (_Gravity.sqrMagnitude == 0f ? UnityEngine.Physics.gravity : _Gravity);
                                if (_IsGravityRelative) { lWorldGravity = -mWorldUp * lWorldGravity.magnitude; }

                                // Apply slope movement based on the gravity
                                ProcessSlope(lMovement, mState, lActorUp, lWorldGravity, rDeltaTime);
                                lMovement = lMovement + mState.MovementSlideAdjust;
                            }
                        }
                    }

                    // ----------------------------------------------------------------------
                    // Search for collisions on our path
                    // ----------------------------------------------------------------------

                    Vector3 lSegmentPositionDelta = Vector3.zero;
                    float lOriginalMovementSpeed = lMovement.magnitude / rDeltaTime;

                    // In order to handle movement, we predict the resulting position. To do this,
                    // We break the movement up based on each collision that occurs while trying to move.
                    // This essencially becomes a path of segments that leads us to the final position.
                    for (int lSegmentIndex = 0; lSegmentIndex < MAX_SEGMENTS; lSegmentIndex++)
                    {
                        bool lContinue = true;
                        Vector3 lSegmentMovement = lMovement;
                        Vector3 lRemainingSegmentMovement = Vector3.zero;

                        //Log.FileWrite("seg:" + lSegmentIndex + " is-g:" + mState.IsGrounded + " g-dist:" + mState.GroundSurfaceDistance.ToString("f8") + " g:" + (mState.Ground == null ? "null" : mState.Ground.name) + " s-mvm:" + StringHelper.ToString(lSegmentMovement));

                        if (lSegmentMovement.sqrMagnitude > 0f)
                        {
                            bool lOrientToGround = lIsGrounded && lOriginalMovementSpeed > 20f;

                            lContinue = ProcessMovement(lSegmentIndex, lSegmentPositionDelta, lOrientToGround, ref lSegmentMovement, ref lRemainingSegmentMovement);
                            lFinalMovement = lFinalMovement + lSegmentMovement;
                        }

                        // Don't continue if we're not supposed to
                        if (!lContinue) { break; }

                        // If there's no more movement, we are done
                        if (lRemainingSegmentMovement.sqrMagnitude < EPSILON_SQR) { break; }

                        // Store the amount we moved this segment so we can off set future tests
                        lSegmentPositionDelta += lSegmentMovement;

                        // Set the new movement goal for the segment
                        lMovement = lRemainingSegmentMovement;

                    } // Loop segments

                    // Grab the ground information for our new position
                    Vector3 lTiltUp = mTilt.Up();
                    lIsGrounded = ProcessGrounding(_Transform.position, lFinalMovement, lTiltUp, mWorldUp, _BaseRadius, out lGroundHitInfo);

                    // If we're stepping down, we consider ourselves grounded
                    if (!lIsGrounded && mPrevState.IsSteppingDown && mState.GroundSurfaceDirectDistance < _MaxStepHeight && mState.GroundSurfaceDirectDistance <= mPrevState.GroundSurfaceDirectDistance)
                    {
                        lIsGrounded = true;
                        mState.IsGrounded = true;
                    }

                    // ----------------------------------------------------------------------
                    // Handle stepping and forcing to ground
                    // ----------------------------------------------------------------------
                    bool lForceSteppingDown = mPrevState.IsSteppingDown;
                    if (lForceSteppingDown)
                    {
                        if (mState.GroundSurfaceDirectDistance > _MaxStepHeight) { lForceSteppingDown = false; }
                    }

                    // If we're not going to be grounded, we may need to force our way there. If we
                    // don't have an upward velocity and the ground 
                    if (_FixGroundPenetration && (lForceSteppingDown || (!lIsGrounded && mPrevState.IsGrounded)))
                    {
                        if (lForceSteppingDown || (_StepDownSpeed > 0f && mState.GroundSurfaceDirectDistance > _SkinWidth && mState.GroundSurfaceDirectDistance < _MaxStepHeight + _SkinWidth))
                        {
                            // Continue the stepping down process if we were in it or we're high enough
                            //if (lForceSteppingDown || mState.GroundSurfaceDistance > _ForceGroundingDistance)
                            {
                                // Continue if we're already moving down (or not vertically at all). Stop if we're
                                // trying to move up.
                                //Vector3 lVerticalFinalMovement = Vector3.Project(lFinalMovement, lActorUp);
                                //float lVerticalFinalMovementDot = Vector3.Dot(lVerticalFinalMovement.normalized, lActorUp);
                                //if (lVerticalFinalMovementDot <= 0f)

                                if (Vector3.Dot(mState.MovementForceAdjust.normalized, lActorUp) <= 0f)
                                {
                                    if (!lForceSteppingDown)
                                    {
                                        float lHeightPercent = Mathf.Clamp01(mState.GroundSurfaceDirectDistance / _MaxStepHeight);
                                        mCurrentStepSpeed = (_StepDownSpeed * 0.1f) + (_StepDownSpeed * 0.9f * lHeightPercent);
                                    }

                                    float lStepDownSpeed = Mathf.Min(mCurrentStepSpeed * rDeltaTime, mState.GroundSurfaceDirectDistance - _SkinWidth);

                                    lFinalMovement = lFinalMovement - (lActorUp * lStepDownSpeed);
                                    ProcessGrounding(_Transform.position, lFinalMovement, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);

                                    lIsGrounded = true;
                                    mState.IsGrounded = true;

                                    if (mState.GroundSurfaceDirectDistance > _SkinWidth)
                                    {
                                        mState.IsSteppingDown = true;
                                    }
                                }
                            }
                        }

                        // If we're not stepping down and we're above the ground some, pull us down
                        if (_ForceGrounding && !mState.IsSteppingDown && mState.GroundSurfaceDistance > _SkinWidth && mState.GroundSurfaceDistance < _ForceGroundingDistance)
                        {
                            // We won't pull us down if there's a posative force pushing us up
                            //Vector3 lVerticalForceMovementProj = Vector3.Project(mState.MovementForceAdjust, mWorldUp);
                            if (Vector3.Dot(mState.MovementForceAdjust.normalized, mWorldUp) <= 0f)
                            {
                                // If we're already moving down, allow that to continue
                                Vector3 lVerticalFinalMovement = Vector3.Project(lFinalMovement, lActorUp);
                                if (Vector3.Dot(lVerticalFinalMovement.normalized, lActorUp) <= 0f) { lVerticalFinalMovement = Vector3.zero; }

                                // Determine the final downward force to put us on the ground
                                lFinalMovement = lFinalMovement - lVerticalFinalMovement + (mState.GroundSurfaceDirection * (mState.GroundSurfaceDistance - COLLISION_BUFFER));
                                lIsGrounded = ProcessGrounding(_Transform.position, lFinalMovement, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);
                            }
                        }
                    }

                    // If we're under the ground, we need to force ourselves up to it. This way, we're
                    // not falling through.
                    if (lIsGrounded && mState.GroundSurfaceDirectDistance < EPSILON)
                    {
                        float lGroundAdjust = Mathf.Abs(mState.GroundSurfaceDirectDistance) + EPSILON;

                        // We'll continue as long as we aren't in orientation mode. This way
                        // we don't sink into the walls and ramps.
                        //if (!_OrientToGround)
                        if (lOriginalMovementSpeed < 20f)
                        {
                            // Fast moving platforms can cause us to jigger everything is moving. So,
                            // we'll continue as long as we're NOT moving with a platform...
                            if (mState.MovementPlatformAdjust.sqrMagnitude < 0.2f)
                            {
                                // We'll continue as long as we're NOT moving down a slope...
                                // TRT 11/20/15: Changed the max slope angle so we don't keep walking into
                                //               ramps when we're trying to step up.
                                if (mState.MovementSlideAdjust.sqrMagnitude == 0f ||
                                   (_MinSlopeAngle > 0f && mPrevState.GroundSurfaceAngle < _MaxStepUpAngle)) //_MinSlopeAngle + 0.5f))
                                {
                                    // We'll continue as long as we're NOT going down with a platform...
                                    if (mState.MovementPlatformAdjust.sqrMagnitude < EPSILON ||
                                        Vector3.Dot(mState.MovementPlatformAdjust.normalized, mWorldUp) >= 0f ||
                                        Vector3.Dot(lFinalMovement.normalized, mState.MovementPlatformAdjust.normalized) < 0f)
                                    {
                                        // We'll continue as long as we're NOT moving down... well, we'll 
                                        // allow a small amount of down to compensate for tiny slope
                                        Vector3 lVerticalMovement = Vector3.Project(lFinalMovement, lActorUp);
                                        float lVerticalMovementDot = Vector3.Dot(lVerticalMovement, lActorUp);
                                        if (lVerticalMovementDot >= -0.1f)
                                        {
                                            if (_StepUpSpeed > 0f && mState.GroundSurfaceDirectDistance < _MaxStepHeight)
                                            {
                                                if (!mPrevState.IsSteppingUp)
                                                {
                                                    float lHeightPercent = Mathf.Clamp01(lGroundAdjust / _MaxStepHeight);
                                                    mCurrentStepSpeed = (_StepUpSpeed * 0.1f) + (_StepUpSpeed * 0.9f * lHeightPercent);
                                                }

                                                mState.IsSteppingUp = true;
                                                lGroundAdjust = Mathf.Min(mCurrentStepSpeed * rDeltaTime, lGroundAdjust);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (_FixGroundPenetration)
                        {
                            lFinalMovement = lFinalMovement + (-mState.GroundSurfaceDirection * lGroundAdjust);
                            lIsGrounded = ProcessGrounding(_Transform.position, lFinalMovement, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);
                        }
                    }

                    // ----------------------------------------------------------------------
                    // Determine the tilting based on the ground (if enabled)
                    // ----------------------------------------------------------------------
                    bool lAllowPushBack = _AllowPushback;
                    bool lStopOnRotationCollision = _StopOnRotationCollision;
                    bool lUseOrientation = _OrientToGround || mPrevState.IsTilting || mState.IsTilting;

                    // Orient if we can
                    if (lUseOrientation)
                    {
                        mOrientToGroundNormalTarget = Vector3.up;

                        if (_OrientToGround)
                        {
                            // If we're told to go do a specific normal, do it
                            if (mTargetGroundNormal.sqrMagnitude > 0f)
                            {
                                mOrientToGroundNormalTarget = mTargetGroundNormal;
                            }
                            // If we're "keeping" orientation or NOT jumping, stay with the surface direction
                            else if (lIsGrounded || _KeepOrientationInAir || (mAccumulatedForceVelocity.sqrMagnitude == 0f && mState.GroundSurfaceDistance < _OrientToGroundDistance))
                            {
                                if (_MaxSlopeAngle == 0f || mState.GroundSurfaceAngle < _MaxSlopeAngle - 0.5f)
                                {
                                    mOrientToGroundNormalTarget = mState.GroundSurfaceDirectNormal;
                                }
                            }
                        }

                        float lTiltAngle = Vector3.Angle(lTiltUp, mOrientToGroundNormalTarget);

                        // If we're finished tilting, we may need to pretend we're not
                        if (lTiltAngle == 0f)
                        {
                            mOrientToSpeed = 0f;

                            // Clear out any target we may have set
                            //mTargetGroundNormal = Vector3.zero;

                            // Cancel out our tilting if we are
                            if (!_OrientToGround && mState.IsGrounded && lTiltUp == Vector3.up)
                            {
                                mState.IsTilting = false;

                                // With tilting done, we need to reset our rotations
                                lUseOrientation = false;
                                mYaw = _Transform.rotation;
                                mTilt = Quaternion.identity;
                            }
                            // If we haven't hit the ground, we may be falling through objects that
                            // we rotated into. If that's the case, pretend we're tilting until
                            // we are no longer grounded.
                            else if (mPrevState.IsTilting && (!_OrientToGround || !mState.IsGrounded))
                            {
                                mState.IsTilting = true;

                                lAllowPushBack = true;
                                lStopOnRotationCollision = true;
                            }
                        }
                        // If we're on too steep of an angle, don't tilt
                        else if (mState.IsGrounded &&
                            (mState.IsGroundSurfaceDirect) && // || mState.GroundSurfaceDirectDistance < _BaseRadius * 0.5f) && 
                            (_MaxSlopeAngle > 0f && mState.GroundSurfaceAngle > _MaxSlopeAngle - 0.5f))
                        {
                            //Log.FileWrite("max angle, no orient to ground");
                            mState.IsTilting = false;
                        }
                        // At this point, we really do need to tilt
                        else
                        {
                            bool lIgnoreTilt = false;

                            // In the rare chance we catch a corner and rotating the character causes
                            // the ground raycast to hit a different ground plane, we will ignore tilting.
                            //
                            // TRT 11/22: Causing too much stuttering during tilt rotations. Not worth the 
                            // off chance that this might happen. We can't test for mTargetGroundNormal like I
                            // hoped because going ontop of buildings causes an inplace rotation without setting
                            // mTargetGroundNormal.
                            if (mTargetGroundNormal.sqrMagnitude == 0f && _Transform.position == mPrevState.Position)
                            {
                                if (mState.GroundSurfaceAngle != mPrevState.GroundSurfaceAngle)
                                {
                                    //lIgnoreTilt = true;
                                }
                            }

                            // Grab the angle. If we're dealing with a drastic angle change, then
                            // we want to make sure we're not dealing with a "blip".
                            float lGroundSurfaceAngle = mState.GroundSurfaceAngle;
                            if (!mPrevState.IsTilting && lTiltAngle > 45f)
                            {
                                int lStateCount = Mathf.Min(_StateCount, 20);
                                for (int i = 0; i < lStateCount; i++)
                                {
                                    int lStateIndex = (mStateIndex + i < _StateCount ? mStateIndex + i : mStateIndex + i - _StateCount);
                                    if (mStates[lStateIndex] != null) { lGroundSurfaceAngle = lGroundSurfaceAngle + mStates[lStateIndex].GroundSurfaceAngle; }
                                }

                                lGroundSurfaceAngle = lGroundSurfaceAngle / (float)lStateCount;
                                if (mState.GroundSurfaceAngle - lGroundSurfaceAngle > 30f)
                                {
                                    lIgnoreTilt = true;
                                }
                            }

                            // If we're not ignoring the angle change, we need to see if we should
                            // move there smoothly.
                            if (!lIgnoreTilt)
                            {
                                if (!mPrevState.IsTilting)
                                {
                                    // The angle is so small, immediately move to it
                                    if (_OrientToGroundSpeed == 0f || _MinOrientToGroundAngleForSpeed == 0f || lTiltAngle < _MinOrientToGroundAngleForSpeed)
                                    {
                                        mOrientToGroundNormal = mOrientToGroundNormalTarget;
                                    }
                                    // Determine the speed that we'll use to get to the new angle. We want
                                    // to go faster for smaller angles.
                                    else
                                    {
                                        mState.IsTilting = true;

                                        lAllowPushBack = true;
                                        lStopOnRotationCollision = true;

                                        //mOrientToSpeed = lTiltAngle / _OrientToGroundSpeed * rDeltaTime;
                                        float lFactor = Mathf.Max((lTiltAngle - _MinOrientToGroundAngleForSpeed) / (180f - _MinOrientToGroundAngleForSpeed), 0.1f);
                                        mOrientToSpeed = ((lTiltAngle / _OrientToGroundSpeed) / lFactor);

                                        mOrientToGroundNormal = Vector3.RotateTowards(mOrientToGroundNormal, mOrientToGroundNormalTarget, mOrientToSpeed * rDeltaTime * Mathf.Deg2Rad, 0f);
                                    }
                                }
                                // If we're really close to the angle, go there
                                else if (lTiltAngle < 0.1f)
                                {
                                    mOrientToGroundNormal = mOrientToGroundNormalTarget;
                                }
                                // Continue tilting
                                else
                                {
                                    mState.IsTilting = true;

                                    lAllowPushBack = true;
                                    lStopOnRotationCollision = true;

                                    float lFactor = Mathf.Max((lTiltAngle - _MinOrientToGroundAngleForSpeed) / (180f - _MinOrientToGroundAngleForSpeed), 0.1f);
                                    mOrientToSpeed = Mathf.Max(((lTiltAngle / _OrientToGroundSpeed) / lFactor), mOrientToSpeed);

                                    mOrientToGroundNormal = Vector3.RotateTowards(mOrientToGroundNormal, mOrientToGroundNormalTarget, mOrientToSpeed * rDeltaTime * Mathf.Deg2Rad, 0f);
                                }
                            }

                            // Determine the final tilt
                            mTilt = QuaternionExt.FromToRotation(lTiltUp, mOrientToGroundNormal) * mTilt;
                        }
                    }
                    // If we're not allowed to orient to the ground, we're going to make sure
                    // the character is always oreinted up. 
                    else if (!_OrientToGround)
                    {
                        // We shouldn't have to do this, but in the case, where a wierd collision
                        // forces us over, we'll upright ourselves.
                        float lTiltAngle = Vector3.Angle(_Transform.up, Vector3.up);

  					    // TRT 2/25/2016
                        if (lTiltAngle > 0f && mTilt.eulerAngles.sqrMagnitude < 0.001f)
                        {
                            Quaternion lUpFix = QuaternionExt.FromToRotation(_Transform.up, Vector3.up);
                            mYaw = lUpFix * mYaw;

                            mTilt = Quaternion.identity;
                        }
                    }

					// TRT 2/25/2016
                    //lFinalRotation = (lUseOrientation ? mTilt * mYaw : mYaw);
                    lFinalRotation = mTilt * mYaw;

                    // ----------------------------------------------------------------------
                    // Determine if we're intruding on anything
                    // ----------------------------------------------------------------------

                    if (_IsCollisionEnabled && (lStopOnRotationCollision || lAllowPushBack))
                    {
                        for (int i = 0; i < BodyShapes.Count; i++)
                        {
                            BodyShape lBodyShape = BodyShapes[i];
                            if (lBodyShape.IsEnabledOnGround && lIsGrounded || lBodyShape.IsEnabledAboveGround && !lIsGrounded)
                            {
                                Quaternion lDeltaRotation = _Transform.rotation.RotationTo(lFinalRotation);

                                // This is really checking the rotation that was set LAST update. So, if we're going to counter,
                                // we need to use what was valid the frame before last.
                                List<BodyShapeHit> lBodyShapeHits = lBodyShape.CollisionOverlap(lFinalMovement, lDeltaRotation, _CollisionLayers);
                                if (lBodyShapeHits != null && lBodyShapeHits.Count > 0)
                                {
                                    if (lAllowPushBack)
                                    {
                                        Vector3 lDirection = (lBodyShapeHits[0].HitOrigin - lBodyShapeHits[0].HitPoint).normalized;
                                        lFinalMovement = lFinalMovement + (lDirection * -lBodyShapeHits[0].HitDistance);
                                    }

                                    if (lStopOnRotationCollision)
                                    {
                                        mYaw = mPrevState.RotationYaw;
                                        mTilt = mPrevState.RotationTilt;

                    					// TRT 2/25/2016
                                        // lFinalRotation = (lUseOrientation ? mTilt * mYaw : mYaw);
                                        lFinalRotation = mTilt * mYaw;

                                        mOrientToGroundNormal = mTilt.Up();
                                    }

                                    // No need to continue
                                    break;
                                }
                            }
                        }
                    }

                    // ----------------------------------------------------------------------
                    // Apply the rotations and movement
                    // ----------------------------------------------------------------------

                    if (_FreezeRotationX || _FreezeRotationY || _FreezeRotationZ)
                    {
                        Vector3 lEuler = lFinalRotation.eulerAngles;
                        Vector3 lPrevEuler = mPrevState.Rotation.eulerAngles;

                        if (_FreezeRotationX) { lEuler.x = lPrevEuler.x; }
                        if (_FreezeRotationY) { lEuler.y = lPrevEuler.y; }
                        if (_FreezeRotationZ) { lEuler.z = lPrevEuler.z; }
                        lFinalRotation.eulerAngles = lEuler;
                    }

                    if (lFinalMovement.sqrMagnitude > 0f)
                    {
                        if (_FreezePositionX) { lFinalMovement.x = 0f; }
                        if (_FreezePositionY) { lFinalMovement.y = 0f; }
                        if (_FreezePositionZ) { lFinalMovement.z = 0f; }
                    }
                }

                Vector3 lFinalPosition = _Transform.position + lFinalMovement;

                // Allow hooks to modify the final position
                if (mOnPreControllerMove != null)
                {
                    // Set info here so it can be used by the callback
                    mState.Rotation = lFinalRotation;
                    mState.RotationYaw = mYaw;
                    mState.RotationTilt = mTilt;
                    mState.Position = lFinalPosition;

                    // Determine the per second velocity
                    float lCurrentDeltaTime = (rDeltaTime > 0f ? rDeltaTime : Time.fixedDeltaTime);
                    mState.Velocity = (mState.Position - mPrevState.Position) / lCurrentDeltaTime;

                    mOnPreControllerMove(this, ref lFinalPosition, ref lFinalRotation);
                }

                // Finally, set the values
                _Transform.rotation = lFinalRotation;
                _Transform.position = lFinalPosition;

                //Log.FileWrite("dt:" + rDeltaTime.ToString("f8") + " pos-y:" + lFinalPosition.y.ToString("f8") + " p-pos-y:" + mPrevState.Position.y.ToString("f8") + " d-y:" + (lFinalPosition.y - mPrevState.Position.y).ToString("f8") + " s-mfa" + StringHelper.ToString(mState.MovementForceAdjust));

                // ----------------------------------------------------------------------
                // Clean up
                // ----------------------------------------------------------------------

                // Set info here just in case it was changed
                mState.Rotation = lFinalRotation;
                mState.RotationYaw = mYaw;
                mState.RotationTilt = mTilt;
                mState.Position = lFinalPosition;

                // Determine the per second velocity
                float lDeltaTime = (rDeltaTime > 0f ? rDeltaTime : Time.fixedDeltaTime);
                mState.Velocity = (mState.Position - mPrevState.Position) / lDeltaTime;

                // Last thing we do is override the ground information if we're meant to
                if (mTargetGround != null)
                {
                    mState.IsGrounded = true;
                    mState.Ground = mTargetGround;
                    mState.GroundPosition = mTargetGround.position;
                    mState.GroundRotation = mTargetGround.rotation;
                }

                // Since we're grounded, the contact point we use this frame will help us determine
                // movement for the next frame.
                if (mState.IsGrounded && mState.Ground != null)
                {
                    Vector3 lVerticalMovementForce = Vector3.Project(mState.MovementForceAdjust, mWorldUp);
                    Vector3 lLateralMovementForce = mState.MovementForceAdjust - lVerticalMovementForce;

                    // Several things could cause us to have to re-calculate our contact point
                    if (mState.IsMoveRequested ||
                        mState.Ground != mPrevState.Ground ||
                        lLateralMovementForce.sqrMagnitude > 0f ||
                        mState.MovementSlideAdjust.sqrMagnitude > 0f ||
                        mState.GroundLocalContactPoint.sqrMagnitude == 0f ||
                        lFinalMovement.sqrMagnitude < mState.MovementPlatformAdjust.sqrMagnitude)
                    {
                        mState.GroundLocalContactPoint = mState.Ground.InverseTransformPoint(lFinalPosition);
                    }
                }
                else
                {
                    mState.GroundLocalContactPoint = Vector3.zero;
                }

#if OOTII_PROFILE
                Utilities.Profiler.Stop("CLU2");
                Utilities.Profiler.Stop("CLU0");

                Log.ScreenWrite(String.Format("ui:{4} dt:{5:f4} udt:{6:f4} Hits:{3} Profilers - Tot:{0:f4} Sw:{1:f4} Ht:{2:f4}", Utilities.Profiler.ProfilerTime("CLU0"), Utilities.Profiler.ProfilerTime("CLU1"), Utilities.Profiler.ProfilerTime("CLU2"), mBodyShapeHits.Count, rUpdateIndex, rDeltaTime, Time.deltaTime), 0);
                Log.ScreenWrite("isG:" + (mState.IsGrounded ? "1" : "0") + " isP:" + (mState.IsPoppingUp ? "1" : "0") + " isU:" + (mState.IsSteppingUp ? "1" : "0") + " isD:" + (mState.IsSteppingDown ? "1" : "0") + " ga:" + mState.GroundSurfaceAngle.ToString("f3") + " gd:" + mState.GroundSurfaceDistance.ToString("f6") + " pos-y:" + _Transform.position.y.ToString("f6"), 1);

                if (mState.IsColliding)
                {
                    Log.ScreenWrite("isC:" + (mState.IsColliding ? "1" : "0") + " c-nml:" + StringHelper.ToString(mState.ColliderHit.normal), 2);
                }

                Log.FileWrite("isG:" + (mState.IsGrounded ? "1" : "0") + " isP:" + (mState.IsPoppingUp ? "1" : "0") + " isU:" + (mState.IsSteppingUp ? "1" : "0") + " isD:" + (mState.IsSteppingDown ? "1" : "0") + " isC:" + mState.IsColliding + " ga:" + mState.GroundSurfaceAngle.ToString("f3") + " gd:" + mState.GroundSurfaceDistance.ToString("f6") + " gn:" + StringHelper.ToString(mState.GroundSurfaceDirectNormal) + " pos-y:" + _Transform.position.y.ToString("f6"));
                Log.FileWrite("f-mvm:" + StringHelper.ToString(lFinalMovement));
                Log.FileWrite("");
#endif

                // If we find we are grounded, this is the time to clear our acceleration.
                // We don't do this in FixedUpdate since the frame rate may be low and cause
                // FixedUpdate to run before LateUpdate does.
                if (!_IsGravityEnabled || mState.IsGrounded)
                {
                    mAccumulatedVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Ground we want the actor "virtually" parented to
        /// </summary>
        /// <param name="rGround"></param>
        public void SetGround(Transform rGround)
        {
            mTargetGround = rGround;

            if (mTargetGround == null)
            {
                mAccumulatedVelocity = Vector3.zero;
                mAccumulatedMovement = Vector3.zero;
            }
        }

        /// <summary>
        /// Sets and absolute tilt to orient the actor to
        /// </summary>
        /// <param name="rPosition"></param>
        public void SetTargetGroundNormal(Vector3 rTargetGroundNormal)
        {
            if (_OrientToGround)
            {
                mTargetGroundNormal = rTargetGroundNormal;
            }
        }

        /// <summary>
        /// Sets and absolute rotation to rotate the actor to
        /// </summary>
        /// <param name="rRotation"></param>
        public void SetRotation(Quaternion rRotation)
        {
            mTargetRotation = rRotation;
        }

        /// <summary>
        /// Sets an absolution "yaw" rotation for the frame. This rotation
        /// is relative to the actor's current rotation
        /// </summary>
        /// <param name="rRotation"></param>
        public void Rotate(Quaternion rRotation)
        {
            mTargetRotate = mTargetRotate * rRotation;
            mTargetTilt = Quaternion.identity;
        }

        /// <summary>
        /// Sets an absolution "yaw" rotation and "pitch" rotation for the frame. This rotation
        /// is relative to the actor's current rotation
        /// </summary>
        /// <param name="rRotation"></param>
        public void Rotate(Quaternion rRotation, Quaternion rTilt)
        {
            mTargetRotate = mTargetRotate * rRotation;
            mTargetTilt = mTargetTilt * rTilt;
        }

        /// <summary>
        /// Sets a velocity that will cause rotation over time.
        /// </summary>
        /// <param name="rVelocity"></param>
        public void SetRotationVelocity(Vector3 rVelocity)
        {
            mTargetRotationVelocity = rVelocity;
        }

        /// <summary>
        /// Sets and absolute position to move the actor to
        /// </summary>
        /// <param name="rPosition"></param>
        public void SetPosition(Vector3 rPosition)
        {
            mTargetPosition = rPosition;
        }

        /// <summary>
        /// Sets an absolute movement for the frame. This is in addition
        /// to any velocity that is set.
        /// </summary>
        /// <param name="rMovement"></param>
        public void Move(Vector3 rMovement)
        {
            mTargetMove = mTargetMove + rMovement;
        }

        /// <summary>
        /// Sets an absolute movement for the frame. This is in addition
        /// to any velocity that is set.
        /// </summary>
        /// <param name="rMovement"></param>
        public void RelativeMove(Vector3 rMovement)
        {
            mTargetMove = mTargetMove + (_Transform.rotation * rMovement);
        }

        /// <summary>
        /// Sets a velocity that will cause movement over time.
        /// </summary>
        /// <param name="rVelocity"></param>
        public void SetVelocity(Vector3 rVelocity)
        {
            mTargetVelocity = rVelocity;
        }

        /// <summary>
        /// Sets a velocity that will cause movement over time.
        /// </summary>
        /// <param name="rVelocity"></param>
        public void SetRelativeVelocity(Vector3 rVelocity)
        {
            mTargetVelocity = _Transform.rotation * rVelocity;
        }

        /// <summary>
        /// Grabs the closest point on the actor's body shapes to the origin
        /// </summary>
        /// <param name="rOrigin">Position we're testing from</param>
        /// <returns>Position on the body shape surfaces that is the closest point or Vector3.zero if no point is found</returns>
        public Vector3 ClosestPoint(Vector3 rOrigin)
        {
            Vector3 lClosestPoint = Vector3.zero;
            float lClosestDistance = float.MaxValue;

            for (int i = 0; i < BodyShapes.Count; i++)
            {
                Vector3 lPoint = BodyShapes[i].ClosestPoint(rOrigin);
                if (lPoint.sqrMagnitude > 0f)
                {
                    float lDistance = (lPoint - rOrigin).sqrMagnitude;
                    if (lDistance < lClosestDistance)
                    {
                        lClosestPoint = lPoint;
                        lClosestDistance = lDistance;
                    }                        
                }
            }

            return lClosestPoint;
        }

        /// <summary>
        /// Determines if we're meant to ignore the specified collider
        /// </summary>
        /// <param name="rCollider">Collider to test</param>
        /// <returns>Tells if we are meant to ignore the collision</returns>
        public bool IsIgnoringCollision(Collider rCollider)
        {
            if (mIgnoreCollisions == null) { return false; }
            return mIgnoreCollisions.Contains(rCollider);
        }

        /// <summary>
        /// Clears any colliders that were meant to be ignored
        /// </summary>
        public void ClearIgnoreCollisions()
        {
            if (mIgnoreCollisions != null)
            {
                for (int i = 0; i < mIgnoreCollisions.Count; i++)
                {
                    for (int j = 0; j < BodyShapes.Count; j++)
                    {
                        for (int k = 0; k < BodyShapes[j].Colliders.Length; k++)
                        {
                            UnityEngine.Physics.IgnoreCollision(BodyShapes[j].Colliders[k], mIgnoreCollisions[i], false);
                        }
                    }
                }

                mIgnoreCollisions.Clear();
                mIgnoreTransforms.Clear();
            }
        }

        /// <summary>
        /// Sets a collider to be ignored or not by the character controller
        /// </summary>
        /// <param name="rCollider">Collider to ignore or not</param>
        /// <param name="rIgnore">Flag to determine if we are ignoring</param>
        public void IgnoreCollision(Collider rCollider, bool rIgnore = true)
        {
            // Get out if there is no work to do
            if (rIgnore && IsIgnoringCollision(rCollider)) { return; }
            if (!rIgnore && !IsIgnoringCollision(rCollider)) { return; }

            // First, ensure any unity colliders are disabled
            for (int i = 0; i < BodyShapes.Count; i++)
            {
                for (int j = 0; j < BodyShapes[i].Colliders.Length; j++)
                {
                    UnityEngine.Physics.IgnoreCollision(BodyShapes[i].Colliders[j], rCollider, rIgnore);
                }
            }

            // Add the collider to our list
            if (rIgnore)
            {
                if (mIgnoreCollisions == null) { mIgnoreCollisions = new List<Collider>(); }
                if (!mIgnoreCollisions.Contains(rCollider)) { mIgnoreCollisions.Add(rCollider); }

                if (mIgnoreTransforms == null) { mIgnoreTransforms = new List<Transform>(); }
                if (!mIgnoreTransforms.Contains(rCollider.transform)) { mIgnoreTransforms.Add(rCollider.transform); }
            }
            // Remove the collider from our list
            else
            {
                if (mIgnoreCollisions != null) { mIgnoreCollisions.Remove(rCollider); }
                if (mIgnoreTransforms != null) { mIgnoreTransforms.Remove(rCollider.transform); }
            }
        }

        /// <summary>
        /// Simple test to get the ground information that is directly under the actor. This is meant to be fast.
        /// </summary>
        /// <param name="rPosition">Position to test</param>
        /// <param name="rActorUp">Actor's up vector</param>
        /// <param name="rWorldUp">World's up vector</param>
        /// <param name="rGroundHitInfo"></param>
        /// <returns>Boolean that determines if a hit took place</returns>
        protected bool TestGrounding(Vector3 rPosition, Vector3 rActorUp, Vector3 rWorldUp, out RaycastHit rGroundHitInfo)
        {
            Vector3 lRayStart = rPosition + (rActorUp * _GroundingStartOffset);
            Vector3 lRayDirection = -rActorUp;
            float lRayDistance = _GroundingStartOffset + _GroundingDistance;

            // Start with a simple ray. This would be the object directly under the actor
            bool lGroundHit = false;
            if (_IsGroundingLayersEnabled)
            {
                lGroundHit = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out rGroundHitInfo, lRayDistance, _GroundingLayers, _Transform, mIgnoreTransforms);
            }
            else
            {
                lGroundHit = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out rGroundHitInfo, lRayDistance, -1, _Transform, mIgnoreTransforms);
            }

            if (lGroundHit)
            {
                rGroundHitInfo.distance = rGroundHitInfo.distance - _GroundingStartOffset;
            }

            return lGroundHit;
        }

        /// <summary>
        /// Determines if we're able to continue moving based on the angle of the slope. It's a little more complex
        /// as we need to "step" forward to try to hit a bad slope. Then, we squeeze to find the exact point where the
        /// slope becomes invalid.
        /// </summary>
        /// <param name="rPosition">Position of the actor</param>
        /// <param name="rActorUp">Actor's up vector</param>
        /// <param name="rMovement">Movement we are trying to achieve</param>
        /// <param name="rCurrentGroundSurfaceAngle">Current angle the character starts on</param>
        /// <param name="rSafeMovement">Amount of movement that can occur before we get to the angle</param>
        /// <param name="rGroundNormal">Bad slope that we stop at</param>
        /// <returns>Determines if a slope change was hit</returns>
        protected bool TestSlope(Vector3 rPosition, Vector3 rActorUp, Vector3 rMovement, float rCurrentGroundSurfaceAngle, ref Vector3 rSafeMovement, ref Vector3 rGroundNormal)
        {
            if (rMovement.sqrMagnitude == 0f)
            {
                return false;
            }

            Vector3 lMovementDirection = rMovement.normalized;
            float lMovementDistance = rMovement.magnitude;

            // First, shoot a ray forward to determine if we end up hitting a slope at all
            RaycastHit lHitInfo;

            bool lGroundHit = false;
            if (_IsGroundingLayersEnabled)
            {
                lGroundHit = RaycastExt.SafeRaycast(rPosition + (rActorUp * EPSILON), lMovementDirection, out lHitInfo, lMovementDistance + _SkinWidth, _GroundingLayers, _Transform, mIgnoreTransforms);
            }
            else
            {
                lGroundHit = RaycastExt.SafeRaycast(rPosition + (rActorUp * EPSILON), lMovementDirection, out lHitInfo, lMovementDistance + _SkinWidth, -1, _Transform, mIgnoreTransforms);
            }

            if (!lGroundHit)
            {
                return false;
            }

            // TRT 10/25: We shouldn't need this any more. Jumping while moving caused this condition to
            //             occur and we'd sink in when we shouldn't.
            //
            // We hit a slope at "root + epsilon". However, it may just be a small bump we can step over. So, we'll
            // do another check to see if we also hit something above step height.
            //RaycastHit lStepHitInfo;
            //if (!RaycastExt.SafeRaycast(rPosition + (rActorUp * (_MaxStepHeight + EPSILON)), lMovementDirection, lMovementDistance + _OverlapRadius, _Transform, out lStepHitInfo))
            //{
            //    return false;
            //}

            // Test if there's anything further above the hit point. If not, we may be able to step over it
            Vector3 lLocalHitPoint = _Transform.InverseTransformPoint(lHitInfo.point);
            if (lLocalHitPoint.y < _MaxStepHeight)
            {
                RaycastHit lStepHitInfo;
                
                if (_IsGroundingLayersEnabled)
                {
                    lGroundHit = RaycastExt.SafeRaycast(rPosition + (rActorUp * _MaxStepHeight), lMovementDirection, out lStepHitInfo, 1.5f, _GroundingLayers, _Transform, mIgnoreTransforms);
                }
                else
                {
                    lGroundHit = RaycastExt.SafeRaycast(rPosition + (rActorUp * _MaxStepHeight), lMovementDirection, out lStepHitInfo, 1.5f, -1, _Transform, mIgnoreTransforms);
                }

                if (!lGroundHit)
                {
                    return false;
                }
            }

            // If we're not dealing with a slope (ie an actor-up facing normal), we can stop looking
            Vector3 lHitNormalProj = Vector3.Project(lHitInfo.normal, rActorUp);
            if (Vector3.Dot(lHitNormalProj.normalized, rActorUp) == 0f)
            {
                return false;
            }

            float lHitAngle = Vector3.Angle(lHitInfo.normal, rActorUp);
            rGroundNormal = lHitInfo.normal;

            // Phase #2 - Squeeze the ends to find the exact point the slope starts
            // We have to process the raycast at least once
            bool lSlopeHit = false;

            Vector3 lLastGood = rPosition;

            Vector3 lStart = rPosition;
            Vector3 lEnd = rPosition + (lMovementDirection * lHitInfo.distance);
            Vector3 lMid = lStart + ((lEnd - lStart) / 2f);

            float lDistanceSqr = (lEnd - lStart).sqrMagnitude;

            float lMinDistanceSqr = _SlopeMovementStep * _SlopeMovementStep;
            if (lMinDistanceSqr > lDistanceSqr) { lMinDistanceSqr = lDistanceSqr; }

            while (lDistanceSqr > EPSILON_SQR && lDistanceSqr >= lMinDistanceSqr)
            {
                lSlopeHit = false;

                if (_IsGroundingLayersEnabled)
                {
                    lGroundHit = RaycastExt.SafeRaycast(lMid + (rActorUp * _MaxStepHeight), -rActorUp, out lHitInfo, _MaxStepHeight + 0.05f, _GroundingLayers, _Transform, mIgnoreTransforms);
                }
                else
                {
                    lGroundHit = RaycastExt.SafeRaycast(lMid + (rActorUp * _MaxStepHeight), -rActorUp, out lHitInfo, _MaxStepHeight + 0.05f, -1, _Transform, mIgnoreTransforms);
                }

                if (lGroundHit)
                {
                    float lGroundSurfaceAngle = Vector3.Angle(lHitInfo.normal, rActorUp);
                    if (lGroundSurfaceAngle == lHitAngle)
                    {
                        lSlopeHit = true;
                    }
                }

                // Close the gap
                if (lSlopeHit)
                {
                    lEnd = lMid;
                }
                else
                {
                    lStart = lMid;
                    lLastGood = lMid;
                }

                lDistanceSqr = (lEnd - lStart).sqrMagnitude;

                // Determine the new mid
                lMid = lStart + ((lEnd - lStart) / 2f);
            }

            rSafeMovement = lLastGood - rPosition;

            // Return that we had an invalid movement
            return true;
        }

        /// <summary>
        /// Inner function responsible for each movement step in a single frame. This allows
        /// us to compartmentalize our logic a bit.
        /// </summary>
        /// <param name="rSegmentIndex">Index of the segment for this frame</param>
        /// <param name="rSegmentPositionDelta">Starting position change we need to factor in</param>
        /// <param name="rSegmentMovement">Amount of movement we want to do</param>
        /// <param name="rOrientToGround">Determines if we should orient to the ground</param>
        /// <returns></returns>
        protected bool ProcessMovement(int rSegmentIndex, Vector3 rSegmentPositionDelta, bool rOrientToGround, ref Vector3 rSegmentMovement, ref Vector3 rRemainingMovement)
        {
            bool lIsSlopePushingDown = false;
            bool lIsSlopePushingUp = false;

            Vector3 lActorUp = mTilt.Up();

            // In order to test an acurate ground, we need to test the ground including
            // the platform movement. Otherwise, the platform will get ahead of us and we won't
            // be testing accurately. However, we don't want to overcompensate as we 
            // step closer to the actual endpoint (which includes the platform movement). So, we 
            // remove old movement as if it came from the platform.
            //bool lPlatformMovementExists = false;
            //Vector3 lPostPlatformMovement = Vector3.zero;

            Vector3 lPlatformMovement = mState.MovementPlatformAdjust;
            if (lPlatformMovement.sqrMagnitude > 0f)
            {
                if (rSegmentPositionDelta.sqrMagnitude < lPlatformMovement.sqrMagnitude)
                {
                    //lPlatformMovementExists = true;
                    lPlatformMovement = lPlatformMovement - rSegmentPositionDelta;
                }
                else
                {
                    lPlatformMovement = Vector3.zero;
                }
            }

            // ----------------------------------------------------------------------
            // Get the new ground information for the segment (again, assume the
            // platform movement is valid so we get the right ground info)
            // ----------------------------------------------------------------------
            RaycastHit lGroundHitInfo;
            bool lIsGrounded = ProcessGrounding(_Transform.position + lPlatformMovement, rSegmentPositionDelta, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);

            //Log.FileWrite("");
            //Log.FileWrite("seg:" + rSegmentIndex + " is-g:" + mState.IsGrounded + " g:" + (mState.Ground == null ? "null" : mState.Ground.name) + " f-pos:" + StringHelper.ToString(_Transform.position + rSegmentPositionDelta) + " f+m-pos:" + StringHelper.ToString(_Transform.position + rSegmentPositionDelta + rSegmentMovement) + " d-pos:" + StringHelper.ToString(rSegmentPositionDelta) + " mvm:" + StringHelper.ToString(rSegmentMovement));
            //Log.FileWrite("actor-to-surface angle:" + Vector3.Angle(lActorUp, mState.GroundSurfaceNormal) + " actor-up:" + StringHelper.ToString(lActorUp) + " g-nml:" + StringHelper.ToString(mState.GroundSurfaceNormal));

            // If we're stepping down, we consider ourselves grounded
            if (!lIsGrounded && mPrevState.IsSteppingDown && mState.GroundSurfaceDirectDistance < _MaxStepHeight && mState.GroundSurfaceDistance <= mPrevState.GroundSurfaceDistance)
            {
                lIsGrounded = true;
                mState.IsGrounded = true;
            }

            // ----------------------------------------------------------------------
            // If we're penetrating into the ground, push out
            // ----------------------------------------------------------------------

            // We're testing our "current" position for the segment. So, the first time through, this
            // is the position we're currently at. Usually that means we'll catch a "step-up" first. However,
            // if we have multiple segments or if an object moves under us (like a platform), we could get a pop-up here. 
            if (_FixGroundPenetration)
            {
                Vector3 lVerticalPlatformMovement = Vector3.Project(mState.MovementPlatformAdjust, lActorUp);
                if (lIsGrounded && !mPrevState.IsSteppingUp && !mPrevState.IsSteppingDown && mState.GroundSurfaceDistance + lVerticalPlatformMovement.magnitude < 0f)
                {
                    // Finally, do a speed test. We don't want to pop if we don't have to
                    if (rSegmentMovement.sqrMagnitude > 0.01f)
                    {
                        rSegmentMovement = rSegmentMovement + (mTilt.Up() * -(mState.GroundSurfaceDistance + lVerticalPlatformMovement.magnitude));
                    }
                }
            }

            // ----------------------------------------------------------------------
            // Determine if there is a slope change
            // ----------------------------------------------------------------------
            if (lIsGrounded && rSegmentMovement.sqrMagnitude > 0f)
            {
                Vector3 lSafeMovement = Vector3.zero;
                Vector3 lGroundSurfaceNormal = mState.GroundSurfaceNormal;

                if (TestSlope(_Transform.position + rSegmentPositionDelta, lActorUp, rSegmentMovement, mState.GroundSurfaceAngle, ref lSafeMovement, ref lGroundSurfaceNormal))
                {
                    float lHitAngle = Vector3.Angle(lGroundSurfaceNormal, lActorUp);
                    if (lHitAngle > (_MaxSlopeAngle > 0f ? _MaxSlopeAngle - 0.5f : MAX_GROUNDING_ANGLE) && lHitAngle < 90f - EPSILON)
                    //if (lHitAngle < 85f && (_MaxSlopeAngle > 0f && lHitAngle > _MaxSlopeAngle - 0.5f))
                    {
                        // Treat the normal as if it's a vertical wall.
                        Vector3 lGroundSurfaceProj = Vector3.Project(lGroundSurfaceNormal, lActorUp);
                        lGroundSurfaceNormal = (lGroundSurfaceNormal - lGroundSurfaceProj).normalized;

                        // Push back from slope point that we are ending at
                        lSafeMovement = lSafeMovement + (lGroundSurfaceNormal * _SkinWidth);

                        // Deflect the remaining movement off this "wall"
                        rRemainingMovement = rSegmentMovement - lSafeMovement;
                        rRemainingMovement = rRemainingMovement - Vector3.Project(rRemainingMovement, lGroundSurfaceNormal);
                    }
                    else
                    {
                        //Log.FileWrite("PM() slope hit, not max. nml:" + StringHelper.ToString(lGroundSurfaceNormal));
                        rRemainingMovement = rSegmentMovement - lSafeMovement;
                        rRemainingMovement = rRemainingMovement - Vector3.Project(rRemainingMovement, lGroundSurfaceNormal);
                    }

                    rSegmentMovement = lSafeMovement;

                    if (rSegmentMovement.magnitude < EPSILON)
                    {
                        if (rRemainingMovement.magnitude < EPSILON)
                        {
                            rRemainingMovement = Vector3.zero;
                        }

                        return true;
                    }

                    // Since there will be movement, we need to check grounding for this new position

                    // TRT 3/16/2016 - When on very steep slopes, can cause 'is grounded' to be false. This
                    //                 will enable 'in-air' only colliders and can cause unwanted issues when
                    //                 we're really not in the air.
                    //lIsGrounded = ProcessGrounding(_Transform.position + rSegmentMovement, rSegmentPositionDelta, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);
                }
            }

            // ----------------------------------------------------------------------
            // Clamp to slope
            // ----------------------------------------------------------------------
            if (mPrevState.IsGrounded && mState.GroundSurfaceAngle > EPSILON && rSegmentMovement.sqrMagnitude > 0f)
            {
                // TRT 11/13/15: Added this so that we don't do the logic when we're forcing the ground to
                //               be something like a ladder. In this case, a slanted floor can push us away
                //               from the ladder. Since we're trying to find a slope... makes sense the slope is the same ground as before
                if (mPrevState.Ground == mState.Ground)
                {
                    RaycastHit lForwardGroundHitInfo;
                    bool lIsValidGroundTest = TestGrounding(_Transform.position + rSegmentPositionDelta + rSegmentMovement, lActorUp, mWorldUp, out lForwardGroundHitInfo);

                    // Ensure we are over the sloping ground directly (no spherecast)
                    if (lIsValidGroundTest && lForwardGroundHitInfo.collider.transform == mState.Ground)
                    {
                        // Test if we're on a slope. If the forward normal and ground normal are the same, changes
                        // are we are on a slope with a (near) constant angle.
                        float lDeltaAngle = Vector3.Angle(lForwardGroundHitInfo.normal, mState.GroundSurfaceNormal);

                        // If the angle of the ground has some slope and is nearly the same...
                        if (lDeltaAngle < 5f)
                        {
                            // If we're not moving up (ie a jump)...
                            if (Vector3.Dot(mState.MovementForceAdjust.normalized, mWorldUp) <= 0f)
                            {
                                // If we've gotten here, it's safe to say we're on a slope.
                                Vector3 lSlopeRight = Vector3.Cross(mState.GroundSurfaceNormal, -mWorldUp).normalized;
                                Vector3 lSlopeDirection = Vector3.Cross(lSlopeRight, mState.GroundSurfaceNormal).normalized;

                                // If we're moving in the direction of the slope (not against it, but down with it), we'll adjust the
                                // vertical component of the movement to compensate for the slope
                                float lMovementDot = Vector3.Dot(rSegmentMovement.normalized, lSlopeDirection);
                                if (lMovementDot > 0f)
                                {
                                    Vector3 lSlopeMovement = Vector3.Project(rSegmentMovement, lSlopeDirection) + Vector3.Project(rSegmentMovement, lSlopeRight);
                                    rSegmentMovement = lSlopeMovement.normalized * rSegmentMovement.magnitude;

                                    lIsSlopePushingDown = true;
                                }
                                else if (lMovementDot < 0f)
                                {
                                    lIsSlopePushingUp = true;
                                }

                                // We'll consider this whole movement the slope adjust as it will be tested
                                // later if we should stop pushing down.
                                mState.MovementSlideAdjust = rSegmentMovement;
                            }
                        }
                    }
                }
            }

            // ----------------------------------------------------------------------
            // Collisions
            // ----------------------------------------------------------------------
            if (_IsCollisionEnabled)
            {
                bool lIsSlope = false;

                // Clear out the hit list so we can refill it
                for (int i = 0; i < mBodyShapeHits.Count; i++) { BodyShapeHit.Release(mBodyShapeHits[i]); }
                mBodyShapeHits.Clear();

                Quaternion lCurrentRotation = _Transform.rotation;
                _Transform.rotation = mTilt * mYaw;

                // For each body shape, we want to see if there will be a collision
                // as we attempt to move. If there is, we may need to stop our deflect
                // our movement.
                for (int i = 0; i < BodyShapes.Count; i++)
                {
                    if (lIsGrounded && !BodyShapes[i].IsEnabledOnGround) { continue; }
                    if (!lIsGrounded && !BodyShapes[i].IsEnabledAboveGround) { continue; }

                    // This one is a little trickier. We want to see if we're on a "slope" or "ramp". We'll
                    // define that as any angular surface whose angle is consistant.
                    if (mState.GroundSurfaceAngle > 5f && !BodyShapes[i].IsEnabledOnSlope)
                    {
                        // TRT 11/12: Removed the condition because steep angles were ignoring this as the ground
                        // surface distance was greater than the radius.
                        //if (mState.GroundSurfaceDistance < _BaseRadius)
                        {
                            lIsSlope = true;

                            int lStateCount = Mathf.Min(_StateCount, 20);
                            for (int j = 0; j < lStateCount; j++)
                            {
                                int lStateIndex = (mStateIndex + j < _StateCount ? mStateIndex + j : mStateIndex + j - _StateCount);
                                if (mStates[lStateIndex].GroundSurfaceAngle != mState.GroundSurfaceAngle)
                                {
                                    lIsSlope = false;
                                    break;
                                }
                            }

                            if (lIsSlope) { continue; }
                        }
                    }

                    // If we got here, we have a valid shape to test collisions against
                    Vector3 lSegmentMovementDirection = rSegmentMovement.normalized;

                    BodyShapeHit[] lBodyShapeHits = BodyShapes[i].CollisionCastAll(rSegmentPositionDelta, lSegmentMovementDirection, rSegmentMovement.magnitude, _CollisionLayers);
                    if (lBodyShapeHits != null && lBodyShapeHits.Length > 0)
                    {
                        for (int j = 0; j < lBodyShapeHits.Length; j++)
                        {
                            if (lBodyShapeHits[j] == null) { continue; }

                            // Test if we're hitting an object connected to our current platform
                            Transform lCurrentTransform = lBodyShapeHits[j].HitCollider.transform;
                            while (lCurrentTransform != null)
                            {
                                lBodyShapeHits[j].IsPlatformHit = (lCurrentTransform == mState.Ground);
                                if (lBodyShapeHits[j].IsPlatformHit) { break; }

                                lCurrentTransform = lCurrentTransform.parent;
                            }

                            // If we're on a slope and we've pushed ourselves down to move with it, we don't
                            // want that collision to stop us.
                            if (lIsSlopePushingDown && lBodyShapeHits[j].IsPlatformHit)
                            {
                                float lDeltaAngle = Vector3.Angle(lBodyShapeHits[j].HitNormal, mState.GroundSurfaceNormal);
                                if (lDeltaAngle < 2f)
                                {
                                    continue;
                                }
                                else
                                {
                                    // Without this, we get a small bump as we hit the bottom of the ramp. With it,
                                    // if the player is moving a minicule amount, we get a small bump going up the ramp.
                                    // Better with it.
                                    continue;
                                }
                            }

                            // Check if the hit is below our step height
                            if (_MaxStepHeight > 0f && lBodyShapeHits[j].HitRootDistance < _MaxStepHeight)
                            {
                                Vector3 lVerticalMovement = Vector3.Project(rSegmentMovement, lActorUp);
                                if (lVerticalMovement.sqrMagnitude == 0f)
                                {
                                    //continue;
                                }
                            }

                            // Only handle colliders whose hit normal collides with the direction we're moving. Otherwise,
                            // it's not in the way. For platform hits, we need to remove the platform movement first
                            if (lBodyShapeHits[j].IsPlatformHit)
                            {
                                Vector3 lNonPlatformMovement = rSegmentMovement - lPlatformMovement;
                                if (Vector3.Dot(lNonPlatformMovement.normalized, lBodyShapeHits[j].HitNormal) > -EPSILON)
                                {
                                    continue;
                                }
                            }
                            // Here we can use the full movement
                            else if (Vector3.Dot(lSegmentMovementDirection, lBodyShapeHits[j].HitNormal) > -EPSILON)
                            {
                                continue;
                            }

                            // If we get here, re-allocate and then add the shape
                            BodyShapeHit lBodyShapeHit = BodyShapeHit.Allocate(lBodyShapeHits[j]);
                            mBodyShapeHits.Add(lBodyShapeHit);
                        }

                        // Release our local allocations
                        for (int j = 0; j < lBodyShapeHits.Length; j++)
                        {
                            BodyShapeHit.Release(lBodyShapeHits[j]);
                        }
                    }
                }

                // Sort the collisions
                if (mBodyShapeHits.Count > 1)
                {
                    mBodyShapeHits = mBodyShapeHits.OrderBy(x => x.HitDistance).ToList();
                }

                // We only process one collision at a time. Otherwise, we could be bouncing
                // around. So, this is the closest collision that occured
                //for (int i = 0; i < mBodyShapeHits.Count; i = mBodyShapeHits.Count)
                if (mBodyShapeHits.Count > 0)
                {
                    int i = 0;
                    BodyShapeHit lBodyShapeHit = mBodyShapeHits[i];

                    // Store the fact that we're colliding with something
                    mState.IsColliding = true;
                    mState.Collider = lBodyShapeHit.HitCollider;
                    mState.ColliderHit = lBodyShapeHit.Hit;
                    mState.ColliderHit.point = lBodyShapeHit.HitPoint;
                    mState.ColliderHit.normal = lBodyShapeHit.HitNormal;
                    mState.ColliderHitOrigin = lBodyShapeHit.HitOrigin;

                    // Store the hit normal for easy access
                    Vector3 lMovementHitNormal = lBodyShapeHit.HitNormal;
                    float lMovementHitAngle = Vector3.Angle(lMovementHitNormal, lActorUp);

                    // We need to support 'step-up' for solid body shapes. We'll basically ignore the
                    // collision which will cause us to penetrate. Then our normal step-up process will continue.
                    if (mState.IsGrounded && 
                        (lMovementHitAngle < 2f || lMovementHitAngle >= MAX_GROUNDING_ANGLE) && 
                        (_MaxStepHeight > 0f && lBodyShapeHit.HitRootDistance < _MaxStepHeight))
                    {
                        mState.IsPoppingUp = true;

                        // Grab the lateral distance to the edge
                        Vector3 lToHitPoint = lBodyShapeHit.HitPoint - (_Transform.position + rSegmentPositionDelta);
                        Vector3 lVerticalToHitPoint = Vector3.Project(lToHitPoint, lActorUp);
                        Vector3 lLateralToHitPoint = lToHitPoint - lVerticalToHitPoint;

                        // If our movement exceeds it, allow us to move up to the edge. Then, 
                        // continue with the remaining movement.
                        if (rSegmentMovement.sqrMagnitude > lLateralToHitPoint.sqrMagnitude)
                        {
                            // Determine how much to move to get past the edge
                            Vector3 lPreCollisionMovement = rSegmentMovement.normalized * (lLateralToHitPoint.magnitude + _SkinWidth);

                            // Use the remaining movement to continue
                            rRemainingMovement = rRemainingMovement + (rSegmentMovement - lPreCollisionMovement);

                            // Reset this segment's movement to what we discovered
                            rSegmentMovement = lPreCollisionMovement;
                        }
                        // If there isn't extra movement, we'll add some to get our axis past the edge
                        else
                        {
                            Vector3 lVerticalSegmentMovement = Vector3.Project(rSegmentMovement, lActorUp);
                            Vector3 lLateralSegmentMovement = rSegmentMovement - lVerticalSegmentMovement;

                            rSegmentMovement = rSegmentMovement + (lLateralSegmentMovement.normalized * (lLateralToHitPoint.magnitude + _SkinWidth));
                        }
                    }
                    // If we're not popping up, we need to check for collisions and adjust our movement so we 
                    // don't penetrate. Remaining movement will be handled next update.
                    else
                    {
                        // If there is a positive distance, this is the initial room that
                        // we have before we get to the collider.
                        Vector3 lPreCollisionMovement = Vector3.zero;
                        if (lBodyShapeHit.HitDistance > COLLISION_BUFFER - EPSILON)
                        {
                            // Move forward to the collision point
                            lPreCollisionMovement = rSegmentMovement.normalized * Mathf.Min(lBodyShapeHit.HitDistance - COLLISION_BUFFER, rSegmentMovement.magnitude);
                        }
                        // If there is a negative distance, we've penetrated the collider and
                        // we need to back up before we can continue.
                        else if (lBodyShapeHit.HitDistance < COLLISION_BUFFER + EPSILON)
                        {
                            // From the point on the body shape center/axis to the hit point
                            Vector3 lFromOrigin = lBodyShapeHit.HitPoint - lBodyShapeHit.HitOrigin;

                            // Pull back from the original position along the inverted collision vector (HitDistance is negative)
                            lPreCollisionMovement = lFromOrigin.normalized * (lBodyShapeHit.HitDistance - COLLISION_BUFFER);
                        }

                        // If we've hit a max angle, remove any upward push
                        if (lMovementHitAngle > (_MaxSlopeAngle > 0f ? _MaxSlopeAngle - 0.5f : MAX_GROUNDING_ANGLE) && lMovementHitAngle < 90f - EPSILON)
                        {
                            Vector3 lVerticalPreCollisionMovement = Vector3.Project(lPreCollisionMovement, lActorUp);
                            if (Vector3.Dot(lPreCollisionMovement, lActorUp) > 0f)
                            {
                                lPreCollisionMovement = lPreCollisionMovement - lVerticalPreCollisionMovement;
                                lPreCollisionMovement = Vector3.zero;
                            }
                        }

                        // Track the amount of remaining movement we can deflect
                        rRemainingMovement = rRemainingMovement + (rSegmentMovement - lPreCollisionMovement);

                        // Reset this segment's movement to what we discovered
                        rSegmentMovement = lPreCollisionMovement;
                    }

                    // After we reach the collider, this is the additional movement that needs to occur
                    if (rRemainingMovement.sqrMagnitude > 0f)
                    {
                        // Normally, the remaining movement will simply be used for the next segment. However,
                        // we need to ensure we're not being pushed into places we shouldn't go
                        if (mState.Ground != null && mState.Ground == lBodyShapeHit.HitCollider.transform)
                        {
                            // If we're moving in the direciton of the platform's movement we may not break
                            if (mState.MovementPlatformAdjust.sqrMagnitude > 0f && Vector3.Dot(rRemainingMovement.normalized, mState.MovementPlatformAdjust.normalized) > 0f)
                            {
                                // If our movement is less than the platform's movement
                                if (rRemainingMovement.sqrMagnitude < mState.MovementPlatformAdjust.sqrMagnitude + EPSILON)
                                {
                                    // TRT: Removing so we can collide with a platform structure (like the ship cabin)
                                    //lPreCollisionMovement = rSegmentMovement;
                                    //rRemainingMovement = Vector3.zero;
                                }
                            }
                        }

                        // If the angle we're dealing with is steeper than we can move, we want to treat it as a vertical wall

                        // Deflect the remaining movement based on what we hit
                        Vector3 lDeflectedRemainingSegmentMovement = rRemainingMovement - Vector3.Project(rRemainingMovement, lBodyShapeHit.HitNormal);

                        Vector3 lVerticalDeflectedRemainingSegmentMovement = Vector3.Project(lDeflectedRemainingSegmentMovement, lActorUp);
                        float lVerticalDeflectedRemainingSegmentMovementDot = Vector3.Dot(lVerticalDeflectedRemainingSegmentMovement.normalized, lActorUp);

                        // If we're already grounded, we don't want the remaining movement to push us into the ground
                        if (mState.IsGrounded && mState.IsGroundSurfaceDirect && lVerticalDeflectedRemainingSegmentMovementDot < 0f)
                        {
                            if (!lIsSlopePushingDown)
                            {
                                lDeflectedRemainingSegmentMovement = lDeflectedRemainingSegmentMovement - lVerticalDeflectedRemainingSegmentMovement;
                            }
                        }
                        // If we're getting pushed up by, we may need to stop it
                        //else if (mState.IsGrounded && lVerticalDeflectedRemainingSegmentMovementDot > 0f)
                        else if (lVerticalDeflectedRemainingSegmentMovementDot > 0f)
                        {
                            // If we've hit the max angle, remove an upward push
                            if (lMovementHitAngle > (_MaxSlopeAngle > 0f ? _MaxSlopeAngle - 0.5f : MAX_GROUNDING_ANGLE) && lMovementHitAngle < 90f - EPSILON)
                            {
                                // We do this by treating the steep slope like a wall. Remove any of it's 'up' value
                                Vector3 lVerticalMovementHitNormal = Vector3.Project(lMovementHitNormal, lActorUp);
                                if (Vector3.Dot(lVerticalMovementHitNormal.normalized, lActorUp) > 0f)
                                {
                                    lMovementHitNormal = (lMovementHitNormal - lVerticalMovementHitNormal).normalized;
                                }

                                lDeflectedRemainingSegmentMovement = rRemainingMovement - Vector3.Project(rRemainingMovement, lMovementHitNormal);
                                //lRemainingSegmentMovement = lRemainingSegmentMovement - lDeflectedVerticalRemainingSegmentMovement;
                            }
                        }
                        // If the collision stopped our forward movement, we probably want to stop the upward movement 
                        else if (mState.IsGrounded && mState.Ground != lBodyShapeHit.HitCollider.transform)
                        {
                            Vector3 lLateralMovement = lDeflectedRemainingSegmentMovement - lVerticalDeflectedRemainingSegmentMovement;
                            float lGroundSurfaceHitAngle = Vector3.Angle(lBodyShapeHit.HitNormal, mState.GroundSurfaceNormal);

                            // When the hit angle for the normals is < 90, that means the angle
                            // between the surfaces is > 90. When it's greater than 0, we're going up
                            // a ramp. If it's a head on collision, stop the upward ramp movement. This
                            // keeps us from stopping a jump against wall that is == 90 degrees.
                            if (lLateralMovement.sqrMagnitude < EPSILON_SQR && lGroundSurfaceHitAngle < 89f)
                            {
                                lDeflectedRemainingSegmentMovement = Vector3.zero;
                            }
                        }

                        rRemainingMovement = lDeflectedRemainingSegmentMovement;
                    }

                    // Update our grounding information so we can get correct normals
                    lIsGrounded = ProcessGrounding(_Transform.position + lPlatformMovement, rSegmentPositionDelta, lActorUp, mWorldUp, _BaseRadius, out lGroundHitInfo);
                    if (!lIsGrounded && mPrevState.IsSteppingDown && mState.GroundSurfaceDirectDistance < _MaxStepHeight && mState.GroundSurfaceDistance <= mPrevState.GroundSurfaceDistance)
                    {
                        lIsGrounded = true;
                        mState.IsGrounded = true;
                    }

                    // If we're already grounded, we don't want the movement to push us into the ground.
                    if (mState.IsGrounded && mState.IsGroundSurfaceDirect && rSegmentMovement.sqrMagnitude > 0f)
                    {
                        // Check if the movmeent is pushing us down
                        Vector3 lVerticalSegmentMovement = Vector3.Project(rSegmentMovement, lActorUp);
                        float lVerticalSegmentMovementDot = Vector3.Dot(lVerticalSegmentMovement, lActorUp);
                        //if (Vector3.Dot(lVerticalSegmentMovement.normalized, lActorUp) < 0f)
                        if (lVerticalSegmentMovementDot < -EPSILON)
                        {
                            // There are some exceptions (platforms and slopes)
                            if (Vector3.Dot(mState.MovementPlatformAdjust, lActorUp) >= 0f)
                            {
                                // Don't remove any downward movement due to sliding
                                Vector3 lVerticalSlideMovement = Vector3.Project(mState.MovementSlideAdjust, lActorUp);

                                // Determine the amount of vertical movement there is and counter it
                                rSegmentMovement = rSegmentMovement - (lVerticalSegmentMovement - lVerticalSlideMovement);

                                // Get rid of the remaining movement
                                rRemainingMovement = Vector3.zero;
                            }
                        }

                        // As we remove the vertical component, we may find that the remaining movement still
                        // pushes us into the collider. If so, we just want to stop.
                        float lDeltaAngle = Vector3.Angle(rRemainingMovement.normalized, rSegmentMovement.normalized);
                        if (lDeltaAngle < 0.1f)
                        {
                            if (!lIsSlopePushingUp || lBodyShapeHit.HitCollider.transform != mState.Ground)
                            {
                                rRemainingMovement = Vector3.zero;
                            }
                        }
                    }

                    // If we have a force pushing us up (like a jump), but we
                    // collide with something pushing us down, we want to cancel the force.
                    if (mState.MovementForceAdjust.sqrMagnitude > 0f)
                    {
                        float lBodyShapeHitUpDot = Vector3.Dot(lBodyShapeHit.HitNormal, lActorUp);
                        if (lBodyShapeHitUpDot < -EPSILON)
                        {
                            // TRT 3/27/16 - In the case of a perpendicular collision, we don't actually
                            // want it to stop our vertical movement.
                            float lHitDot = Vector3.Dot(lBodyShapeHit.Hit.normal, lActorUp);
                            if (Mathf.Abs(lHitDot) > EPSILON)
                            {
                                Vector3 lVerticalAccumulatedGravity = Vector3.Project(mAccumulatedVelocity, mWorldUp);
                                if (Vector3.Dot(lVerticalAccumulatedGravity.normalized, mWorldUp) > 0f)
                                {
                                    mAccumulatedVelocity = Vector3.zero;
                                }
                            }
                        }
                    }
                }

                // Reset the rotation
                _Transform.rotation = lCurrentRotation;
            }

            // Check if the remaining movement causes us to bounce back against our movement direction. If so, we'll stop
            // so that we don't get stuttering.
            Vector3 lRemainingSegmentMovementProj = Vector3.Project(rRemainingMovement, mState.Movement.normalized);
            if (Vector3.Dot(lRemainingSegmentMovementProj.normalized, mState.Movement.normalized) < 0f)
            {
                // We'll keep ther vertical component only
                rRemainingMovement = Vector3.Project(rRemainingMovement, mWorldUp);
            }

            // ----------------------------------------------------------------------
            // Determine the tilting based on the ground.
            // ----------------------------------------------------------------------
            if (rOrientToGround)
            {
                //Log.FileWrite("PM() orient to ground nml:" + StringHelper.ToString(mState.GroundSurfaceNormal));
                if (_OrientToGround || mPrevState.IsTilting || mState.IsTilting)
                {
                    mOrientToGroundNormalTarget = Vector3.up;

                    if (_OrientToGround)
                    {
                        // If we're told to go do a specific normal, do it
                        if (mTargetGroundNormal.sqrMagnitude > 0f)
                        {
                            mOrientToGroundNormalTarget = mTargetGroundNormal;
                        }
                        // If we're "keeping" orientation or NOT jumping, stay with the surface direction
                        else if (lIsGrounded || _KeepOrientationInAir || (mAccumulatedForceVelocity.sqrMagnitude == 0f && mState.GroundSurfaceDistance < _OrientToGroundDistance))
                        {
                            if (_MaxSlopeAngle == 0f || mState.GroundSurfaceAngle < _MaxSlopeAngle - 0.5f)
                            {
                                mOrientToGroundNormalTarget = mState.GroundSurfaceDirectNormal;
                            }
                        }
                    }

                    // Determine the final tilt
                    Vector3 lTiltUp = mTilt.Up();

                    mOrientToGroundNormal = mOrientToGroundNormalTarget;
                    mTilt = QuaternionExt.FromToRotation(lTiltUp, mOrientToGroundNormal) * mTilt;
                }
            }

            return true;
        }

        /// <summary>
        /// Used to determine if the actor is on a surface (ground) or not. This is a simple 
        /// test function that primarily focuses on gathering angles and such.
        /// </summary>
        /// <param name="rActorPosition">Position of the actor</param>
        /// <param name="rOffset">Any position offset</param>
        /// <param name="rActorUp">Up vector of the actor</param>
        /// <param name="rWorldUp">Up vector ofthe world</param>
        /// <param name="rGroundRadius">Radius for the sphere cast</param>
        /// <param name="rGroundHitInfo">RaycastHit information</param>
        /// <returns>Determines if the actor is grounded or not</returns>
        protected bool ProcessGrounding(Vector3 rActorPosition, Vector3 rOffset, Vector3 rActorUp, Vector3 rWorldUp, float rGroundRadius, out RaycastHit rGroundHitInfo)
        {
            Vector3 lRayStart = rActorPosition + rOffset + (rActorUp * _GroundingStartOffset);
            Vector3 lRayDirection = -rActorUp;
            float lRayDistance = _GroundingStartOffset + _GroundingDistance;

            // Start with a simple ray. This would be the object directly under the actor
            bool lIsGrounded = false;

            if (_IsGroundingLayersEnabled)
            {
                lIsGrounded = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out rGroundHitInfo, lRayDistance, _GroundingLayers, _Transform, mIgnoreTransforms);
            }
            else
            {
                lIsGrounded = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out rGroundHitInfo, lRayDistance, -1, _Transform, mIgnoreTransforms);
            }

            if (lIsGrounded)
            {
                lIsGrounded = (rGroundHitInfo.distance - _GroundingStartOffset < _SkinWidth + EPSILON);

                // Whether we're grounded or not, return the ground information
                mState.Ground = rGroundHitInfo.collider.gameObject.transform;
                mState.GroundPosition = mState.Ground.position;
                mState.GroundRotation = mState.Ground.rotation;

                mState.GroundSurfaceAngle = Vector3.Angle(rGroundHitInfo.normal, rActorUp);
                mState.GroundSurfaceNormal = rGroundHitInfo.normal;
                mState.GroundSurfaceDistance = rGroundHitInfo.distance - _GroundingStartOffset;
                mState.GroundSurfacePoint = rGroundHitInfo.point;
                mState.GroundSurfaceDirection = lRayDirection;

                mState.IsGroundSurfaceDirect = true;
                mState.GroundSurfaceDirectNormal = mState.GroundSurfaceNormal;
                mState.GroundSurfaceDirectDistance = mState.GroundSurfaceDistance;

                // Just in case we need to shoot the sphere cast
                lRayDistance = rGroundHitInfo.distance + rGroundRadius;
            }
            else
            {
                // We're not grounded, but we may not want to revert to the previous normal
                if (_KeepOrientationInAir)
                {
                    mState.GroundSurfaceNormal = mPrevState.GroundSurfaceNormal;
                    mState.GroundSurfaceDirectNormal = mPrevState.GroundSurfaceDirectNormal;
                }
            }

            // If we aren't grounded, do a sphere test
            if (!lIsGrounded)
            {
                bool lIgnore = false;
                Vector3 lClosestPoint = rGroundHitInfo.point;

                // Test if there's additional colliders to support us. We increate the ground radius
                // to reachout to points that are at our diagonal.
                lRayStart = rActorPosition + rOffset + (rActorUp * rGroundRadius);

                int lHits = 0;
                Collider[] lColliders = null;

                if (_IsGroundingLayersEnabled)
                {
                     lHits = RaycastExt.SafeOverlapSphere(lRayStart, rGroundRadius * ONE_OVER_COS45, out lColliders, _GroundingLayers, _Transform, mIgnoreTransforms);
                }
                else
                {
                    lHits = RaycastExt.SafeOverlapSphere(lRayStart, rGroundRadius * ONE_OVER_COS45, out lColliders, -1, _Transform, mIgnoreTransforms);
                }

                // With one or no colliders, this is easy
                if (lColliders == null || lHits == 0)
                {
                    lIgnore = true;
                    lIsGrounded = false;
                }
                // With one collider, we just test if it's close enough to ground us.
                // If not, we need to slide down... hence we're not grounded.
                else if (lHits == 1)
                {
                    lClosestPoint = GeometryExt.ClosestPoint(lRayStart, lColliders[0]);
                    if (lClosestPoint.sqrMagnitude > 0f)
                    {
                        Vector3 lLocalOrbitPoint = _Transform.InverseTransformPoint(lClosestPoint - rOffset);
                        lLocalOrbitPoint.y = 0f;

                        // If any of our hit points are super close to the root, we have to consider 
                        // ourselves grounded.  This just keeps us safe
                        if (lLocalOrbitPoint.magnitude > (rGroundRadius * 0.25f))
                        {
                            float lMaxAngle = (_MaxSlopeAngle > 0f ? _MaxSlopeAngle - 0.5f : MAX_GROUNDING_ANGLE);

                            // The problem we have is if we're on a steep slope that isn't too steep.
                            // The hit will be outside of our desired range. So, we need to test.
                            if (rGroundHitInfo.collider == lColliders[0] && mState.GroundSurfaceAngle < lMaxAngle + EPSILON)
                            {
                                lIgnore = false;
                                lIsGrounded = true;
                            }
                            else
                            {
                                lIgnore = true;
                                lIsGrounded = false;
                            }
                        }
                    }
                }
                // With more than one colliders, we need to see if they are spread out enough to
                // support the object. We do this by gathering "orbit points". If there's enough
                // angular difference between the points, we know we have supports. Otherwise, the
                // supports are all one one side and we still slide down.
                else
                {
                    bool lTestOrbitAngles = true;

                    int lOrbitAnglesCount = 0;
                    Vector3[] lOrbitPoints = new Vector3[lHits];
                    Vector3[] lLocalOrbitPoints = new Vector3[lHits];
                    float[] lOrbitAngles = new float[lHits];

                    // First, gather all the points. Imagine we're orbiting the ray start and
                    // collecting all the hit points. We want the angular difference between them.
                    for (int i = 0; i < lHits; i++)
                    {
                        if (lColliders[i] == rGroundHitInfo.collider) { continue; }

                        Vector3 lOrbitPoint = GeometryExt.ClosestPoint(lRayStart, lColliders[i]);
                        if (lOrbitPoint.sqrMagnitude == 0f) { continue; }

                        Vector3 lLocalOrbitPoint = _Transform.InverseTransformPoint(lOrbitPoint - rOffset);

                        // If the hit is too high, we won't count it as grounding
                        if (lLocalOrbitPoint.y >= rGroundRadius + EPSILON)
                        {
                            continue;
                        }

                        // If the hit it too low (meaning too far way, we won't count it as grounding
                        if (-lLocalOrbitPoint.y > _SkinWidth + EPSILON)
                        {
                            continue;
                        }

                        // Since we're in the safe zone, figure out the lateral distance
                        lLocalOrbitPoint.y = 0f;

                        // If any of our hit points are super close to the root, we have to consider 
                        // ourselves grounded.  This just keeps us safe
                        if (lLocalOrbitPoint.magnitude < (rGroundRadius * 0.25f))
                        {
                            lClosestPoint = lOrbitPoint;

                            lIsGrounded = true;
                            lTestOrbitAngles = false;
                            break;
                        }

                        // We're now far enough away to think about the orbit angle
                        lOrbitPoints[lOrbitAnglesCount] = lOrbitPoint;
                        lLocalOrbitPoints[lOrbitAnglesCount] = lLocalOrbitPoint;
                        lOrbitAngles[lOrbitAnglesCount] = Vector3Ext.SignedAngle(lLocalOrbitPoint.normalized, _Transform.forward);
                        lOrbitAnglesCount++;
                    }

                    // With the hit points, we can grab the angular difference between them and see if
                    // they are far enough appart to create supports.
                    if (lTestOrbitAngles)
                    {
                        if (lOrbitAnglesCount > 0)
                        {
                            lIgnore = true;
                            for (int i = 0; i < lOrbitAnglesCount; i++)
                            {
                                for (int j = i + 1; j < lOrbitAnglesCount; j++)
                                {
                                    float lLocalAngle = Vector3.Angle(lLocalOrbitPoints[i], lLocalOrbitPoints[j]);
                                    if (lLocalAngle > 60f)
                                    {
                                        lClosestPoint = lOrbitPoints[i];
                                        if (Vector3.SqrMagnitude(lOrbitPoints[j] - lRayStart) < Vector3.SqrMagnitude(lClosestPoint - lRayStart))
                                        {
                                            lClosestPoint = lOrbitPoints[j];
                                        }

                                        lIgnore = false;
                                        lIsGrounded = true;

                                        break;
                                    }
                                }

                                if (!lIgnore) { break; }
                            }
                        }
                        else
                        {
                            lIgnore = true;
                        }
                    }

                    // Clean up based on the results
                    if (lIgnore)
                    {
                        lIsGrounded = false;
                    }
                }

                // "Ignoring" means that we DON'T consider this hit a grounding event and we'll
                // essencially slide off the edge. The collision detection part will cause that to happen.
                //
                // If we are "Not Ignoring", that means we need to consider this edge a grounding event
                // and we don't want to slide. This is important if we're over something like a gap.
                if (!lIgnore)
                {
                    Vector3 lLocalOrbitPoint = _Transform.InverseTransformPoint(lClosestPoint - rOffset);

                    Vector3 lToClosestPoint = lClosestPoint - lRayStart;
                    lRayDirection = lToClosestPoint.normalized;
                    lRayDistance = lToClosestPoint.magnitude + _SkinWidth;

                    RaycastHit lRaycastHit;

                    bool lGroundHit = false;
                    if (_IsGroundingLayersEnabled)
                    {
                        lGroundHit = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lRaycastHit, lRayDistance, _GroundingLayers, _Transform, mIgnoreTransforms);
                    }
                    else
                    {
                        lGroundHit = RaycastExt.SafeRaycast(lRayStart, lRayDirection, out lRaycastHit, lRayDistance, -1, _Transform, mIgnoreTransforms);
                    }

                    if (lGroundHit)
                    {
                        float lGroundDistance = lRaycastHit.distance - rGroundRadius;
                        lIsGrounded = lGroundDistance < (_SkinWidth + EPSILON) * ONE_OVER_COS45;

                        mState.Ground = lRaycastHit.collider.gameObject.transform;
                        mState.GroundPosition = mState.Ground.position;
                        mState.GroundRotation = mState.Ground.rotation;

                        mState.GroundSurfaceAngle = Vector3.Angle(lRaycastHit.normal, rActorUp);
                        mState.GroundSurfaceNormal = lRaycastHit.normal;
                        mState.GroundSurfaceDistance = -lLocalOrbitPoint.y;
                        mState.GroundSurfacePoint = lClosestPoint;
                        mState.GroundSurfaceDirection = lRayDirection;
                        mState.IsGroundSurfaceDirect = false;

                        rGroundHitInfo = lRaycastHit;
                    }
                }
            }

            // Set the final grounding state
            mState.IsGrounded = lIsGrounded;

            // Debug info
            //DebugDraw.DrawSphereMesh(rGroundHitInfo.point, 0.02f, (lIsGrounded ? Color.green : Color.yellow), 1f);

            // Return the grounded value
            return lIsGrounded;
        }

        /// <summary>
        /// Grab the acceleration to use in our movement
        /// </summary>
        /// <param name="rDeltaTime">Delta time to be used with the forces</param>
        /// <returns>The sum of our forces</returns>
        protected Vector3 ProcessForces(float rDeltaTime)
        {
            Vector3 lAcceleration = Vector3.zero;

            // Apply each force
            if (mAppliedForces != null)
            {
                for (int i = mAppliedForces.Count - 1; i >= 0; i--)
                {
                    Force lForce = mAppliedForces[i];
                    if (lForce.StartTime == 0f) { lForce.StartTime = Time.time; }

                    // If the force is no longer valid, remove it
                    if (lForce.Value.sqrMagnitude == 0f)
                    {
                        mAppliedForces.RemoveAt(i);
                        Force.Release(lForce);
                    }
                    // If the force has started, look to apply it
                    else if (lForce.StartTime <= Time.time)
                    {
                        // For an impulse, apply it and remove it
                        if (lForce.Type == ForceMode.Impulse)
                        {
                            lAcceleration += (lForce.Value / _Mass);

                            mAppliedForces.RemoveAt(i);
                            Force.Release(lForce);
                        }
                        // Determine if the force has expired
                        else if (lForce.Duration > 0f && lForce.StartTime + lForce.Duration < Time.time)
                        {
                            mAppliedForces.RemoveAt(i);
                            Force.Release(lForce);
                        }
                        // Since it hasn't expired, apply it
                        else
                        {
                            lAcceleration += (lForce.Value / _Mass);
                        }
                    }
                }
            }

            return lAcceleration;
        }

        /// <summary>
        /// Determines if we're on a slope and we need to slide
        /// </summary>
        /// <param name="rMovement">Movement we want to occur</param>
        /// <param name="rState">Current state we're working with</param>
        /// <param name="rActorUp">Actor's up vector</param>
        /// <param name="rGravity">Gravity being applied</param>
        /// <param name="rDeltaTime">Current delta time</param>
        protected void ProcessSlope(Vector3 rMovement, ActorState rState, Vector3 rActorUp, Vector3 rGravity, float rDeltaTime)
        {
            if (_MinSlopeAngle > 0f && rState.GroundSurfaceAngle < _MinSlopeAngle) { return; }

            // As we hit corners, we sometimes get these odd angles from the raycast. Just
            // in case, we'll exit if we're getting the side normal.
            if (rState.GroundSurfaceAngle >= 85f) { return; }

            // Determine the gravitation component on the slope
            Vector3 lSlopeRight = Vector3.Cross(rState.GroundSurfaceNormal, rGravity.normalized);
            if (lSlopeRight.sqrMagnitude == 0f) { lSlopeRight = Vector3.Cross(rState.GroundSurfaceNormal, -rActorUp); }

            Vector3 lSlopeDirection = Vector3.Cross(lSlopeRight, rState.GroundSurfaceNormal);

            // Counteract the movement along the slide direction
            if (_MaxSlopeAngle > 0f && rState.GroundSurfaceAngle > _MaxSlopeAngle - 0.5f)
            {
                // This part is a bit of a hack. But, if we end up on a very small patch of
                // ground whose angle is too steep, but the bit just beyond it isn't. We'll ignore it.
                // We do this because sometimes there are corners we may run into and we don't want
                // them to bump us.
                if (rMovement.sqrMagnitude > EPSILON)
                {
                    RaycastHit lForwardGroundHitInfo;
                    if (TestGrounding(_Transform.position + (rMovement.normalized * 0.3f), rActorUp, mWorldUp, out lForwardGroundHitInfo))
                    {
                        float lAngle = Vector3.Angle(lForwardGroundHitInfo.normal, mWorldUp);
                        if (lAngle < _MaxSlopeAngle - EPSILON)
                        {
                            return;
                        }
                    }
                }

                // If the area beyond is still steep, slide
                rState.MovementSlideAdjust = lSlopeDirection * (rGravity.magnitude * rDeltaTime);
            }
            // Slide down the slope if it's at the right angle
            else if (_MinSlopeAngle > 0f && rState.GroundSurfaceAngle >= _MinSlopeAngle)
            {
                float lGravityCoefficent = _MinSlopeGravityCoefficient;

                // Instead of a flat coefficient, we'll ramp it 
                // based on the current ground angle as it falls
                // the min (x0) and max (x1) slope angle
                if (_MaxSlopeAngle > _MinSlopeAngle)
                {
                    float lSlopeAngleRange = _MaxSlopeAngle - _MinSlopeAngle;
                    float lSlopeAngleValue = rState.GroundSurfaceAngle - _MinSlopeAngle;
                    lGravityCoefficent *= Mathf.Min((lSlopeAngleValue / lSlopeAngleRange) + 0.1f, 1f);
                }

                rState.MovementSlideAdjust = Vector3.Project(rGravity * lGravityCoefficent, lSlopeDirection) * rDeltaTime;
            }
        }

        /// <summary>
        /// If we're on an object, we need to use it's position and rotation as a basis for our actor
        /// </summary>
        /// <param name="rActorPosition">Actor's current position</param>
        /// <param name="rActorUp">Actor's up vector</param>
        /// <param name="rMovement">Desired movement</param>
        /// <param name="rState">Current state</param>
        /// <param name="rPrevState">Previous state</param>
        protected void ProcessPlatforming(Vector3 rActorPosition, Vector3 rActorUp, Vector3 rMovement, ActorState rPrevState)
        {
            Transform lGround = rPrevState.Ground;
            bool lIsGrounded = rPrevState.IsGrounded;

            if (mTargetGround != null)
            {
                lIsGrounded = true;
                lGround = mTargetGround;
            }

            if (lGround == null) { return; }

            if (rPrevState.GroundLocalContactPoint.sqrMagnitude == 0f) { return; }

            Vector3 lGroundTranslate = Vector3.zero;
            //Vector3 lGroundOrbit = Vector3.zero;
            Vector3 lGroundMove = Vector3.zero;
            Quaternion lGroundRotate = Quaternion.identity;

            // Since we are on the same ground process movement
            if (lGround == rPrevState.PrevGround)
            {
                // Test if the platform as moved
                lGroundTranslate = lGround.position - rPrevState.GroundPosition;

                // Test if the platform has rotated
                if (!QuaternionExt.IsEqual(lGround.rotation, rPrevState.GroundRotation))
                {
                    // Rotate the avatar
                    lGroundRotate = Quaternion.Inverse(rPrevState.GroundRotation) * lGround.rotation;

                    // Find the difference between the last orbit and this one. This will remove
                    // any constant offset the local contact point has from the support's center
                    //Vector3 lPrevOrbit = rPrevState.GroundRotation * rPrevState.GroundLocalContactPoint;
                    //Vector3 lCurrentOrbit = lGround.rotation * rPrevState.GroundLocalContactPoint;
                    //lGroundOrbit = (lCurrentOrbit - lPrevOrbit);
                }

                //lGroundMove = lGroundTranslate + lGroundOrbit;

                if (lGroundTranslate.sqrMagnitude > 0f || !QuaternionExt.IsIdentity(lGroundRotate))
                {
                    Vector3 lNewPosition = lGround.TransformPoint(rPrevState.GroundLocalContactPoint);
                    lGroundMove = (lNewPosition - _Transform.position) - rMovement;

                    if (lGroundMove.sqrMagnitude > 0f)
                    {
                        //float lGroundSurfaceDistance = mPrevState.GroundSurfaceDistance;

                        // If we get here, we need to find the distance to the ground
                        //RaycastHit lGroundHitInfo;
                        //if (TestGrounding(rActorPosition + lGroundMove, rActorUp, rActorUp, out lGroundHitInfo))
                        //{
                        //    if (lGroundHitInfo.collider.transform == mPrevState.Ground)
                        //    {
                        //        lGroundSurfaceDistance = lGroundHitInfo.distance;
                        //    }
                        //}

                        // Determine how the platform is moving. 
                        Vector3 lVerticalGroundMoveProj = Vector3.Project(lGroundMove, rActorUp);
                        float lVerticalGroundMoveDot = Vector3.Dot(lVerticalGroundMoveProj, rActorUp);
                        if (lVerticalGroundMoveDot > 0f)
                        {
                            // If it's moving up and we're penetrating it, simply add enough movement that gets
                            // us onto the platform.
                            if (mState.GroundSurfaceDistance < 0f)
                            {
                                //lGroundMove = lGroundMove - lVerticalGroundMoveProj + (rActorUp * (-lGroundSurfaceDistance + _SkinWidth));
                            }
                        }
                        else if (lVerticalGroundMoveDot < 0f)
                        {
                            //If the platform is moving down, we may need to push ourselves down to the platform
                            if (!lIsGrounded && rPrevState.IsGrounded)
                            {
                                // Since there's already gravity attached, we need to take that into account
                                //Vector3 lVerticalMovementProj = Vector3.Project(rMovement, rActorUp);
                                //float lVerticalMovementDot = Vector3.Dot(lVerticalMovementProj, rActorUp);
                                //float lVerticalMovement = (lVerticalMovementDot < 0f ? lVerticalMovementProj.magnitude : 0f);

                                lIsGrounded = true;
                                //lGroundMove = lGroundMove - lVerticalGroundMoveProj + (rActorUp * -Mathf.Max(lGroundSurfaceDistance - _SkinWidth - lVerticalMovement, 0f));
                            }
                        }
                    }
                }
            }

            // If we're still not grounded an there was not adjusted move, we must simply need to stop
            if (lIsGrounded)
            {
                mYaw = mYaw * lGroundRotate;

                mState.RotationPlatformAdjust = lGroundRotate;
                mState.MovementPlatformAdjust = lGroundMove;
            }
        }

        /// <summary>
        /// Processes the shapes and store thier definitions so we can deserialize later
        /// </summary>
        public void SerializeBodyShapes()
        {
            mBodyShapeDefinitions.Clear();

            for (int i = 0; i < BodyShapes.Count; i++)
            {
                string lDefinition = BodyShapes[i].Serialize();
                mBodyShapeDefinitions.Add(lDefinition);
            }
        }

        /// <summary>
        /// Processes the definitions and updates the shapes to match.
        /// </summary>
        public void DeserializeBodyShapes()
        {
            int lBodyShapeCount = BodyShapes.Count;
            int lBodyShapeDefCount = mBodyShapeDefinitions.Count;

            // First, remove any extra motions that may exist
            for (int i = lBodyShapeCount - 1; i > lBodyShapeDefCount; i--)
            {
                BodyShapes.RemoveAt(i);
            }

            // We need to match the motion definitions to the motions
            for (int i = 0; i < lBodyShapeDefCount; i++)
            {
                string lDefinition = mBodyShapeDefinitions[i];
                JSONNode lDefinitionNode = JSONNode.Parse(lDefinition);
                if (lDefinitionNode == null) { continue; }

                BodyShape lBodyShape = null;
                string lTypeString = lDefinitionNode["Type"].Value;

                Type lType = Type.GetType(lTypeString);
                if (lType == null) { continue; }

                // If don't have a motion matching the type, we need to create one
                if (BodyShapes.Count <= i || lTypeString != BodyShapes[i].GetType().AssemblyQualifiedName)
                {
                    lBodyShape = Activator.CreateInstance(lType) as BodyShape;
                    if (BodyShapes.Count <= i)
                    {
                        BodyShapes.Add(lBodyShape);
                    }
                    else
                    {
                        BodyShapes[i] = lBodyShape;
                    }
                }
                // Grab the matching motion
                else
                {
                    lBodyShape = BodyShapes[i];
                }

                // Fill the motion with data from the definition
                if (lBodyShape != null)
                {
                    lBodyShape._Parent = transform;
                    lBodyShape._CharacterController = this;
                    lBodyShape.Deserialize(lDefinition);
                }
            }
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

        // Stores the index in our list
        public int EditorBodyShapeIndex = 0;

        public bool EditorShowAdvanced = false;

        public bool EditorCollideWithObjects = true;

        public bool EditorWalkOnWalls = false;

        public bool EditorSlideOnSlopes = false;

        public bool EditorRespondToColliders = false;


#if UNITY_EDITOR

        /// <summary>
        /// Allows us to draw to the editor
        /// </summary>
        public void OnSceneGUI()
        {
            Color lHandleColor = Handles.color;

            if (_Transform == null) { _Transform = transform; }

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);

            Vector3 lPosition = _Transform.position + (_Transform.rotation * _OverlapCenter);
            Handles.DrawWireArc(lPosition, _Transform.forward, _Transform.up, 360f, _OverlapRadius);
            Handles.DrawWireArc(lPosition, _Transform.up, _Transform.forward, 360f, _OverlapRadius);
            Handles.DrawWireArc(lPosition, _Transform.right, _Transform.up, 360f, _OverlapRadius);

            Handles.color = new Color(0f, 1f, 0f, 0.5f);

            for (int i = 0; i < BodyShapes.Count; i++)
            {
                if (BodyShapes[i]._Parent == null)
                {
                    BodyShapes[i]._Parent = _Transform;
                }

                if (BodyShapes[i] is BodySphere)
                {
                    BodySphere lShape = BodyShapes[i] as BodySphere;

                    float lRadius = lShape._Radius;
                    Transform lTransform = (lShape._Transform != null ? lShape._Transform : _Transform);
                    lPosition = lTransform.position + (lTransform.rotation * lShape._Offset);

                    Handles.DrawWireArc(lPosition, lTransform.forward, lTransform.up, 360f, lRadius);
                    Handles.DrawWireArc(lPosition, lTransform.up, lTransform.forward, 360f, lRadius);
                    Handles.DrawWireArc(lPosition, lTransform.right, lTransform.up, 360f, lRadius);
                    Handles.SphereCap(0, lPosition, Quaternion.identity, 0.05f);
                }
                else if (BodyShapes[i] is BodyCapsule)
                {
                    BodyCapsule lShape = BodyShapes[i] as BodyCapsule;

                    float lRadius = lShape._Radius;

                    lPosition = (lShape._Transform == null ? lShape._Parent.position + (lShape._Parent.rotation * lShape._Offset) : lShape._Transform.position + (lShape._Transform.rotation * lShape._Offset));

                    Vector3 lEndPosition = (lShape._EndTransform == null ? lShape._Parent.position + (lShape._Parent.rotation * lShape._EndOffset) : lShape._EndTransform.position + (lShape._EndTransform.rotation * lShape._EndOffset));

                    //Transform lTransform = (lShape._Transform != null ? lShape._Transform : _Transform);
                    //Vector3 lPosition = lTransform.position + (lTransform.rotation * lShape._Offset);

                    //Transform lEndTransform = (lShape._EndTransform != null ? lShape._EndTransform : _Transform);
                    //Vector3 lEndPosition = lEndTransform.position + (lEndTransform.rotation * lShape._EndOffset);

                    Vector3 lDirection = (lEndPosition - lPosition).normalized;
                    Quaternion lRotation = (lDirection.sqrMagnitude == 0f ? Quaternion.identity : Quaternion.LookRotation(lDirection, _Transform.up));

                    Vector3 lForward = lRotation * Vector3.forward;
                    Vector3 lRight = lRotation * Vector3.right;
                    Vector3 lUp = lRotation * Vector3.up;

                    Handles.DrawWireArc(lPosition, lForward, lUp, 360f, lRadius);
                    Handles.DrawWireArc(lPosition, lUp, lRight, 180f, lRadius);
                    Handles.DrawWireArc(lPosition, lRight, -lUp, 180f, lRadius);

                    Handles.DrawWireArc(lEndPosition, lForward, lUp, 360f, lRadius);
                    Handles.DrawWireArc(lEndPosition, lUp, -lRight, 180f, lRadius);
                    Handles.DrawWireArc(lEndPosition, lRight, lUp, 180f, lRadius);

                    Handles.DrawLine(lPosition + (lRight * lRadius), lEndPosition + (lRight * lRadius));
                    Handles.DrawLine(lPosition + (-lRight * lRadius), lEndPosition + (-lRight * lRadius));
                    Handles.DrawLine(lPosition + (lUp * lRadius), lEndPosition + (lUp * lRadius));
                    Handles.DrawLine(lPosition + (-lUp * lRadius), lEndPosition + (-lUp * lRadius));

                    Handles.SphereCap(0, lPosition, Quaternion.identity, 0.025f);
                    Handles.SphereCap(0, lEndPosition, Quaternion.identity, 0.025f);
                }
            }

            if (mState.IsColliding)
            {
                Handles.color = Color.red;
                Handles.SphereCap(0, mState.ColliderHit.point, Quaternion.identity, 0.025f);
                Handles.DrawLine(mState.ColliderHit.point, mState.ColliderHit.point + mState.ColliderHit.normal);

                Handles.color = Color.magenta;
                Handles.SphereCap(0, mState.ColliderHitOrigin, Quaternion.identity, 0.025f);
            }

            Handles.color = lHandleColor;
        }

#endif
    }
}
