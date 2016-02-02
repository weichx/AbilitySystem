using System;

namespace AbilitySystem {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class WritableAttribute : EditorCallableAttribute {

        public WritableAttribute(string fnName) : base(fnName, 0) { }

        public WritableAttribute(bool resultOverride) : base(resultOverride, 1) { }
    }
}