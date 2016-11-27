
namespace EntitySystem {
	
	public struct FloatModifier {

	    public string id;
	    public readonly float flatBonus;
		public readonly float percentBonus;

		public FloatModifier(string id, float flatBonus, float percentBonus) {
		    this.id = id;
	        this.flatBonus = flatBonus;
			this.percentBonus = percentBonus;
		}

	    public FloatModifier(string id, FloatModifier other) {
	        this.id = id;
	        flatBonus = other.flatBonus;
	        percentBonus = other.percentBonus;
	    }

		public static FloatModifier Percent(float percent) {
			return new FloatModifier("", 0, percent);
		}

		public static FloatModifier Value(float flatBonus) {
			return new FloatModifier("", flatBonus, 0);
		}

	    public static implicit operator IntModifier(FloatModifier modifier) {
	        return new IntModifier(modifier.id, (int)modifier.flatBonus, modifier.percentBonus);
	    }

	}

}