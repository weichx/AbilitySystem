using UnityEngine;

namespace com.ootii.Actors
{
    /// <summary>
    /// Delegate that allows for tapping into the controller.
    /// </summary>
    /// <param name="rController">Controller calling the delegate</param>
    /// <param name="rDeltaTime">Delta time of the update this frame</param>
    /// <param name="rUpdateIndex">When using fixed update the index of the call this frame (0=invalid, 1=standard, +1=additional).</param>
    public delegate void ControllerLateUpdateDelegate(ICharacterController rController, float rDeltaTime, int rUpdateIndex);

    /// <summary>
    /// Delegate that allows for modifying the position and rotation before being set on the transform
    /// </summary>
    /// <param name="rController">Controller calling the delegate</param>
    /// <param name="rFinalPosition">Final position to be set</param>
    /// <param name="rFinalRotation">Final rotation to be set</param>
    public delegate void ControllerMoveDelegate(ICharacterController rController, ref Vector3 rFinalPosition, ref Quaternion rFinalRotation);

    /// <summary>
    /// Simple interface for identifying character controllers and providing
    /// access to basic functions.
    /// </summary>
    public interface ICharacterController
    {
        /// <summary>
        /// Sets and absolute rotation to rotate the actor to
        /// </summary>
        void SetRotation(Quaternion rRotation);

        /// <summary>
        /// Sets and absolute position to move the actor to
        /// </summary>
        void SetPosition(Vector3 rPosition);

        /// <summary>
        /// Determines if we're meant to ignore the specified collider
        /// </summary>
        /// <param name="rCollider">Collider to test</param>
        /// <returns>Tells if we are meant to ignore the collision</returns>
        bool IsIgnoringCollision(Collider rCollider);

        /// <summary>
        /// Clears any colliders that were meant to be ignored
        /// </summary>
        void ClearIgnoreCollisions();

        /// <summary>
        /// Sets a collider to be ignored or not by the character controller
        /// </summary>
        /// <param name="rCollider">Collider to ignore or not</param>
        /// <param name="rIgnore">Flag to determine if we are ignoring collisions</param>
        void IgnoreCollision(Collider rCollider, bool rIgnore = true);

        /// <summary>
        /// Allows for external processing prior to the actor controller doing it's 
        /// work this frame.
        /// </summary>
        ControllerLateUpdateDelegate OnControllerPreLateUpdate { get; set; }

        /// <summary>
        /// Allows for external processing after the actor controller doing it's 
        /// work this frame.
        /// </summary>
        ControllerLateUpdateDelegate OnControllerPostLateUpdate { get; set; }

        /// <summary>
        /// Callback that allows the caller to change the final position/rotation
        /// before it's set on the actual transform.
        /// </summary>
        ControllerMoveDelegate OnPreControllerMove { get; set; }
    }
}
