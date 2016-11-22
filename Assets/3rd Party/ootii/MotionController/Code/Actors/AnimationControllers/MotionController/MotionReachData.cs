using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Reach data is used to help animations reach specific targets. This is 
    /// especially important for things like climbing where we want some flexability
    /// where a player can start
    /// </summary>
    public class MotionReachData
    {
        /// <summary>
        /// State this reach data applies to
        /// </summary>
        public int StateID;

        /// <summary>
        /// Transition this reach data applies to
        /// </summary>
        public int TransitionID;

        /// <summary>
        /// Normalized time in the animation we start using the reach data
        /// </summary>
        public float StartTime;

        /// <summary>
        /// Normalized time in the animation we stop using the reach data
        /// </summary>
        public float EndTime;

        /// <summary>
        /// Used to create a quadradic curve that determines who quickly we head to the ReachTarget
        /// </summary>
        public int Power;

        /// <summary>
        /// Determines if we're storing the reach target in local space or
        /// world space. The result of ReachTarget get is based on this
        /// </summary>
        public bool IsReachTargetLocal;

        /// <summary>
        /// Returns the world space target the animation should reach. However,
        /// if there is a ReachTargetGround, the internal target is stored as
        /// local to the ground and the result is then converted to world space
        /// relatvie to the ReachTargetGround
        /// </summary>
        public Vector3 _ReachTarget;
        public Vector3 ReachTarget
        {
            get
            {
                if (IsReachTargetLocal && _ReachTargetGround != null)
                {
                    return _ReachTargetGround.TransformPoint(_ReachTarget);
                }
                else
                {
                    return _ReachTarget;
                }
            }

            set
            {
                _ReachTarget = value;

                if (_ReachTargetGround != null)
                {
                    IsReachTargetLocal = true;
                    _ReachTarget = _ReachTargetGround.InverseTransformPoint(_ReachTarget);
                }
            }
        }

        /// <summary>
        /// Transform value we'll use to transform the world space ReachTarget to
        /// local space. When we return the value, it will be transformed back into
        /// world space relative to this transform
        /// </summary>
        public Transform _ReachTargetGround;
        public Transform ReachTargetGround
        {
            get { return _ReachTargetGround; }

            set
            {
                if (IsReachTargetLocal && _ReachTargetGround != null)
                {
                    IsReachTargetLocal = false;
                    _ReachTarget = _ReachTargetGround.TransformPoint(_ReachTarget);
                }

                _ReachTargetGround = value;
                if (_ReachTargetGround != null)
                {
                    IsReachTargetLocal = true;
                    _ReachTarget = _ReachTargetGround.InverseTransformPoint(_ReachTarget);
                }
            }
        }

        /// <summary>
        /// Determines if we've reached the target
        /// </summary>
        public bool IsComplete;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<MotionReachData> sPool = new ObjectPool<MotionReachData>(10, 5);

        /// <summary>
        /// Returns the number of items allocated
        /// </summary>
        /// <value>The allocated.</value>
        public static int Length
        {
            get { return sPool.Length; }
        }

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static MotionReachData Allocate()
        {
            // Grab the next available object
            MotionReachData lInstance = sPool.Allocate();

            // Return it for use
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(MotionReachData rInstance)
        {
            // Clear the values
            rInstance.StateID = 0;
            rInstance.TransitionID = 0;
            rInstance.Power = 1;
            rInstance.IsReachTargetLocal = false;
            rInstance._ReachTarget = Vector3.zero;
            rInstance._ReachTargetGround = null;
            rInstance.StartTime = 0f;
            rInstance.EndTime = 0f;
            rInstance.IsComplete = false;

            // Send it back to the pool
            sPool.Release(rInstance);
        }
    }
}
