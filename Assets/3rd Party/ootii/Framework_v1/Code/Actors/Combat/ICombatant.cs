using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors.LifeCores;
using com.ootii.Actors.AnimationControllers;

namespace com.ootii.Actors.Combat
{
    /// <summary>
    /// Provides combat based information about a specific actor.
    /// </summary>
    public interface ICombatant
    {
        /// <summary>
        /// Transform this combatant is tied to
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Position on the character where combat originates (typically the chest or shoulders)
        /// </summary>
        Vector3 CombatOrigin { get; }

        /// <summary>
        /// Minimum distance the combatant can reach for melee combat.
        /// </summary>
        float MinMeleeReach { get; }

        /// <summary>
        /// Maximum distance the combatant can reach for melee combat (not including weapon length).
        /// </summary>
        float MaxMeleeReach { get; }

        /// <summary>
        /// Target the combatant is focusing on.
        /// </summary>
        Transform Target { get; set; }

        /// <summary>
        /// Determines if the actor is locked onto the target.
        /// </summary>
        bool IsTargetLocked { get; set; }

        /// <summary>
        /// Primary weapon being used. We track it here to make it easier for
        /// others to access it
        /// </summary>
        IWeaponCore PrimaryWeapon { get; set; }

        /// <summary>
        /// Secondary weapon being used. We track it here to make it easier for
        /// others to access it
        /// </summary>
        IWeaponCore SecondaryWeapon { get; set; }

        /// <summary>
        /// Attack style that is currently ready (or in use).
        /// </summary>
        ICombatStyle CombatStyle { get; set; }

        /// <summary>
        /// Grab all the combat targets that could be affected by the style and the primary weapon
        /// </summary>
        /// <param name="rStyle">AttackStyle that defines the field-of-attack</param>
        /// <param name="rCombatTargets">List of CombatTargets we will fill with the results</param>
        /// <returns></returns>
        int QueryCombatTargets(AttackStyle rStyle, List<CombatTarget> rCombatTargets);

        /// <summary>
        /// Allows the combatant a chance to modify the motion before it is fully activated.
        /// </summary>
        /// <param name="rMotion">Motion that is being activated</param>
        /// <returns>Boolean used to determine if the motion should continue activation.</returns>
        bool OnMotionActivated(MotionControllerMotion rMotion);

        /// <summary>
        /// Allows the attacker to handle pre-attack messages
        /// </summary>
        /// <param name="rMessage">CombatMessage that contains information about the attack.</param>
        void OnPreAttack(CombatMessage rMessage);

        /// <summary>
        /// Allows the attacker to handle post-attack messages
        /// </summary>
        /// <param name="rMessage">CombatMessage that contains information about the attack.</param>
        void OnPostAttack(CombatMessage rMessage);

        /// <summary>
        /// Allows the defender to handle attack messages
        /// </summary>
        /// <param name="rMessage">CombatMessage that contains information about the attack.</param>
        void OnAttacked(CombatMessage rMessage);
    }
}
