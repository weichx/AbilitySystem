using System.Collections.Generic;
using UnityEngine;

namespace com.ootii.Actors.Attributes
{
    /// <summary>
    /// Creates a simple attribute system. That is both the "AttributeSource" and the
    /// character attributes.
    /// 
    /// If you use a more advanced attribute system, simply create an "AttributeSource" 
    /// that represents a bridge for your system.
    /// </summary>
    public class BasicAttributes : MonoBehaviour, IAttributeSource
    {
        /// <summary>
        /// List of inventory items
        /// </summary>
        public List<BasicAttribute> Items = new List<BasicAttribute>();

        /// <summary>
        /// Determines if the attribute exists.
        /// </summary>
        /// <param name="rItemID">String representing the name or ID of the attribute we're checking</param>
        /// <returns></returns>
        public virtual bool AttributeExists(string rAttributeID)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].ID == rAttributeID)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Given the specified attribute, grab the float value
        /// </summary>
        /// <param name="rAttribute">string representing the attribute type we want</param>
        /// <param name="rDefault">Default value if the attribute isn't found</param>
        /// <returns>Float representing the value of the attribute or default if not found.</returns>
        public virtual float GetAttributeValue(string rAttributeID, float rDefault = 0f)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].ID == rAttributeID)
                {
                    return Items[i].FloatValue;
                }
            }

            return rDefault;
        }

        /// <summary>
        /// Given the specified attribute, set the value associated with the attribute
        /// </summary>
        /// <param name="rAttribute">String representing the name or ID of the item we want</param>
        /// <param name="rValue">value to set on the attribute</param>
        public virtual void SetAttributeValue(string rAttributeID, float rValue)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].ID == rAttributeID)
                {
                    Items[i].FloatValue = rValue;
                }
            }
        }
    }
}
