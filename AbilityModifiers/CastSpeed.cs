

public class CastSpeedModifier : AbilityModifier {

    public float power;

    public CastSpeedModifier(string id, float power) : base(id) {
        this.power = power;
    }

    public override void OnApply(Ability ability) {
        UnityEngine.Debug.Log(power);
        ability.castTime.SetPercentBonus(id, -power);
    }

    public override void OnRemove(Ability ability) {
        ability.castTime.RemoveModifier(id);
    }

}