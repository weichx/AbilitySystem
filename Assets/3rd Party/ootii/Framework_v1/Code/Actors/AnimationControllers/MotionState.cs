using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Geometry;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Stores information about the motion controller state such as
    /// angle, speed, etc. By keeping the information here, we
    /// can trace current state and previous state.
    /// </summary>
    public struct MotionState
    {
        /// <summary>
        /// The current stance of the player
        /// </summary>
        //public int Stance;

        /// <summary>
        /// Current velocity of the avatar
        /// </summary>
        //public Vector3 Velocity;

        /// <summary>
        /// Current position of the avatar
        /// </summary>
        //public Vector3 Position;

        /// <summary>
        /// State information for each of the animator layers. Because
        /// we have an array (a reference type), we'll need to reset the
        /// values when we do the 'prev = current'. Otherwise they will
        /// share the same array (which we don't want).
        /// </summary>
        public AnimatorLayerState[] AnimatorStates;

        /// <summary>
        /// Object representing what the player is grounded/attached to.
        /// It doesn't have to be terrain as the player could be grounded
        /// to a platform or climbing a wall.
        /// </summary>
        //public GameObject Support;

        /// <summary>
        /// Position of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        //public Vector3 SupportPosition;

        /// <summary>
        /// Rotation of the ground. We used this since the reference to the
        /// ground itself will change the values.
        /// </summary>
        //public Quaternion SupportRotation;

        /// <summary>
        /// Position on the support (in object space) where the controller
        /// is in contact. This becomes inportant for rotations.
        /// </summary>
       // public Vector3 SupportContactPosition;

        /// <summary>
        /// A flag that allows us to determine if we can move forward. A
        /// slope or wall could keep us from moving forward.
        /// </summary>
        public bool IsForwardPathBlocked;

        /// <summary>
        /// When blocked by an object, this holds the normal of that object.
        /// </summary>
        public Vector3 ForwardPathBlockNormal;

        /// <summary>
        /// Determines if the avatar is currently on the ground
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// Distance from the avatar's position to the ground
        /// </summary>
        //public float GroundDistance;

        /// <summary>
        /// Normal used for the ground if we have a conflict
        /// </summary>
        //public Vector3 GroundNormal;

        /// <summary>
        /// Rotation angle for the ground plane
        /// </summary>
        //public float GroundAngle;

        /// <summary>
        /// Speed we were moving when launching from the ground
        /// </summary>
        public Vector3 GroundLaunchVelocity;

        /// <summary>
        /// Current direction of the controller which represents left(-1) to right(1)
        /// </summary>
        public float InputX;

        /// <summary>
        /// Current direction of the controller which represents forward(1) to back(-1)
        /// </summary>
        public float InputY;

        /// <summary>
        /// This value tracks the average of the input magnitude over time.
        /// Because the array inside of the FloatValue is a reference type,
        /// the 'prev' and 'current' will actually share the same array when
        /// the assignment of 'prev = current' occurs. This is exactly what we want.
        /// </summary>
        public FloatValue InputMagnitudeTrend;

        /// <summary>
        /// The forward normal of the controller
        /// </summary>
        public Vector3 InputForward;

        /// <summary>
        /// Relative angle (in degrees) from current heading of the avatar
        /// </summary>
        public float InputFromAvatarAngle;

        /// <summary>
        /// Relative angle (in degrees) from current heading of the camera
        /// </summary>
        public float InputFromCameraAngle;

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
        /// <param name="rSource">MotionState that has the data to copy</param>
        public static void Shift(ref MotionState rCurrent, ref MotionState rPrev)
        {
            // Reference objects need to be held onto otherwise our 'prev' 
            // container will actually hold the reference in the 'current' container
            AnimatorLayerState[] lAnimatorStates = rPrev.AnimatorStates;

            // Move the current contents into the previous bucket
            rPrev = rCurrent;

            // Update the members of the states array. We have to do this
            // so 'prev' doesn't accidentally hold a reference sot the array in 'current'
            rPrev.AnimatorStates = lAnimatorStates;
            if (rPrev.AnimatorStates != null)
            {
                int lCount = lAnimatorStates.Length;
                for (int i = 0; i < lCount; i++)
                {
                    //rPrev.AnimatorStates[i] = rCurrent.AnimatorStates[i];

                    //TT
                    rPrev.AnimatorStates[i].MotionPhase = rCurrent.AnimatorStates[i].MotionPhase;
                    rPrev.AnimatorStates[i].MotionParameter = rCurrent.AnimatorStates[i].MotionParameter;
                    rPrev.AnimatorStates[i].AutoClearMotionPhase = rCurrent.AnimatorStates[i].AutoClearMotionPhase;
                    rPrev.AnimatorStates[i].AutoClearMotionPhaseReady = rCurrent.AnimatorStates[i].AutoClearMotionPhaseReady;
                    rPrev.AnimatorStates[i].AutoClearActiveTransitionID = rCurrent.AnimatorStates[i].AutoClearActiveTransitionID;
                    rPrev.AnimatorStates[i].StateInfo = rCurrent.AnimatorStates[i].StateInfo;
                    rPrev.AnimatorStates[i].TransitionInfo = rCurrent.AnimatorStates[i].TransitionInfo;
                }
            }

            // Initialize the object we're attached to. Don't clear
            // the contact position as we'll store it for future use
            //rCurrent.Support = null;
            //rCurrent.SupportPosition = Vector3.zero;
            //rCurrent.SupportRotation = Quaternion.identity;

            // Initialize the ground information
            //rCurrent.IsGrounded = false;
            //rCurrent.GroundAngle = 0f;
            //rCurrent.GroundNormal = Vector3.up;
            //rCurrent.GroundDistance = float.MaxValue;
        }
    }
}

