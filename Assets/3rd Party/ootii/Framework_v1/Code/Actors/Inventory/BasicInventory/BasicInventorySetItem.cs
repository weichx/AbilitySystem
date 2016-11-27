using System;
using UnityEngine;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// An inventory set is a collection of items that go together. A BasicInventorySetItem
    /// is an individual item in the set.
    /// </summary>
    [Serializable]
    public class BasicInventorySetItem
    {
        /// <summary>
        /// Determine if the item is instanciated when activated
        /// </summary>
        public bool Instantiate = true;

        /// <summary>
        /// Inventory item this descriptor represents
        /// </summary>
        public string ItemID = "";

        /// <summary>
        /// Slot the item would be placed in when equipped.
        /// </summary>
        public string SlotID = "";
    }
}