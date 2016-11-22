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
        /// Determines if the specified mode can be activated. This does not activate
        /// the mode, but determines if it can be
        /// </summary>
        void EnableMode(int rMode, bool rEnable);

        /// <summary>
        /// Clears out any target we're moving to
        /// </summary>
        void ClearTargetYawPitch();

        /// <summary>
        /// Causes us to ignore user input and force the camera to the specified localangles
        /// </summary>
        /// <param name="rYaw">Target local yaw</param>
        /// <param name="rPitch">Target local pitch</param>
        /// <param name="rSpeed">Degrees per second we'll rotate. A value of -1 uses the current yaw speed.</param>
        /// <param name="rAutoClearTarget">Determines if we'll clear the target once we reach it.</param>
        void SetTargetYawPitch(float rYaw, float rPitch, float rSpeed = -1f, bool rAutoClearTarget = true);

        /// <summary>
        /// Clears the forward direction target we're trying to reach
        /// </summary>
        void ClearTargetForward();

        /// <summary>
        /// Causes us to ignore user input and force the camera to a specific direction.
        /// </summary>
        /// <param name="rForward">Forward direction the camera should look.</param>
        /// <param name="rSpeed">Speed at which we get there. A value of -1 uses the current yaw speed.</param>
        /// <param name="rAutoClearTarget">Determines if we'll clear the target once we reach it.</param>
        void SetTargetForward(Vector3 rForward, float rSpeed = -1f, bool rAutoClearTarget = true);

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
