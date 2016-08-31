using System;

namespace com.ootii.Actors.AnimationControllers
{
    /// <summary>
    /// Defines the tooltip value for motion properties
    /// </summary>
    public class MotionNameAttribute : Attribute
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
        public MotionNameAttribute(string rValue)
        {
            mValue = rValue;
        }
    }

    /// <summary>
    /// Defines the tooltip value for motion properties
    /// </summary>
    public class MotionDescriptionAttribute : Attribute
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
        public MotionDescriptionAttribute(string rValue)
        {
            this.mValue = rValue;
        }
    }
}
