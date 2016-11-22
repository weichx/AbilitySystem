using System;

namespace com.ootii.Actors.Attributes
{
    /// <summary>
    /// Very basic inventory item
    /// </summary>
    [Serializable]
    public class BasicAttribute
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string ID = "";

        /// <summary>
        /// Value of the attribute
        /// </summary>
        public float FloatValue = 0f;
    }
}
