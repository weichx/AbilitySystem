using  System;
using Intelligence;

namespace Intelligence {
	
	public abstract class Consideration {

		public string name;
		public string description;
		public ResponseCurve curve;

	    internal Consideration() {}

	    public abstract float Score(Context context);

	    public virtual Type GetContextType() {
	        return typeof(Context);
	    }
	}

    public abstract class Consideration<T> : Consideration where T : Context {

        public override float Score(Context context) {
            return Score(context as T);
        }

        public abstract float Score(T context);

        public override Type GetContextType() {
            return typeof(T);
        }
    }
}