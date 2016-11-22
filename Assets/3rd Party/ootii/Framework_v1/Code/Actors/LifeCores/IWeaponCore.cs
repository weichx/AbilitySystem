using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors.Combat;

namespace com.ootii.Actors.LifeCores
{
    /// <summary>
    /// Foundation for weapons that need basic information
    /// </summary>
    public interface IWeaponCore : IItemCore
    {
        /// <summary>
        /// Determines if the weapon is actively looking for contact
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Determines if the weapon uses colliders
        /// </summary>
        bool HasColliders { get; }

        /// <summary>
        /// Collider used by the item
        /// </summary>
        Collider Collider { get; }

        /// <summary>
        /// Minimum range the weapon can apply damage
        /// </summary>
        float MinRange { get; }

        /// <summary>
        /// Maximum range the weapon can apply damage
        /// </summary>
        float MaxRange { get; }

        /// <summary>
        /// Motion name to use when damage is taken
        /// </summary>
        string DamagedMotion { get; }

        /// <summary>
        /// Motion name to use when death occurs
        /// </summary>
        string DeathMotion { get; }

        /// <summary>
        /// Apply all the impacts that could occur
        /// </summary>
        /// <param name="rCombatant">Combatant that is holding the weapon</param>
        /// <param name="rCombatTargets">Targets who we may be impacting</param>
        /// <returns>The number of impacts that occurred</returns>
        int ProcessCombatTargets(ICombatant rCombatant, List<CombatTarget> rCombatTargets);
    }
}
