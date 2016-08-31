using System;
using UnityEngine;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors
{
    /// <summary>
    /// Base class for all system controllers for the
    /// actor. This provides basic functionality for updating
    /// and managing the system. This could be the core
    /// character controller, bone controller, etc.
    /// </summary>
    [Serializable]
    public class BaseSystemController : MonoBehaviour
    {
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
        /// Determines if we call update ourselves or wait for 
        /// another component to call it.
        /// </summary>
        public bool _IsInternalUpdateEnabled = true;
        public bool IsInternalUpdateEnabled
        {
            get { return _IsInternalUpdateEnabled; }
            set { _IsInternalUpdateEnabled = value; }
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

            set
            {
                _FixedUpdateFPS = value;
                mFixedUpdateFrameTime = 1f / _FixedUpdateFPS;
            }
        }

        /// <summary>
        /// Tracks the elapsed time this frame so we can pass it along
        /// </summary>
        [NonSerialized]
        public float _DeltaTime = 0f;
        public float DeltaTime
        {
            get { return _DeltaTime; }
        }

        /// <summary>
        /// Tracks the time for each frame that we're targeting
        /// </summary>
        protected float mFixedUpdateFrameTime = 1f / 60f;

        /// <summary>
        /// Determines if this is the first update
        /// </summary>
        protected bool mIsFirstUpdate = true;

        /// <summary>
        /// Number of valid updates to issue this frame
        /// </summary>
        protected int mUpdateCount = 0;

        /// <summary>
        /// Track the current update index we're in
        /// </summary>
        protected int mUpdateIndex = 1;

        /// <summary>
        /// For physics calculations, the amount of time that has elapsed
        /// </summary>
        protected float mFixedElapsedTime = 0f;

        /// <summary>
        /// Track the last time the editor updated
        /// </summary>
        protected float mEditorLastTime = 0f;

        /// <summary>
        /// Allows us to track time in the editor
        /// </summary>
        protected float mEditorDeltaTime = 0f;

#if UNITY_EDITOR

        /// <summary>
        /// Value used in the editor to determine if the component has
        /// been initialized. This should only be processed once in the life-time
        /// of the component.
        /// </summary>
        public bool EditorComponentInitialized = false;

#endif

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected virtual void Awake()
        {
            mFixedUpdateFrameTime = 1f / _FixedUpdateFPS;

            // Cache the transform since gameObject.transform is a 'get'
            _Transform = gameObject.transform;
        }

        /// <summary>
        /// Called every frame to perform processing. We only use
        /// this function if it's not called by another component.
        /// 
        /// NOTE: It's important to realize that when Time.TimeScale is used,
        /// Update() is still called every 0.0167 ms (if possible). However, the
        /// time.deltaTime will be less/more to represent time slowing/speeding.
        /// 
        /// We do this because that's what Unity does and we want to try to stay
        /// in synch with thier animations.
        /// </summary>
        protected virtual void Update()
        {
            if (!_IsInternalUpdateEnabled) { return; }

#if UNITY_EDITOR

            // Time.deltaTime isn't valid when editing. So, we need to create our own here.
            float lCurrentTime = (float)EditorApplication.timeSinceStartup;
            mEditorDeltaTime = lCurrentTime - mEditorLastTime;
            mEditorLastTime = lCurrentTime;

#else

            // Time.deltaTime isn't valid when editing. So, we need to create our own here.
            float lCurrentTime = (float)Time.realtimeSinceStartup;
            mEditorDeltaTime = lCurrentTime - mEditorLastTime;
            mEditorLastTime = lCurrentTime;

#endif

            // Counts how many valid updates we have
            mUpdateCount = 0;

            // Current time since the last frame
            _DeltaTime = Time.deltaTime;

            // Sometimes when debugging, we can get into odd situations with the
            // unity delta time. Ensure we don't do anything crazy.
            if (_DeltaTime > 0.2f)
            {
                _DeltaTime = 1.0f / _FixedUpdateFPS;
            }

            // If we're not fixed, update as fast as possible
            if (!_IsFixedUpdateEnabled || _FixedUpdateFPS <= 0f)
            {
                mUpdateCount = 1;
            }
            // If we are fixed, update on the interval based on our FPS
            else
            {
                _DeltaTime = mFixedUpdateFrameTime * Time.timeScale;

                // We'll cheat a bit. If the delta time is withing 10% of our desired time,
                // We'll just go with the fixed time. 
                if (Mathf.Abs(_DeltaTime - Time.deltaTime) < mFixedUpdateFrameTime * 0.0016f)
                {
                    mUpdateCount = 1;
                }
                // Outside of the 10%, we need to adjust accordingly
                else
                {
                    // Build up our elapsed time
                    mFixedElapsedTime += mEditorDeltaTime;

                    // If the elapsed time exceeds our desired update schedule, it's
                    // time for us to do a physics update. In fact, if the system
                    // is running slow we may need to do multiple updates
                    while (mFixedElapsedTime >= mFixedUpdateFrameTime)
                    {
                        mUpdateCount++;
                        mFixedElapsedTime -= mFixedUpdateFrameTime;

                        // Fail safe. We can have long delta times when debugging and such
                        if (mUpdateCount >= 5)
                        {
                            mFixedElapsedTime = 0;
                            break;
                        }
                    }
                }
            }

            // Do as many updates as we need to in order to simulate
            // the desired frame rates
            if (mUpdateCount > 0)
            {
                // We do this 'first update dance' so we can use the
                // first update variable in LateUpdate too.
                bool lIsFirstUpdate = mIsFirstUpdate;

                // Update the controller itself
                for (mUpdateIndex = 1; mUpdateIndex <= mUpdateCount; mUpdateIndex++)
                {
                    ControllerUpdate(_DeltaTime, mUpdateIndex);
                    mIsFirstUpdate = false;
                }

                // Reset the first update variable for the frame
                mIsFirstUpdate = lIsFirstUpdate;
            }
            // In this case, there shouldn't be an update. This typically
            // happens when the true FPS is much faster than our desired FPS
            else
            {
                mUpdateIndex = 0;
                ControllerUpdate(_DeltaTime, mUpdateIndex);
            }

            // End with a clean index so it doesn't effect things like adding forces
            mUpdateIndex = 1;
        }

        /// <summary>
        /// Called every frame to perform processing. We only use
        /// this function if it's not called by another component.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!_IsInternalUpdateEnabled) { return; }

            // Do as many updates as we need to in order to simulate
            // the desired frame rates
            if (mUpdateCount > 0)
            {
                for (mUpdateIndex = 1; mUpdateIndex <= mUpdateCount; mUpdateIndex++)
                {
                    ControllerLateUpdate(_DeltaTime, mUpdateIndex);
                    mIsFirstUpdate = false;
                }
            }
            // In this case, there shouldn't be an update. This typically
            // happens when the true FPS is much faster than our desired FPS
            else
            {
                mUpdateIndex = 0;
                ControllerLateUpdate(_DeltaTime, mUpdateIndex);
            }

            // End with a clean index so it doesn't effect things like adding forces
            mUpdateIndex = 1;
        }

        /// <summary>
        /// Update logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public virtual void ControllerUpdate(float rDeltaTime, int rUpdateIndex)
        {
        }

        /// <summary>
        /// LateUpdate logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public virtual void ControllerLateUpdate(float rDeltaTime, int rUpdateIndex)
        {
        }
    }
}
