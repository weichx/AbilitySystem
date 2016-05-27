using System;

[Serializable]
public class ReduceMovementSpeed : StatusEffectComponent {

    public float power;
    public const string SlowStatusId = "Slow Status";

    public override void OnEffectApplied() {
//        FloatAttribute attr = target.GetAttribute("MovementSpeed");
//        if (attr != null) {
//            attr.SetModifier(SlowStatusId, FloatModifier.Percent(power));
//        }
    }

    public override void OnEffectRemoved() {
//        FloatAttribute attr = target.GetAttribute("MovementSpeed");
//        if (attr != null) {
//            attr.RemoveModifier(SlowStatusId);
//        }
    }

}