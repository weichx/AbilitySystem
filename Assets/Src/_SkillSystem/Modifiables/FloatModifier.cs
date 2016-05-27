
public struct FloatModifier {

	public float flatBonus;
	public float percentBonus;

	public FloatModifier(float flatBonus, float percentBonus) {
		this.flatBonus = flatBonus;
		this.percentBonus = percentBonus;
	}

	public static FloatModifier Percent(float percent) {
		return new FloatModifier(0, percent);
	}

	public static FloatModifier Value(float flatBonus) {
		return new FloatModifier(flatBonus, 0);
	}

}
