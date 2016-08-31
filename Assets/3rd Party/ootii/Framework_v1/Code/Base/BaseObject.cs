/// Tim Tryzbiak, ootii, LLC
using System;
using com.ootii.Data.Serializers;

namespace com.ootii.Base
{
    /// <summary>
    /// Provides a signature for reporting when a GUID changes
    /// </summary>
    /// <param name="rOldGUID">GUID that was the unique identifier</param>
    /// <param name="rNewGUID">GUID that is now the unique identifier</param>
    public delegate void GUIDChangedDelegate(string rOldGUID, string rNewGUID);

    /// <summary>
    /// Provides a simple foundation for all of our objects
    /// </summary>
    [Serializable]
    public class BaseObject : IBaseObject
    {
        /// <summary>
        /// Allows others to register and listen for when the GUID changes
        /// </summary>
        public GUIDChangedDelegate GUIDChangedEvent = null;

        /// <summary>
        /// If a value exists, that value represents a 
        /// unique id or key for the object across all objects
        /// </summary>
        public string _GUID = "";
        public virtual string GUID
        {
            get
            {
                if (_GUID.Length == 0) { GenerateGUID(); }
                return _GUID;
            }

            set
            {
                if (value.Length == 0) { return; }

                string lOldGUID = _GUID;
                _GUID = value;

                if (lOldGUID.Length > 0 && value != lOldGUID)
                {
                    OnGUIDChanged(lOldGUID, _GUID);
                }
            }
        }

        /// <summary>
        /// Friendly name for the object that doesn't have to be unique
        /// </summary>
        public string _Name = "";
        public virtual string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseObject()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseObject(string rGUID)
        {
            _GUID = rGUID;
        }

        /// <summary>
        /// Generates a unique ID for the object
        /// </summary>
        public string GenerateGUID()
        {
            _GUID = Guid.NewGuid().ToString();
            return _GUID;
        }

        /// <summary>
        /// If the GUID changes (which can happen when coping object
        /// or creating objects from prefabs, we may need to do something special
        /// </summary>
        public virtual void OnGUIDChanged(string rOldGUID, string rNewGUID)
        {
            // Fire off the delegates
            if (GUIDChangedEvent != null) { GUIDChangedEvent(rOldGUID, rNewGUID); }
        }

        ///// <summary>
        ///// Raised after the object has been deserialized. It allows
        ///// for any initialization that may need to happen
        ///// </summary>
        //public virtual void OnDeserialized()
        //{
        //}

        ///// <summary>
        ///// Raised after all objects have been deserialized. It allows us
        ///// to perform initialization. This is especially important if
        ///// the initialization relies on other objects.
        ///// </summary>
        //public virtual void OnPostDeserialized()
        //{
        //}
    }
}

