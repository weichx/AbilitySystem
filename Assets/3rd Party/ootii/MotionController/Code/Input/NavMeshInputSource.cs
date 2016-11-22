using UnityEngine;
using com.ootii.Actors;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Timing;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace com.ootii.Input
{
    /// <summary>
    /// Used to manage information between the MC and NavMeshAgent that
    /// will control the MC.
    /// </summary>
    [AddComponentMenu("ootii/Input Sources/Nav Mesh Input Source")]
    public class NavMeshInputSource : UnityInputSource
    {
        /// <summary>
        /// Provides an error amount for determining distance
        /// </summary>
        public const float EPSILON = 0.01f;

        /// <summary>
        /// Determines if the driver is enabled
        /// </summary>
        public override bool IsEnabled
        {
            get { return _IsEnabled; }

            set
            {
                if (_IsEnabled && !value)
                {
                    if (mIsTargetSet)
                    {
                        mNavMeshAgent.Stop();
                    }
                }
                else if (!_IsEnabled && value)
                {
                    if (mIsTargetSet)
                    {
                        SetDestination(_TargetPosition);
                    }
                }

                _IsEnabled = value;
            }
        }

        /// <summary>
        /// Target we're moving towards
        /// </summary>
        public Transform _Target = null;
        public Transform Target
        {
            get { return _Target; }

            set
            {
                _Target = value;
                if (_Target == null)
                {
                    mNavMeshAgent.Stop();
                    mHasArrived = false;
                    mIsInSlowDistance = false;

                    mIsTargetSet = false;
                    _TargetPosition = Vector3Ext.Null;
                }
                else
                {
                    mIsTargetSet = true;
                    _TargetPosition = _Target.position;
                }
            }
        }

        /// <summary>
        /// Target we're moving towards
        /// </summary>
        public Vector3 _TargetPosition = Vector3.zero;
        public Vector3 TargetPosition
        {
            get { return _TargetPosition; }

            set
            {
                _Target = null;
                _TargetPosition = value;

                if (_TargetPosition == Vector3Ext.Null)
                {
                    mNavMeshAgent.Stop();
                    mHasArrived = false;
                    mIsInSlowDistance = false;

                    mIsTargetSet = false;
                }
                else
                {
                    mIsTargetSet = true;
                }
            }
        }

        /// <summary>
        /// Determines how far from the destination we'll consider
        /// us to have arrived
        /// </summary>
        public float _StopDistance = 0.5f;
        public float StopDistance
        {
            get { return _StopDistance; }
            set { _StopDistance = value; }
        }

        /// <summary>
        /// Distance we'll use to start slowing down so we can arrive nicely.
        /// </summary>
        public float _SlowDistance = 4.0f;
        public float SlowDistance
        {
            get { return _SlowDistance; }
            set { _SlowDistance = value; }
        }

        /// <summary>
        /// Speed we'll ultimately reduce to before stopping
        /// </summary>
        public float _SlowFactor = 0.0f;
        public float SlowFactor
        {
            get { return _SlowFactor; }
            set { _SlowFactor = value; }
        }

        /// <summary>
        /// Determines if button presses and action aliaces will be returned
        /// through this input source
        /// </summary>
        public bool _AllowUserInput = false;
        public bool AllowUserInput
        {
            get { return _AllowUserInput; }
            set { _AllowUserInput = value; }
        }

        /// <summary>
        /// Height of the path from the actual navmesh surface. This is
        /// This height is added to the path by unity
        /// </summary>
        public float _PathHeight = 0.05f;
        public float PathHeight
        {
            get { return _PathHeight; }
            set { _PathHeight = value; }
        }

        /// <summary>
        /// Determines if a target is currently set
        /// </summary>
        protected bool mIsTargetSet = false;
        public bool IsTargetSet
        {
            get { return mIsTargetSet; }
        }

        /// <summary>
        /// Determines if we've arrived at the final destination
        /// </summary>
        protected bool mHasArrived = false;
        public bool HasArrived
        {
            get { return mHasArrived; }
        }

        /// <summary>
        /// Set when we're within the slow distance
        /// </summary>
        protected bool mIsInSlowDistance = false;
        public bool IsInSlowDistance
        {
            get { return IsInSlowDistance; }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        protected float mInputFromCameraAngle = 0f;
        public override float InputFromCameraAngle
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mInputFromCameraAngle;
            }

            set { }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        protected float mInputFromAvatarAngle = 0f;
        public override float InputFromAvatarAngle
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mInputFromAvatarAngle;
            }

            set { }
        }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        protected float mMovementX = 0f;
        public override float MovementX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mMovementX;
            }
        }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        protected float mMovementY = 0f;
        public override float MovementY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mMovementY;
            }
        }

        /// <summary>
        /// Determine if we can enable viewing
        /// </summary>
        public override bool IsViewingActivated
        {
            get
            {
                return mViewX != 0f;
            }
        }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        protected float mViewX = 0f;
        public override float ViewX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mViewX;
            }
        }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        protected float mViewY = 0f;
        public override float ViewY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return mViewY;
            }
        }

        /// <summary>
        /// Desired degrees of rotation per second
        /// </summary>
        public float _MaxViewSpeed = 1f;
        public float MaxViewSpeed
        {
            get { return _MaxViewSpeed; }

            set
            {
                _MaxViewSpeed = value;
                mViewSpeedPer60FPSTick = _MaxViewSpeed * 60f;
            }
        }

        /// <summary>
        /// Transform of our game object
        /// </summary>
        protected Transform _Transform = null;

        /// <summary>
        /// NavMeshAgent we'll use to help manage the AI based
        /// navigation of the actor.
        /// </summary>
        protected NavMeshAgent mNavMeshAgent = null;

        /// <summary>
        /// Cache the actor controller
        /// </summary>
        protected ActorController mActorController = null;

        /// <summary>
        /// Cache the motion controller
        /// </summary>
        protected MotionController mMotionController = null;

        /// <summary>
        /// Next position as reported by the agent
        /// </summary>
        protected Vector3 mWaypoint = Vector3.zero;

        /// <summary>
        /// Destination that is set on the NMA. This will
        /// equal _TargetPosition once it's set on the NMA.
        /// </summary>
        protected Vector3 mAgentDestination = Vector3.zero;

        /// <summary>
        /// Speed we'll actually apply to the view component. This is essencially the
        /// value tick assuming we're running at 60 FPS
        /// </summary>
        protected float mViewSpeedPer60FPSTick = 1f;

        /// <summary>
        /// Direction we're traveling to
        /// </summary>
        protected Vector3 mTargetVector = Vector3.zero;

        /// <summary>
        /// Distance between the current position and actual target
        /// </summary>
        protected float mTargetDistance = 0f;

        /// <summary>
        /// Diection we're traveling to
        /// </summary>
        protected Vector3 mWaypointVector = Vector3.zero;

        /// <summary>
        /// Distance between the current position and temporary target
        /// </summary>
        protected float mWaypointDistance = 0f;

        /// <summary>
        /// Determines if we have a valid waypoint path. This really only watches for
        /// the first valid path. After that, the path can change a lot if the target moves
        /// </summary>
        protected bool mFirstPathSet = false;
        protected bool mFirstPathValid = false;

        /// <summary>
        /// Determines if the current path is valid
        /// </summary>
        protected bool mIsPathValid = true;

        /// <summary>
        /// Used for initialization before any Start or Updates are called
        /// </summary>
        protected void Awake()
        {
            // Grab our transform
            _Transform = gameObject.transform;

            // Hold onto the actor controller so we can tap into it
            mActorController = gameObject.GetComponent<ActorController>();
            if (this.enabled) { OnEnable(); }

            // Hold onto the motion controller 
            mMotionController = gameObject.GetComponent<MotionController>();

            // Grab the nav mesh agent
            mNavMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            // We don't want the agent updating our position and rotation. That's
            // what the MC will do. We just want the path info
            if (mNavMeshAgent != null)
            {
                mNavMeshAgent.updatePosition = false;
                mNavMeshAgent.updateRotation = false;
            }

            // Default the speed we'll use to rotate
            mViewSpeedPer60FPSTick = _MaxViewSpeed * 60f;
        }

        /// <summary>
        /// Allows us to initialize before updates are called
        /// </summary>
        protected virtual void Start()
        {
            // Initialize the target if it exists
            if (_Target != null)
            {
                Target = _Target;
            }
            else if (_TargetPosition.magnitude > 0f)
            {
                TargetPosition = _TargetPosition;
            }
        }

        /// <summary>
        /// Called when the component is enabled. This is also called after awake. So,
        /// we need to ensure we're not doubling up on the assignment.
        /// </summary>
        protected void OnEnable()
        {
            if (mActorController != null)
            {
                // Remove our function if it's existing
                if (mActorController.OnControllerPreLateUpdate != null)
                {
                    mActorController.OnControllerPreLateUpdate -= OnControllerLateUpdate;
                }

                // We want our function to update first. So, we sneak in front
                ControllerLateUpdateDelegate lCurrentValue = mActorController.OnControllerPreLateUpdate;
                mActorController.OnControllerPreLateUpdate = OnControllerLateUpdate;
                mActorController.OnControllerPreLateUpdate += lCurrentValue;
            }
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            if (mActorController != null && mActorController.OnControllerPreLateUpdate != null)
            {
                mActorController.OnControllerPreLateUpdate -= OnControllerLateUpdate;
            }
        }

        /// <summary>
        /// Clears all the target properties
        /// </summary>
        public void ClearTarget()
        {
            _Target = null;
            _TargetPosition = Vector3Ext.Null;

            mNavMeshAgent.Stop();

            mHasArrived = false;
            mIsPathValid = true;
            mFirstPathSet = false;
            mFirstPathValid = false;
            mIsInSlowDistance = false;
            mIsTargetSet = false;

            mViewX = 0f;
            mViewY = 0f;
            mMovementX = 0f;
            mMovementY = 0f;
            mInputFromAvatarAngle = 0f;
            mInputFromCameraAngle = 0f;
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public override bool IsJustPressed(KeyCode rKey)
        {
            if (_AllowUserInput) { return base.IsJustPressed(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public override bool IsJustPressed(int rKey)
        {
            if (_AllowUserInput) { return base.IsJustPressed(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific action is pressed this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public override bool IsJustPressed(string rAction)
        {
            if (_AllowUserInput) { return base.IsJustPressed(rAction); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public override bool IsPressed(KeyCode rKey)
        {
            if (_AllowUserInput) { return base.IsPressed(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public override bool IsPressed(int rKey)
        {
            if (_AllowUserInput) { return base.IsPressed(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public override bool IsPressed(string rAction)
        {
            if (rAction == "ActivateRotation")
            {
                return (mViewX != 0f);
            }

            if (_AllowUserInput) { return base.IsPressed(rAction); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public override bool IsJustReleased(KeyCode rKey)
        {
            if (_AllowUserInput) { return base.IsJustReleased(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public override bool IsJustReleased(int rKey)
        {
            if (_AllowUserInput) { return base.IsJustReleased(rKey); }
            return false;
        }

        /// <summary>
        /// Test if a specific action is released this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public override bool IsJustReleased(string rAction)
        {
            if (_AllowUserInput) { return base.IsJustReleased(rAction); }
            return false;
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public override bool IsReleased(KeyCode rKey)
        {
            if (_AllowUserInput) { return base.IsReleased(rKey); }
            return true;
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public override bool IsReleased(int rKey)
        {
            if (_AllowUserInput) { return base.IsReleased(rKey); }
            return true;
        }

        /// <summary>
        /// Test if a specific action is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public override bool IsReleased(string rAction)
        {
            if (_AllowUserInput) { return base.IsReleased(rAction); }
            return true;
        }

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Float value as determined by the key</returns>
        public override float GetValue(int rKey)
        {
            if (_AllowUserInput) { return base.GetValue(rKey); }
            return 0f;
        }

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Float value as determined by the action</returns>
        public override float GetValue(string rAction)
        {
            if (_AllowUserInput) { return base.GetValue(rAction); }
            return 0;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        protected void OnControllerLateUpdate(ICharacterController rController, float rDeltaTime, int rUpdateIndex)
        {
            if (!_IsEnabled) { return; }
            if (mActorController == null) { return; }
            if (mNavMeshAgent == null) { return; }
            if (!mIsTargetSet) { return; }

            // Reset our input values
            mViewX = 0f;
            mViewY = 0f;
            mMovementX = 0f;
            mMovementY = 0f;
            mInputFromAvatarAngle = 0f;
            mInputFromCameraAngle = 0f;

            // Check if our first path is set and done
            if (mFirstPathSet && mNavMeshAgent.hasPath && !mNavMeshAgent.pathPending)
            {
                mFirstPathValid = true;
            }

            // Set the destination
            if (_Target != null) { _TargetPosition = _Target.position; }
            SetDestination(_TargetPosition);

            // Determine if we're at the destination
            mTargetVector = mAgentDestination - _Transform.position;
            mTargetDistance = mTargetVector.magnitude;

            // Check if we've arrived
            if (mTargetDistance < _StopDistance)
            {
                ClearTarget();
                mHasArrived = true;
                mFirstPathSet = false;
                mFirstPathValid = false;

                OnArrived();

                mNavMeshAgent.nextPosition = _Transform.position;
            }

            // Determine the next move
            if (!mHasArrived && mFirstPathValid)
            {
                // If we've reset the path, we may not be ready for a new steering target yet
                if (mNavMeshAgent.hasPath && !mNavMeshAgent.pathPending)
                {
                    mIsPathValid = true;

                    mWaypoint = mNavMeshAgent.steeringTarget;
                    mWaypointVector = mWaypoint - _Transform.position;
                    mWaypointDistance = mWaypointVector.magnitude;

                    if (mTargetDistance > _SlowDistance) { mIsInSlowDistance = false; }
                }

                // Determine if we're within the slow distance. We only want to fire the event once
                if (_SlowDistance > 0f && mTargetDistance < _SlowDistance)
                {
                    if (!mIsInSlowDistance) { OnSlowDistanceEntered(); }
                    mIsInSlowDistance = true;
                }

                // Using the waypoint information, we want to simulate the input 
                SimulateInput();

                // Force the agent to stay with our actor. This way, the path is
                // alway relative to our current position. Then, we can use the AC
                // to move to a valid position.
                mNavMeshAgent.nextPosition = _Transform.position;
            }
        }

        /// <summary>
        /// Converts the nav mesh agent data into psuedo-input that the motion controller
        /// will use to drive animations.
        /// </summary>
        protected void SimulateInput()
        {
            float lDeltaTime = TimeManager.SmoothedDeltaTime;

            // Direction we need to travel in
            Vector3 lDirection = mWaypointVector;
            lDirection.y = lDirection.y - _PathHeight;
            lDirection.Normalize();

            // Determine our view
            Vector3 lVerticalDirection = Vector3.Project(lDirection, _Transform.up);
            Vector3 lLateralDirection = lDirection - lVerticalDirection;

            mInputFromAvatarAngle = Vector3Ext.SignedAngle(_Transform.forward, lLateralDirection);

            // Determine how much we simulate the view x. We temper it to make it smooth
            float lYawAngleAbs = Mathf.Min(Mathf.Abs(mInputFromAvatarAngle * lDeltaTime), mViewSpeedPer60FPSTick * lDeltaTime);
            if (lYawAngleAbs < EPSILON) { lYawAngleAbs = 0f; }

            mViewX = Mathf.Sign(mInputFromAvatarAngle) * lYawAngleAbs;

            // Determine our movement
            if (mTargetDistance > _StopDistance)
            {
                // Calculate our own slowing
                float lRelativeMoveSpeed = 1f;
                if (mIsInSlowDistance && _SlowFactor > 0f)
                {
                    float lSlowPercent = (mTargetDistance - _StopDistance) / (_SlowDistance - _StopDistance);
                    lRelativeMoveSpeed = ((1f - _SlowFactor) * lSlowPercent) + _SlowFactor;
                }

                // TRT 4/5/2016: Force the slow distance as an absolute value
                if (mIsInSlowDistance && _SlowFactor > 0f)
                {
                    lRelativeMoveSpeed = _SlowFactor;
                }

                mMovementY = 1f * lRelativeMoveSpeed;
            }

            // Grab extra input information if we can
            if (mMotionController._CameraTransform == null)
            {
                mInputFromCameraAngle = mInputFromAvatarAngle;
            }
            else
            {
                Vector3 lInputForward = new Vector3(mMovementX, 0f, mMovementY);

                // We do the inverse tilt so we calculate the rotation in "natural up" space vs. "actor up" space. 
                Quaternion lInvTilt = QuaternionExt.FromToRotation(_Transform.up, Vector3.up);

                // Camera forward in "natural up"
                Vector3 lCameraForward = lInvTilt * mMotionController._CameraTransform.forward;

                // Create a quaternion that gets us from our world-forward to our camera direction.
                Quaternion lToCamera = Quaternion.LookRotation(lCameraForward, lInvTilt * _Transform.up);

                // Transform joystick from world space to camera space. Now the input is relative
                // to how the camera is facing.
                Vector3 lMoveDirection = lToCamera * lInputForward;
                mInputFromCameraAngle = NumberHelper.GetHorizontalAngle(lCameraForward, lMoveDirection);
            }
        }

        /// <summary>
        /// Sets the new destination that we need to travel to
        /// </summary>
        /// <param name="rDestination"></param>
        protected virtual void SetDestination(Vector3 rDestination)
        {
            if (!mHasArrived && mAgentDestination == rDestination) { return; }

            // Reset the properties
            mHasArrived = false;

            // Set the new destination
            mAgentDestination = rDestination;

            // Recalculate the path
            if (mIsPathValid && !mNavMeshAgent.pathPending)
            {
                mIsPathValid = false;

                mNavMeshAgent.updatePosition = false;
                mNavMeshAgent.updateRotation = false;
                mNavMeshAgent.stoppingDistance = _StopDistance;

                mNavMeshAgent.ResetPath();
                mNavMeshAgent.SetDestination(mAgentDestination);

                mFirstPathSet = true;
            }
        }

        /// <summary>
        /// Event function for when we arrive at the destination
        /// </summary>
        protected virtual void OnArrived()
        {
        }

        /// <summary>
        /// Event function for when we are within the slow distance
        /// </summary>
        protected virtual void OnSlowDistanceEntered()
        {
        }

        /// <summary>
        /// Renders out the path the agent is taking to get to the destination.
        /// This is purely for debugging.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (mNavMeshAgent == null) { return; }
            if (mNavMeshAgent.path == null) { return; }

            Color lGUIColor = Gizmos.color;

            Gizmos.color = Color.green;
            for (int i = 1; i < mNavMeshAgent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(mNavMeshAgent.path.corners[i - 1], mNavMeshAgent.path.corners[i]);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(mNavMeshAgent.steeringTarget, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(mWaypoint, 0.1f);
            Gizmos.DrawRay(transform.position, mTargetVector.normalized);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mAgentDestination, _StopDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mAgentDestination, _SlowDistance);

            Gizmos.color = lGUIColor;
        }
    }
}
