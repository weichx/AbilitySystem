using System;
using System.Collections.Generic;
using System.Text;

namespace com.ootii.Base
{
    /// <summary>
    /// provides a simple interface for all all
    /// base object that exposes identifiers
    /// </summary>
    public interface IBaseObject
    {
        /// <summary>
        /// If a value exists, that value represents a 
        /// unique id or key for the object
        /// </summary>
        string GUID { get; set; }

        /// <summary>
        /// Friendly name for the object that doesn't have to be unique
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Generates a unique ID for the object
        /// </summary>
        string GenerateGUID();
    }
}
