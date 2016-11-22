using UnityEngine;

namespace com.ootii.Actors.LifeCores
{
    /// <summary>
    /// Foundation for weapons that need basic information
    /// </summary>
    public interface IItemCore : ILifeCore
    {
        /// <summary>
        /// Game object that owns the item. This is not the item itself.
        /// </summary>
        GameObject Owner { get; set; }

        /// <summary>
        /// Game object that is this item.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Local position relative to its parent object
        /// </summary>
        Vector3 LocalPosition { get; }

        /// <summary>
        /// Local rotation relative to its parent object
        /// </summary>
        Quaternion LocalRotation { get; }

        /// <summary>
        /// Max amount of damage the item can take before being destroyed
        /// </summary>
        float MaxHealth { get; set; }

        /// <summary>
        /// Current amount of damage the item can take before being destroyed
        /// </summary>
        float Health { get; set; }

        /// <summary>
        /// Sound to play when the item is equipped
        /// </summary>
        AudioClip EquipSound { get; set; }

        /// <summary>
        /// Raised when the item is equipped
        /// </summary>
        void OnEquipped();

        /// <summary>
        /// Rased when the item is stored
        /// </summary>
        void OnStored();
    }
}
