using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Base;
using com.ootii.Utilities.Debug;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Stub that is attached to the Animator sub-state in order
    /// to pass events and information back to the actual motion
    /// </summary>
    public class MotionControllerBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Key used to uniquely associate the behavior with motions
        /// </summary>
        public string _MotionKey = "";
        public string MotionKey
        {
            get { return _MotionKey; }
        }

        /// <summary>
        /// Cache the motion count for performance
        /// </summary>
        protected int mMotionCount = 0;

        /// <summary>
        /// Look up for associating specific animators with specific character motions
        /// </summary>
        protected List<MotionControllerMotion> mMotions = new List<MotionControllerMotion>();

        /// <summary>
        /// Registers a motion with the behavior
        /// </summary>
        /// <param name="rAnimator"></param>
        /// <param name="rMotion"></param>
        public virtual void AddMotion(MotionControllerMotion rMotion)
        {
            if (mMotions.IndexOf(rMotion) < 0)
            {
                mMotions.Add(rMotion);
            }

            mMotionCount = mMotions.Count;
        }

        /// <summary>
        /// Removes a motion from the behavior
        /// </summary>
        /// <param name="rMotion"></param>
        public virtual void RemoveMotion(MotionControllerMotion rMotion)
        {
            mMotions.Remove(rMotion);
            mMotionCount = mMotions.Count;
        }

        /// <summary>
        /// Called on the first Update frame when a transition from a state from another statemachine transition 
        /// to one of this statemachine's state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rStateMachinePathHash">Path the the transition is coming from</param>
        public override void OnStateMachineEnter(Animator rAnimator, int rStateMachinePathHash)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateMachineEnter");

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateMachineEnter(rAnimator, rStateMachinePathHash);
            }
        }

        /// <summary>
        /// Called on the last Update frame when one of the statemachine's state is transitionning 
        /// toward another state in another state machine.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rStateMachinePathHash">Path the the transition is coming from</param>
        public override void OnStateMachineExit(Animator rAnimator, int rStateMachinePathHash)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateMachineExit");

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateMachineExit(rAnimator, rStateMachinePathHash);
            }
        }

        /// <summary>
        /// Called on the first Update frame when a statemachine enters this state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public override void OnStateEnter(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateEnter " + rAnimatorStateInfo.fullPathHash);

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateEnter(rAnimator, rAnimatorStateInfo, rLayerIndex);
            }
        }

        /// <summary>
        /// Called on the last update frame when a statemachine evaluate this state.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public override void OnStateExit(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateExit " + rAnimatorStateInfo.fullPathHash);

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateExit(rAnimator, rAnimatorStateInfo, rLayerIndex);
            }
        }

        /// <summary>
        /// Called right after MonoBehaviour.OnAnimatorIK.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public override void OnStateIK(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateIK " + rAnimatorStateInfo.fullPathHash);

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateIK(rAnimator, rAnimatorStateInfo, rLayerIndex);
            }
        }

        /// <summary>
        /// Called right after MonoBehaviour.OnAnimatorMove.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public override void OnStateMove(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateMove " + rAnimatorStateInfo.fullPathHash);

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateMove(rAnimator, rAnimatorStateInfo, rLayerIndex);
            }
        }

        /// <summary>
        /// Called at each Update frame except for the first and last frame.
        /// </summary>
        /// <param name="rAnimator">Animator the behavior is tied to</param>
        /// <param name="rAnimatorStateInfo">Information about the current animator state</param>
        /// <param name="rLayerIndex">Layer this state is part of</param>
        public override void OnStateUpdate(Animator rAnimator, AnimatorStateInfo rAnimatorStateInfo, int rLayerIndex)
        {
            //Log.FileWrite("MotionControllerBehaviour.OnStateUpdate " + rAnimatorStateInfo.fullPathHash);

            for (int i = 0; i < mMotionCount; i++)
            {
                mMotions[i].OnStateUpdate(rAnimator, rAnimatorStateInfo, rLayerIndex);
            }
        }
    }
}
