using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Fall that occurs when the player is no longer grounded
    /// and didn't come off of a jump
    /// </summary>
    [MotionName("Fall")]
    [MotionDescription("Motion the avatar moves into when they are no longer grounded and are falling. Once they land, " +
                   "the avatar can move into the idle pose or a run.")]
    public class Fall : Jump
    {
        // Enum values for the motion
        public new const int PHASE_UNKNOWN = 0;
        public new const int PHASE_START_FALL = 250;

        /// <summary>
        /// The minimum distance the avatar can fall from
        /// </summary>
        public float _MinFallHeight = 0.3f;
        public float MinFallHeight
        {
            get { return _MinFallHeight; }
            set { _MinFallHeight = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Fall()
            : base()
        {
            _Priority = 20;
            _IsControlEnabled = false;
            _ConvertToHipBase = false;

            _Impulse = 0f;
            mIsStartable = true;
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Fall(MotionController rController)
            : base(rController)
        {
            _Priority = 20;
            _IsControlEnabled = false;
            _ConvertToHipBase = false;

            _Impulse = 0f;
            mIsStartable = true;
        }

        /// <summary>
        /// Initialize is called after all the motions have been initialized. This allow us time to
        /// create references before the motions start working
        /// </summary>
        //public override void Initialize()
        //{
        //    if (mMotionController != null)
        //    {
        //        if (mWalkRunPivot == null) { mWalkRunPivot = mMotionController.GetMotion<WalkRunPivot>(); }
        //        if (mWalkRunStrafe == null) { mWalkRunStrafe = mMotionController.GetMotion<WalkRunStrafe>(); }
        //    }
        //}

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable) { return false; }            
            if (IsInMidJumpState) { return false; }

            if (mActorController.State.IsGrounded) { return false; }
            if (mActorController.State.GroundSurfaceDistance < _MinFallHeight) { return false; }

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Flag the motion as active
            mIsActive = true;
            mIsAnimatorActive = false;
            mIsActivatedFrame = true;
            mIsStartable = false;

            // Force the camera to the default mode
            if (mMotionController.CameraRig != null)
            {
                // TRT 10/13/16: Removed as not needed with CC
                //mMotionController.CameraRig.Mode = 0;
            }

            // Attempt to find the hip bone if we have a name
            if (_ConvertToHipBase)
            {
                if (mHipBone == null)
                {
                    if (_HipBoneName.Length > 0)
                    {
                        mHipBone = mMotionController._Transform.FindChild(_HipBoneName);
                    }

                    if (mHipBone == null)
                    {
                        mHipBone = mMotionController.gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                        if (mHipBone != null) { _HipBoneName = mHipBone.name; }
                    }
                }
            }

            // Reset the distance flag for this jump
            mLastHipDistance = 0f;

            // Clear out the impulse
            mIsImpulseApplied = false;

            // Grab the current velocities
            mLaunchForward = mActorController._Transform.forward;

            mLaunchVelocity = mActorController.State.Velocity;

            Vector3 lVerticalLaunchVelocity = Vector3.Project(mLaunchVelocity, mActorController._Transform.up);
            mLaunchVelocity = mLaunchVelocity - lVerticalLaunchVelocity;

            // Initialize the fall
            mMotionController.SetAnimatorMotionPhase(mMotionLayer._AnimatorLayerIndex, PHASE_START_FALL, true);

            // Report this motion as activated
            if (mMotionController.MotionActivated != null) { mMotionController.MotionActivated(mMotionLayer._AnimatorLayerIndex, this, rPrevMotion); }

            // Report that we're good to enter the fall
            return true;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            bool lNewIsMomenumEnabled = EditorGUILayout.Toggle(new GUIContent("Is Momentum Enabled", "Determines if lateral momentum is kept during the fall."), IsMomentumEnabled);
            if (lNewIsMomenumEnabled != IsMomentumEnabled)
            {
                lIsDirty = true;
                IsMomentumEnabled = lNewIsMomenumEnabled;
            }

            float lNewFallHeight = EditorGUILayout.FloatField(new GUIContent("Min Fall Height", "Minimum ground distance before the fall will kick in."), MinFallHeight);
            if (lNewFallHeight != MinFallHeight)
            {
                lIsDirty = true;
                MinFallHeight = lNewFallHeight;
            }

            return lIsDirty;
        }

#endif

    }
}
