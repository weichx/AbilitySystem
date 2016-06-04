using System;
using UnityEngine;

namespace Intelligence {

    public class HasComponent : Requirement {

        public string componentTypeName;
        private Type componentType;

        //todo build a custom editor for this
        public override bool Check(Context context) {
            //if (componentTypeName == null) {
            //    componentTypeName = "";
            //}
            //componentType = Type.GetType(componentTypeName);
            //if (componentType == null) {
            //    Debug.Log("Cannot find component type: " + componentTypeName);
            //}
            return true;// context.entity.GetComponent(componentType) != null;
        }

    }

}