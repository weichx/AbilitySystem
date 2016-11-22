using System;
using UnityEngine;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Holds the mecanim animation state information
    /// for a layer. This keeps us from having to ask for
    /// it over and over. We can also use it to track changes
    /// </summary>
    public struct AnimatorLayerState
    {
        /// <summary>
        /// Contains the current state information for the layer
        /// </summary>
        public AnimatorStateInfo StateInfo;

        /// <summary>
        /// Contains the current transition information for the layer
        /// </summary>
        public AnimatorTransitionInfo TransitionInfo;

        /// <summary>
        /// Tracks the last state name hash that was running
        /// </summary>
        //public int LastAnimatorStateHash;

        /// <summary>
        /// The phase of the curren motion to pass to the animator. While
        /// many motions and motion layers can exist, eventually the information has
        /// to be placed here so it can be sent to the animator.
        /// </summary>
        public int MotionPhase;

        /// <summary>
        /// Extra parameter that can be sent to the motion in order to help it
        /// manage transitions. This value is controlled by each motion.
        /// </summary>
        public int MotionParameter;

        /// <summary>
        /// Clear out the phase if we need to. This way we don't re-enter. Especially
        /// usefull for not re-entering from the 'AnyState'.
        /// </summary>
        public bool AutoClearMotionPhase;

        /// <summary>
        /// Determines if we're ready to clear the motion phase. We don't want to
        /// clear it if the animator hasn't picked it up.
        /// </summary>
        public bool AutoClearMotionPhaseReady;

        /// <summary>
        /// Store the current transition ID in case we're trying to go to a new
        /// motion, but we're currently in a transition.
        /// </summary>
        public int AutoClearActiveTransitionID;
    }
}
