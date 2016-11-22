using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Geometry;

namespace com.ootii.Actors
{
    /// <summary>
    /// Stores information about the actor state such as
    /// angle, speed, etc. By keeping the information here, we
    /// can trace current state and previous state.
    /// </summary>
    public class ActorState
    {
        /// <summary>
        /// Number of states stored in the state array
        /// </summary>
        public const int STATE_COUNT = 20;

        /// <summary>
        /// Unique ID that allows us to track and reapply state values
        /// </summary>
        public int ID;

        /// <summary>
        /// The current stance of the player. Used to help manage the
        /// state that the actor is in (sneaking, combat, etc)
        /// </summary>
        public int Stance;

        /// <summary>
        /// Determine is movement was caused by the user
        /// </summary>
        public bool IsMoveRequested;

        /// <summary>
        /// Current rotation of the actor
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Current rotation of the actor representing the yaw around
        /// his up axis.
        /// </summary>
        public Quaternion RotationYaw;

        /// <summary>
        /// Current rotation of the actor representing the pitch/roll around
        /// his up axis.
        /// </summary>
        public Quaternion RotationTilt;

        /// <summary>
        /// Current position of the actor
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current velocity of the actor
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Raw user initiated movement occuring this frame
        /// </summary>
        public Vector3 Movement;

        /// <summary>
        /// Adjustment required due to penetration or hovering this frame
        /// </summary>
        public Vector3 MovementGroundAdjust;

        /// <summary>
        /// Movement due to gravity and slope this frame
        /// </summary>
        public Vector3 MovementSlideAdjust;

        /// <summary>
        /// Movement due external forces this frame
        /// </summary>
        public Vector3 MovementForceAdjust;

        /// <summary>
        /// Movement due to reacting to collisions this frame
        /// </summary>
        public Vector3 MovementCounterAdjust;

        /// <summary>
        /// Movement due to the platform we are on
        /// </summary>
        public Vector3 MovementPlatformAdjust;

        /// <summary>
        /// Rotation due to the platform we are on
        /// </summary>
        public Quaternion RotationPlatformAdjust;

        /// <summary>
        /// Determines if the avatar is currently on the ground. 
        /// while stepping, we consider the actor grounded.
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// Determines if the actor is currently stepping up
        /// </summary>
        public bool IsSteppingUp;

        /// <summary>
        /// Determine if the actor is currently stepping down
        /// </summary>
        public bool IsSteppingDown;

        /// <summary>
        /// Determine if the actor is popping up due to surface collision
        /// </summary>
        public bool IsPoppingUp;

        /// <summary>
        /// Determine if the actor is currently tilting to a desired direction
        /// </summary>
        public bool IsTilting;

        /// <summary>
        /// Determines if the movement is blocked by a slope or collision
        /// </summary>
        public bool IsMovementBlocked;

        /// <summary>
        /// Object representing what the player is grounded/attached to.
        /// It doesn't have to be terrain as the player could be grounded
        /// to a platform or climbing a wall.
        /// </summary>
        public Transform Ground;

        /// <summary>
        /// Position of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        public Vector3 GroundPosition;

        /// <summary>
        /// Rotation of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        public Quaternion GroundRotation;

        /// <summary>
        /// Position on the support (in object space) where the controller
        /// is in contact. This becomes inportant for rotations.
        /// </summary>
        public Vector3 GroundLocalContactPoint;

        /// <summary>
        /// Distance from the avatar's position to the ground
        /// </summary>
        public float GroundSurfaceDistance;

        /// <summary>
        /// World space point where the ground hit was made.
        /// </summary>
        public Vector3 GroundSurfacePoint;

        /// <summary>
        /// Normal used for the ground if we have a conflict
        /// </summary>
        public Vector3 GroundSurfaceNormal;

        /// <summary>
        /// Direction from the origin to the  surface point
        /// </summary>
        public Vector3 GroundSurfaceDirection;

        /// <summary>
        /// Determines if our grounding is due to a direct ray-cast hit
        /// </summary>
        public bool IsGroundSurfaceDirect = true;

        /// <summary>
        /// Distance from the avatar's position to the ground directly under the character
        /// </summary>
        public float GroundSurfaceDirectDistance;

        /// <summary>
        /// Normal that is directly under the character. This can be different
        /// if the sphere cast finds a collision to the side of the actor.
        /// </summary>
        public Vector3 GroundSurfaceDirectNormal;

        /// <summary>
        /// Rotation angle for the ground plane
        /// </summary>
        public float GroundSurfaceAngle;

        /// <summary>
        /// Determines if we collided with something this frame
        /// </summary>
        public bool IsColliding;

