using System;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Defines the different modes of the controller
    /// </summary>
    public class EnumControllerStance
    {
        public const int TRAVERSAL = 0;
        public const int COMBAT_MELEE = 1;
        public const int COMBAT_RANGED = 2;
        public const int SWIMMING = 3;
        public const int STEALTH = 4;
        public const int SNEAK = 4;     // Depricated
        public const int CLIMB_CROUCH = 5;
        public const int CLIMB_LADDER = 6;
        public const int CLIMB_WALL = 7;
        public const int CASTING = 8;
        public const int CHANNELING = 9;
        public const int COMBAT_RANGED_LONGBOW = 10;
        public const int COMBAT_MELEE_SWORD_SHIELD = 11;

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
            "Channeling",
            "Combat-Ranged (longbow)",
            "Combat-Melee (sword and shield)"
        };
    }
}

