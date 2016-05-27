using System;

namespace Intelligence {
	
	[Serializable]
	public abstract class Requirement {

		public string name;
		public string description;

		public abstract bool Check(Context context);

	}

}