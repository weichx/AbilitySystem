using System;
using UnityEngine;

namespace Intelligence {

    public class HasComponent : Requirement {

        public string componentTypeName;

        //todo build a custom editor for this
        public override bool Check(Context context) {
            return true;
        }

    }

}