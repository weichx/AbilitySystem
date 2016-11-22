// ********************************************************************************
// Use this file a a starting point for creating your own Motion Controller motion.
// These motions will be compatable with the Motion Controller v2 found here:
// https://www.assetstore.unity3d.com/en/#!/content/15672
//
// This file will also be used by the Motion Builder's Guide found here:
// http://www.ootii.com/unity/motioncontroller/MCMotionBuilder.pdf
//
// As always, feel free to ask questions on the forum here:
// http://forum.unity3d.com/threads/motion-controller.229900
//
// 
// This template isn't the end-all be-all of Motion Building, but it should be a
// good place to start. Simply look for these code sections
// ********************************************************************************
using UnityEngine;
using com.ootii.Actors.AnimationControllers;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ********************************************************************************
// 0. Copy this file and rename it something associated with your motion.
//
//    You want to modify that new file and keep this file intact for future
//    motions you want to create.
// ********************************************************************************

// ********************************************************************************
// 1. Namespaces are like "last names" for code. It helps distinguish between two
//    different classes that may have the same name.
//
//    So, replace the ootii namespace with your own. It can be anything you want.
//    For example, let's say your name is Joe Smith. You could use something like:
//    'namespace smith.motions'
// ********************************************************************************
namespace com.ootii.Actors.AnimationControllers
{
    // ********************************************************************************
    // 2. Replace 'MotionTemplate' class name with the name of your motion. In this example, 
    //    let's assume you are creating a duck motion. So, we'll put 'Duck'.
    //
    // 3. Remove the 'abstract' keyword from the class. This way it shows up in the list
    //    of motions.
    //
    // 4. Add a friendly name for the 'MotionName' attribute. In our example, you could put:
    //    '[MotionName("Smith Duck")]
    //
    // 5. Add a friendly description for the 'MotionDescription' attribute.
    // ********************************************************************************
    /// <summary>
    /// Generic motion that can support any basic mecanim animation
    /// </summary>
    [MotionName("Motion Template")]
    [MotionDescription("Starting point for your own custom motions.")]
    public abstract class MotionTemplate : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;

        // ********************************************************************************
        // 6. Replace the '0' with a valid motion phase ID. When following the Motion
        //    Builder's guide, you created a unique number to be used by the Mecanim
        //    Animator transition condition that starts your animation. That's the number
        //    you want to use here. 
        // ********************************************************************************
        public const int PHASE_START = 0;

        // ********************************************************************************
        // 7. Change the class name of this constructor to match your new class. In our
        //    example, it would be 'Duck'
        //
        // 8. Provide a default priority. The higher the priority, the more important the
        //    motion and it will take precidence over other motions that want to activate.
        //
        // 9. Provide the name of your sub-state machine. For example "Riding-SM" or "Idle-SM"
        // ********************************************************************************
        /// <summary>
        /// Default constructor
        /// </summary>
        public MotionTemplate()
            : base()
        {
            _Priority = 10;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "YOUR-SUB-STATE-MACHINE-NAME"; }
#endif
        }

        // ********************************************************************************
        // 10. Change the class name of this constructor to match your new class. In our
        //    example, it would be 'Duck'
        //
        // 11. Provide a default priority. The higher the priority, the more important the
        //     motion and it will take precidence over other motions that want to activate.
        //
        // 12. Provide the name of your sub-state machine. For example "Riding-SM" or "Idle-SM"
        // ********************************************************************************
        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public MotionTemplate(MotionController rController)
            : base(rController)
        {
            _Priority = 10;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "YOUR-SUB-STATE-MACHINE-NAME"; }
