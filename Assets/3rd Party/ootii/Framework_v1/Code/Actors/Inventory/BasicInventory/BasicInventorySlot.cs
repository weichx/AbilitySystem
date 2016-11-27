using System;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// Very basic inventory slot (where an inventory item would go)
    /// </summary>
    [Serializable]
    public class BasicInventorySlot
    {
        /// <summary>
        /// Unique ID for the slot
        /// </summary>
        public string ID = "";

        /// <summary>
        /// ID of the item in the slot
        /// </summary>
        public string ItemID = "";
    }
}
