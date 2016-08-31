/// Tim Tryzbiak, ootii, LLC
using System;

namespace com.ootii.Data.Serializers
{
    /// <summary>
    /// Defines the name for the object
    /// </summary>
    public class SerializationNameAttribute : Attribute
    {
        /// <summary>
        /// Default value for the item that we won't serialize
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Name to use when serializing</param>
        public SerializationNameAttribute(string rValue)
        {
            mValue = rValue;
        }
    }
    
    /// <summary>
    /// Defines the default value for the object that won't be serialized
    /// </summary>
    public class SerializationDefaultAttribute : Attribute
    {
        /// <summary>
        /// Default value for the item that we won't serialize
        /// </summary>
        protected object mValue;
        public object Value
        {
            get { return this.mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Default value to use if no value is set</param>
        public SerializationDefaultAttribute(object rValue)
        {
            this.mValue = rValue;
        }
    }

    /// <summary>
    /// Defines the write (and load) order of the entry
    /// type when it comes to storing and loading the data.
    /// </summary>
    public class SerializationOrderAttribute : Attribute
    {
        /// <summary>
        /// Default value for the item that we won't serialize
        /// </summary>
        protected int mValue;
        public int Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Order used when storing and loading data</param>
        public SerializationOrderAttribute(int rValue)
        {
            mValue = rValue;
        }
    }

    /// <summary>
    /// Determines if the element should be ignored during serialization
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class SerializationIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        public SerializationIgnoreAttribute()
        {
        }
    }
}
