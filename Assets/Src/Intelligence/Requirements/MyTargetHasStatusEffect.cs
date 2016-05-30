using System;

namespace Intelligence {

    [Serializable]
    public class MyTargetHasStatusEffect : Requirement {

        public StatusEffectCreator statusEffect;
        public string[] strings;

        public override bool Check(Context context) {
            var targetContext = context as SingleTargetContext;
            return true;//return targetContext.target.statusManager.HasStatus(statusEffect);
        }
    }

}
