using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;

namespace com.ootii.Cameras
{
    /// <summary>
    /// Third-person camera that follows the transform, but
    /// allows the camera to orbit around the target.
    /// </summary>
    [AddComponentMenu("ootii/Camera Rigs/Orbit Rig")]
    public class OrbitRig : BaseCameraRig
    {
        // Keep us from going past the poles
        private const float MIN_PITCH = 10f;
        private const float MAX_PITCH = 170f;

        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }

            set
            {
                _InputSourceOwner = value;

                // Object that will provide access to the keyboard, mouse, etc
                if (_InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }
            }
        }

        /// <summary>
        /// Transform that represents the anchor we want to follow
        /// </summary>
        public override Transform Anchor
        {
            get { return _Anchor; }

            set
            {
                if (_Anchor != null)
                {
                    // Stop listening to the old controller
                    ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                    if (lController != null) { lController.OnControllerPostLateUpdate -= OnControllerLateUpdate; }
                }

                _Anchor = value;
                if (_Anchor != null && this.enabled)
                {
                    // Start listening to the new controller
                    ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                    if (lController != null)
                    {
                        IsInternalUpdateEnabled = false;
                        IsFixedUpdateEnabled = false;
                        lController.OnControllerPostLateUpdate += OnControllerLateUpdate;
                    }
                }
            }
        }

        /// <summary>
        /// Offset from the anchor that the camera will be positioned
        /// </summary>
        public Vector3 _AnchorOffset = new Vector3(0f, 2f, 0f);
        public Vector3 AnchorOffset
        {
            get { return _AnchorOffset; }
            set { _AnchorOffset = value; }
        }

        /// <summary>
        /// Radius of the orbit
        /// </summary>
        public float _Radius = 4f;
        public float Radius
        {
            get { return _Radius; }
            set { _Radius = value; }
        }

        /// <summary>
        /// Degrees per second the actor rotates
        /// </summary>
        public float _RotationSpeed = 120f;
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
        /// Determines if we invert the pitch information we get from the input
        /// </summary>
        public bool _InvertPitch = true;
        public virtual bool InvertPitch
        {
            get { return _InvertPitch; }
            set { _InvertPitch = value; }
        }

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Represents the "pole" that the camera is attched to the anchor with. This pole
        /// is the direction from the anchor to the camera (in natural "up" space)
        /// </summary>
        protected Vector3 mToCameraDirection = Vector3.back;

        /// <summary>
        /// We keep track of the tilt so we can make small changes to it as the actor rotates.
        /// This is safter than trying to do a full rotation all at once which can cause odd
        /// rotations as we hit 180 degrees.
        /// </summary>
        protected Quaternion mTilt = Quaternion.identity;

        /// <summary>
        /// Provides access to the keyboard, mouse, etc.
        /// </summary>
        protected IInputSource mInputSource = null;

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_Anchor != null && this.enabled)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null)
                {
                    IsInternalUpdateEnabled = false;
                    IsFixedUpdateEnabled = false;
                    lController.OnControllerPostLateUpdate += OnControllerLateUpdate;
                }

                mTilt = QuaternionExt.FromToRotation(_Transform.up, _Anchor.up);

                mToCameraDirection = _Transform.position - _Anchor.position;
                mToCameraDirection.y = 0f;
                mToCameraDirection.Normalize();

                if (mToCameraDirection.sqrMagnitude == 0f) { mToCameraDirection = -_Anchor.forward; }
            }

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // Default the speed we'll use to rotate
            mDegreesPer60FPSTick = _RotationSpeed / 60f;
        }

        /// <summary>
        /// Called when the component is enabled. This is also called after awake. So,
        /// we need to ensure we're not doubling up on the assignment.
        /// </summary>
        protected void OnEnable()
        {
            if (_Anchor != null)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null)
                {
                    if (lController.OnControllerPostLateUpdate != null) { lController.OnControllerPostLateUpdate -= OnControllerLateUpdate; }
                    lController.OnControllerPostLateUpdate += OnControllerLateUpdate;
                }
            }
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            if (_Anchor != null)
            {
                ICharacterController lController = InterfaceHelper.GetComponent<ICharacterController>(_Anchor.gameObject);
                if (lController != null && lController.OnControllerPostLateUpdate != null)
                {
                    lController.OnControllerPostLateUpdate -= OnControllerLateUpdate;
                }
            }
        }

        /// <summary>
        /// LateUpdate logic for the controller should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void RigLateUpdate(float rDeltaTime, int rUpdateIndex)
        {
            Vector3 lNewAnchorPosition = _Anchor.position + (_Anchor.rotation * _AnchorOffset);
            Vector3 lNewCameraPosition = _Transform.position;
            Quaternion lNewCameraRotation = _Transform.rotation;

            // At certain times, we may force the rig to face the direction of the actor
            if (_FrameForceToFollowAnchor)
            {
                // Grab the rotation amount. We do the inverse tilt so we calculate the rotation in
                // "natural up" space. Later we'll use the tilt to put it back into "anchor up" space.
                Quaternion lInvTilt = QuaternionExt.FromToRotation(_Anchor.up, Vector3.up);

                // Determine the global direction the character should face
                float lAngle = NumberHelper.GetHorizontalAngle(_Transform.forward, _Anchor.forward, _Anchor.up);
                Quaternion lYaw = Quaternion.AngleAxis(lAngle, lInvTilt * _Anchor.up);

                // Pitch is more complicated since we can't go beyond the north/south pole
                Quaternion lPitch = Quaternion.identity;
                if (mInputSource.IsViewingActivated)
                {
                    float lPitchAngle = Vector3.Angle(mToCameraDirection, lInvTilt * _Anchor.up);

                    float lPitchDelta = (_InvertPitch ? -1f : 1f) * mInputSource.ViewY;
                    if (lPitchAngle < MIN_PITCH && lPitchDelta > 0f) { lPitchDelta = 0f; }
                    else if (lPitchAngle > MAX_PITCH && lPitchDelta < 0f) { lPitchDelta = 0f; }

                    lPitch = Quaternion.AngleAxis(lPitchDelta, lInvTilt * _Transform.right);
                }

                // Calculate the new "natural up" direction
                mToCameraDirection = lPitch * lYaw * mToCameraDirection;

                // Update our tilt to match the anchor's tilt
                mTilt = QuaternionExt.FromToRotation(mTilt.Up(), _Anchor.up) * mTilt;

                // Put the new direction relative to the anchor's tilt
                Vector3 lToCameraDirection = mTilt * mToCameraDirection;
                if (lToCameraDirection.sqrMagnitude == 0f) { lToCameraDirection = -_Anchor.forward; }

                // Calculate the new orbit center (anchor) and camera position
                lNewCameraPosition = lNewAnchorPosition + (lToCameraDirection.normalized * _Radius);
                lNewCameraRotation = Quaternion.LookRotation(lNewAnchorPosition - lNewCameraPosition, _Anchor.up);

                // Disable the force
                _FrameForceToFollowAnchor = false;
            }
            // If we're not forcing a follow, do our normal processing
            else
            {
                if (mInputSource.IsViewingActivated)
                {
                    // Grab the rotation amount. We do the inverse tilt so we calculate the rotation in
                    // "natural up" space. Later we'll use the tilt to put it back into "anchor up" space.
                    Quaternion lInvTilt = QuaternionExt.FromToRotation(_Anchor.up, Vector3.up);

                    // Yaw is simple as we can go 360
                    Quaternion lYaw = Quaternion.AngleAxis(mInputSource.ViewX * mDegreesPer60FPSTick, lInvTilt * _Transform.up);

                    // Pitch is more complicated since we can't go beyond the north/south pole
                    float lPitchAngle = Vector3.Angle(mToCameraDirection, lInvTilt * _Anchor.up);

                    float lPitchDelta = (_InvertPitch ? -1f : 1f) * mInputSource.ViewY;
                    if (lPitchAngle < MIN_PITCH && lPitchDelta > 0f) { lPitchDelta = 0f; }
                    else if (lPitchAngle > MAX_PITCH && lPitchDelta < 0f) { lPitchDelta = 0f; }

                    Quaternion lPitch = Quaternion.AngleAxis(lPitchDelta, lInvTilt * _Transform.right);

                    // Calculate the new "natural up" direction
                    mToCameraDirection = lPitch * lYaw * mToCameraDirection;
                }

                // Update our tilt to match the anchor's tilt
                mTilt = QuaternionExt.FromToRotation(mTilt.Up(), _Anchor.up) * mTilt;

                // Put the new direction relative to the anchor's tilt
                Vector3 lToCameraDirection = mTilt * mToCameraDirection;
                if (lToCameraDirection.sqrMagnitude == 0f) { lToCameraDirection = -_Anchor.forward; }

                // Calculate the new orbit center (anchor) and camera position
                lNewCameraPosition = lNewAnchorPosition + (lToCameraDirection.normalized * _Radius);
                lNewCameraRotation = Quaternion.LookRotation(lNewAnchorPosition - lNewCameraPosition, _Anchor.up);
            }

            // Set the values
            _Transform.position = lNewCameraPosition;
            _Transform.rotation = lNewCameraRotation;
        }

        /// <summary>
        /// Delegate callback for handling the camera movement AFTER the character controller
        /// </summary>
        /// <param name="rController"></param>
        /// <param name="rDeltaTime"></param>
        /// <param name="rUpdateIndex"></param>
        private void OnControllerLateUpdate(ICharacterController rController, float rDeltaTime, int rUpdateIndex)
        {
            RigLateUpdate(rDeltaTime, rUpdateIndex);

            // Call out to our events if needed
            if (mOnPostLateUpdate != null) { mOnPostLateUpdate(rDeltaTime, mUpdateIndex, this); }
        }
    }
}

