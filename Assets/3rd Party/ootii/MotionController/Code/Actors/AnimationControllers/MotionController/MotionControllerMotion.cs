using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;
using com.ootii.Actors;
using com.ootii.Base;
using com.ootii.Data.Serializers;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Messages;
using com.ootii.Utilities;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Base class for motion controller behaviors that will 
    /// represent our "motions". By inheriting from StateMachineBehavior,
    /// we can take advantage of the Unity 5 animator features
    /// </summary>
    public abstract class MotionControllerMotion : BaseObject
    {
        /// <summary>
        /// Key used to uniquely  associate the behavior with motions
        /// </summary>
        public string _Key = "";
        public string Key
        {
            get { return _Key; }
        }

        /// <summary>
        /// Tracks the qualified type of the motion
        /// </summary>
        public string _Type = "";
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// Provides a way for us to group motions. So, we could group
        /// all idle motions, all movement motions, etc. It's up to the
        /// motion creator to use as needed.
        /// </summary>
        public int _Category = 0;
        public int Category
        {
            get { return _Category; }
            set { _Category = value; }
        }

        /// <summary>
        /// Pack this motion was distributed with.
        /// </summary>
        public string _Pack = "Basic";
        public string Pack
        {
            get { return _Pack; }
            set { _Pack = value; }
        }

        /// <summary>
        /// Root actor controller used to simplify access
        /// </summary>
        protected ActorController mActorController;

        /// <summary>
        /// Controller this motion is tied to
        /// </summary>
        protected MotionController mMotionController;
        public MotionController MotionController
        {
            get { return mMotionController; }

            set
            {
                mMotionController = value;
                mActorController = (mMotionController != null ? mMotionController.ActorController : null);
            }
        }

        /// <summary>
        /// Layer the motion is tied to
        /// </summary>
        protected MotionControllerLayer mMotionLayer;
        public MotionControllerLayer MotionLayer
        {
            get { return mMotionLayer; }
            set
            {
                mMotionLayer = value;
                //mMotionLayer.AnimatorLayerIndexIndex = (mMotionLayer == null ? 0 : mMotionLayer.AnimatorLayerIndex);
            }
        }

        /// <summary>
        /// Determines how important this motion is to other
        /// motions. The higher the priority, the higher the
        /// importance.
        /// </summary>
        public float _Priority = 0;
        public float Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        /// <summary>
        /// Determines if the motion is enabled. If it is
        /// running and then disabled, the motion will finish
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// Determines if the motion is enabled. If it is
        /// running and then disabled, the motion will finish
        /// </summary>
        public bool _ShowDebug = false;
        public bool ShowDebug
        {
            get
            {
                if (!mMotionController.ShowDebug) { return false; }
                return (mMotionController.ShowDebugForAllMotions ||_ShowDebug);
            }

            set { _ShowDebug = value; }
        }

        /// <summary>
        /// Input alias to use when testing to enter the climb
        /// </summary>
        public string _ActionAlias = "";
        public string ActionAlias
        {
            get { return _ActionAlias; }
            set { _ActionAlias = value; }
        }

        /// <summary>
        /// Determines if the motion is currently active
        /// </summary>
        protected bool mIsActive = false;
        public bool IsActive
        {
            get { return mIsActive; }
        }

        /// <summary>
        /// Used to tell if the animator has entered the
        /// sub-state machine associate with the motion.
        /// </summary>
        protected bool mIsAnimatorActive = false;
        public bool IsAnimatorActive
        {
            get { return mIsAnimatorActive; }
            set { mIsAnimatorActive = value; }
        }

        /// <summary>
        /// Used to tell if the animator has entered the
        /// sub-state machine associate with the motion.
        /// </summary>
        protected bool mIsInSubStateMachine = false;
        public bool IsInSubStateMachine
        {
            get { return mIsInSubStateMachine; }
            set { mIsInSubStateMachine = value; }
        }

        /// <summary>
        /// Defines how long the motion has been active
        /// </summary>
        protected float mAge = 0f;
        public float Age
        {
            get { return mAge; }
        }

        /// <summary>
        /// Defines the normalized time within the current state
        /// </summary>
        public float StateNormalizedTime
        {
            get
            {
                return mMotionLayer._AnimatorStateNormalizedTime;
            }
        }

        /// <summary>
        /// Determines if the motion is capable of being started.
        /// </summary>
        protected bool mIsStartable = true;
        public bool IsStartable
        {
            get { return mIsStartable; }
        }

        /// <summary>
        /// The phase or state the motion is in. This differs for each
        /// motion and is a way to track the state internally.
        /// </summary>
        protected int mPhase = 0;
        public int Phase
        {
            get { return mPhase; }
            set { mPhase = value; }
        }

        /// <summary>
        /// Parameter to send to the animator along with the phase
        /// </summary>
        protected int mParameter = 0;
        public int Parameter
        {
            get { return mParameter; }
            set { mParameter = value; }
        }

        /// <summary>
        /// Determines if this motion can be interrupted by another motion.
        /// When interrupted, this motion will need to handle it and may
        /// shut down.
        /// </summary>
        protected bool mIsInterruptible = true;
        public bool IsInterruptible
        {
            get { return mIsInterruptible; }
            set { mIsInterruptible = value; }
        }

        /// <summary>
        /// Flags the motion for activation on the next update.
        /// We stick to activating in the update phase so all states stay valid.
        /// </summary>
        protected bool mQueueActivation = false;
        public bool QueueActivation
        {
            get { return mQueueActivation; }
            set { mQueueActivation = value; }
        }

        /// <summary>
        /// Once deactivated, a delay before we start activating again
        /// </summary>
        public float _ReactivationDelay = 0f;
        public float ReactivationDelay
        {
            get { return _ReactivationDelay; }
            set { _ReactivationDelay = value; }
        }

        /// <summary>
        /// Tracks the last time the motion was deactivate
        /// </summary>
        protected float mDeactivationTime = 0f;
        public float DeactivationTime
        {
            get { return mDeactivationTime; }
        }

        /// <summary>
        /// Determines if this is the frame the motion was activated in
        /// </summary>
        protected bool mIsActivatedFrame = false;
        public bool IsActivatedFrame
        {
            get { return mIsActivatedFrame; }
            set { mIsActivatedFrame = value; }
        }

        /// <summary>
        /// Determines if we use trend data to delay sending speed
        /// to the animator
        /// </summary>
        protected bool mUseTrendData = false;
        public bool UseTrendData
        {
            get { return mUseTrendData; }
            set { mUseTrendData = value; }
        }

        /// <summary>
        /// Current velocity caused by the motion. This should be
        /// multiplied by delta-time to create displacement
        /// </summary>
        protected Vector3 mVelocity = Vector3.zero;
        public Vector3 Velocity
        {
            get { return mVelocity; }
        }

        /// <summary>
        /// Current movement caused by the motion. This should NOT be
        /// multiplied by delta-time to create displacement
        /// </summary>
        protected Vector3 mMovement = Vector3.zero;
        public Vector3 Movement
        {
            get { return mMovement; }
        }

        /// <summary>
        /// Amount of rotation caused by the motion. This should be
        /// multiplied by delta-time to create angular displacement
        /// </summary>
        protected Vector3 mAngularVelocity = Vector3.zero;
        public Vector3 AngularVelocity
        {
            get { return mAngularVelocity; }
        }

        /// <summary>
        /// Amount of rotation (yaw) caused by the motion. This should NOT be
        /// multiplied by delta-time to create angular displacement. This is
        /// also considered a "local" rotation. So, it is relative to the current
        /// rotation.
        /// </summary>
        protected Quaternion mRotation = Quaternion.identity;
        public Quaternion Rotation
        {
            get { return mRotation; }
        }

        /// <summary>
        /// Amount of rotation (pitch) caused by the motion. This should NOT be
        /// multiplied by delta-time to create angular displacement. This is
        /// also considered a "local" rotation. So, it is relative to the current
        /// rotation.
        /// </summary>
        protected Quaternion mTilt = Quaternion.identity;
        public Quaternion Tilt
        {
            get { return mTilt; }
        }

        /// <summary>
        /// Determines if we should use a fixed update or the standard update. The
        /// fixed update is important for smoothing out lerps or using physics.
        /// </summary>
        public bool _IsFixedUpdateEnabled = false;
        public virtual bool IsFixedUpdateEnabled
        {
            get { return _IsFixedUpdateEnabled; }
            set { _IsFixedUpdateEnabled = value; }
        }

        /// <summary>
        /// Determines the frame rate we're targeting for fixed updates. The
        /// fixed update is important for smoothing out lerps or using physics.
        /// </summary>
        public float _FixedUpdateFPS = 60f;
        public virtual float FixedUpdateFPS
        {
            get { return _FixedUpdateFPS; }
            set { _FixedUpdateFPS = value; }
        }

        /// <summary>
        /// Allows us to do some pre-processing if needed
        /// </summary>
        protected bool mIsFirstUpdate = true;

        /// <summary>
        /// For physics calculations, the amount of time that has elapsed
        /// </summary>
        protected float mFixedElapsedTime = 0f;

        /// <summary>
        /// Reach data is used to add extra movement to help portions of the animations
        /// reach specific spots. The actual data depends on the animation itself.
        /// </summary>
        protected List<MotionReachData> mReachData = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MotionControllerMotion()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MotionControllerMotion(string rGUID)
        {
            _GUID = rGUID;
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public MotionControllerMotion(MotionController rController)
            : base()
        {
            MotionController = rController;
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public virtual void Awake()
        {
            if (mMotionController != null)
            {
                Animator lAnimator = mMotionController.GetComponent<Animator>();
                if (lAnimator != null)
                {
                    MotionControllerBehaviour[] lBehaviors = lAnimator.GetBehaviours<MotionControllerBehaviour>();
                    for (int i = 0; i < lBehaviors.Length; i++)
                    {
                        if (lBehaviors[i].MotionKey == (_Key.Length > 0 ? _Key : this.GetType().FullName))
                        {
                            lBehaviors[i].AddMotion(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows for any processing after the motion has been deserialized
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public virtual void LoadAnimatorData()
        {
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns>Boolean that determines if the motion should start</returns>
        public virtual bool TestActivate()
        {
            return false;
        }

        /// <summary>
        /// Allows an external activator to test if this motion should be started. Things like
        /// an input check would not go here as the external activator would control that.
        /// </summary>
        /// <returns></returns>
        public virtual bool ExternalTestActivate()
        {
            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public virtual bool TestUpdate()
        {
            return true;
        }

        /// <summary>
        /// Raised when a motion is being interrupted by another motion
        /// </summary>
        /// <param name="rMotion">Motion doing the interruption</param>
        /// <returns>Boolean determining if it can be interrupted</returns>
        public virtual bool TestInterruption(MotionControllerMotion rMotion)
        {
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public virtual bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Flag the motion as active
            mIsActive = true;
            mIsAnimatorActive = false;
            mIsInSubStateMachine = false;

            // Reset the age
            mAge = 0f;

            // Report this motion as activated
            if (mMotionController.MotionActivated != null) { mMotionController.MotionActivated(mMotionLayer._AnimatorLayerIndex, this, rPrevMotion); }

            // Report that we're good enter the motion
            return true;
        }

        /// <summary>
        /// Called to interrupt the motion if it is currently active. This
        /// gives the motion a chance to stop itself how it sees fit. The motion
        /// may simply ignore the call.
        /// </summary>
        /// <param name="rParameter">Any value you wish to pass</param>
        /// <returns>Boolean determining if the motion accepts the interruption. It doesn't mean it will deactivate.</returns>
        public virtual bool Interrupt(object rParameter)
        {
            return true;
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public virtual void Deactivate()
        {
            mIsActive = false;
            mIsStartable = true;
            mMovement = Vector3.zero;
            mVelocity = Vector3.zero;
            mRotation = Quaternion.identity;
            mAngularVelocity = Vector3.zero;
            mDeactivationTime = Time.time;
            mIsAnimatorActive = false;
            mIsInSubStateMachine = false;

            // Report this motion as activated
            if (mMotionController.MotionDeactivated != null) { mMotionController.MotionDeactivated(mMotionLayer._AnimatorLayerIndex, this); }
        }

        /// <summary>
        /// Internal update called by the motion layer. We use this as the base
        /// for updating the motion. Here, we also control any fixed updating we do
        /// </summary>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateIndex"></param>
        public void UpdateMotion(float rDeltaTime, int rUpdateIndex)
        {
            // No need to continue if we aren't active
            if (!_IsEnabled) { return; }

            // Flag if we're fully entered the motion
            if (mIsAnimatorActive && mMotionLayer._AnimatorTransitionID == 0)
            {
                mIsInSubStateMachine = true;
            }

            // If we're already dealing with fixed time processing,
            // we don't have to do it here
            if (mActorController._IsFixedUpdateEnabled)
            {
                Update(rDeltaTime, rUpdateIndex);

                // Report the update so others can hook into it
                if (mMotionController.MotionUpdated != null) { mMotionController.MotionUpdated(rDeltaTime, rUpdateIndex, mMotionLayer._AnimatorLayerIndex, this); }

                // We're done
                return;
            }

            // Counts how many valid updates we have
            int lUpdateCount = 0;

            // Current time since the last frame
            float lDeltaTime = rDeltaTime;

            // If we're not fixed, update as fast as possible
            if (!_IsFixedUpdateEnabled || _FixedUpdateFPS <= 0f)
            {
                lUpdateCount = 1;
            }
            // If we are fixed, update on the interval based on our FPS
            else
            {
                lDeltaTime = 1.0f / _FixedUpdateFPS;

                // We'll cheat a bit. If the delta time is withing 10% of our desired time,
                // We'll just go with the fixed time. It makes things smoother
                if (Mathf.Abs(lDeltaTime - rDeltaTime) < lDeltaTime * 0.1f)
                {
                    lUpdateCount = 1;
                }
                // Outside of the 10%, we need to adjust accordingly
                else
                {

                    // Build up our elapsed time
                    mFixedElapsedTime += rDeltaTime;

                    // If the elapsed time exceeds our desired update schedule, it's
                    // time for us to do a physics update. In fact, if the system
                    // is running slow we may need to do multiple updates
                    while (mFixedElapsedTime >= lDeltaTime)
                    {
                        lUpdateCount++;
                        mFixedElapsedTime -= lDeltaTime;

                        // Fail safe. We can have long delta times when debugging and such
                        if (lUpdateCount >= 5)
                        {
                            mFixedElapsedTime = 0;
                            break;
                        }
                    }
                }
            }

            // Grab any state information we may use in the update
            mAge += rDeltaTime;

            // Do as many updates as we need to in order to simulate
            // the desired frame rates
            if (lUpdateCount > 0)
            {
                // Update the controller itself
                for (int i = 1; i <= lUpdateCount; i++)
                {
                    Update(lDeltaTime, i);

                    // Report the update so others can hook into it
                    if (mMotionController.MotionUpdated != null) { mMotionController.MotionUpdated(lDeltaTime, i, mMotionLayer._AnimatorLayerIndex, this); }

                    // Flag that we've passed the first update
                    mIsFirstUpdate = false;
                }
            }
            // In this case, there shouldn't be an update. This typically
            // happens when the true FPS is much faster than our desired FPS
            else
            {
                Update(lDeltaTime, lUpdateCount);

                // Report the update so others can hook into it
                if (mMotionController.MotionUpdated != null) { mMotionController.MotionUpdated(lDeltaTime, lUpdateCount, mMotionLayer._AnimatorLayerIndex, this); }
            }
        }

        /// <summary>
        /// Allows the motion to modify the velocity and rotation before it is applied.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        public virtual void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated. This occurs
        /// before the functions in the events in the StateMachineBehaviour
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public virtual void Update(float rDeltaTime, int rUpdateIndex)
        {
            //// Default is to simply deactivate the motion
            //if (rUpdateIndex <= 1)
            //{
            //    Deactivate();
            //}
        }

        /// <summary>
        /// Removes any existing reach data so it can be re-used
        /// </summary>
        protected virtual void ClearReachData()
        {
            if (mReachData == null) { mReachData = new List<MotionReachData>(); }

            for (int i = 0; i < mReachData.Count; i++) { MotionReachData.Release(mReachData[i]); }
            mReachData.Clear();
        }

        /// <summary>
        /// Grab movement associated with the current state and limits
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 GetReachMovement(Vector3 rTarget, Vector3 rPosition, float rStart, float rEnd = 1f, bool rStay = true, bool rTest = true, bool rX = true, bool rY = true, bool rZ = true)
        {
            Vector3 lReach = Vector3.zero;

            // We want to start pulling the actor's grab anchor to the ground target
            if (rTest)
            {
                if (mMotionLayer._AnimatorStateNormalizedTime > rStart && (mMotionLayer._AnimatorStateNormalizedTime < rEnd || rStay))
                {
                    float lPercent = (mMotionLayer._AnimatorStateNormalizedTime - rStart) / (rEnd - rStart);
                    if (lPercent < 0f) { lPercent = 0f; } else if (lPercent > 1f) { lPercent = 1f; }

                    if (lPercent <= 0f) { lReach = Vector3.zero; }
                    else if (lPercent >= 1f) { lReach = (rTarget - rPosition); }
                    else { lReach = (rTarget - rPosition) * lPercent; }

                    if (!rX) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.right); }
                    if (!rY) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.up); }
                    if (!rZ) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.forward); }
                }
            }

            return lReach;
        }

        /// <summary>
        /// Grab movement associated with the current state and limits
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 GetReachMovement(Vector3 rTarget, Vector3 rPosition, float rPercent = 1f, bool rTest = true, bool rX = true, bool rY = true, bool rZ = true)
        {
            Vector3 lReach = Vector3.zero;

            if (rTest)
            {
                if (rPercent <= 0f) { lReach = Vector3.zero; }
                else if (rPercent >= 1f) { lReach = (rTarget - rPosition); }
                else { lReach = (rTarget - rPosition) * rPercent; }

                if (!rX) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.right); }
                if (!rY) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.up); }
                if (!rZ) { lReach = lReach - Vector3.Project(lReach, mMotionController._Transform.forward); }
            }

            return lReach;
        }

        /// <summary>
        /// Grabs the movement associated with the reach data. This reach movement helps us line up
        /// at specific points in the animation.
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 GetReachMovement()
        {
            Vector3 lMovement = Vector3.zero;
            //Transform lGround = mActorController.State.Ground;

            if (mReachData != null)
            {
                // Use the reach data to move towards the target over time
                int lStateID = mMotionLayer._AnimatorStateID;
                float lStateTime = mMotionLayer._AnimatorStateNormalizedTime;

                int lTransitionID = mMotionLayer._AnimatorTransitionID;
                float lTransitionTime = mMotionLayer._AnimatorTransitionNormalizedTime;

                // Grab the active reach data and process it
                for (int i = 0; i < mReachData.Count; i++)
                {
                    if (!mReachData[i].IsComplete)
                    {
                        // Base it on the state
                        if (mReachData[i].StateID == lStateID && lStateTime >= mReachData[i].StartTime) // && lStateTime <= mReachData[i].EndTime)
                        {
                            float lPercent = (lStateTime - mReachData[i].StartTime) / (mReachData[i].EndTime - mReachData[i].StartTime);

                            // If we go over the end time one step, that's fine. It allows us to
                            // compensate for the unreliable animation time that may not allow us to finish 
                            // the reach data.
                            if (lStateTime >= mReachData[i].EndTime)
                            {
                                lPercent = 1f;
                                mReachData[i].IsComplete = true;
                            }

                            //DebugDraw.DrawSphereMesh(mActorController._Transform.position + (mActorController._Transform.rotation * mReachData[i].Anchor), 0.025f, Color.blue, 1f);
                            //DebugDraw.DrawSphereMesh(mReachData[i].ReachTarget, 0.03f, Color.red, 1f);

                            Vector3 lActorAnchor = mActorController._Transform.position;

                            Vector3 lTargetDelta = mReachData[i].ReachTarget - lActorAnchor;
                            lMovement = lMovement + (lTargetDelta * NumberHelper.Pow(lPercent, mReachData[i].Power));
                        }
                        // Base it on the transition
                        else if (mReachData[i].TransitionID == lTransitionID && lTransitionTime >= mReachData[i].StartTime)
                        {
                            float lPercent = (lTransitionTime - mReachData[i].StartTime) / (mReachData[i].EndTime - mReachData[i].StartTime);

                            // If we go over the end time one step, that's fine. It allows us to
                            // compensate for the unreliable animation time that may not allow us to finish 
                            // the reach data.
                            if (lTransitionTime >= mReachData[i].EndTime)
                            {
                                lPercent = 1f;
                                mReachData[i].IsComplete = true;
                            }

                            //DebugDraw.DrawSphereMesh(mActorController._Transform.position + (mActorController._Transform.rotation * mReachData[i].Anchor), 0.025f, Color.blue, 1f);
                            //DebugDraw.DrawSphereMesh(mReachData[i].ReachTarget, 0.03f, Color.red, 1f);

                            Vector3 lActorAnchor = mActorController._Transform.position;

                            Vector3 lTargetDelta = mReachData[i].ReachTarget - lActorAnchor;
                            lMovement = lMovement + (lTargetDelta * NumberHelper.Pow(lPercent, mReachData[i].Power));
                        }
                    }
                }
            }

            return lMovement;
        }

        /// <summary>
        /// Grabs the rotation that rotates us towards the specific angle based on the parameters
        /// </summary>
        /// <returns></returns>
        protected virtual Quaternion GetReachRotation(float rStartTime, float rEndTime, float rTotalAngle, ref float rUsedAngle)
        {
            Quaternion lRotation = Quaternion.identity;

            if (rUsedAngle != rTotalAngle)
            {
                float lStateTime = mMotionLayer._AnimatorStateNormalizedTime;

                if (lStateTime > rStartTime && lStateTime <= rEndTime)
                {
                    float lPercent = (lStateTime - rStartTime) / (rEndTime - rStartTime);
                    float lFrameYaw = (rTotalAngle * lPercent) - rUsedAngle;

                    lRotation = Quaternion.AngleAxis(lFrameYaw, Vector3.up);
                    rUsedAngle = rTotalAngle * lPercent;
                }
                else if (lStateTime > rEndTime)
                {
                    float lFrameYaw = rTotalAngle - rUsedAngle;

                    lRotation = Quaternion.AngleAxis(lFrameYaw, Vector3.up);
                    rUsedAngle = rTotalAngle;
                }
            }

            return lRotation;
        }

        /// <summary>
        /// Callback for setting up animation IK(inverse kinematics).
        /// </summary>
        /// <param name="rAnimator">Mecanim animator whose IK we're processing</param>
        /// <param name="rLayer">Index of the layer in the animator we're processing</param>
        public virtual void OnAnimatorIK(Animator rAnimator, int rLayer)
        {
        }

        /// <summary>
        /// Raised when the animator's state has changed.
        /// </summary>
        public virtual void OnAnimatorStateChange(int rLastStateID, int rNewStateID)
        {
        }

        /// <summary>
        /// Raised by the animation when an event occurs.
        /// </summary>
        public virtual void OnAnimationEvent(AnimationEvent rEvent)
        {
        }

        /// <summary>
        /// Raised by the controller when a message is received
        /// </summary>
        public virtual void OnMessageReceived(IMessage rMessage)
        {
        }

        /// <summary>
        /// Determines if we're using auto-generated code
        /// </summary>
        public virtual bool HasAutoGeneratedCode
        {
            get { return false; }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInMotionState
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMotionState(int rState)
        {
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMotionState(int rStateID, int rTransitionID)
        {
            return false;
        }

        /// <summary>
        /// Creates a JSON string that represents the motion's serialized state. We
        /// do this since Unity can't handle putting lists of derived objects into
        /// prefabs.
        /// </summary>
        /// <returns>JSON string representing the object</returns>
        public virtual string SerializeMotion()
        {
            if (_Type.Length == 0) { _Type = this.GetType().AssemblyQualifiedName; }

            StringBuilder lStringBuilder = new StringBuilder();
            lStringBuilder.Append("{");

            // These four properties are important from the base MotionControllerMotion
            lStringBuilder.Append(", \"Type\" : \"" + _Type + "\"");
            lStringBuilder.Append(", \"Name\" : \"" + _Name + "\"");
            lStringBuilder.Append(", \"Priority\" : \"" + _Priority.ToString() + "\"");
            lStringBuilder.Append(", \"ActionAlias\" : \"" + _ActionAlias.ToString() + "\"");
            lStringBuilder.Append(", \"IsEnabled\" : \"" + _IsEnabled.ToString() + "\"");
            lStringBuilder.Append(", \"ReactivationDelay\" : \"" + _ReactivationDelay.ToString() + "\"");
            lStringBuilder.Append(", \"ShowDebug\" : \"" + _ShowDebug.ToString() + "\"");

            // Cycle through all the properties. 
            // Unfortunately Binding flags don't seem to be working. So,
            // we need to ensure we don't include base properties
            PropertyInfo[] lBaseProperties = typeof(MotionControllerMotion).GetProperties();
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

                object[] lAttributes = lProperty.GetCustomAttributes(typeof(SerializationIgnoreAttribute), true);
                if (lAttributes != null && lAttributes.Length > 0) { continue; }

                bool lAdd = true;
                for (int i = 0; i < lBaseProperties.Length; i++)
                {
                    if (lProperty.Name == lBaseProperties[i].Name)
                    {
                        lAdd = false;
                        break;
                    }
                }

                if (!lAdd || lProperty.GetValue(this, null) == null) { continue; }

                object lValue = lProperty.GetValue(this, null);
                if (lProperty.PropertyType == typeof(Vector2))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector2)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Vector3))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector3)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Vector4))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector4)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Transform))
                {
                    string lTransformPath = "";

                    Transform lCurrentTransform = (Transform)lValue;
                    while (lCurrentTransform != null)
                    {
                        // We use this condition to ensure we're dealing with a relative path
                        // and not a global path
                        if (lCurrentTransform.parent != null)
                        {
                            lTransformPath = lCurrentTransform.name + (lTransformPath.Length > 0 ? "/" : "") + lTransformPath;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                        if (lCurrentTransform == mMotionController._Transform) { break; }
                    }

                    if (lTransformPath.Length == 0) { lTransformPath = "."; }
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + lTransformPath + "\"");
                }
                else
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + lValue.ToString() + "\"");
                }
            }

            lStringBuilder.Append("}");

            return lStringBuilder.ToString();
        }

        /// <summary>
        /// Gieven a JSON string that is the definition of the object, we parse
        /// out the properties and set them.
        /// </summary>
        /// <param name="rDefinition">JSON string</param>
        public virtual void DeserializeMotion(string rDefinition)
        {
            JSONNode lDefinitionNode = JSONNode.Parse(rDefinition);
            if (lDefinitionNode == null) { return; }

            // Cycle through the properties and load the values we can
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }
                //if (lProperty.GetValue(this, null) == null) { continue; }

                JSONNode lValueNode = lDefinitionNode[lProperty.Name];
                if (lValueNode == null)
                {
                    if (lProperty.PropertyType == typeof(string))
                    {
                        lProperty.SetValue(this, "", null);
                    }

                    continue;
                }

                if (lProperty.PropertyType == typeof(string))
                {
                    lProperty.SetValue(this, lValueNode.Value, null);
                }
                else if (lProperty.PropertyType == typeof(int))
                {
                    lProperty.SetValue(this, lValueNode.AsInt, null);
                }
                else if (lProperty.PropertyType == typeof(float))
                {
                    lProperty.SetValue(this, lValueNode.AsFloat, null);
                }
                else if (lProperty.PropertyType == typeof(bool))
                {
                    lProperty.SetValue(this, lValueNode.AsBool, null);
                }
                else if (lProperty.PropertyType == typeof(Vector2))
                {
                    Vector2 lVector2Value = Vector2.zero;
                    lVector2Value = lVector2Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector2Value, null);
                }
                else if (lProperty.PropertyType == typeof(Vector3))
                {
                    Vector3 lVector3Value = Vector3.zero;
                    lVector3Value = lVector3Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector3Value, null);
                }
                else if (lProperty.PropertyType == typeof(Vector4))
                {
                    Vector4 lVector4Value = Vector4.zero;
                    lVector4Value = lVector4Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector4Value, null);
                }
                else if (lProperty.PropertyType == typeof(Transform))
                {
                    Transform lParent = mMotionController._Transform;
                    if (lParent == null) { lParent = mMotionController.gameObject.transform; }

                    if (lParent != null)
                    {
                        if (lValueNode.Value == ".")
                        {
                            // In this case, we want ourselves
                            lProperty.SetValue(this, lParent, null);
                        }
                        else
                        {
                            // In this case, we want a relative path
                            Transform lTransform = lParent.Find(lValueNode.Value);
                            if (lTransform != null) { lProperty.SetValue(this, lTransform, null); }
                        }
                    }
                }
                else
                {
                    //JSONClass lObject = lValueNode.AsObject;
                }
            }
        }

        /// <summary>
        /// Called on the first Update frame when a transition from a state from another statemachine transition 
        /// to one of this statemachine's state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rStateMachinePathHash">Path the the transition is coming from</param>
        public virtual void OnStateMachineEnter(Animator rAnimator, int rStateMachinePathHash)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateMachineEnter");
        }

        /// <summary>
        /// Called on the last Update frame when one of the statemachine's state is transitionning 
        /// toward another state in another state machine.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rStateMachinePathHash">Path the the transition is coming from</param>
        public virtual void OnStateMachineExit(Animator rAnimator, int rStateMachinePathHash)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateMachineExit");
        }

        /// <summary>
        /// Called on the first Update frame when a statemachine enters this state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public virtual void OnStateEnter(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateEnter " + rAnimatorStateInfo.fullPathHash);
        }

        /// <summary>
        /// Called on the last update frame when a statemachine evaluate this state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public virtual void OnStateExit(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateExit " + rAnimatorStateInfo.fullPathHash);
        }

        /// <summary>
        /// Called at each Update frame except for the first and last frame. Occurs
        /// before Move and IK. It also occurs after MotionControllerMotion.UpdateMotion()
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public virtual void OnStateUpdate(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateUpdate " + rAnimatorStateInfo.fullPathHash);
        }

        /// <summary>
        /// Called right after MonoBehaviour.OnAnimatorMove.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public virtual void OnStateMove(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateMove " + rAnimatorStateInfo.fullPathHash);
        }

        /// <summary>
        /// Called right after MonoBehaviour.OnAnimatorIK.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public virtual void OnStateIK(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerMotion.OnStateIK " + rAnimatorStateInfo.fullPathHash);
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        // Variables for the settings
        public bool _EditorShowSettings = false;

        public string _EditorAnimatorSMName = "";
        public bool _EditorAttachBehaviour = false;

        protected bool mShowScriptGenerators = false;

        [NonSerialized]
        public AnimatorController _EditorAnimatorController = null;

        /// <summary>
        /// Reset to default values. Reset is called when the user hits the Reset button in the Inspector's 
        /// context menu or when adding the component the first time. This function is only called in editor mode.
        /// </summary>
        public virtual void Reset()
        {
        }
        
        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public virtual bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            // Unfortunately Binding flags don't seem to be working. So,
            // we need to ensure we don't include base properties
            PropertyInfo[] lBaseProperties = typeof(MotionControllerMotion).GetProperties();

            // Render out the accessable properties using reflection
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

                string lTooltip = "";
                object[] lAttributes = lProperty.GetCustomAttributes(typeof(MotionDescriptionAttribute), true);
                foreach (MotionDescriptionAttribute lAttribute in lAttributes)
                {
                    lTooltip = lAttribute.Value;
                }

                // Unfortunately Binding flags don't seem to be working. So,
                // we need to ensure we don't include base properties
                bool lAdd = true;
                for (int i = 0; i < lBaseProperties.Length; i++)
                {
                    if (lProperty.Name == lBaseProperties[i].Name)
                    {
                        lAdd = false;
                        break;
                    }
                }

                if (!lAdd) { continue; }

                // Grab the current value
                object lOldValue = lProperty.GetValue(this, null);

                // Based on the type, show an edit field
                if (lProperty.PropertyType == typeof(string))
                {
                    string lNewValue = EditorGUILayout.TextField(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (string)lOldValue);
                    if (lNewValue != (string)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(int))
                {
                    int lNewValue = EditorGUILayout.IntField(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (int)lOldValue);
                    if (lNewValue != (int)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(float))
                {
                    float lNewValue = EditorGUILayout.FloatField(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (float)lOldValue);
                    if (lNewValue != (float)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(bool))
                {
                    bool lNewValue = EditorGUILayout.Toggle(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (bool)lOldValue);
                    if (lNewValue != (bool)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(Vector2))
                {
                    Vector2 lNewValue = EditorGUILayout.Vector2Field(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (Vector2)lOldValue);
                    if (lNewValue != (Vector2)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(Vector3))
                {
                    Vector3 lNewValue = EditorGUILayout.Vector3Field(new GUIContent(StringHelper.FormatCamelCase(lProperty.Name), lTooltip), (Vector3)lOldValue);
                    if (lNewValue != (Vector3)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
                else if (lProperty.PropertyType == typeof(Vector4))
                {
                    Vector4 lNewValue = EditorGUILayout.Vector4Field(StringHelper.FormatCamelCase(lProperty.Name), (Vector4)lOldValue);
                    if (lNewValue != (Vector4)lOldValue)
                    {
                        lIsDirty = true;
                        lProperty.SetValue(this, lNewValue, null);
                    }
                }
            }

            return lIsDirty;
        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public virtual void OnSettingsGUI()
        {
            GUILayout.Space(2);

            if (GUILayout.Button(new GUIContent("Create Animator State Machine"), EditorStyles.miniButton, GUILayout.MinWidth(30)))
            {
                CreateStateMachine();
            }

            if (GUILayout.Button(new GUIContent("Create Input Manager Settings"), EditorStyles.miniButton, GUILayout.MinWidth(30)))
            {
                CreateInputManagerSettings();
            }

            GUILayout.Space(5);

            EditorGUI.indentLevel++;
            mShowScriptGenerators = EditorGUILayout.Foldout(mShowScriptGenerators, new GUIContent("Script Generators"));
            EditorGUI.indentLevel--;

            if (mShowScriptGenerators)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Space(2);

                if (GUILayout.Button(new GUIContent("Generate Script"), EditorStyles.miniButton, GUILayout.MinWidth(30)))
                {
                    GenerateScripts(_EditorAnimatorController, mMotionLayer.AnimatorLayerIndex, _EditorAnimatorSMName);
                }

                GUILayout.Space(2);

                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Allow for rendering in the editor. 
        /// </summary>
        public virtual void OnSceneGUI()
        {
        }

        /// <summary>
        /// Creates the animator substate machine for this motion
        /// </summary>
        public virtual void CreateStateMachine(AnimatorController rAnimator)
        {
            _EditorAnimatorController = rAnimator;

            FindAnimations();
            CreateStateMachine();
        }

        /// <summary>
        /// Creates the animator substate machine for this motion
        /// </summary>
        protected virtual void CreateStateMachine()
        {
        }

        /// <summary>
        /// Creates input settings in the Unity Input Manager
        /// </summary>
        public virtual void CreateInputManagerSettings()
        {
        }

        /// <summary>
        /// Initializes the animation clips if they aren't found
        /// </summary>
        public virtual void FindAnimations()
        {
        }

        /// <summary>
        /// Creates a blend tree that we can assign to a node
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="rAnimatorController"></param>
        /// <param name="rAnimatorLayer"></param>
        /// <returns></returns>
        protected virtual BlendTree CreateBlendTree(string rName, AnimatorController rAnimatorController, int rAnimatorLayer)
        {
            // Create the blend tree. This is so bad, but apparently what we have to do or the blend
            // tree vanishes after we run.
            BlendTree lBlendTree;
            AnimatorState lDeleteState = rAnimatorController.CreateBlendTreeInController("DELETE_ME", out lBlendTree);

            lBlendTree.name = rName;
            lBlendTree.blendType = BlendTreeType.SimpleDirectional2D;
            lBlendTree.blendParameter = "Input X";
            lBlendTree.blendParameterY = "Input Y";

            // Get rid of the dummy state we created to create the blend tree
            lDeleteState.motion = null;
            rAnimatorController.layers[rAnimatorLayer].stateMachine.RemoveState(lDeleteState);

            return lBlendTree;
        }

        /// <summary>
        /// Renders out an object field for assigning an animation
        /// </summary>
        /// <param name="rName">Friendly name to display</param>
        /// <param name="rAssetPath">Path of the asset to load</param>
        /// <param name="rAssetName">Name of the animation clip to return</param>
        /// <param name="rClip">Current clip value</param>
        /// <returns></returns>
        protected virtual AnimationClip CreateAnimationField(string rDisplayName, string rAssetPath, string rAssetName, AnimationClip rClip)
        {
            rClip = EditorGUILayout.ObjectField(new GUIContent(rDisplayName, rDisplayName), rClip, typeof(AnimationClip), true, null) as AnimationClip;
            if (rClip == null) { rClip = FindAnimationClip(rAssetPath, rAssetName); }

            return rClip;
        }

        /// <summary>
        /// Loads an asset and looks to return an animation clip
        /// </summary>
        /// <param name="rAssetPath">Path of the asset to load</param>
        /// <param name="rAssetName">Name of the animation clip to return</param>
        /// <returns>Animation Clip found in the path with the name</returns>
        protected AnimationClip FindAnimationClip(string rAssetPath, string rAssetName)
        {
            AnimationClip lClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(rAssetPath);
            if (lClip != null) { return lClip; }

            int lParseIndex = rAssetPath.IndexOf(".fbx/");
            if (lParseIndex >= 0)
            {
                lParseIndex += 5;
                rAssetName = rAssetPath.Substring(lParseIndex, rAssetPath.Length - lParseIndex - 5);
                rAssetPath = rAssetPath.Substring(0, lParseIndex - 1);
            }

            UnityEngine.Object[] lAssets = AssetDatabase.LoadAllAssetsAtPath(rAssetPath);
            for (int i = 0; i < lAssets.Length; i++)
            {
                lClip = lAssets[i] as AnimationClip;
                if (lClip != null && lClip.name == rAssetName)
                {
                    return lClip;
                }
            }

            return null;
        }

        private List<string> mUniqueStateNames = new List<string>();

        private List<AnimatorStateMachine> mAnimatorStateMachines = new List<AnimatorStateMachine>();
        private List<string> mAnimatorStateMachinePaths = new List<string>();

        private List<AnimatorState> mAnimatorStates = new List<AnimatorState>();
        private List<string> mAnimatorStatePaths = new List<string>();
        private List<string> mAnimatorStateNames = new List<string>();

        private List<AnimatorStateTransition> mAnimatorTransitions = new List<AnimatorStateTransition>();
        private List<string> mAnimatorTransitionPaths = new List<string>();
        private List<string> mAnimatorTransitionNames = new List<string>();

        private List<Motion> mAnimatorMotions = new List<Motion>();

        private List<int> mAnimationClipIDs = new List<int>();
        private List<string> mAnimationClips = new List<string>();
        private List<string> mAnimationPaths = new List<string>();
        private List<string> mAnimationNames = new List<string>();

        /// <summary>
        /// Creates all the auto-generated scripts that can be used inside of the motion. 
        /// </summary>
        /// <param name="rAnimatorController"></param>
        /// <param name="rAnimatorLayer"></param>
        /// <param name="rAnimatorSMName"></param>
        protected virtual void GenerateScripts(AnimatorController rAnimatorController, int rAnimatorLayer, string rAnimatorSMName)
        {
            AnimatorStateMachine lRootStateMachine = rAnimatorController.layers[rAnimatorLayer].stateMachine;
            string lSMID = "lSM_" + CleanName(lRootStateMachine.GetInstanceID());

            Vector3 lMotionStateMachinePosition = Vector3.zero;
            AnimatorStateMachine lMotionStateMachine = null;

            // If we find the sm with our name, save it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                if (lRootStateMachine.stateMachines[i].stateMachine.name == rAnimatorSMName)
                {
                    lMotionStateMachinePosition = lRootStateMachine.stateMachines[i].position;
                    lMotionStateMachine = lRootStateMachine.stateMachines[i].stateMachine;
                    break;
                }
            }

            // If we couldn't find the state machine, bail
            if (lMotionStateMachine == null)
            {
                EditorUtility.DisplayDialog("Motion Controller", "'" + rAnimatorSMName + "' state machine not found.", "ok");
                return;
            }

            // Start parsing the information
            mUniqueStateNames.Clear();

            mAnimatorStateMachines.Clear();
            mAnimatorStateMachinePaths.Clear();

            mAnimatorStates.Clear();
            mAnimatorStatePaths.Clear();
            mAnimatorStateNames.Clear();

            mAnimatorTransitions.Clear();
            mAnimatorTransitionPaths.Clear();
            mAnimatorTransitionNames.Clear();

            mAnimatorMotions.Clear();

            mAnimationClipIDs.Clear();
            mAnimationClips.Clear();
            mAnimationPaths.Clear();
            mAnimationNames.Clear();

            ParseSubStateMachine(lRootStateMachine, lMotionStateMachine, lRootStateMachine.name);
            ParseTransitions(lRootStateMachine);

            StringBuilder lText = new StringBuilder();
            lText.AppendLine("#region Auto-Generated");
            lText.AppendLine("// ************************************ START AUTO GENERATED ************************************");
            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// These declarations go inside the class so you can test for which state");
            lText.AppendLine("/// and transitions are active. Testing hash values is much faster than strings.");
            lText.AppendLine("/// </summary>");

            // Write out any of the 'Any State' transitions
            List<string> lIsAdded = new List<string>();

            //for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            //{
            //    for (int j = 0; j < mAnimatorStates.Count; j++)
            //    {
            //        if (lRootStateMachine.anyStateTransitions[i].destinationState == mAnimatorStates[j])
            //        {
            //            string lTransitionName = "TRANS_AnyState_" + CleanName(mAnimatorStateNames[j]);
            //            if (!lIsAdded.Contains(lTransitionName))
            //            {
            //                lIsAdded.Add(lTransitionName);
            //                lText.AppendLine("public static int " + lTransitionName + " = -1;");
            //            }
            //        }
            //    }
            //}

            // Write out all the states
            lIsAdded.Clear();

            for (int i = 0; i < mAnimatorStateNames.Count; i++)
            {
                string lStateName = "STATE_" + CleanName(mAnimatorStateNames[i]);
                if (!lIsAdded.Contains(lStateName))
                {
                    lIsAdded.Add(lStateName);
                    lText.AppendLine("public static int " + lStateName + " = -1;");
                }
            }

            // Write out all the transitions
            lIsAdded.Clear();

            for (int i = 0; i < mAnimatorTransitionNames.Count; i++)
            {
                string lTransitionName = "TRANS_" + CleanName(mAnimatorTransitionNames[i]);
                if (!lIsAdded.Contains(lTransitionName))
                {
                    lIsAdded.Add(lTransitionName);
                    lText.AppendLine("public static int " + lTransitionName + " = -1;");
                }
            }

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Determines if we're using auto-generated code");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override bool HasAutoGeneratedCode");
            lText.AppendLine("{");
            lText.AppendLine("    get { return true; }");
            lText.AppendLine("}");

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("/// <returns></returns>");
            lText.AppendLine("public override bool IsInMotionState");
            lText.AppendLine("{");
            lText.AppendLine("    get");
            lText.AppendLine("    {");
            lText.AppendLine("        int lStateID = mMotionLayer._AnimatorStateID;");
            lText.AppendLine("        int lTransitionID = mMotionLayer._AnimatorTransitionID;");
            lText.AppendLine("");

            for (int i = 0; i < mAnimatorStateNames.Count; i++)
            {
                lText.AppendLine("        if (lStateID == STATE_" + CleanName(mAnimatorStateNames[i]) + ") { return true; }");
            }

            for (int i = 0; i < mAnimatorTransitionNames.Count; i++)
            {
                lText.AppendLine("        if (lTransitionID == TRANS_" + CleanName(mAnimatorTransitionNames[i]) + ") { return true; }");
            }

            lText.AppendLine("        return false;");
            lText.AppendLine("    }");
            lText.AppendLine("}");

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("/// <returns></returns>");
            lText.AppendLine("public override bool IsMotionState(int rStateID)");
            lText.AppendLine("{");

            for (int i = 0; i < mAnimatorStateNames.Count; i++)
            {
                lText.AppendLine("    if (rStateID == STATE_" + CleanName(mAnimatorStateNames[i]) + ") { return true; }");
            }

            lText.AppendLine("    return false;");
            lText.AppendLine("}");

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("/// <returns></returns>");
            lText.AppendLine("public override bool IsMotionState(int rStateID, int rTransitionID)");
            lText.AppendLine("{");

            for (int i = 0; i < mAnimatorStateNames.Count; i++)
            {
                lText.AppendLine("    if (rStateID == STATE_" + CleanName(mAnimatorStateNames[i]) + ") { return true; }");
            }

            for (int i = 0; i < mAnimatorTransitionNames.Count; i++)
            {
                lText.AppendLine("    if (rTransitionID == TRANS_" + CleanName(mAnimatorTransitionNames[i]) + ") { return true; }");
            }

            lText.AppendLine("    return false;");
            lText.AppendLine("}");

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Preprocess any animator data so the motion can use it later");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override void LoadAnimatorData()");
            lText.AppendLine("{");

            // Extract the any states that transition to this state
            for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < mAnimatorStates.Count; j++)
                {
                    if (lRootStateMachine.anyStateTransitions[i].destinationState == mAnimatorStates[j])
                    {
                        string lStatePath = mAnimatorStatePaths[j];
                        lText.AppendLine("    TRANS_AnyState_" + CleanName(mAnimatorStateNames[j]) + " = mMotionController.AddAnimatorName(\"AnyState -> " + lStatePath + "\");");
                        lText.AppendLine("    TRANS_EntryState_" + CleanName(mAnimatorStateNames[j]) + " = mMotionController.AddAnimatorName(\"Entry -> " + lStatePath + "\");");

                        break;
                    }
                }
            }

            // Create the string to paste
            for (int i = 0; i < mAnimatorStates.Count; i++)
            {
                AnimatorState lState = mAnimatorStates[i];

                lText.AppendLine("    STATE_" + CleanName(mAnimatorStateNames[i]) + " = mMotionController.AddAnimatorName(\"" + mAnimatorStatePaths[i] + "\");");
            
                for (int j = 0; j < lState.transitions.Length; j++)
                {
                    int lTransitionIndex = mAnimatorTransitions.IndexOf(lState.transitions[j]);
                    string lTransitionName = mAnimatorTransitionNames[lTransitionIndex];
                    string lTransitionPath = mAnimatorTransitionPaths[lTransitionIndex];

                    if (lState.transitions[j].isExit)
                    {
                        lText.AppendLine("    TRANS_" + CleanName(mAnimatorStateNames[i]) + "_ExitState = mMotionController.AddAnimatorName(\"" + mAnimatorStatePaths[i] + " -> Exit\");");
                    }
                    else if (lState.transitions[j].destinationState != null)
                    {
                        //string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.transitions[j].destinationState.name;
                        lText.AppendLine("    TRANS_" + CleanName(lTransitionName) + " = mMotionController.AddAnimatorName(\"" + lTransitionPath + "\");");
                    }
                    else if (lState.transitions[j].destinationStateMachine != null)
                    {
                        //string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.transitions[j].destinationStateMachine.name;
                        lText.AppendLine("    TRANS_" + CleanName(lTransitionName) + " = mMotionController.AddAnimatorName(\"" + lTransitionPath + "\");");
                    }
                }
            }

            lText.AppendLine("}");
            lText.AppendLine();

            lText.AppendLine("#if UNITY_EDITOR");
            lText.AppendLine();

            for (int i = 0; i < mAnimationClipIDs.Count; i++)
            {
                lText.AppendLine("private AnimationClip m" + CleanName(mAnimationClipIDs[i]) + " = null;");
            }

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Creates the animator substate machine for this motion.");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("protected override void CreateStateMachine()");
            lText.AppendLine("{");

            lText.AppendLine("    // Grab the root sm for the layer");
            lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;");
            lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine " + lSMID + " = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;");
            lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lRootSubStateMachine = null;");
            lText.AppendLine();
            lText.AppendLine("    // If we find the sm with our name, remove it");
            lText.AppendLine("    for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)");
            lText.AppendLine("    {");
            lText.AppendLine("        // Look for a sm with the matching name");
            lText.AppendLine("        if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)");
            lText.AppendLine("        {");
            lText.AppendLine("            lRootSubStateMachine = lRootStateMachine.stateMachines[i].stateMachine;");
            lText.AppendLine();
            lText.AppendLine("            // Allow the user to stop before we remove the sm");
            lText.AppendLine("            if (!UnityEditor.EditorUtility.DisplayDialog(\"Motion Controller\", _EditorAnimatorSMName + \" already exists. Delete and recreate it?\", \"Yes\", \"No\"))");
            lText.AppendLine("            {");
            lText.AppendLine("                return;");
            lText.AppendLine("            }");
            lText.AppendLine();
            lText.AppendLine("            // Remove the sm");
            lText.AppendLine("            //lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);");
            lText.AppendLine("            break;");
            lText.AppendLine("        }");
            lText.AppendLine("    }");

            CreateRootSubStateMachine(lSMID, lMotionStateMachine, "_EditorAnimatorSMName", lMotionStateMachinePosition, lText);

            lText.AppendLine();

            // Extract the any states that transition to this state
            for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < mAnimatorStates.Count; j++)
                {
                    if (lRootStateMachine.anyStateTransitions[i].destinationState == mAnimatorStates[j])
                    {
                        AnimatorStateTransition lTransition = lRootStateMachine.anyStateTransitions[i];
                        string lTransitionID = "lT_" + CleanName(lTransition.GetInstanceID());

                        AnimatorState lDestinationState = lTransition.destinationState;
                        string lDestinationStateID = "lS_" + CleanName(lDestinationState.GetInstanceID());

                        lText.AppendLine("    // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root");
                        lText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition " + lTransitionID + " = lRootStateMachine.AddAnyStateTransition(" + lDestinationStateID + ");");
                        lText.AppendLine("    " + lTransitionID + ".hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
                        lText.AppendLine("    " + lTransitionID + ".hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
                        lText.AppendLine("    " + lTransitionID + ".exitTime = " + lTransition.exitTime + "f;");
                        lText.AppendLine("    " + lTransitionID + ".duration = " + lTransition.duration + "f;");
                        lText.AppendLine("    " + lTransitionID + ".offset = " + lTransition.offset + "f;");
                        lText.AppendLine("    " + lTransitionID + ".mute = " + (lTransition.mute ? "true" : "false") + ";");
                        lText.AppendLine("    " + lTransitionID + ".solo = " + (lTransition.solo ? "true" : "false") + ";");
                        lText.AppendLine("    " + lTransitionID + ".canTransitionToSelf = " + (lTransition.canTransitionToSelf ? "true" : "false") + ";");

                        for (int k = 0; k < lTransition.conditions.Length; k++)
                        {
                            AnimatorCondition lCondition = lTransition.conditions[k];
                            lText.AppendLine("    " + lTransitionID + ".AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
                        }

                        lText.AppendLine();
                    }
                }
            }

            CreateTransitions(lText);

            lText.AppendLine("}");
            lText.AppendLine();

            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Gathers the animations so we can use them when creating the sub-state machine.");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override void FindAnimations()");
            lText.AppendLine("{");

            // Generate the animation clip fields
            for (int i = 0; i < mAnimationClipIDs.Count; i++)
            {
                string lClipPath = mAnimationPaths[i];
                string lClipName = "m" + CleanName(mAnimationClipIDs[i]);

                if (lClipPath.IndexOf(mAnimationClips[i] + ".anim") >= 0)
                {
                    lText.AppendLine("    " + lClipName + " = FindAnimationClip(\"" + lClipPath + "\", \"" + mAnimationClips[i] + "\");");
                }
                else
                {
                    lText.AppendLine("    " + lClipName + " = FindAnimationClip(\"" + lClipPath + "/" + mAnimationClips[i] + ".anim\", \"" + mAnimationClips[i] + "\");");
                }
            }

            lText.AppendLine();
            lText.AppendLine("    // Add the remaining functionality");
            lText.AppendLine("    base.FindAnimations();");
            lText.AppendLine("}");
            lText.AppendLine();

            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Used to show the settings that allow us to generate the animator setup.");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override void OnSettingsGUI()");
            lText.AppendLine("{");
            lText.AppendLine("    UnityEditor.EditorGUILayout.IntField(new GUIContent(\"Phase ID\", \"Phase ID used to transition to the state.\"), PHASE_START);");

            // Generate the animation clip fields
            for (int i = 0; i < mAnimationClipIDs.Count; i++)
            {
                string lClipPath = mAnimationPaths[i];
                string lClipName = "m" + CleanName(mAnimationClipIDs[i]);

                if (lClipPath.IndexOf(mAnimationClips[i] + ".anim") >= 0)
                {
                    lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + mAnimationNames[i] + "\", \"" + lClipPath + "\", \"" + mAnimationClips[i] + "\", " + lClipName + ");");
                }
                else
                {
                    lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + mAnimationNames[i] + "\", \"" + lClipPath + "/" + mAnimationClips[i] + ".anim\", \"" + mAnimationClips[i] + "\", " + lClipName + ");");
                }
            }

            lText.AppendLine();
            lText.AppendLine("    // Add the remaining functionality");
            lText.AppendLine("    base.OnSettingsGUI();");
            lText.AppendLine("}");
            lText.AppendLine();
            lText.AppendLine("#endif");
            lText.AppendLine();

            lText.AppendLine("// ************************************ END AUTO GENERATED ************************************");
            lText.AppendLine("#endregion");

            // Move the string to the copy buffer
            EditorGUIUtility.systemCopyBuffer = lText.ToString();
            EditorUtility.DisplayDialog("Motion Controller", "'" + rAnimatorSMName + "' identifiers copied to the clipboard. You may now paste it into your motion.", "ok");
        }

        /// <summary>
        /// A sub-state machine can have:
        /// Sub-state machines
        /// States
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rText"></param>
        private void ParseSubStateMachine(AnimatorStateMachine rParentStateMachine, AnimatorStateMachine rStateMachine, string rPath)
        {
            if (!mAnimatorStateMachines.Contains(rStateMachine))
            {
                mAnimatorStateMachines.Add(rStateMachine);
                mAnimatorStateMachinePaths.Add(rPath + "." + rStateMachine.name);
            }

            for (int i = 0; i < rStateMachine.stateMachines.Length; i++)
            {
                ParseSubStateMachine(rStateMachine, rStateMachine.stateMachines[i].stateMachine, rPath + "." + rStateMachine.name);
            }

            for (int i = 0; i < rStateMachine.states.Length; i++)
            {
                ParseState(rStateMachine, rStateMachine.states[i].state, rPath + "." + rStateMachine.name);
            }
        }

        /// <summary>
        /// A state can have:
        /// Transitions
        /// Motion (a motion is an animation-clip or a blend-tree)
        /// </summary>
        /// <param name="rState"></param>
        /// <param name="rText"></param>
        private void ParseState(AnimatorStateMachine rStateMachine, AnimatorState rState, string rPath)
        {
            if (!mAnimatorStates.Contains(rState))
            {
                mAnimatorStates.Add(rState);

                string lName = rState.name;

                if (!mUniqueStateNames.Contains(lName))
                {
                    mUniqueStateNames.Add(lName);
                }
                else
                {
                    lName = rPath + "." + rState.name;
                }

                mAnimatorStateNames.Add(lName);

                mAnimatorStatePaths.Add(rPath + "." + rState.name);
            }

            ParseMotion(rState, rState.motion);
        }

        /// <summary>
        /// A motion is an animation-clip or a blend-tree
        /// </summary>
        /// <param name="rState"></param>
        /// <param name="rMotion"></param>
        /// <param name="rText"></param>
        private void ParseMotion(AnimatorState rState, Motion rMotion)
        {
            if (!mAnimatorMotions.Contains(rMotion)) { mAnimatorMotions.Add(rMotion); }

            if (rMotion is AnimationClip)
            {
                ParseAnimationClip(rState, rMotion as AnimationClip);
            }
            else if (rMotion is BlendTree)
            {
                ParseBlendTree(rState, rMotion as BlendTree);
            }
        }

        /// <summary>
        /// A blend-tree can have nested motions:
        /// Motion (a motion in an animation clip or a blend tree)
        /// </summary>
        /// <param name="rState"></param>
        /// <param name="rBlendTree"></param>
        /// <param name="rText"></param>
        private void ParseBlendTree(AnimatorState rState, BlendTree rBlendTree)
        {
            for (int i = 0; i < rBlendTree.children.Length; i++)
            {
                ParseMotion(rState, rBlendTree.children[i].motion);
            }
        }

        private void ParseAnimationClip(AnimatorState rState, AnimationClip rAnimationClip)
        {
            if (!mAnimationClipIDs.Contains(rAnimationClip.GetInstanceID()))
            {
                mAnimationClipIDs.Add(rAnimationClip.GetInstanceID());
                mAnimationClips.Add(rAnimationClip.name);
                mAnimationPaths.Add(AssetDatabase.GetAssetPath(rAnimationClip));

                string lName = rState.name;
                if (rAnimationClip.name != lName) { lName = lName + "." + rAnimationClip.name; }

                mAnimationNames.Add(lName);
            }
        }

        /// <summary>
        /// Extract information about the transforms
        /// </summary>
        /// <param name="rPath"></param>
        private void ParseTransitions(AnimatorStateMachine rStateMachine)
        {
            //string lStateMachinePath = rStateMachine.name;

            for (int i = 0; i < rStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < mAnimatorStates.Count; j++)
                {
                    if (rStateMachine.anyStateTransitions[i].destinationState == mAnimatorStates[j])
                    {
                        AnimatorStateTransition lTransition = rStateMachine.anyStateTransitions[i];
                        if (!mAnimatorTransitions.Contains(lTransition))
                        {
                            mAnimatorTransitions.Add(lTransition);

                            string lName = "AnyState_" + mAnimatorStateNames[j];
                            mAnimatorTransitionNames.Add(lName);

                            string lPath = "AnyState" + " -> " + mAnimatorStatePaths[j];
                            mAnimatorTransitionPaths.Add(lPath);

                            mAnimatorTransitions.Add(lTransition);

                            lName = "EntryState_" + mAnimatorStateNames[j];
                            mAnimatorTransitionNames.Add(lName);

                            lPath = "Entry" + " -> " + mAnimatorStatePaths[j];
                            mAnimatorTransitionPaths.Add(lPath);
                        }

                        break;
                    }
                }
            }

            for (int i = 0; i < mAnimatorStates.Count; i++)
            {
                AnimatorState lState = mAnimatorStates[i];
                for (int j = 0; j < lState.transitions.Length; j++)
                {
                    AnimatorStateTransition lTransition = lState.transitions[j];
                    if (!mAnimatorTransitions.Contains(lTransition))
                    {
                        mAnimatorTransitions.Add(lTransition);

                        string lName = lTransition.name;
                        if (lName.Length == 0)
                        {
                            if (lTransition.destinationState != null)
                            {
                                int lOutIndex = mAnimatorStates.IndexOf(lTransition.destinationState);
                                lName = mAnimatorStateNames[i] + "_" + mAnimatorStateNames[lOutIndex];
                            }
                            else if (lTransition.destinationStateMachine != null)
                            {
                                lName = mAnimatorStateNames[i] + "_" + lTransition.destinationStateMachine.name;
                            }
                        }

                        mAnimatorTransitionNames.Add(lName);

                        string lPath = lTransition.name;
                        if (lPath.Length == 0)
                        {
                            if (lTransition.destinationState != null)
                            {
                                int lOutIndex = mAnimatorStates.IndexOf(lTransition.destinationState);
                                lPath = mAnimatorStatePaths[i] + " -> " + mAnimatorStatePaths[lOutIndex];
                            }
                            else if (lTransition.destinationStateMachine != null)
                            {
                                int lOutIndex = mAnimatorStateMachines.IndexOf(lTransition.destinationStateMachine);
                                lPath = mAnimatorStatePaths[i] + " -> " + mAnimatorStateMachinePaths[lOutIndex];
                            }
                        }

                        mAnimatorTransitionPaths.Add(lPath);
                    }
                }
            }
        }

        /// <summary>
        /// A sub-state machine can have:
        /// Sub-state machines
        /// States
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rText"></param>
        private void CreateRootSubStateMachine(string rParentID, AnimatorStateMachine rStateMachine, string rName, Vector3 rPosition, StringBuilder rText)
        {
            string lSMID = "lSM_" + CleanName(rStateMachine.GetInstanceID());
            string lName = rStateMachine.name;
            Vector3 lPosition = rPosition;

            rText.AppendLine();
            rText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine " + lSMID + " = lRootSubStateMachine;");
            rText.AppendLine("    if (" + lSMID + " != null)");
            rText.AppendLine("    {");
            rText.AppendLine("        for (int i = " + lSMID + ".entryTransitions.Length - 1; i >= 0; i--)");
            rText.AppendLine("        {");
            rText.AppendLine("            " + lSMID + ".RemoveEntryTransition(" + lSMID + ".entryTransitions[i]);");
            rText.AppendLine("        }");
            rText.AppendLine();
            rText.AppendLine("        for (int i = " + lSMID + ".anyStateTransitions.Length - 1; i >= 0; i--)");
            rText.AppendLine("        {");
            rText.AppendLine("            " + lSMID + ".RemoveAnyStateTransition(" + lSMID + ".anyStateTransitions[i]);");
            rText.AppendLine("        }");
            rText.AppendLine();
            rText.AppendLine("        for (int i = " + lSMID + ".states.Length - 1; i >= 0; i--)");
            rText.AppendLine("        {");
            rText.AppendLine("            " + lSMID + ".RemoveState(" + lSMID + ".states[i].state);");
            rText.AppendLine("        }");
            rText.AppendLine();
            rText.AppendLine("        for (int i = " + lSMID + ".stateMachines.Length - 1; i >= 0; i--)");
            rText.AppendLine("        {");
            rText.AppendLine("            " + lSMID + ".RemoveStateMachine(" + lSMID + ".stateMachines[i].stateMachine);");
            rText.AppendLine("        }");
            rText.AppendLine("    }");
            rText.AppendLine("    else");
            rText.AppendLine("    {");

            if (rName.Length > 0)
            {
                rText.AppendLine("    " + lSMID + " = " + rParentID + ".AddStateMachine(" + rName + ", new Vector3(" + lPosition.x + ", " + lPosition.y + ", " + lPosition.z + "));");
            }
            else
            {
                rText.AppendLine("    " + lSMID + " = " + rParentID + ".AddStateMachine(\"" + lName + "\", new Vector3(" + lPosition.x + ", " + lPosition.y + ", " + lPosition.z + "));");
            }

            rText.AppendLine("    }");

            for (int i = 0; i < rStateMachine.states.Length; i++)
            {
                CreateState(lSMID, rStateMachine.states[i].state, rStateMachine.states[i].position, rText);
            }

            for (int i = 0; i < rStateMachine.stateMachines.Length; i++)
            {
                CreateSubStateMachine(lSMID, rStateMachine.stateMachines[i].stateMachine, "", rStateMachine.stateMachines[i].position, rText);
            }
        }

        /// <summary>
        /// A sub-state machine can have:
        /// Sub-state machines
        /// States
        /// </summary>
        /// <param name="rStateMachine"></param>
        /// <param name="rText"></param>
        private void CreateSubStateMachine(string rParentID, AnimatorStateMachine rStateMachine, string rName, Vector3 rPosition, StringBuilder rText)
        {
            string lSMID = "lSM_" + CleanName(rStateMachine.GetInstanceID());
            string lName = rStateMachine.name;
            Vector3 lPosition = rPosition;

            rText.AppendLine();

            if (rName.Length > 0)
            {
                rText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine " + lSMID + " = " + rParentID + ".AddStateMachine(" + rName + ", new Vector3(" + lPosition.x + ", " + lPosition.y + ", " + lPosition.z + "));");
            }
            else
            {
                rText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine " + lSMID + " = " + rParentID + ".AddStateMachine(\"" + lName + "\", new Vector3(" + lPosition.x + ", " + lPosition.y + ", " + lPosition.z + "));");
            }

            for (int i = 0; i < rStateMachine.states.Length; i++)
            {
                CreateState(lSMID, rStateMachine.states[i].state, rStateMachine.states[i].position, rText);
            }

            for (int i = 0; i < rStateMachine.stateMachines.Length; i++)
            {
                CreateSubStateMachine(lSMID, rStateMachine.stateMachines[i].stateMachine, "", rStateMachine.stateMachines[i].position, rText);
            }
        }

        /// <summary>
        /// A state can have:
        /// Motion (a motion is an animation-clip or a blend-tree)
        /// </summary>
        /// <param name="rState"></param>
        /// <param name="rText"></param>
        private void CreateState(string rParentID, AnimatorState rState, Vector3 rPosition, StringBuilder rText)
        {
            string lStateID = "lS_" + CleanName(rState.GetInstanceID());
            string lName = rState.name;
            Vector3 lPosition = rPosition;

            rText.AppendLine();
            rText.AppendLine("    UnityEditor.Animations.AnimatorState " + lStateID + " = " + rParentID + ".AddState(\"" + lName + "\", new Vector3(" + lPosition.x + ", " + lPosition.y + ", " + lPosition.z + "));");
            rText.AppendLine("    " + lStateID + ".speed = " + rState.speed + "f;");

            if (rState.motion is AnimationClip)
            {
                CreateAnimationClip(lStateID, rState.motion as AnimationClip, rText);
            }
            else if (rState.motion is BlendTree)
            {
                CreateBlendTree(lStateID, rState.motion as BlendTree, rText);
            }
        }

        /// <summary>
        /// A blend-tree can have nested motions:
        /// Motion (a motion in an animation clip or a blend tree)
        /// </summary>
        /// <param name="rParentID"></param>
        /// <param name="rBlendTree"></param>
        /// <param name="rText"></param>
        private void CreateBlendTree(string rParentID, BlendTree rBlendTree, StringBuilder rText, float rThreshold = -1f, bool rAddAsChild = false)
        {
            string lBlendTreeID = "lM_" + CleanName(rBlendTree.GetInstanceID());

            rText.AppendLine();
            rText.AppendLine("    UnityEditor.Animations.BlendTree " + lBlendTreeID + " = CreateBlendTree(\"" + rBlendTree.name + "\", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);");
            rText.AppendLine("    " + lBlendTreeID + ".blendType = UnityEditor.Animations.BlendTreeType." + rBlendTree.blendType.ToString() + ";");
            rText.AppendLine("    " + lBlendTreeID + ".blendParameter = \"" + rBlendTree.blendParameter + "\";");
            rText.AppendLine("    " + lBlendTreeID + ".blendParameterY = \"" + rBlendTree.blendParameterY + "\";");

#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
            rText.AppendLine("#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)");
            rText.AppendLine("    " + lBlendTreeID + ".useAutomaticThresholds = " + (rBlendTree.useAutomaticThresholds ? "true" : "false") + ";");
            rText.AppendLine("#endif");
#endif

            for (int i = 0; i < rBlendTree.children.Length; i++)
            {
                ChildMotion lChild = rBlendTree.children[i];

                if (rBlendTree.blendType == BlendTreeType.Simple1D)
                {
                    if (lChild.motion == null)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(null, " + lChild.threshold + "f);");
                    }
                    else if (lChild.motion is AnimationClip)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(m" + CleanName(lChild.motion.GetInstanceID()) + ", " + lChild.threshold + "f);");
                    }
                    else if (lChild.motion is BlendTree)
                    {
                        CreateBlendTree(lBlendTreeID, rBlendTree.children[i].motion as BlendTree, rText, lChild.threshold, true);
                    }
                }
                else if (rBlendTree.blendType == BlendTreeType.FreeformCartesian2D ||
                    rBlendTree.blendType == BlendTreeType.FreeformDirectional2D ||
                    rBlendTree.blendType == BlendTreeType.SimpleDirectional2D)
                {
                    if (lChild.motion == null)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(null, new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");
                    }
                    else if (lChild.motion is AnimationClip)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(m" + CleanName(lChild.motion.GetInstanceID()) + ", new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");

                        string lArrayName = lBlendTreeID + "_" + i.ToString() + "_Children";
                        rText.AppendLine("    UnityEditor.Animations.ChildMotion[] " + lArrayName + " = " + lBlendTreeID + ".children;");
                        rText.AppendLine("    " + lArrayName + "[" + lArrayName + ".Length - 1].mirror = " + (lChild.mirror ? "true" : "false") + ";");
                        rText.AppendLine("    " + lArrayName + "[" + lArrayName + ".Length - 1].timeScale = " + lChild.timeScale + "f;");
                        rText.AppendLine("    " + lBlendTreeID + ".children = " + lArrayName + ";");
                        rText.AppendLine();
                    }
                    else if (lChild.motion is BlendTree)
                    {
                        CreateBlendTree(lBlendTreeID, rBlendTree.children[i].motion as BlendTree, rText, lChild.threshold, true);
                    }
                }
                else
                {
                    if (lChild.motion == null)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(null, " + lChild.threshold + "f);");
                    }
                    else if (lChild.motion is AnimationClip)
                    {
                        rText.AppendLine("    " + lBlendTreeID + ".AddChild(m" + CleanName(lChild.motion.GetInstanceID()) + ", " + lChild.threshold + "f);");
                    }
                    else if (lChild.motion is BlendTree)
                    {
                        CreateBlendTree(lBlendTreeID, rBlendTree.children[i].motion as BlendTree, rText, lChild.threshold, true);
                    }
                }
            }

            if (rAddAsChild)
            {
                if (rThreshold >= 0f)
                {
                    rText.AppendLine("    " + rParentID + ".AddChild(" + lBlendTreeID + ", " + rThreshold + "f);");
                }
                else
                {
                    rText.AppendLine("    " + rParentID + ".AddChild(" + lBlendTreeID + ");");
                }
            }
            else
            {
                rText.AppendLine("    " + rParentID + ".motion = " + lBlendTreeID + ";");
            }
        }

        /// <summary>
        /// An animation clip is a simple reference
        /// </summary>
        /// <param name="rParentID"></param>
        /// <param name="rAnimationClip"></param>
        /// <param name="rText"></param>
        private void CreateAnimationClip(string rParentID, AnimationClip rAnimationClip, StringBuilder rText)
        {
            rText.AppendLine("    " + rParentID + ".motion = m" + CleanName(rAnimationClip.GetInstanceID()) + ";");
        }

        /// <summary>
        /// Now that the states are created, we can create all the transitions
        /// </summary>
        /// <param name="rText"></param>
        private void CreateTransitions(StringBuilder rText)
        {
            for (int i = 0; i < mAnimatorStates.Count; i++)
            {
                AnimatorState lState = mAnimatorStates[i];
                string lStateID = "lS_" + CleanName(lState.GetInstanceID());

                for (int j = 0; j < lState.transitions.Length; j++)
                {
                    AnimatorStateTransition lTransition = lState.transitions[j];
                    string lTransitionID = "lT_" + CleanName(lTransition.GetInstanceID());

                    if (lTransition.isExit)
                    {
                        rText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition " + lTransitionID + " = " + lStateID + ".AddExitTransition();");
                    }
                    else if (lTransition.destinationState != null)
                    {
                        AnimatorState lDestinationState = lTransition.destinationState;
                        if (lDestinationState.motion == null)
                        {
                            rText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition " + lTransitionID + " = " + lStateID + ".AddTransition(lRootStateMachine);");
                        }
                        else
                        {
                            string lDestinationStateID = "lS_" + CleanName(lDestinationState.GetInstanceID());
                            rText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition " + lTransitionID + " = " + lStateID + ".AddTransition(" + lDestinationStateID + ");");
                        }
                    }
                    else if (lTransition.destinationStateMachine != null)
                    {
                        string lDestinationStateMachineID = "lSM_" + CleanName(lTransition.destinationStateMachine.GetInstanceID());
                        rText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition " + lTransitionID + " = " + lStateID + ".AddTransition(" + lDestinationStateMachineID + ");");
                    }

                    rText.AppendLine("    " + lTransitionID + ".hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
                    rText.AppendLine("    " + lTransitionID + ".hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
                    rText.AppendLine("    " + lTransitionID + ".exitTime = " + lTransition.exitTime + "f;");
                    rText.AppendLine("    " + lTransitionID + ".duration = " + lTransition.duration + "f;");
                    rText.AppendLine("    " + lTransitionID + ".offset = " + lTransition.offset + "f;");
                    rText.AppendLine("    " + lTransitionID + ".mute = " + (lTransition.mute ? "true" : "false") + ";");
                    rText.AppendLine("    " + lTransitionID + ".solo = " + (lTransition.solo ? "true" : "false") + ";");
                    rText.AppendLine("    " + lTransitionID + ".canTransitionToSelf = " + (lTransition.canTransitionToSelf ? "true" : "false") + ";");

                    for (int k = 0; k < lTransition.conditions.Length; k++)
                    {
                        AnimatorCondition lCondition = lTransition.conditions[k];
                        rText.AppendLine("    " + lTransitionID + ".AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
                    }

                    rText.AppendLine();
                }
            }
        }

        ///// <summary>
        ///// Creates all the auto-generated scripts that can be used inside of the motion. 
        ///// </summary>
        ///// <param name="rAnimatorController"></param>
        ///// <param name="rAnimatorLayer"></param>
        ///// <param name="rAnimatorSMName"></param>
        //protected virtual void GenerateScripts2(AnimatorController rAnimatorController, int rAnimatorLayer, string rAnimatorSMName)
        //{
        //    AnimatorStateMachine lMotionStateMachine = null;
        //    AnimatorStateMachine lRootStateMachine = rAnimatorController.layers[rAnimatorLayer].stateMachine;

        //    // If we find the sm with our name, save it
        //    for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
        //    {
        //        if (lRootStateMachine.stateMachines[i].stateMachine.name == rAnimatorSMName)
        //        {
        //            lMotionStateMachine = lRootStateMachine.stateMachines[i].stateMachine;
        //            break;
        //        }
        //    }

        //    // If we couldn't find the state machine, bail
        //    if (lMotionStateMachine == null)
        //    {
        //        EditorUtility.DisplayDialog("Motion Controller", "'" + rAnimatorSMName + "' state machine not found.", "ok");
        //        return;
        //    }

        //    // Start writing out the identifiers
        //    StringBuilder lText = new StringBuilder();
        //    lText.AppendLine("#region Auto-Generated");
        //    lText.AppendLine("// ************************************ START AUTO GENERATED ************************************");
        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// These declarations go inside the class so you can test for which state");
        //    lText.AppendLine("/// and transitions are active. Testing hash values is much faster than strings.");
        //    lText.AppendLine("/// </summary>");

        //    List<string> lTransitions = new List<string>();
        //    List<string> lStateDeclarations = new List<string>();

        //    // Extract the any states that transition to this state
        //    for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
        //    {
        //        for (int j = 0; j < lMotionStateMachine.states.Length; j++)
        //        {
        //            if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
        //            {
        //                string lTransitionName = "TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name);
        //                if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
        //                if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
        //                {
        //                    lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
        //                }

        //                lTransitionName = "TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name);
        //                if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
        //                if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
        //                {
        //                    lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
        //                }

        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;
        //        if (!lStateDeclarations.Contains("public static int STATE_" + CleanName(lState.name) + " = -1;"))
        //        {
        //            lStateDeclarations.Add("public static int STATE_" + CleanName(lState.name) + " = -1;");
        //        }

        //        for (int j = 0; j < lState.transitions.Length; j++)
        //        {
        //            if (lState.transitions[j].isExit)
        //            {
        //                string lTransitionName = " TRANS_" + CleanName(lState.name) + "_ExitState";
        //                if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
        //                if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
        //                {
        //                    lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
        //                }
        //            }
        //            else
        //            {
        //                string lTransitionName = " TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name);
        //                if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
        //                if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
        //                {
        //                    lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
        //                }
        //            }
        //        }
        //    }

        //    for (int i = 0; i < lStateDeclarations.Count; i++)
        //    {
        //        lText.AppendLine(lStateDeclarations[i]);
        //    }

        //    lStateDeclarations.Clear();

        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("/// <returns></returns>");
        //    lText.AppendLine("public override bool IsInMotionState");
        //    lText.AppendLine("{");
        //    lText.AppendLine("    get");
        //    lText.AppendLine("    {");
        //    lText.AppendLine("        int lStateID = mMotionLayer._AnimatorStateID;");
        //    lText.AppendLine("        int lTransitionID = mMotionLayer._AnimatorTransitionID;");
        //    lText.AppendLine("");

        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;
        //        lText.AppendLine("        if (lStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
        //    }

        //    for (int i = 0; i < lTransitions.Count; i++)
        //    {
        //        lText.AppendLine("        if (lTransitionID == " + lTransitions[i] + ") { return true; }");
        //    }

        //    lText.AppendLine("        return false;");
        //    lText.AppendLine("    }");
        //    lText.AppendLine("}");

        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("/// <returns></returns>");
        //    lText.AppendLine("public override bool IsMotionState(int rStateID)");
        //    lText.AppendLine("{");

        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;
        //        lText.AppendLine("    if (rStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
        //    }

        //    lText.AppendLine("    return false;");
        //    lText.AppendLine("}");

        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Used to determine if the actor is in one of the states for this motion");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("/// <returns></returns>");
        //    lText.AppendLine("public override bool IsMotionState(int rStateID, int rTransitionID)");
        //    lText.AppendLine("{");

        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;
        //        lText.AppendLine("    if (rStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
        //    }

        //    for (int i = 0; i < lTransitions.Count; i++)
        //    {
        //        lText.AppendLine("    if (rTransitionID == " + lTransitions[i] + ") { return true; }");
        //    }

        //    lText.AppendLine("    return false;");
        //    lText.AppendLine("}");

        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Preprocess any animator data so the motion can use it later");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("public override void LoadAnimatorData()");
        //    lText.AppendLine("{");

        //    // Extract the any states that transition to this state
        //    for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
        //    {
        //        for (int j = 0; j < lMotionStateMachine.states.Length; j++)
        //        {
        //            if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
        //            {
        //                string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lRootStateMachine.anyStateTransitions[i].destinationState.name;

        //                if (!lStateDeclarations.Contains("TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"Entry -> " + lNextStateString + "\");"))
        //                {
        //                    lStateDeclarations.Add("TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"Entry -> " + lNextStateString + "\");");
        //                }

        //                if (!lStateDeclarations.Contains("TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"AnyState -> " + lNextStateString + "\");"))
        //                {
        //                    lStateDeclarations.Add("TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"AnyState -> " + lNextStateString + "\");");
        //                }

        //                break;
        //            }
        //        }
        //    }

        //    // Create the string to paste
        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;

        //        string lStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.name;
        //        if (!lStateDeclarations.Contains("STATE_" + CleanName(lState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + "\");"))
        //        {
        //            lStateDeclarations.Add("STATE_" + CleanName(lState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + "\");");
        //        }

        //        for (int j = 0; j < lState.transitions.Length; j++)
        //        {
        //            if (lState.transitions[j].isExit)
        //            {
        //                if (!lStateDeclarations.Contains("TRANS_" + CleanName(lState.name) + "_ExitState = mMotionController.AddAnimatorName(\"" + lStateString + " -> Exit\");"))
        //                {
        //                    lStateDeclarations.Add("TRANS_" + CleanName(lState.name) + "_ExitState = mMotionController.AddAnimatorName(\"" + lStateString + " -> Exit\");");
        //                }
        //            }
        //            else
        //            {
        //                string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.transitions[j].destinationState.name;
        //                if (!lStateDeclarations.Contains("TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + " -> " + lNextStateString + "\");"))
        //                {
        //                    lStateDeclarations.Add("TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + " -> " + lNextStateString + "\");");
        //                }
        //            }
        //        }
        //    }

        //    for (int i = 0; i < lStateDeclarations.Count; i++)
        //    {
        //        lText.AppendLine(lStateDeclarations[i]);
        //    }

        //    lStateDeclarations.Clear();

        //    lText.AppendLine("}");
        //    lText.AppendLine();

        //    lText.AppendLine("#if UNITY_EDITOR");
        //    lText.AppendLine();

        //    List<int> lAnimationClipIDs = new List<int>();
        //    List<string> lAnimationClips = new List<string>();
        //    List<string> lAnimationPaths = new List<string>();
        //    List<string> lAnimationStateNames = new List<string>();

        //    // Add an entry for each of our states
        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;

        //        string lStateName = lState.name;

        //        if (lState.motion is AnimationClip)
        //        {
        //            if (!lAnimationClipIDs.Contains(lState.motion.GetInstanceID()))
        //            {
        //                lAnimationClipIDs.Add(lState.motion.GetInstanceID());
        //                lAnimationClips.Add(lState.motion.name);
        //                lAnimationPaths.Add(AssetDatabase.GetAssetPath(lState.motion));
        //                lAnimationStateNames.Add(lStateName);
        //            }
        //        }
        //        else if (lState.motion is BlendTree)
        //        {
        //            BlendTree lBlendTree = lState.motion as BlendTree;
        //            for (int j = 0; j < lBlendTree.children.Length; j++)
        //            {
        //                ChildMotion lChild = lBlendTree.children[j];
        //                if (lChild.motion is AnimationClip && !lAnimationClipIDs.Contains(lChild.motion.GetInstanceID()))
        //                {
        //                    lAnimationClipIDs.Add(lChild.motion.GetInstanceID());
        //                    lAnimationClips.Add(lChild.motion.name);
        //                    lAnimationPaths.Add(AssetDatabase.GetAssetPath(lChild.motion));
        //                    lAnimationStateNames.Add(lBlendTree.name + "." + lChild.motion.name);
        //                }
        //            }
        //        }
        //    }

        //    for (int i = 0; i < lAnimationClipIDs.Count; i++)
        //    {
        //        lText.AppendLine("private AnimationClip m" + CleanName(lAnimationClipIDs[i].ToString()) + " = null;");
        //    }

        //    lText.AppendLine();
        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Creates the animator substate machine for this motion.");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("protected override void CreateStateMachine()");
        //    lText.AppendLine("{");

        //    lText.AppendLine("    // Grab the root sm for the layer");
        //    lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;");
        //    lText.AppendLine();

        //    lText.AppendLine("    // If we find the sm with our name, remove it");
        //    lText.AppendLine("    for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)");
        //    lText.AppendLine("    {");
        //    lText.AppendLine("        // Look for a sm with the matching name");
        //    lText.AppendLine("        if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)");
        //    lText.AppendLine("        {");
        //    lText.AppendLine("            // Allow the user to stop before we remove the sm");
        //    lText.AppendLine("            if (!UnityEditor.EditorUtility.DisplayDialog(\"Motion Controller\", _EditorAnimatorSMName + \" already exists. Delete and recreate it?\", \"Yes\", \"No\"))");
        //    lText.AppendLine("            {");
        //    lText.AppendLine("                return;");
        //    lText.AppendLine("            }");
        //    lText.AppendLine();
        //    lText.AppendLine("            // Remove the sm");
        //    lText.AppendLine("            lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);");
        //    lText.AppendLine("        }");
        //    lText.AppendLine("    }");
        //    lText.AppendLine();

        //    lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lMotionStateMachine = lRootStateMachine.AddStateMachine(_EditorAnimatorSMName);");
        //    lText.AppendLine();

        //    lText.AppendLine("    // Attach the behaviour if needed");
        //    lText.AppendLine("    if (_EditorAttachBehaviour)");
        //    lText.AppendLine("    {");
        //    lText.AppendLine("        MotionControllerBehaviour lBehaviour = lMotionStateMachine.AddStateMachineBehaviour(typeof(MotionControllerBehaviour)) as MotionControllerBehaviour;");
        //    lText.AppendLine("        lBehaviour._MotionKey = (_Key.Length > 0 ? _Key : this.GetType().FullName);");
        //    lText.AppendLine("    }");
        //    lText.AppendLine();

        //    // Add an entry for each of our states
        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lState = lMotionStateMachine.states[i].state;

        //        string lStateName = CleanName(lState.name);
        //        Vector3 lStatePosition = lMotionStateMachine.states[i].position;

        //        lText.AppendLine("    UnityEditor.Animations.AnimatorState l" + lStateName + " = lMotionStateMachine.AddState(\"" + lState.name + "\", new Vector3(" + lStatePosition.x + ", " + lStatePosition.y + ", " + lStatePosition.z + "));");

        //        if (lState.motion is AnimationClip)
        //        {
        //            lText.AppendLine("    l" + lStateName + ".motion = m" + CleanName(lState.motion.GetInstanceID().ToString()) + ";");
        //            lText.AppendLine("    l" + lStateName + ".speed = " + lState.speed + "f;");
        //        }
        //        else if (lState.motion is BlendTree)
        //        {
        //            BlendTree lBlendTree = lState.motion as BlendTree;
        //            lText.AppendLine("    l" + lStateName + ".motion = CreateBlendTree(\"" + lState.name + "\", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);");
        //            lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendType = UnityEditor.Animations.BlendTreeType." + lBlendTree.blendType.ToString() + ";"); //BlendTreeType.Simple1D;");
        //            lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendParameter = \"" + lBlendTree.blendParameter + "\";");
        //            lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendParameterY = \"" + lBlendTree.blendParameterY + "\";");

        //            for (int j = 0; j < lBlendTree.children.Length; j++)
        //            {
        //                ChildMotion lChild = lBlendTree.children[j];

        //                if (lBlendTree.blendType == BlendTreeType.FreeformCartesian2D ||
        //                    lBlendTree.blendType == BlendTreeType.FreeformDirectional2D ||
        //                    lBlendTree.blendType == BlendTreeType.SimpleDirectional2D)
        //                {
        //                    if (lChild.motion == null)
        //                    {
        //                        lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(null, new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");
        //                    }
        //                    else
        //                    {
        //                        lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(m" + CleanName(lChild.motion.GetInstanceID().ToString()) + ", new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");
        //                    }
        //                }
        //                else
        //                {
        //                    if (lChild.motion == null)
        //                    {
        //                        lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(null, " + lChild.threshold + "f);");
        //                    }
        //                    else
        //                    {
        //                        lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(m" + CleanName(lChild.motion.GetInstanceID().ToString()) + ", " + lChild.threshold + "f);");
        //                    }
        //                }
        //            }
        //        }

        //        lText.AppendLine();
        //    }

        //    lText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;");
        //    lText.AppendLine();

        //    // Extract the any states that transition to this state
        //    for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
        //    {
        //        for (int j = 0; j < lMotionStateMachine.states.Length; j++)
        //        {
        //            if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
        //            {
        //                AnimatorStateTransition lTransition = lRootStateMachine.anyStateTransitions[i];
        //                AnimatorState lDestinationState = lTransition.destinationState;

        //                lText.AppendLine("    // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root");
        //                lText.AppendLine("    lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(l" + CleanName(lDestinationState.name) + ");");
        //                lText.AppendLine("    lAnyStateTransition.hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
        //                lText.AppendLine("    lAnyStateTransition.hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
        //                lText.AppendLine("    lAnyStateTransition.exitTime = " + lTransition.exitTime + "f;");
        //                lText.AppendLine("    lAnyStateTransition.duration = " + lTransition.duration + "f;");
        //                lText.AppendLine("    lAnyStateTransition.offset = " + lTransition.offset + "f;");
        //                lText.AppendLine("    lAnyStateTransition.mute = " + (lTransition.mute ? "true" : "false") + ";");
        //                lText.AppendLine("    lAnyStateTransition.solo = " + (lTransition.solo ? "true" : "false") + ";");

        //                for (int k = 0; k < lTransition.conditions.Length; k++)
        //                {
        //                    AnimatorCondition lCondition = lTransition.conditions[k];
        //                    lText.AppendLine("    lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
        //                }

        //                lText.AppendLine();
        //            }
        //        }
        //    }

        //    bool lIsStateTransitionDeclared = false;

        //    // Extract the any states that transition to this state
        //    for (int i = 0; i < lMotionStateMachine.states.Length; i++)
        //    {
        //        AnimatorState lSourceState = lMotionStateMachine.states[i].state;

        //        for (int j = 0; j < lSourceState.transitions.Length; j++)
        //        {
        //            if (!lIsStateTransitionDeclared)
        //            {
        //                lText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;");
        //                lText.AppendLine();

        //                lIsStateTransitionDeclared = true;
        //            }

        //            AnimatorStateTransition lTransition = lSourceState.transitions[j];
        //            AnimatorState lDestinationState = lTransition.destinationState;

        //            if (lTransition.isExit)
        //            {
        //                lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddExitTransition();");
        //            }
        //            else if (lDestinationState.motion == null)
        //            {
        //                lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddTransition(lRootStateMachine);");
        //            }
        //            else
        //            {
        //                lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddTransition(l" + CleanName(lDestinationState.name) + ");");
        //            }

        //            lText.AppendLine("    lStateTransition.hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
        //            lText.AppendLine("    lStateTransition.hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
        //            lText.AppendLine("    lStateTransition.exitTime = " + lTransition.exitTime + "f;");
        //            lText.AppendLine("    lStateTransition.duration = " + lTransition.duration + "f;");
        //            lText.AppendLine("    lStateTransition.offset = " + lTransition.offset + "f;");
        //            lText.AppendLine("    lStateTransition.mute = " + (lTransition.mute ? "true" : "false") + ";");
        //            lText.AppendLine("    lStateTransition.solo = " + (lTransition.solo ? "true" : "false") + ";");

        //            for (int k = 0; k < lTransition.conditions.Length; k++)
        //            {
        //                AnimatorCondition lCondition = lTransition.conditions[k];
        //                lText.AppendLine("    lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
        //            }

        //            lText.AppendLine();
        //        }
        //    }

        //    lText.AppendLine("}");
        //    lText.AppendLine();

        //    lText.AppendLine("/// <summary>");
        //    lText.AppendLine("/// Used to show the settings that allow us to generate the animator setup.");
        //    lText.AppendLine("/// </summary>");
        //    lText.AppendLine("public override void OnSettingsGUI()");
        //    lText.AppendLine("{");
        //    lText.AppendLine("    UnityEditor.EditorGUILayout.IntField(new GUIContent(\"Phase ID\", \"Phase ID used to transition to the state.\"), PHASE_START);");

        //    // Generate the animation clip fields
        //    for (int i = 0; i < lAnimationClipIDs.Count; i++)
        //    {
        //        string lClipPath = lAnimationPaths[i];
        //        string lClipName = "m" + CleanName(lAnimationClipIDs[i].ToString());

        //        if (lClipPath.IndexOf(lAnimationClips[i] + ".anim") >= 0)
        //        {
        //            lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + lAnimationStateNames[i] + "\", \"" + lClipPath + "\", \"" + lAnimationClips[i] + "\", " + lClipName + ");");
        //        }
        //        else
        //        {
        //            lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + lAnimationStateNames[i] + "\", \"" + lClipPath + "/" + lAnimationClips[i] + ".anim\", \"" + lAnimationClips[i] + "\", " + lClipName + ");");
        //        }
        //    }

        //    lText.AppendLine();
        //    lText.AppendLine("    // Add the remaining functionality");
        //    lText.AppendLine("    base.OnSettingsGUI();");
        //    lText.AppendLine("}");
        //    lText.AppendLine();
        //    lText.AppendLine("#endif");
        //    lText.AppendLine();

        //    lText.AppendLine("// ************************************ END AUTO GENERATED ************************************");
        //    lText.AppendLine("#endregion");

        //    // Move the string to the copy buffer
        //    EditorGUIUtility.systemCopyBuffer = lText.ToString();
        //    EditorUtility.DisplayDialog("Motion Controller", "'" + rAnimatorSMName + "' identifiers copied.", "ok");
        //}

        /// <summary>
        /// Ensure we have a string friendly state and transition name
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        private string CleanName(string rName)
        {
            rName = rName.Replace(" ", string.Empty);
            rName = rName.Replace(".", string.Empty);
            rName = rName.Replace(",", string.Empty);
            rName = rName.Replace("-", string.Empty);
            rName = rName.Replace(":", string.Empty);
            return rName;
        }

        /// <summary>
        /// Ensure we have a string friendly state and transition name
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        private string CleanName(int rID)
        {
            string lName = Mathf.Abs(rID).ToString();
            if (rID < 0) { lName = "N" + lName; }

            return lName;
        }

#endif
    }
}
