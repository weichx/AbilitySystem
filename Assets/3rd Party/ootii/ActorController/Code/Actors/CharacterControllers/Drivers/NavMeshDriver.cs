using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Timing;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace com.ootii.Actors
{
    /// <summary>
    /// Used to manage information between the MC and NavMeshAgent that
    /// will control the MC.
    /// </summary>
    [AddComponentMenu("ootii/Actor Drivers/Nav Mesh Driver")]
    public class NavMeshDriver : AnimatorDriver
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
        /// Determines if we'll use the nav mesh agent's position
        /// directly or if we'll us our internal speeds.
        /// </summary>
        public bool _UseNavMeshPosition = false;
        public bool UseNavMeshPosition
        {
            get { return _UseNavMeshPosition; }
            set { _UseNavMeshPosition = value; }
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

                    mActorController.SetRelativeVelocity(Vector3.zero);
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

                    mActorController.SetRelativeVelocity(Vector3.zero);
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
        public float _StopDistance = 0.1f;
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
        public float _SlowFactor = 0.25f;
        public float SlowFactor
        {
            get { return _SlowFactor; }
            set { _SlowFactor = value; }
        }

        /// <summary>
        /// Determines if we clear the target object and position when the 
        /// actor reaches the target
        /// </summary>
        public bool _ClearTargetOnStop = true;
        public bool ClearTargetOnStop
        {
            get { return _ClearTargetOnStop; }
            set { _ClearTargetOnStop = value; }
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
        /// Next position as reported by the agent
        /// </summary>
        protected Vector3 mWaypoint = Vector3.zero;

        /// <summary>
        /// Destination that is set on the NMA. This will
        /// equal _TargetPosition once it's set on the NMA.
        /// </summary>
        protected Vector3 mAgentDestination = Vector3.zero;

        /// <summary>
        /// Direction we're traveling to
        /// </summary>
        protected Vector3 mTargetVector = Vector3.zero;

        /// <summary>
        /// Distance between the current position and actual target
        /// </summary>
        protected float mTargetDistance = 0f;

        /// <summary>
        /// NavMeshAgent we'll use to help manage the AI based
        /// navigation of the actor.
        /// </summary>
        protected NavMeshAgent mNavMeshAgent = null;

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
        protected override void Awake()
        {
            base.Awake();

            // Grab the nav mesh agent
            mNavMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            // We don't want the agent updating our position and rotation. That's
            // what the MC will do. We just want the path info
            if (mNavMeshAgent != null)
            {
                mNavMeshAgent.updatePosition = false;
                mNavMeshAgent.updateRotation = false;
                if (_MovementSpeed > 0f) { mNavMeshAgent.speed = _MovementSpeed; }
                if (_RotationSpeed > 0f) { mNavMeshAgent.angularSpeed = _RotationSpeed; }
            }
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
        /// Clears all the target properties
        /// </summary>
        public void ClearTarget()
        {
            if (_ClearTargetOnStop)
            {
                _Target = null;
                _TargetPosition = Vector3Ext.Null;
                mIsTargetSet = false;
            }

            mNavMeshAgent.Stop();

            mHasArrived = false;
            mIsPathValid = true;
            mFirstPathSet = false;
            mFirstPathValid = false;
            mIsInSlowDistance = false;

            mActorController.SetRelativeVelocity(Vector3.zero);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        protected override void Update()
        {
            if (!_IsEnabled) { return; }
            if (mActorController == null) { return; }
            if (mNavMeshAgent == null) { return; }
            if (!mIsTargetSet) { return; }

            // Simulated input for the animator
            Vector3 lMovement = Vector3.zero;
            Quaternion lRotation = Quaternion.identity;

            // Check if our first path is set and done
            if (mFirstPathSet && mNavMeshAgent.hasPath && !mNavMeshAgent.pathPending)
            {
                mFirstPathValid = true;
            }

            // Set the destination
            if (_Target != null) { _TargetPosition = _Target.position; }
            SetDestination(_TargetPosition);

            // Determine if we're at the destination
            mTargetVector = mAgentDestination - transform.position;
            mTargetDistance = mTargetVector.magnitude;

            // Check if we've arrived
            if (_UseNavMeshPosition)
            {
                if (!mNavMeshAgent.pathPending &&
                    mNavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete &&
                    mNavMeshAgent.remainingDistance == 0f)
                {
                    ClearTarget();
                    mHasArrived = true;
                    mFirstPathSet = false;
                    mFirstPathValid = false;

                    OnArrived();
                }
            }
            else
            {
                if (mTargetDistance < _StopDistance)
                {
                    ClearTarget();
                    mHasArrived = true;

                    OnArrived();
                }
            }

            // Determine the next move
            if (!mHasArrived && mFirstPathValid)
            {
                // Hold on to our next position before we change it
                if (mNavMeshAgent.hasPath && !mNavMeshAgent.pathPending)
                {
                    mIsPathValid = true;

                    mWaypoint = mNavMeshAgent.steeringTarget;
                    if (mTargetDistance > _SlowDistance) { mIsInSlowDistance = false; }
                }

                // Determine if we're within the slow distance. We only want to fire the 
                // event once
                if (_SlowDistance > 0f && mTargetDistance < _SlowDistance)
                {
                    if (!mIsInSlowDistance) { OnSlowDistanceEntered(); }
                    mIsInSlowDistance = true;
                }

                // Calculate 
                CalculateMove(mWaypoint, ref lMovement, ref lRotation);

                // Check if we've reached the destination
                //if (!mNavMeshAgent.pathPending)
                {
                    mActorController.Move(lMovement);
                    mActorController.Rotate(lRotation);
                }

                // Force the agent to stay with our actor. This way, the path is
                // alway relative to our current position. Then, we can use the AC
                // to move to a valid position.
                if (!_UseNavMeshPosition)
                {
                    mNavMeshAgent.nextPosition = transform.position;
                }
            }

            // Tell the animator what to do next
            SetAnimatorProperties(Vector3.zero, lMovement, lRotation);
        }

        /// <summary>
        /// Calculate how much to move an rotate by 
        /// </summary>
        /// <param name="rMove"></param>
        /// <param name="rRotate"></param>
        protected virtual void CalculateMove(Vector3 rWaypoint, ref Vector3 rMove, ref Quaternion rRotate)
        {
            float lDeltaTime = TimeManager.SmoothedDeltaTime;

            // Direction we need to travel in
            Vector3 lDirection = rWaypoint - transform.position;
            lDirection.y = lDirection.y - _PathHeight;
            lDirection.Normalize();

            // Determine our rotation
            Vector3 lVerticalDirection = Vector3.Project(lDirection, transform.up);
            Vector3 lLateralDirection = lDirection - lVerticalDirection;

            float lYawAngle = Vector3Ext.SignedAngle(transform.forward, lLateralDirection);

            if (_RotationSpeed == 0f)
            {
                rRotate = Quaternion.AngleAxis(lYawAngle, transform.up);
            }
            else
            {
                rRotate = Quaternion.AngleAxis(Mathf.Sign(lYawAngle) * Mathf.Min(Mathf.Abs(lYawAngle), _RotationSpeed * lDeltaTime), transform.up);
            }

            // Determine the movement
            if (_UseNavMeshPosition)
            {
                rMove = mNavMeshAgent.nextPosition - transform.position;
            }
            // In this case, we'll calculate movement ourselves
            else
            {
                // Grab the base movement speed
                float lMoveSpeed = mRootMotionMovement.magnitude / lDeltaTime;
                if (lMoveSpeed == 0f) { lMoveSpeed = _MovementSpeed; }

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
                    lMoveSpeed = _SlowFactor;
                    lRelativeMoveSpeed = 1f;
                }

                // Set the final velocity based on the future rotation
                Quaternion lFutureRotation = transform.rotation * rRotate;
                rMove = lFutureRotation.Forward() * (lMoveSpeed * lRelativeMoveSpeed * lDeltaTime);
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

        ///// <summary>
        ///// Renders out the path the agent is taking to get to the destination.
        ///// This is purely for debugging.
        ///// </summary>
        //protected void OnDrawGizmos()
        //{
        //    if (mNavMeshAgent == null) { return; }
        //    if (mNavMeshAgent.path == null) { return; }

        //    Color lGUIColor = Gizmos.color;

        //    Gizmos.color = Color.green;
        //    for (int i = 1; i < mNavMeshAgent.path.corners.Length; i++)
        //    {
        //        Gizmos.DrawLine(mNavMeshAgent.path.corners[i - 1], mNavMeshAgent.path.corners[i]);
        //    }

        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawSphere(mNavMeshAgent.steeringTarget, 0.1f);

        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawSphere(mWaypoint, 0.1f);
        //    Gizmos.DrawRay(transform.position, mTargetVector.normalized);

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(mAgentDestination, _StopDistance);

        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireSphere(mAgentDestination, _SlowDistance);

        //    Gizmos.color = lGUIColor;
        //}
    }
}
