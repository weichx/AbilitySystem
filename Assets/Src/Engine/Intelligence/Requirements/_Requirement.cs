using System;

namespace Intelligence {
	
	public abstract class Requirement {

		public string name;
		public string description;

	    internal Requirement() {}

	    public virtual void Initialize() {}

	    public abstract bool Check(Context context);

	    public virtual Type GetContextType() {
	        return typeof(Context);
	    }

	}

    public abstract class Requirement<T> : Requirement where T : Context {

        public override bool Check(Context context) {
            return Check(context as T);
        }

        public abstract bool Check(T context);

        public override Type GetContextType() {
            return typeof(T);
        }
    }

}