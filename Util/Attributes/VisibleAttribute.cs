using System;

namespace AbilitySystem {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class VisibleAttribute : EditorCallableAttribute {

        public VisibleAttribute(string fnName) : base(fnName, 1) {}

        public VisibleAttribute(bool resultOverride) : base(resultOverride, 1) { }
    }
}