using System;
using UnityEngine;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// Interface used to abstract how inventory and item information is retrieved. Implement from this interface
    /// as needed in order to allow other assets to provide access to your character's inventory.
    /// </summary>
    public interface IInventorySource
    {
        /// <summary>
        /// Some motions will use this to determine if they should test
        /// for activation or allow the inventory source to drive activation.
        /// </summary>
        bool AllowMotionSelfActivation { get; }

        /// <summary>
        /// Instantiates the specified item and equips it. We return the instantiated item.
        /// </summary>
        /// <param name="rItemID">String representing the name or ID of the item to equip</param>
        /// <param name="rSlotID">String representing the name or ID of the slot to equip</param>
        /// <param name="rResourcePath">Alternate resource path to override the ItemID's</param>
        /// <returns>GameObject that is the instance or null if it could not be created</returns>
        GameObject EquipItem(string rItemID, string rSlotID, string rResourcePath = "");

        /// <summary>
        /// Destroys the equipped item and clears the slot.
        /// </summary>
        /// <param name="rSlotID">String representing the name or ID of the slot to clear</param>
        void StoreItem(string rSlotID);        

        /// <summary>
        /// Retrieves the item id for the item that is in the specified slot. If no item is slotted, returns an empty string.
        /// </summary>
        /// <param name="rSlotID">String representing the name or ID of the slot we're checking</param>
        /// <returns>ID of the item that is in the slot or the empty string</returns>
        string GetItemID(string rSlotID);

        /// <summary>
        /// Retrieves a specific item's property value.
        /// </summary>
        /// <typeparam name="T">Type of property being retrieved</typeparam>
        /// <param name="rItemID">String representing the name or ID of the item whose property we want.</param>
        /// <param name="rPropertyID">String representing the name or ID of the property whose value we want.</param>
        /// <returns>Value of the property or the type's default</returns>
        T GetItemPropertyValue<T>(string rItemID, string rPropertyID);
    }
}
