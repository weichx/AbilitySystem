using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Utilities.Debug;

namespace com.ootii.Actors
{
    /// <summary>
    /// The SphereActorDriver is a simple driver for controlling a rolling sphere.
    /// </summary>
    public class SphereDriver : ActorDriver
    {
        // Determines how quickly we rotate as we move
        private float mAnglePerUnit = 45f;

        // Underlying sphere we rotate
        private Transform mSphere = null;

        // Tracks how much we're rolling the sphere
        private Quaternion mRoll = Quaternion.identity;

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Attach our post movement function so we can rotate
            if (mActorController != null)
            {
                mActorController.OnPreControllerMove = OnControllerMoved;
            }

            // Hold onto our child sphere so we can rotate it
            if (transform.childCount > 0)
            {
                mSphere = transform.GetChild(0);
                mRoll = mSphere.rotation;
            }

            // Determine how much rotation per unity unit
            float lRadius = transform.lossyScale.y * 0.5f;
            mAnglePerUnit = 360f / (2 * Mathf.PI * lRadius);
        }

        /// <summary>
        /// Callback for when the controller moves or rotates. We'll use this to help rotate the inner sphere
        /// </summary>
        /// <param name="rController">The actor controller this driver is associated with</param>
        /// <param name="rNewPosition">Position the actor will be moved to</param>
        /// <param name="rNewRotation">Rotation the actor will be turned to</param>
        private void OnControllerMoved(ICharacterController rController, ref Vector3 rNewPosition, ref Quaternion rNewRotation)
        {
            // We don't want platform movement to effect our sphere
            Vector3 lNewPosition = rNewPosition - mActorController.State.MovementPlatformAdjust;

            // We'll apply the platform rotation to our sphere
            Quaternion lPlatformRotation = mActorController.State.RotationPlatformAdjust;

            // Figure out the roll based on the movement
            Vector3 lLocalMove = transform.InverseTransformPoint(lNewPosition);
            Vector3 lLocalRotation = new Vector3(lLocalMove.z, 0f, -lLocalMove.x) * mAnglePerUnit;

            Quaternion lRotation = Quaternion.Euler(transform.rotation * lLocalRotation);
            mRoll = lPlatformRotation * lRotation * mRoll;

            // Just keep resetting the roll we had even if there was no change. This will keep
            // the sphere from rotating as we rotate the actor.
            mSphere.rotation = mRoll;
        }
    }
}