#endif
        }

        /// <summary>
        /// Allows for any processing after the motion has been deserialized
        /// </summary>
        public override void Initialize()
        {
            // ********************************************************************************
            // 13. Any initialization you need to do for the motion can happen here.
            //
            //     Typically, this function is only called once when the motion is deserialized.
            //     However, it could happen multiple times. So, just be aware of that.
            // ********************************************************************************
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable)
            {
                return false;
            }

            // ********************************************************************************
            // 14. Here, you would add code to determine if it is time for your motion to
            //     activate. If multiple motions can activate, the one with the highest 
            //     priority wins.
            //
            //     When you determine it is time to activate, just return 'true'.
            // ********************************************************************************

            // Return the final result
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns>Boolean that determines if the motion continues</returns>
        public override bool TestUpdate()
        {
            // Ensure we're in the animation
            if (mIsAnimatorActive)
            {
                // ********************************************************************************
                // 15. Here, add code to determine if you should continue to run this motion.
                //
                //     If you've determined it's time to deactivate the motion, just return 'false'.
                // ********************************************************************************
            }

            return true;
        }


        /// <summary>
        /// Raised when a motion is being interrupted by another motion
        /// </summary>
        /// <param name="rMotion">Motion doing the interruption</param>
        /// <returns>Boolean determining if it can be interrupted</returns>
        public override bool TestInterruption(MotionControllerMotion rMotion)
        {
            // ********************************************************************************
            // 16. If another motion with a higher priority wants to activate and deactivate
            //     your motion, you can prevent that from happening here. Simply return 'false'.
            // ********************************************************************************

            return true;
        }
        
        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // ********************************************************************************
            // 17. If there's code you want to run when the motion first activates, you can
            //     do that here. This function will run once for every time the motion is 
            //     activated.
            // ********************************************************************************

            // Tell the animator to start your animations
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START, true);

            // Return
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            // ********************************************************************************
            // 18. If there's code you want to run when the motion deactivates, you can
            //     do that here. This function will run once for every time the motion is 
            //     deactivated.
            // ********************************************************************************

            // Finish the deactivation process
            base.Deactivate();
        }

        /// <summary>
        /// Allows the motion to modify the root-motion velocities before they are applied. 
        /// 
        /// NOTE:
        /// Be careful when removing rotations as some transitions will want rotations even 
        /// if the state they are transitioning from don't.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rVelocityDelta">Root-motion linear velocity relative to the actor's forward</param>
        /// <param name="rRotationDelta">Root-motion rotational velocity</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rVelocityDelta, ref Quaternion rRotationDelta)
        {
            // ********************************************************************************
            // 19. If you want to add, modify, or remove the animation's root-motion, do 
            //     it here.
            // ********************************************************************************
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            // ********************************************************************************
            // 20. If there's code you want to run when your motion is activated and running,
            //     you can do it here.
            //
            //     This function typically runs once per frame.
            // ********************************************************************************
        }

        // ********************************************************************************
        // 21. Auto-generated code...
        //
        //     In a lot of my motions, you'll see code in a section labeled 'auto-generated'.
        //     This isn't manditory, but it helps in a couple of ways:
        //
        //     a. It provides others with a one-click way of setting up the animator 
        //        sub-state machine that goes with this motion. It's useful if you plan
        //        on sharing the motion.
        //
        //     b. It includes some helpful identifiers and functions that makes motion 
        //        building easier.
        //        
        //        IsMotionState() is especially helpful to know what state or transition
        //        the animator is currently in.
        //
        //     To generate the auto-generated code:
        //
        //     a. Finish building out your associated animator sub-state machine.
        //
        //     b. Select the character in your scene and go to the Motion Controller.
        //
        //     c. Add this motion to his list of motions and select it.
        //
        //     d. Press the blue gear icon in the motion details.
        //
        //     e. Open the 'Script Generators' area.
        //
        //     f. Press the 'Generate Script' button. You'll see dialog box pop up.
        //
        //     g. The generated code is now on your clip board. Paste it below.
        // ********************************************************************************
    }
}

// ********************************************************************************
// Wrap Up:
//
// That's the basics of creating your own motion. Now, what you do inside the motion
// totally depends on what your motion is all about. So, you can create any
// motion you can think of... you just need to code it within this framework.
// ********************************************************************************

