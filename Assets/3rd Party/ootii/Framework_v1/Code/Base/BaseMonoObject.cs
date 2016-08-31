/// Tim Tryzbiak, ootii, LLC
using System;
using UnityEngine;
using com.ootii.Data.Serializers;

namespace com.ootii.Base
{
    /// <summary>
    /// Provides a simple foundation for all of our objects
    /// </summary>
    [Serializable]
    public class BaseMonoObject : MonoBehaviour, IBaseObject
    {
        /// <summary>
        /// Allows others to register and listen for when the GUID changes
        /// </summary>
        public GUIDChangedDelegate GUIDChangedEvent = null;

        /// <summary>
        /// If a value exists, that value represents a 
        /// unique id or key for the object
        /// </summary>
        [HideInInspector]
        public string _GUID = "";

        [SerializationIgnore]
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
        [SerializationIgnore]
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseMonoObject()
        {
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

        /// <summary>
        /// If the GUID changes (which can happen when coping object
        /// or creating objects from prefabs, we may need to do something special
        /// </summary>
        public virtual void OnGUIDChanged()
        {
        }
    }
}

