using System;
using UnityEngine;
namespace AbilitySystem {
    [Serializable]
    public class StatusDelegate : ISerializationCallbackReceiver {
        public string[] typeNames;
        public string[] methodNames;
        public Type attrType = typeof(object);
        public string typeString;

        public Action<Status> callback = delegate { };
        public GameObject obj;
        public StatusDelegate(Type attrType) {
            this.attrType = attrType;
            typeNames = new string[] { };
            methodNames = new string[] { };
        }

        public void OnAfterDeserialize() {
            if (attrType == typeof(object)) return;
            for (int i = 0; i < typeNames.Length; i++) {
                //var function = Reflector.FindActionWithAttribute<Status>(attrType.GetType(), typeNames[i], methodNames[i], typeof(Status));
                //if (function != null) {
                //    callback += function;
                //}
            }
        }

        public void OnBeforeSerialize() {
            if (attrType != typeof(object)) {
                typeString = attrType.Name;
            }
        }
    }
}