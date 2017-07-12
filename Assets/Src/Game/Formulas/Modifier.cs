using System;
using System.Collections.Generic;
using UnityEngine;
using EntitySystem;

namespace Intelligence {
    public abstract class Modifier {

        [NonSerialized]
        protected FormulaCalculation formula;

        protected Context contextType;
        public virtual void ApplyModifier(ref float inValue) { }

        public virtual void SetContext(Context context) {
            this.contextType = context;
        }

        public virtual Type GetContextType() {
            return typeof(Context);
        }
    }

    public abstract class Modifier<T> : Modifier where T : Context {
        protected new T context;

        public override void SetContext(Context context) {
            this.context = context as T;
        }

        public override Type GetContextType() {
            return typeof(T);
        }
    }
}
