using System;

namespace AbilitySystem {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireAbilityAttrAttribute : Attribute {
        public string requiredAttribute;

        public RequireAbilityAttrAttribute(string requiredAttribute) {
            this.requiredAttribute = requiredAttribute;
        }
    }
}