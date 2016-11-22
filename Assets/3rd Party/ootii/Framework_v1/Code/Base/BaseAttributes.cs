using System;

namespace com.ootii.Base
{
    /// <summary>
    /// Defines the tooltip value for properties
    /// </summary>
    public class BaseNameAttribute : Attribute
    {
        /// <summary>
        /// Default tooltip value
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Value that is the tooltip</param>
        public BaseNameAttribute(string rValue)
        {
            mValue = rValue;
        }
    }

    /// <summary>
    /// Defines the tooltip value for properties
    /// </summary>
    public class BaseDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Default tooltip value
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Value that is the tooltip</param>
        public BaseDescriptionAttribute(string rValue)
        {
            this.mValue = rValue;
        }
    }
}
