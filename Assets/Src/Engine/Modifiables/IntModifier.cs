
namespace EntitySystem {
	
	public struct IntModifier {

	    public string id;
	    public readonly int flatBonus;
	    public readonly float percentBonus;

	    public IntModifier(string id, int flatBonus, float percentBonus) {
	        this.id = id;
	        this.flatBonus = flatBonus;
	        this.percentBonus = percentBonus;
	    }

	    public IntModifier(string id, IntModifier other) {
	        this.id = id;
	        flatBonus = other.flatBonus;
	        percentBonus = other.percentBonus;
	    }

	    public static IntModifier Percent(float percent) {
	        return new IntModifier("", 0, percent);
	    }

	    public static IntModifier Value(int flatBonus) {
	        return new IntModifier("", flatBonus, 0);
	    }

	    public static implicit operator FloatModifier(IntModifier modifier) {
	       return new FloatModifier(modifier.id, modifier.flatBonus, modifier.percentBonus);
	    }

	}

}