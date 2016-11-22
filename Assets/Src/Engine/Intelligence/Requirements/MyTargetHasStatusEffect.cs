using EntitySystem;

namespace Intelligence {

    public class MyTargetHasStatusEffect : Requirement<SingleTargetContext> {

        public StatusEffectCreator statusEffect;
        public string[] strings;

        public override bool Check(SingleTargetContext targetContext) {
           return targetContext.target.statusManager.HasStatus(statusEffect);
        }
    }

}
