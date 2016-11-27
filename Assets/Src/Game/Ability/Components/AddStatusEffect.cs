using Intelligence;
using EntitySystem;

public class AddStatusEffect : AbilityComponent<SingleTargetContext> {

    public StatusEffectCreator statusEffect;

    public override void OnCastCompleted() {
        SingleTargetContext ctx = ability.GetContext<SingleTargetContext>();
        if (ctx == null) return;
        StatusEffect effect = statusEffect.Create();
        //ctx.target.statusManager.AddStatusEffect(effect);
    }

}