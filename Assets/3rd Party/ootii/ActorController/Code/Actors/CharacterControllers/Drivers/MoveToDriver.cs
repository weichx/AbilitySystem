using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Timing;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace com.ootii.Actors
{
    /// <summary>
    /// Simple driver used to move the character to a specific target.
    /// </summary>
    [AddComponentMenu("ootii/Actor Drivers/Move To Driver")]
    public class MoveToDriver : MonoBehaviour
    {
        /// <summary>
        /// Provides an error amount for determining distance
        /// </summary>
        public const float EPSILON = 0.01f;

        /// <summary>
        /// Determines if the driver is enabled
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
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
        /// Speed we'll use when not within slow distance
        /// </summary>
        public float _FastSpeed = 5f;
        public float FastSpeed
        {
            get { return _FastSpeed; }
            set { _FastSpeed = value; }
        }

        /// <summary>
        /// Speed we'll ultimately reduce to before stopping
        /// </summary>
        public float _SlowSpeed = 2f;
        public float SlowSpeed
        {
            get { return _SlowSpeed; }
            set { _SlowSpeed = value; }
        }

        /// <summary>
        /// Degrees per second the actor rotates
        /// </summary>
        public float _RotationSpeed = 360f;
        public virtual float RotationSpeed
        {
            get { return _RotationSpeed; }

            set
            {
                _RotationSpeed = value;
                mDegreesPer60FPSTick = _RotationSpeed / 60f;
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
        public float _SlowDistance = 1.0f;
        public float SlowDistance
        {
            get { return _SlowDistance; }
            set { _SlowDistance = value; }
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
        /// Direction we're traveling to
        /// </summary>
        protected Vector3 mTargetVector = Vector3.zero;

        /// <summary>
        /// Distance between the current position and actual target
        /// </summary>
        protected float mTargetDistance = 0f;

        /// <summary>
        /// Actor Controller being controlled
        /// </summary>
        protected ActorController mActorController = null;

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Used for initialization before any Start or Updates are called
        /// </summary>
        protected void Awake()
        {
            mActorController = gameObject.GetComponent<ActorController>();

            // Default the speed we'll use to rotate
            mDegreesPer60FPSTick = _RotationSpeed / 60f;
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

            mHasArrived = false;
            mIsInSlowDistance = false;

            mActorController.SetRelativeVelocity(Vector3.zero);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        protected void Update()
        {
            if (!_IsEnabled) { return; }
            if (!mIsTargetSet) { return; }
            if (mActorController == null) { return; }

            // Simulated input for the animator
            Vector3 lMovement = Vector3.zero;
            Quaternion lRotation = Quaternion.identity;

            // Set the destination
            if (_Target != null) { _TargetPosition = _Target.position; }

            // Determine if we're at the destination
            mTargetVector = _TargetPosition - transform.position;
            mTargetDistance = mTargetVector.magnitude;

            // Check if we've arrived
            if (mTargetDistance <= _StopDistance )
            {
                ClearTarget();
                mHasArrived = true;

                OnArrived();
            }
            else
            {
                // Determine if we're within the slow distance. We only want to fire the 
                // event once
                if (_SlowDistance > 0f && mTargetDistance < _SlowDistance)
                {
                    if (!mIsInSlowDistance) { OnSlowDistanceEntered(); }
                    mIsInSlowDistance = true;
                }

                // Calculate 
                CalculateMove(_TargetPosition, ref lMovement, ref lRotation);

                // Check if we've reached the destination
                mActorController.Move(lMovement);
                mActorController.Rotate(lRotation);
            }
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
            float lMoveSpeed = _FastSpeed; 
            if (mIsInSlowDistance && _SlowSpeed > 0f) { lMoveSpeed = _SlowSpeed; }

            // Set the final velocity based on the future rotation
            Quaternion lFutureRotation = transform.rotation * rRotate;
            rMove = lFutureRotation.Forward() * (lMoveSpeed * lDeltaTime);
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
    }
}
