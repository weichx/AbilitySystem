using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Generic motion that can support any basic mecanim animation
    /// </summary>
    [MotionName("Simple Motion")]
    [MotionDescription("Generic motion that can support any basic Mecanim animation. This is an easy way to associate simple animations with characters.")]
    public class SimpleMotion : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;

        /// <summary>
        /// Phase ID to send to the animator when the motion activates.
        /// </summary>
        public int _PhaseID = 0;
        public int PhaseID
        {
            get { return _PhaseID; }
            set { _PhaseID = value; }
        }

        /// <summary>
        /// Normalized time of the animation to exit the motion.
        /// </summary>
        public float _ExitTime = 1f;
        public float ExitTime
        {
            get { return _ExitTime; }
            set { _ExitTime = value; }
        }

        /// <summary>
        /// State we'll use to text the exit time with. If blank, it will simply
        /// be the first state we've entered.
        /// </summary>
        public string _ExitState = "";
        public string ExitState
        {
            get { return _ExitState; }
            set { _ExitState = value; }
        }

        /// <summary>
        /// Determines if we exit when the input alias is released
        /// </summary>
        public bool _ExitOnRelease = false;
        public bool ExitOnRelease
        {
            get { return _ExitOnRelease; }
            set { _ExitOnRelease = value; }
        }

        /// <summary>
        /// Determines if we disable gravity on activation and re-enable it on de-activation
        /// </summary>
        public bool _DisableGravity = false;
        public bool DisableGravity
        {
            get { return _DisableGravity; }
            set { _DisableGravity = value; }
        }

        /// <summary>
        /// Determines if we disable root motion rotations
        /// </summary>
        public bool _DisableRootMotionRotation = false;
        public bool DisableRootMotionRotation
        {
            get { return _DisableRootMotionRotation; }
            set { _DisableRootMotionRotation = value; }
        }

        /// <summary>
        /// Determines if we disable root motion movement
        /// </summary>
        public bool _DisableRootMotionMovement = false;
        public bool DisableRootMotionMovement
        {
            get { return _DisableRootMotionMovement; }
            set { _DisableRootMotionMovement = value; }
        }

        /// <summary>
        /// Hash for the exit state we'll use to deactivate the motion
        /// </summary>
        private int mExitStateID = 0;

        /// <summary>
        /// Determines if we've actually entered the first animation
        /// </summary>
        private bool mHasEnteredAnimatorState = false;

        /// <summary>
        /// Determine if gravity was enabled previously.
        /// </summary>
        private bool mWasGravityEnabled = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleMotion()
            : base()
        {
            _Priority = 10;
            mIsStartable = true;
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public SimpleMotion(MotionController rController)
            : base(rController)
        {
            _Priority = 10;
            mIsStartable = true;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            // Grab the animator has for the exit state
            if (_ExitState.Length > 0)
            {
                mExitStateID = mMotionController.AddAnimatorName(_ExitState);
            }
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

            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            // Ensure we have input to test
            if (mMotionController.InputSource == null)
            {
                return false;
            }

            // Test the input
            if (_ActionAlias.Length > 0 && mMotionController.InputSource.IsJustPressed(_ActionAlias))
            {
                mHasEnteredAnimatorState = false;
                return true;
            }

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
                // We could be in the transition that's getting us to the animation. So, we need
                // to ensure the transition ID is clear. That means we're in a state
                if (mMotionLayer._AnimatorTransitionID == 0)
                {
                    mHasEnteredAnimatorState = true;
                }
            }

            // Once we know we're in the animation state, we can see if it's time to exit
            if (mHasEnteredAnimatorState)
            {
                // If we're in the exit state...
                if (mExitStateID == 0 || mMotionLayer._AnimatorStateID == mExitStateID)
                {
                    // If the exit time has come...
                    if (_ExitTime > 0f && mMotionLayer._AnimatorStateNormalizedTime >= _ExitTime)
                    {
                        return false;
                    }
                }

                // Check if the alias was released and if we should exit
                if (_ExitOnRelease && _ActionAlias.Length > 0 && mMotionController.InputSource.IsReleased(_ActionAlias))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            mHasEnteredAnimatorState = false;
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, _PhaseID, true);

            // Disable gravity if needed
            if (_DisableGravity)
            {
                mWasGravityEnabled = mActorController._IsGravityEnabled;
                mActorController.IsGravityEnabled = false;
            }

            // Return
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            // Re-enable gravity if needed
            if (_DisableGravity)
            {
                mActorController.IsGravityEnabled = mWasGravityEnabled;
            }

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
            if (_DisableRootMotionRotation)
            {
                rRotationDelta = Quaternion.identity;
            }

            if (_DisableRootMotionMovement)
            {
                rVelocityDelta = Vector3.zero;
            }
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Allow the constraint to render it's own GUI
        /// </summary>
        /// <returns>Reports if the object's value was changed</returns>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Action Alias", "Action alias that triggers the motion."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            int lNewPhaseID = EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID to pass to the animation during activation."), PhaseID, GUILayout.MinWidth(30));
            if (lNewPhaseID != PhaseID)
            {
                lIsDirty = true;
                PhaseID = lNewPhaseID;
            }

            GUILayout.Space(5);

            float lNewExitTime = EditorGUILayout.FloatField(new GUIContent("Exit Time", "Normalized time to exit the motion. Typically this is '1' to exit at the end of the animation we entered."), ExitTime, GUILayout.MinWidth(30));
            if (lNewExitTime != ExitTime)
            {
                lIsDirty = true;
                ExitTime = lNewExitTime;
            }

            string lNewExitState = EditorGUILayout.TextField(new GUIContent("Exit State", "Full path of the state we'll use for the Exit Time. If no path exists, we'll use the first state we enter as the default. For example: Base Layer.Utilities-SM.IdlePose"), ExitState, GUILayout.MinWidth(30));
            if (lNewExitState != ExitState)
            {
                lIsDirty = true;
                ExitState = lNewExitState;
            }

            bool lNewExitOnRelease = EditorGUILayout.Toggle(new GUIContent("Exit On Release", "Determines if we disable the motion when the input that activated the motion is released."), ExitOnRelease, GUILayout.MinWidth(30));
            if (lNewExitOnRelease != ExitOnRelease)
            {
                lIsDirty = true;
                ExitOnRelease = lNewExitOnRelease;
            }

            GUILayout.Space(5);

            bool lNewDisableGravity = EditorGUILayout.Toggle(new GUIContent("Disable Gravity", "Determines if we disable the gravity when the motion is activated."), DisableGravity, GUILayout.MinWidth(30));
            if (lNewDisableGravity != DisableGravity)
            {
                lIsDirty = true;
                DisableGravity = lNewDisableGravity;
            }

            bool lNewDisableRootMotionRotation = EditorGUILayout.Toggle(new GUIContent("Disable RM Rotation", "Determines if we disable the rotation caused by root-motion when this motion is activated."), DisableRootMotionRotation, GUILayout.MinWidth(30));
            if (lNewDisableRootMotionRotation != DisableRootMotionRotation)
            {
                lIsDirty = true;
                DisableRootMotionRotation = lNewDisableRootMotionRotation;
            }

            bool lNewDisableRootMotionMovement = EditorGUILayout.Toggle(new GUIContent("Disable RM Movement", "Determines if we disable the movement caused by root-motion when this motion is activated."), DisableRootMotionMovement, GUILayout.MinWidth(30));
            if (lNewDisableRootMotionMovement != DisableRootMotionMovement)
            {
                lIsDirty = true;
                DisableRootMotionMovement = lNewDisableRootMotionMovement;
            }

            return lIsDirty;
        }

#endif

    }
}
