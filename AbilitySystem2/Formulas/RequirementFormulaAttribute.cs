using System;
using UnityEditor;

namespace AbilitySystem {

    //[AttributeUsage(AttributeTargets.Method)]
    public class RequirementFormulaAttribute : Attribute {
        //Type type;
        public RequirementFormulaAttribute(Type type) {
         //   this.type = type;
        }
    }

    public class RequirementFormulas {

        [AbilityAttributeFormula]
        public static float Formula() {
            return 0;
        }

    }

}