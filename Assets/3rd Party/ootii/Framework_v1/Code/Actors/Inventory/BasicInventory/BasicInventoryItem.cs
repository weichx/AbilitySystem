using System;
using UnityEngine;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// Very basic inventory item
    /// </summary>
    [Serializable]
    public class BasicInventoryItem
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string ID = "";

        /// <summary>
        /// Resource path to the item
        /// </summary>
        public string ResourcePath = "";

        /// <summary>
        /// Motion to use to equip the item
        /// </summary>
        public string EquipMotion = "";

        /// <summary>
        /// Motion to use to unequip the item
        /// </summary>
        public string StoreMotion = "";

        /// <summary>
        /// Scene object representing the item. This is useful if the item
        /// already exists as a child of the character and doesn't need to be re-created.
        /// For example, a sheathed sword or bow on the back.
        /// </summary>
        public GameObject Instance = null;

        /// <summary>
        /// When stored on the character, the bone transform that is the parent
        /// </summary>
        [NonSerialized]
        public Transform StoredParent = null;

        /// <summary>
        /// When stored on the character, the relative position
        /// </summary>
        [NonSerialized]
        public Vector3 StoredPosition = Vector3.zero;

        /// <summary>
        /// When stored on the character, the relative rotation
        /// </summary>
        [NonSerialized]
        public Quaternion StoredRotation = Quaternion.identity;
    }
}
