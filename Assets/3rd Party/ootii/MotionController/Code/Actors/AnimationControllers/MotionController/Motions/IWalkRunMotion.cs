namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Interface to help identify walk/run motions that we may need to
    /// blend into from other motions.
    /// </summary>
    public interface IWalkRunMotion
    {
        /// <summary>
        /// Determines if the motion is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Determines if the motion is active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Determines if running is active
        /// </summary>
        bool IsRunActive { get; }

        /// <summary>
        /// Determines if we shortcut the motion and start in a movement
        /// </summary>
        bool StartInMove { get; set; }

        /// <summary>
        /// Determines if we shortcut the motion and start in a walk
        /// </summary>
        bool StartInWalk { get; set; }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        bool StartInRun { get; set; }
    }
}
