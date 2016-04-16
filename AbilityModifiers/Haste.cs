
public class DummyComponent : AbilityComponent {

    public override void OnCastCompleted() {
        FloatAttribute castTime = ability.castTime;
        float currentBonus = castTime.GetPercentBonus("Spell Haste");
        UnityEngine.Debug.Log("cast time: " + castTime.Value);
        if (currentBonus <= -0.5f) return;
        castTime.SetPercentBonus("Spell Haste", currentBonus - 0.1f);
    }
}

public class Haste : AbilityModifier {

    protected override void OnApply(Ability ability) {
        ability.castTime.SetModifier("Spell Haste", FloatModifier.Percent(-0.1f));
        ability.AddAbilityComponent<DummyComponent>();
    }

}