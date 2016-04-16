
public class IntModifier {

	public readonly int flatBonus;
	public readonly float percentBonus;

	private IntModifier(int flatBonus, float percentBonus) {
		this.flatBonus = flatBonus;
		this.percentBonus = percentBonus;
	}

	public static IntModifier Percent(float percent) {
		return new IntModifier(0, percent);
	}

	public static IntModifier Value(int flatBonus) {
		return new IntModifier(flatBonus, 0);
	}

}
