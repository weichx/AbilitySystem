using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;

namespace com.ootii.Cameras
{
    /// <summary>
    /// Camera rig for a 2.5D scroller. It will follow the character (from the side) as
    /// the character moves and climbs. However, it will not go upside-down or tilt as
    /// the character tilts.
    /// </summary>
    [AddComponentMenu("ootii/Camera Rigs/Scroller Rig")]
    public class ScrollerRig : BaseCameraRig
    {
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
        public Vector3 _AnchorOffset = new Vector3(0f, 1f, -3f);
        public Vector3 AnchorOffset
        {
            get { return _AnchorOffset; }
            set { _AnchorOffset = value; }
        }

        /// <summary>
        /// Sets the direction the camera will look towards
        /// </summary>
        public Vector3 _LookDirection = new Vector3(0f, 0f, 1f);
        public Vector3 LookDirection
        {
            get { return _LookDirection; }

            set
            {
                _LookDirection = value;

                if (_Transform != null)
                {
                    _Transform.rotation = Quaternion.LookRotation(_LookDirection);
                }
            }
        }

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
            }

            _Transform.rotation = Quaternion.LookRotation(_LookDirection);
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
            Vector3 lNewCameraPosition = _Anchor.position + _AnchorOffset;

            _Transform.position = lNewCameraPosition;
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

