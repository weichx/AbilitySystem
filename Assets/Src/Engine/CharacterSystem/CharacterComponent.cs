using System;
using Intelligence;

namespace EntitySystem {
    public abstract class CharacterComponent {
        [NonSerialized] public InventoryItem item;
        protected Context ctx;


        public virtual void SetContext(Context context) {
            this.ctx = context;
        }
        public virtual Type GetContextType() {
            return typeof(Context);
        }
    }

    public abstract class CharacterComponent<T> : CharacterComponent where T : Context {

        protected new T context;

        public virtual void SetContext(Context context) {
            this.ctx = context as T;
        }
        public virtual Type GetContextType() {
            return typeof(T);
        }
    }
}
