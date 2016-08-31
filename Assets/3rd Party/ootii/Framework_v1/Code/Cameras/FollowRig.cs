using UnityEngine;

using com.ootii.Actors;
using com.ootii.Geometry;
using com.ootii.Helpers;

namespace com.ootii.Cameras
{
    /// <summary>
    /// Traditional third-person camera that follows the transform
    /// at a specific offset. Unlike the Adventure Camera, this camera
    /// always rotates around the transform + offset and does not have
    /// an orbiting view
    /// 
    /// The Adventure Camera is a much more advanced camera with several
    /// modes, a physics based spring, and a modern third-person approach
    /// where the player rotates around the camera.
    /// https://www.assetstore.unity3d.com/#/content/13768
    /// </summary>
    [AddComponentMenu("ootii/Camera Rigs/Follow Rig")]
    public class FollowRig : BaseCameraRig
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
        public Vector3 _AnchorOffset = new Vector3(0f, 2f, -3f);
        public Vector3 AnchorOffset
        {
            get { return _AnchorOffset; }
            set { _AnchorOffset = value; }
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
            Vector3 lNewCameraPosition = _Anchor.position + (_Anchor.rotation * _AnchorOffset);

            _Transform.position = lNewCameraPosition;
            _Transform.rotation = _Anchor.rotation;
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

