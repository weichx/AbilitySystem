using System;

namespace Intelligence {
	
	[Serializable]
	public abstract class Consideration {

		public string name;
		public string description;
		public ResponseCurve curve;

		public abstract float Score(Context context);

	}

}