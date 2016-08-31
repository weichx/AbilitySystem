using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;
using com.ootii.Actors;
using com.ootii.Base;
using com.ootii.Geometry;
using com.ootii.Helpers;
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
        /// Raised when the animator's state has changed
        /// </summary>
        public virtual void OnAnimatorStateChange(int rLastStateID, int rNewStateID)
        {
        }

        /// <summary>
        /// Raised by the animation when an event occurs
        /// </summary>
        public virtual void OnAnimationEvent(AnimationEvent rEvent)
        {
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

            // Cycle through all the properties. 
            // Unfortunately Binding flags don't seem to be working. So,
            // we need to ensure we don't include base properties
            PropertyInfo[] lBaseProperties = typeof(MotionControllerMotion).GetProperties();
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

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
                if (lProperty.GetValue(this, null) == null) { continue; }

                JSONNode lValueNode = lDefinitionNode[lProperty.Name];
                if (lValueNode == null) { continue; }

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
        public string _EditorAnimatorSMName = "";
        public bool _EditorAttachBehaviour = false;

        protected bool mShowScriptGenerators = false;

        [NonSerialized]
        public AnimatorController _EditorAnimatorController = null;

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

        /// <summary>
        /// Creates all the auto-generated scripts that can be used inside of the motion. 
        /// </summary>
        /// <param name="rAnimatorController"></param>
        /// <param name="rAnimatorLayer"></param>
        /// <param name="rAnimatorSMName"></param>
        protected virtual void GenerateScripts(AnimatorController rAnimatorController, int rAnimatorLayer, string rAnimatorSMName)
        {
            AnimatorStateMachine lMotionStateMachine = null;
            AnimatorStateMachine lRootStateMachine = rAnimatorController.layers[rAnimatorLayer].stateMachine;

            // If we find the sm with our name, save it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                if (lRootStateMachine.stateMachines[i].stateMachine.name == rAnimatorSMName)
                {
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

            // Start writing out the identifiers
            StringBuilder lText = new StringBuilder();
            lText.AppendLine("#region Auto-Generated");
            lText.AppendLine("// ************************************ START AUTO GENERATED ************************************");
            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// These declarations go inside the class so you can test for which state");
            lText.AppendLine("/// and transitions are active. Testing hash values is much faster than strings.");
            lText.AppendLine("/// </summary>");

            List<string> lTransitions = new List<string>();
            List<string> lStateDeclarations = new List<string>();

            // Extract the any states that transition to this state
            for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < lMotionStateMachine.states.Length; j++)
                {
                    if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
                    {
                        string lTransitionName = "TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name);
                        if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
                        if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
                        {
                            lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
                        }

                        lTransitionName = "TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name);
                        if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
                        if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
                        {
                            lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
                        }

                        break;
                    }
                }
            }

            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;
                if (!lStateDeclarations.Contains("public static int STATE_" + CleanName(lState.name) + " = -1;"))
                {
                    lStateDeclarations.Add("public static int STATE_" + CleanName(lState.name) + " = -1;");
                }

                for (int j = 0; j < lState.transitions.Length; j++)
                {
                    if (lState.transitions[j].isExit)
                    {
                        string lTransitionName = " TRANS_" + CleanName(lState.name) + "_ExitState";
                        if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
                        if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
                        {
                            lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
                        }
                    }
                    else
                    {
                        string lTransitionName = " TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name);
                        if (!lTransitions.Contains(lTransitionName)) { lTransitions.Add(lTransitionName); }
                        if (!lStateDeclarations.Contains("public static int " + lTransitionName + " = -1;"))
                        {
                            lStateDeclarations.Add("public static int " + lTransitionName + " = -1;");
                        }
                    }
                }
            }

            for (int i = 0; i < lStateDeclarations.Count; i++)
            {
                lText.AppendLine(lStateDeclarations[i]);
            }

            lStateDeclarations.Clear();

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

            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;
                lText.AppendLine("        if (lStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
            }

            for (int i = 0; i < lTransitions.Count; i++)
            {
                lText.AppendLine("        if (lTransitionID == " + lTransitions[i] + ") { return true; }");
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

            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;
                lText.AppendLine("    if (rStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
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

            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;
                lText.AppendLine("    if (rStateID == STATE_" + CleanName(lState.name) + ") { return true; }");
            }

            for (int i = 0; i < lTransitions.Count; i++)
            {
                lText.AppendLine("    if (rTransitionID == " + lTransitions[i] + ") { return true; }");
            }

            lText.AppendLine("    return false;");
            lText.AppendLine("}");

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Preprocess any animator data so the motion can use it later");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override void LoadAnimatorData()");
            lText.AppendLine("{");
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// These assignments go inside the 'LoadAnimatorData' function so that we can");
            lText.AppendLine("/// extract and assign the hash values for this run. These are typically used for debugging.");
            lText.AppendLine("/// </summary>");

            // Extract the any states that transition to this state
            for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < lMotionStateMachine.states.Length; j++)
                {
                    if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
                    {
                        string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lRootStateMachine.anyStateTransitions[i].destinationState.name;

                        if (!lStateDeclarations.Contains("TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"Entry -> " + lNextStateString + "\");"))
                        {
                            lStateDeclarations.Add("TRANS_EntryState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"Entry -> " + lNextStateString + "\");");
                        }

                        if (!lStateDeclarations.Contains("TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"AnyState -> " + lNextStateString + "\");"))
                        {
                            lStateDeclarations.Add("TRANS_AnyState_" + CleanName(lRootStateMachine.anyStateTransitions[i].destinationState.name) + " = mMotionController.AddAnimatorName(\"AnyState -> " + lNextStateString + "\");");
                        }

                        break;
                    }
                }
            }

            // Create the string to paste
            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;

                string lStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.name;
                if (!lStateDeclarations.Contains("STATE_" + CleanName(lState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + "\");"))
                {
                    lStateDeclarations.Add("STATE_" + CleanName(lState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + "\");");
                }

                for (int j = 0; j < lState.transitions.Length; j++)
                {
                    if (lState.transitions[j].isExit)
                    {
                        if (!lStateDeclarations.Contains("TRANS_" + CleanName(lState.name) + "_ExitState = mMotionController.AddAnimatorName(\"" + lStateString + " -> Exit\");"))
                        {
                            lStateDeclarations.Add("TRANS_" + CleanName(lState.name) + "_ExitState = mMotionController.AddAnimatorName(\"" + lStateString + " -> Exit\");");
                        }
                    }
                    else
                    {
                        string lNextStateString = lRootStateMachine.name + "." + rAnimatorSMName + "." + lState.transitions[j].destinationState.name;
                        if (!lStateDeclarations.Contains("TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + " -> " + lNextStateString + "\");"))
                        {
                            lStateDeclarations.Add("TRANS_" + CleanName(lState.name) + "_" + CleanName(lState.transitions[j].destinationState.name) + " = mMotionController.AddAnimatorName(\"" + lStateString + " -> " + lNextStateString + "\");");
                        }
                    }
                }
            }

            for (int i = 0; i < lStateDeclarations.Count; i++)
            {
                lText.AppendLine(lStateDeclarations[i]);
            }

            lStateDeclarations.Clear();

            lText.AppendLine("}");
            lText.AppendLine();

            lText.AppendLine("#if UNITY_EDITOR");
            lText.AppendLine();

            List<int> lAnimationClipIDs = new List<int>();
            List<string> lAnimationClips = new List<string>();
            List<string> lAnimationPaths = new List<string>();
            List<string> lAnimationStateNames = new List<string>();

            // Add an entry for each of our states
            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;

                string lStateName = lState.name;

                if (lState.motion is AnimationClip)
                {
                    if (!lAnimationClipIDs.Contains(lState.motion.GetInstanceID()))
                    {
                        lAnimationClipIDs.Add(lState.motion.GetInstanceID());
                        lAnimationClips.Add(lState.motion.name);
                        lAnimationPaths.Add(AssetDatabase.GetAssetPath(lState.motion));
                        lAnimationStateNames.Add(lStateName);
                    }
                }
                else if (lState.motion is BlendTree)
                {
                    BlendTree lBlendTree = lState.motion as BlendTree;
                    for (int j = 0; j < lBlendTree.children.Length; j++)
                    {
                        ChildMotion lChild = lBlendTree.children[j];
                        if (lChild.motion is AnimationClip && !lAnimationClipIDs.Contains(lChild.motion.GetInstanceID()))
                        {
                            lAnimationClipIDs.Add(lChild.motion.GetInstanceID());
                            lAnimationClips.Add(lChild.motion.name);
                            lAnimationPaths.Add(AssetDatabase.GetAssetPath(lChild.motion));
                            lAnimationStateNames.Add(lBlendTree.name + "." + lChild.motion.name);
                        }
                    }
                }
            }

            for (int i = 0; i < lAnimationClipIDs.Count; i++)
            {
                lText.AppendLine("private AnimationClip m" + CleanName(lAnimationClipIDs[i].ToString()) + " = null;");
            }

            lText.AppendLine();
            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Creates the animator substate machine for this motion.");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("protected override void CreateStateMachine()");
            lText.AppendLine("{");

            lText.AppendLine("    // Grab the root sm for the layer");
            lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;");
            lText.AppendLine();

            lText.AppendLine("    // If we find the sm with our name, remove it");
            lText.AppendLine("    for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)");
            lText.AppendLine("    {");
            lText.AppendLine("        // Look for a sm with the matching name");
            lText.AppendLine("        if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)");
            lText.AppendLine("        {");
            lText.AppendLine("            // Allow the user to stop before we remove the sm");
            lText.AppendLine("            if (!UnityEditor.EditorUtility.DisplayDialog(\"Motion Controller\", _EditorAnimatorSMName + \" already exists. Delete and recreate it?\", \"Yes\", \"No\"))");
            lText.AppendLine("            {");
            lText.AppendLine("                return;");
            lText.AppendLine("            }");
            lText.AppendLine();
            lText.AppendLine("            // Remove the sm");
            lText.AppendLine("            lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);");
            lText.AppendLine("        }");
            lText.AppendLine("    }");
            lText.AppendLine();

            lText.AppendLine("    UnityEditor.Animations.AnimatorStateMachine lMotionStateMachine = lRootStateMachine.AddStateMachine(_EditorAnimatorSMName);");
            lText.AppendLine();

            lText.AppendLine("    // Attach the behaviour if needed");
            lText.AppendLine("    if (_EditorAttachBehaviour)");
            lText.AppendLine("    {");
            lText.AppendLine("        MotionControllerBehaviour lBehaviour = lMotionStateMachine.AddStateMachineBehaviour(typeof(MotionControllerBehaviour)) as MotionControllerBehaviour;");
            lText.AppendLine("        lBehaviour._MotionKey = (_Key.Length > 0 ? _Key : this.GetType().FullName);");
            lText.AppendLine("    }");
            lText.AppendLine();

            // Add an entry for each of our states
            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lState = lMotionStateMachine.states[i].state;

                string lStateName = CleanName(lState.name);
                Vector3 lStatePosition = lMotionStateMachine.states[i].position;

                lText.AppendLine("    UnityEditor.Animations.AnimatorState l" + lStateName + " = lMotionStateMachine.AddState(\"" + lState.name + "\", new Vector3(" + lStatePosition.x + ", " + lStatePosition.y + ", " + lStatePosition.z + "));");

                if (lState.motion is AnimationClip)
                {
                    lText.AppendLine("    l" + lStateName + ".motion = m" + CleanName(lState.motion.GetInstanceID().ToString()) + ";");
                    lText.AppendLine("    l" + lStateName + ".speed = " + lState.speed + "f;");
                }
                else if (lState.motion is BlendTree)
                {
                    BlendTree lBlendTree = lState.motion as BlendTree;
                    lText.AppendLine("    l" + lStateName + ".motion = CreateBlendTree(\"" + lState.name + "\", _EditorAnimatorController, mMotionLayer.AnimatorLayerIndex);");
                    lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendType = UnityEditor.Animations.BlendTreeType." + lBlendTree.blendType.ToString() + ";"); //BlendTreeType.Simple1D;");
                    lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendParameter = \"" + lBlendTree.blendParameter + "\";");
                    lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).blendParameterY = \"" + lBlendTree.blendParameterY + "\";");

                    for (int j = 0; j < lBlendTree.children.Length; j++)
                    {
                        ChildMotion lChild = lBlendTree.children[j];

                        if (lBlendTree.blendType == BlendTreeType.FreeformCartesian2D ||
                            lBlendTree.blendType == BlendTreeType.FreeformDirectional2D ||
                            lBlendTree.blendType == BlendTreeType.SimpleDirectional2D)
                        {
                            if (lChild.motion == null)
                            {
                                lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(null, new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");
                            }
                            else
                            {
                                lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(m" + CleanName(lChild.motion.GetInstanceID().ToString()) + ", new Vector2(" + lChild.position.x + "f," + lChild.position.y + "f));");
                            }
                        }
                        else
                        {
                            if (lChild.motion == null)
                            {
                                lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(null, " + lChild.threshold + "f);");
                            }
                            else
                            {
                                lText.AppendLine("    ((UnityEditor.Animations.BlendTree)l" + lStateName + ".motion).AddChild(m" + CleanName(lChild.motion.GetInstanceID().ToString()) + ", " + lChild.threshold + "f);");
                            }
                        }
                    }
                }

                lText.AppendLine();
            }

            lText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;");
            lText.AppendLine();

            // Extract the any states that transition to this state
            for (int i = 0; i < lRootStateMachine.anyStateTransitions.Length; i++)
            {
                for (int j = 0; j < lMotionStateMachine.states.Length; j++)
                {
                    if (lRootStateMachine.anyStateTransitions[i].destinationState == lMotionStateMachine.states[j].state)
                    {
                        AnimatorStateTransition lTransition = lRootStateMachine.anyStateTransitions[i];
                        AnimatorState lDestinationState = lTransition.destinationState;

                        lText.AppendLine("    // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root");
                        lText.AppendLine("    lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(l" + CleanName(lDestinationState.name) + ");");
                        lText.AppendLine("    lAnyStateTransition.hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
                        lText.AppendLine("    lAnyStateTransition.hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
                        lText.AppendLine("    lAnyStateTransition.exitTime = " + lTransition.exitTime + "f;");
                        lText.AppendLine("    lAnyStateTransition.duration = " + lTransition.duration + "f;");
                        lText.AppendLine("    lAnyStateTransition.offset = " + lTransition.offset + "f;");
                        lText.AppendLine("    lAnyStateTransition.mute = " + (lTransition.mute ? "true" : "false") + ";");
                        lText.AppendLine("    lAnyStateTransition.solo = " + (lTransition.solo ? "true" : "false") + ";");

                        for (int k = 0; k < lTransition.conditions.Length; k++)
                        {
                            AnimatorCondition lCondition = lTransition.conditions[k];
                            lText.AppendLine("    lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
                        }

                        lText.AppendLine();
                    }
                }
            }

            bool lIsStateTransitionDeclared = false;

            // Extract the any states that transition to this state
            for (int i = 0; i < lMotionStateMachine.states.Length; i++)
            {
                AnimatorState lSourceState = lMotionStateMachine.states[i].state;

                for (int j = 0; j < lSourceState.transitions.Length; j++)
                {
                    if (!lIsStateTransitionDeclared)
                    {
                        lText.AppendLine("    UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;");
                        lText.AppendLine();

                        lIsStateTransitionDeclared = true;
                    }

                    AnimatorStateTransition lTransition = lSourceState.transitions[j];
                    AnimatorState lDestinationState = lTransition.destinationState;

                    if (lTransition.isExit)
                    {
                        lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddExitTransition();");
                    }
                    else if (lDestinationState.motion == null)
                    {
                        lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddTransition(lRootStateMachine);");
                    }
                    else
                    {
                        lText.AppendLine("    lStateTransition = l" + CleanName(lSourceState.name) + ".AddTransition(l" + CleanName(lDestinationState.name) + ");");
                    }

                    lText.AppendLine("    lStateTransition.hasExitTime = " + (lTransition.hasExitTime ? "true" : "false") + ";");
                    lText.AppendLine("    lStateTransition.hasFixedDuration = " + (lTransition.hasFixedDuration ? "true" : "false") + ";");
                    lText.AppendLine("    lStateTransition.exitTime = " + lTransition.exitTime + "f;");
                    lText.AppendLine("    lStateTransition.duration = " + lTransition.duration + "f;");
                    lText.AppendLine("    lStateTransition.offset = " + lTransition.offset + "f;");
                    lText.AppendLine("    lStateTransition.mute = " + (lTransition.mute ? "true" : "false") + ";");
                    lText.AppendLine("    lStateTransition.solo = " + (lTransition.solo ? "true" : "false") + ";");

                    for (int k = 0; k < lTransition.conditions.Length; k++)
                    {
                        AnimatorCondition lCondition = lTransition.conditions[k];
                        lText.AppendLine("    lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode." + lCondition.mode.ToString() + ", " + lCondition.threshold + "f, \"" + lCondition.parameter + "\");");
                    }

                    lText.AppendLine();
                }
            }

            lText.AppendLine("}");
            lText.AppendLine();

            lText.AppendLine("/// <summary>");
            lText.AppendLine("/// Used to show the settings that allow us to generate the animator setup.");
            lText.AppendLine("/// </summary>");
            lText.AppendLine("public override void OnSettingsGUI()");
            lText.AppendLine("{");
            lText.AppendLine("    UnityEditor.EditorGUILayout.IntField(new GUIContent(\"Phase ID\", \"Phase ID used to transition to the state.\"), PHASE_START);");

            // Generate the animation clip fields
            for (int i = 0; i < lAnimationClipIDs.Count; i++)
            {
                string lClipPath = lAnimationPaths[i];
                string lClipName = "m" + CleanName(lAnimationClipIDs[i].ToString());

                if (lClipPath.IndexOf(lAnimationClips[i] + ".anim") >= 0)
                {
                    lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + lAnimationStateNames[i] + "\", \"" + lClipPath + "\", \"" + lAnimationClips[i] + "\", " + lClipName + ");");
                }
                else
                {
                    lText.AppendLine("    " + lClipName + " = CreateAnimationField(\"" + lAnimationStateNames[i] + "\", \"" + lClipPath + "/" + lAnimationClips[i] + ".anim\", \"" + lAnimationClips[i] + "\", " + lClipName + ");");
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
            EditorUtility.DisplayDialog("Motion Controller", "'" + rAnimatorSMName + "' identifiers copied.", "ok");
        }

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
