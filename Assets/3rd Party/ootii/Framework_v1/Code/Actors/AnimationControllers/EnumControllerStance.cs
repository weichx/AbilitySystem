using System;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Defines the different modes of the controller
    /// </summary>
    public class EnumControllerStance
    {
        /// <summary>
        /// Stance that supports jumping, climbing, etc.
        /// </summary>
        public const int TRAVERSAL = 0;

        /// <summary>
        /// Stance for close quarter combat
        /// </summary>
        public const int COMBAT_MELEE = 1;

        /// <summary>
        /// Stance for ranged combat
        /// </summary>
        public const int COMBAT_RANGED = 2;

        /// <summary>
        /// Stance for swimming and diving
        /// </summary>
        public const int SWIMMING = 3;

        /// <summary>
        /// Stance for stealth
        /// </summary>
        public const int STEALTH = 4;
        public const int SNEAK = 4;     // Depricated

        /// <summary>
        /// Stance for hanging on the ledge in the crouch pose
        /// </summary>
        public const int CLIMB_CROUCH = 5;

        /// <summary>
        /// Stance for climbing ladders
        /// </summary>
        public const int CLIMB_LADDER = 6;

        /// <summary>
        /// Stance for scaling walls
        /// </summary>
        public const int CLIMB_WALL = 7;

        /// <summary>
        /// Stance for casting spells through Spell Activator
        /// </summary>
        public const int CASTING = 8;

        /// <summary>
        /// Stance for when the spell is ready, but not released
        /// </summary>
        public const int CHANNELING = 9;

        /// <summary>
        /// Friendly name of the type
        /// </summary>
        public static string[] Names = new string[] { 
            "Traversal", 
            "Combat-Melee", 
            "Combat-Ranged", 
            "Swimming",
            "Stealth",
            "Climb-Crouch",
            "Climb-Ladder",
            "Climb-Wall",
            "Casting",
            "Channeling"
        };
    }
}