        /// <summary>
        /// Identifies the object we collided with
        /// </summary>
        public Collider Collider;

        /// <summary>
        /// Unadjusted hit information about the collider
        /// </summary>
        public RaycastHit ColliderHit;

        /// <summary>
        /// Where on the actor the hit took place
        /// </summary>
        public Vector3 ColliderHitOrigin;

        /// <summary>
        /// Object representing what the player is grounded/attached to.
        /// It doesn't have to be terrain as the player could be grounded
        /// to a platform or climbing a wall.
        /// </summary>
        public Transform PrevGround;

        /// <summary>
        /// Position of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        public Vector3 PrevGroundPosition;

        /// <summary>
        /// Rotation of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        public Quaternion PrevGroundRotation;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActorState()
        {
        }

        /// <summary>
        /// Clears the state values
        /// </summary>
        public void Clear()
        {
            // Basic properties
            ID = 0;
            Stance = 0;
            IsMoveRequested = false;
            IsMovementBlocked = false;
            Rotation = Quaternion.identity;
            RotationYaw = Quaternion.identity;
            RotationTilt = Quaternion.identity;
            Position = Vector3.zero;
            Velocity = Vector3.zero;
            
            // Clear out our movement results
            Movement = Vector3.zero;
            MovementGroundAdjust = Vector3.zero;
            MovementSlideAdjust = Vector3.zero;
            MovementForceAdjust = Vector3.zero;
            MovementCounterAdjust = Vector3.zero;
            MovementPlatformAdjust = Vector3.zero;
            RotationPlatformAdjust = Quaternion.identity;

            // Initialize the grounding information
            IsGrounded = false;
            IsSteppingUp = false;
            IsSteppingDown = false;
            IsPoppingUp = false;
            IsTilting = false;

            // Initialize the object we're attached to. Don't clear
            // the contact position as we'll store it for future use
            Ground = null;
            GroundPosition = Vector3.zero;
            GroundRotation = Quaternion.identity;

            PrevGround = null;
            PrevGroundPosition = Vector3.zero;
            PrevGroundRotation = Quaternion.identity;

            // Initialize the ground surface information
            GroundLocalContactPoint = Vector3.zero;
            GroundSurfaceDistance = float.MaxValue;
            GroundSurfacePoint = Vector3.zero;
            GroundSurfaceNormal = Vector3.up;
            GroundSurfaceDirection = Vector3.down;
            GroundSurfaceAngle = 0f;

            IsGroundSurfaceDirect = true;
            GroundSurfaceDirectDistance = 0f;
            GroundSurfaceDirectNormal = Vector3.up;

            // Initialize the collider info
            IsColliding = false;
            Collider = null;
            ColliderHit = default(RaycastHit);
            ColliderHitOrigin = Vector3.zero;
        }

        /// <summary>
        /// Safe way to get the desired index. It will do the mod for us.
        /// </summary>
        /// <param name="rDesiredIndex">Unsafe index that we want</param>
        /// <returns></returns>
        public static ActorState State(ref ActorState[] rStates, int rDesiredIndex)
        {
            if (rDesiredIndex < 0) { rDesiredIndex = rStates.Length + rDesiredIndex; }
            return rStates[rDesiredIndex];
        }

        /// <summary>
        /// Sets the value of this state with values from another
        /// 
        /// Note: When the shift occurs, all intrinsict types (float, string, etc)
        /// have thier values copied over. Any reference types (arrays, objects) have
        /// thier references assigned. This includes 'child' properties.
        /// 
        /// For the reference values... if we want them shared between the prev and current
        /// instances, we don't need to do anything.
        /// 
        /// However, if we want reference values to be independant between the prev and current
        /// we need to grab the prev's reference, hold it, and then reset it.
        /// </summary>
        /// <param name="rSource">ActorState that has the data to copy</param>
        public static int Shift(ref ActorState[] rStates, int rCurrentIndex)
        {
            // Increment the index
            int lNextIndex = (rCurrentIndex + 1) % rStates.Length;

            // Grab the next state
            ActorState lCurrentState = rStates[rCurrentIndex];
            ActorState lNextState = rStates[lNextIndex];

            // Clear all the values so we can refill
            lNextState.Clear();

            // Reset the values
            lNextState.ID = lCurrentState.ID++;
            lNextState.Stance = lCurrentState.Stance;

            lNextState.GroundLocalContactPoint = lCurrentState.GroundLocalContactPoint;

            lNextState.PrevGround = lCurrentState.Ground;
            lNextState.PrevGroundPosition = lCurrentState.GroundPosition;
            lNextState.PrevGroundRotation = lCurrentState.GroundRotation;

            // Return the new index
            return lNextIndex;
        }
    }
}

