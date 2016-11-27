using System;
using UnityEngine;
using com.ootii.Base;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Cameras
{
    /// <summary>
    /// Delegate to support update events
    /// </summary>
    public delegate void CameraUpdateEvent(float rDeltaTime, int rUpdateCount, BaseCameraRig rCamera);

    /// <summary>
    /// Camera rigs are used to position and rotate
    /// a unity camera. Think of a unity camera as a lense that
    /// can have filters, a field of view, etc. While they also
    /// can have position and rotation, we use the camera rig
    /// do manage these so we can manage it better
    /// </summary>
    public class BaseCameraRig : MonoBehaviour, IBaseCameraRig
    {
        /// <summary>
        /// This transform. We cache this so we're not actually doing a Get everytime we access 'transform'
        /// </summary>
        [NonSerialized]
        public Transform _Transform = null;
        public virtual Transform Transform
        {
            get { return _Transform; }
        }

        /// <summary>
        /// The camera that is mounted on the rig. This is the 'lens'
        /// that we'll actually see through.
        /// </summary>
        [NonSerialized]
        [HideInInspector]
        public Camera _Camera;
        public virtual Camera Camera
        {
            get { return _Camera; }
        }

        /// <summary>
        /// The type of the camera in order to help determine
        /// how the camera moves and rotates.
        /// </summary>
        public int _Mode = 0;
        public virtual int Mode
        {
            get { return _Mode; }
            set { _Mode = value; }
        }

        /// <summary>
        /// If we lock the mode, we won't change it based on input values
        /// </summary>
        protected bool mLockMode = false;
        public virtual bool LockMode
        {
            get { return mLockMode; }
            set { mLockMode = value; }
        }
        
        /// <summary>
        /// Transform that represents the anchor we want to follow
        /// </summary>
        public Transform _Anchor = null;
        public virtual Transform Anchor
        {
            get { return _Anchor; }
            set { _Anchor = value; }
        }

        /// <summary>
        /// Flag that allows an external caller to tell the camera
        /// to remember its forward direction while finishing a transition.
        /// The exact implementation is up to the camera itself.
        /// </summary>
        protected bool mFrameLockForward = false;
        public virtual bool FrameLockForward
        {
            get { return mFrameLockForward; }
            set { mFrameLockForward = value; }
        }

        /// <summary>
        /// Conviencence flag that allows an external caller to tell the
        /// camera to follow the anchor. The exact implementation is up
        /// to the camera itself.
        /// </summary>
        public bool _FrameForceToFollowAnchor = false;
        public virtual bool FrameForceToFollowAnchor
        {
            get { return _FrameForceToFollowAnchor; }
            set { _FrameForceToFollowAnchor = value; }
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
            set { _FixedUpdateFPS = value; }
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
        /// Allows for external processing after the camera has done its work this frame
        /// </summary>
        protected CameraUpdateEvent mOnPostLateUpdate = null;
        public CameraUpdateEvent OnPostLateUpdate
        {
            get { return mOnPostLateUpdate; }
            set { mOnPostLateUpdate = value; }
        }

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

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected virtual void Awake()
        {
            _Transform = gameObject.transform;

            if (_Camera == null)
            {
                _Camera = gameObject.GetComponent<Camera>();

                if (_Camera == null)
                {
                    Camera[] lCameras = gameObject.GetComponentsInChildren<Camera>();
                    if (lCameras != null && lCameras.Length > 0)
                    {
                        for (int i = 0; i < lCameras.Length; i++)
                        {
                            if (lCameras[i].enabled)
                            {
                                _Camera = lCameras[i];
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// Determines if the specified mode can be activated. This does not activate
        /// the mode, but determines if it can be
        /// </summary>
        /// <param name="rMode"></param>
        /// <param name="rEnable"></param>
        public virtual void EnableMode(int rMode, bool rEnable)
        {

        }

        /// <summary>
        /// Clears out any target we're moving to
        /// </summary>
        public virtual void ClearTargetYawPitch()
        {
        }

        /// <summary>
        /// Causes us to ignore user input and force the camera to the specified localangles
        /// </summary>
        /// <param name="rYaw">Target local yaw</param>
        /// <param name="rPitch">Target local pitch</param>
        /// <param name="rSpeed">Degrees per second we'll rotate. A value of -1 uses the current yaw speed.</param>
        /// <param name="rAutoClearTarget">Determines if we'll clear the target once we reach it.</param>
        public virtual void SetTargetYawPitch(float rYaw, float rPitch, float rSpeed = -1f, bool rAutoClearTarget = true)
        {
        }

        /// <summary>
        /// Clears the forward direction target we're trying to reach
        /// </summary>
        public virtual void ClearTargetForward()
        {
        }

        /// <summary>
        /// Causes us to ignore user input and force the camera to a specific direction.
        /// </summary>
        /// <param name="rForward">Forward direction the camera should look.</param>
        /// <param name="rSpeed">Speed at which we get there. A value of -1 uses the current yaw speed.</param>
        /// <param name="rAutoClearTarget">Determines if we'll clear the target once we reach it.</param>
        public virtual void SetTargetForward(Vector3 rForward, float rSpeed = -1f, bool rAutoClearTarget = true)
        {
        }

        /// <summary>
        /// Called every frame to perform processing. We only use
        /// this function if it's not called by another component.
        /// </summary>
        protected virtual void Update()
        {
            if (!_IsInternalUpdateEnabled) { return; }

#if UNITY_EDITOR

            // Time.deltaTime isn't valid when editing. So, we need to create our own here.
            float lCurrentTime = (float)EditorApplication.timeSinceStartup;
            mEditorDeltaTime = Mathf.Min(lCurrentTime - mEditorLastTime, 0.01666f);
            mEditorLastTime = lCurrentTime;

#endif

            // Counts how many valid updates we have
            mUpdateCount = 0;

            // Current time since the last frame
            _DeltaTime = Time.deltaTime;

            // If we're not fixed, update as fast as possible
            if (!_IsFixedUpdateEnabled || _FixedUpdateFPS <= 0f)
            {
                mUpdateCount = 1;
            }
            // If we are fixed, update on the interval based on our FPS
            else
            {
                _DeltaTime = 1.0f / _FixedUpdateFPS;

                // We'll cheat a bit. If the delta time is withing 10% of our desired time,
                // We'll just go with the fixed time. It makes things smoother
                if (Mathf.Abs(_DeltaTime - Time.deltaTime) < _DeltaTime * 0.1f)
                {
                    mUpdateCount = 1;
                }
                // Outside of the 10%, we need to adjust accordingly
                else
                {

                    // Build up our elapsed time
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        mFixedElapsedTime += Time.deltaTime;
                    }
                    else
                    {
                        mFixedElapsedTime += mEditorDeltaTime;
                    }
#else
                    mFixedElapsedTime += Time.deltaTime;
#endif

                    // If the elapsed time exceeds our desired update schedule, it's
                    // time for us to do a physics update. In fact, if the system
                    // is running slow we may need to do multiple updates
                    while (mFixedElapsedTime >= _DeltaTime)
                    {
                        mUpdateCount++;
                        mFixedElapsedTime -= _DeltaTime;

                        // Fail safe. We can have long delta times when debugging and such
                        if (mUpdateCount >= 5)
                        {
                            mFixedElapsedTime = 0;
                            break;
                        }
                    }
                }
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
                    RigLateUpdate(_DeltaTime, mUpdateIndex);
                    mIsFirstUpdate = false;
                }
            }
            // In this case, there shouldn't be an update. This typically
            // happens when the true FPS is much faster than our desired FPS
            else
            {
                mUpdateIndex = 0;
                RigLateUpdate(_DeltaTime, mUpdateIndex);
            }

            // End with a clean index so it doesn't effect things like adding forces
            mUpdateIndex = 1;

            // Call out to our events if needed
            if (mOnPostLateUpdate != null) { mOnPostLateUpdate(_DeltaTime, mUpdateIndex, this); }
        }

        /// <summary>
        /// LateUpdate logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public virtual void RigLateUpdate(float rDeltaTime, int rUpdateIndex)
        {
        }
    }
}

