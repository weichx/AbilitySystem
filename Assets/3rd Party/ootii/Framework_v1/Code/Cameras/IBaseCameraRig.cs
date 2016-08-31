using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using com.ootii.Base;

namespace com.ootii.Cameras
{
    /// <summary>
    /// Provides a simple interface for all camera rigs
    /// </summary>
    public interface IBaseCameraRig
    {
        /// <summary>
        /// Transform that represents the camera rig
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Transform that represents the anchor we want to follow or look at
        /// </summary>
        Transform Anchor { get; set; }

        /// <summary>
        /// Mode used to determine the state of the camera. It's up to each individual
        /// camera to determine what this means. Typically
        /// 0 = Third Person Orbit
        /// 1 = Third Person Fixed
        /// 2 = First Person
        /// </summary>
        int Mode { get; set; }

        /// <summary>
        /// If we lock the mode, we won't change it based on input values
        /// </summary>
        bool LockMode { get; set; }

        /// <summary>
        /// Determines if the camer uses it's own late update
        /// or it if relies on the character controller to call 
        /// ControllerLateUpdate
        /// </summary>
        bool IsInternalUpdateEnabled { get; set; }

        /// <summary>
        /// LateUpdate logic for the camera should be done here. This allows us
        /// to support dynamic and fixed update times
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        void RigLateUpdate(float rDeltaTime, int rUpdateIndex);
    }
}
